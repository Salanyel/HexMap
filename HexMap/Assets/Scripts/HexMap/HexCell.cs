using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HexCell : MonoBehaviour {

	#region Variables

	[SerializeField]
	HexCoordinates _coordinates;

	[SerializeField]
	int _elevation = int.MinValue;

	[SerializeField]
	RectTransform _uiRect;

	[SerializeField]
	HexCell[] _neighbors;

	[SerializeField]
	HexGridChunk _chunk;

	[SerializeField]
	bool[] _roads;

    int _terrainTypeIndex;

	int _specialFeatureIndex;

	bool _hasIncomingRiver;
	bool _hasOutgoingRiver;
	ENUM_HexDirection _incomingRiver;
	ENUM_HexDirection _outgoingRiver;

	public int SpecialIndex {
		get { return _specialFeatureIndex; }
		set {
			if (_specialFeatureIndex != value && !HasRiver) {
				_specialFeatureIndex = value;
				RemoveRoads ();
				RefreshSelfOnly ();
			}
		}
	}

	public Vector3 Position {
		get { return transform.localPosition; }
	}

	public HexCoordinates Coordinates {
		get { return _coordinates; }
		set { _coordinates = value; }
	}

    public int TerrainTypeIndex
    {
        get { return _terrainTypeIndex; }
        set
        {
            if (_terrainTypeIndex != value)
            {
                _terrainTypeIndex = value;
                Refresh();
            }
        }
    }

	public Color Color {
		get { return HexMetrics._colors[_terrainTypeIndex]; }
	}

	public int Elevation {
		get { return _elevation; }
		set { 
			if (_elevation == value) {
				return;
			}

			_elevation = value;
			RefreshPosition ();
		}
	}

	int _waterLevel;
	public int WaterLevel {
		get { return _waterLevel; }
		set {
			if (_waterLevel == value) {
				return;
			}
			_waterLevel = value;
			ValidateRivers ();
			Refresh ();
		}
	}

	int _urbanLevel;
	public int UrbanLevel {
		get { return _urbanLevel; }
		set {
			if (_urbanLevel != value) {
				_urbanLevel = value;
				RefreshSelfOnly ();
			}
		}
	}

	int _farmLevel;
	public int FarmLevel {
		get { return _farmLevel; }
		set { if (_farmLevel != value) {
				_farmLevel = value;
				RefreshSelfOnly();
			}
		}
	}

	int _plantLevel;
	public int PlantLevel {
		get { return _plantLevel; }
		set { if (_plantLevel != value) {
				_plantLevel = value;
				RefreshSelfOnly();
			}
		}
	}

	bool _isUnderWater;
	public bool IsUnderWater {
		get { return _waterLevel > _elevation; }
	}

	bool _isWalled;
	public bool Walled {
		get { return _isWalled; }
		set { if (_isWalled != value) {
				_isWalled = value;
				Refresh ();
			}
		}
	}

	public RectTransform UIRect {
		get { return _uiRect; }
		set { _uiRect = value; }
	}

	public HexGridChunk Chunk {
		set { _chunk = value; }
	}

	public bool HasIncomingRiver {
		get { return _hasIncomingRiver; }
	}

	public bool HasOutgoingRiver {
		get { return _hasOutgoingRiver; }
	}

	public ENUM_HexDirection IncomingRiver {
		get { return _incomingRiver; }
	}

	public ENUM_HexDirection OutgoingRiver {
		get { return _outgoingRiver; }
	}

	public bool HasRiver {
		get { return _hasIncomingRiver || _hasOutgoingRiver; }
	}

	public bool HasRiverBeginOrEnd {
		get { return _hasIncomingRiver != _hasOutgoingRiver; }
	}

	public float StreamBedY {
		get { return (_elevation + HexMetrics._streamBedElevationOffset) * HexMetrics._elevationStep; }
	}

	public float RiverSurfaceY {
		get { return(_elevation + HexMetrics._waterElevationOffset) * HexMetrics._elevationStep; }
	}

	public float WaterSurfaceY {
		get { return (_waterLevel + HexMetrics._waterElevationOffset) * HexMetrics._elevationStep; }
	}

	#endregion

	#region Methods

	public bool IsSpecial() {
		return _specialFeatureIndex > 0;
	}

	public bool HasRoads() {
		for (int i = 0; i < _roads.Length; ++i) {
			if (_roads [i]) {
				return _roads[i];
			}
		}

		return false;
	}

	void RefreshPosition() {
		Vector3 position = transform.localPosition;
		position.y = _elevation * HexMetrics._elevationStep;
		position.y += (HexMetrics.SampleNoise (position).y * 2f - 1f) * HexMetrics._cellPerturbElevation;
		transform.localPosition = position;

		Vector3 uiPosition = _uiRect.localPosition;
		uiPosition.z = -position.y;
		_uiRect.localPosition = uiPosition;

		ValidateRivers ();

		for (int i = 0; i < _roads.Length; ++i) {
			if (_roads[i] && GetElevationDifference ((ENUM_HexDirection)i) >= 1) {
				SetRoad (i, false);
			}
		}

		Refresh ();
	}

	public ENUM_HexDirection RiverBeginOrEndDirection() {
		return _hasIncomingRiver ? _incomingRiver : _outgoingRiver;
	}

	public bool HasRoadThroughEdge(ENUM_HexDirection p_direction) {
		return _roads [(int)p_direction];
	}

	bool IsValidRiverDestination(HexCell p_neighbor) {
		return p_neighbor && (_elevation >= p_neighbor._elevation || _waterLevel == p_neighbor._elevation);
	}

	void SetRoad(int p_index, bool p_state) {
		_roads [p_index] = p_state;
		_neighbors [p_index]._roads [(int)((ENUM_HexDirection)p_index).Opposite ()] = p_state;
		_neighbors [p_index].RefreshSelfOnly ();
		RefreshSelfOnly ();
	}

	public void AddRoad(ENUM_HexDirection p_direction) {
		if (!_roads [(int)p_direction] && !HasRiverThroughEdge(p_direction) &&
			!IsSpecial() && 
			!GetNeighbor(p_direction).IsSpecial() && 
			GetElevationDifference(p_direction) <= 1) {

			SetRoad ((int)p_direction, true);
		}
	}

	public void RemoveRoads() {
		for (int i = 0; i < _roads.Length; ++i) {
			SetRoad (i, false);
		}
	}

	public bool HasRiverThroughEdge(ENUM_HexDirection p_direction) {
		return
			_hasIncomingRiver && _incomingRiver == p_direction ||
			_hasOutgoingRiver && _outgoingRiver == p_direction;
	}

	public HexCell GetNeighbor(ENUM_HexDirection p_direction) {
		return _neighbors [(int)p_direction];
	}

	public void SetNeighbor(ENUM_HexDirection p_direction, HexCell p_cell) {
		_neighbors [(int)p_direction] = p_cell;
		p_cell._neighbors [(int)p_direction.Opposite ()] = this;
	}

	public int GetElevationDifference(ENUM_HexDirection p_direction) {
		int difference = _elevation - _neighbors [(int)p_direction].Elevation;
		return difference >= 0 ? difference : -difference;
	}

	public ENUM_HexEdgeType GetEdgeType(ENUM_HexDirection p_direction) {
		return HexMetrics.GetEdgeType(_elevation, _neighbors[(int) p_direction].Elevation);
	}

	public ENUM_HexEdgeType GetEdgeType(HexCell p_cell) {
		return HexMetrics.GetEdgeType (_elevation, p_cell.Elevation);
	}

	void ValidateRivers() {
		if (_hasOutgoingRiver && !IsValidRiverDestination(GetNeighbor(_outgoingRiver))) {
			RemoveOutgoingRiver();
		}

		if (_hasIncomingRiver && !GetNeighbor (_incomingRiver).IsValidRiverDestination (this)) {
			RemoveIncomingRiver ();
		}
	}

	public void RemoveRiver() {
		RemoveOutgoingRiver ();
		RemoveIncomingRiver ();
	}

	public void RemoveOutgoingRiver () {
		if (!_hasOutgoingRiver) {
			return;
		}

		_hasOutgoingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor (_outgoingRiver);
		neighbor._hasIncomingRiver = false;
		neighbor.RefreshSelfOnly();
	}

	public void RemoveIncomingRiver() {
		if (!_hasIncomingRiver) {
			return;
		}

		_hasIncomingRiver = false;
		RefreshSelfOnly ();

		HexCell neighbor = GetNeighbor (_incomingRiver);
		neighbor._hasOutgoingRiver = false;
		neighbor.RefreshSelfOnly ();
	}

	public void SetOutgoingRiver(ENUM_HexDirection p_direction) {
		if (_hasOutgoingRiver && _outgoingRiver == p_direction) {
			return;
		}

		HexCell neighbor = GetNeighbor (p_direction);
		if (!IsValidRiverDestination(neighbor)) {
			return;
		}

		RemoveOutgoingRiver ();
		if (_hasIncomingRiver && _incomingRiver == p_direction) {
			RemoveIncomingRiver ();
		}

		_hasOutgoingRiver = true;
		_outgoingRiver = p_direction;
		_specialFeatureIndex = 0;

		neighbor.RemoveIncomingRiver ();
		neighbor._hasIncomingRiver = true;
		neighbor._incomingRiver = p_direction.Opposite ();
		neighbor._specialFeatureIndex = 0;

		SetRoad ((int)p_direction, false);
	}

	void Refresh() {
		if (_chunk) {
			_chunk.Refresh ();

			for (int i = 0; i < _neighbors.Length; ++i) {
				HexCell neighbor = _neighbors [i];
				if (neighbor != null && neighbor._chunk != _chunk) {
					neighbor._chunk.Refresh ();
				}
			}
		}
	}

	void RefreshSelfOnly() {
		_chunk.Refresh ();
	}

	public void Save(BinaryWriter p_writer) {
		p_writer.Write ((byte) _terrainTypeIndex);
		p_writer.Write ((byte) _elevation);
		p_writer.Write ((byte) _waterLevel);
		p_writer.Write ((byte) _urbanLevel);
		p_writer.Write ((byte) _farmLevel);
		p_writer.Write ((byte) _plantLevel);
		p_writer.Write ((byte) _specialFeatureIndex);
		p_writer.Write (_isWalled);

		if (_hasIncomingRiver) {
			p_writer.Write ((byte)(_incomingRiver + 128));
		} else {
			p_writer.Write ((byte)0);
		}
			
		if (_hasOutgoingRiver) {
			p_writer.Write ((byte)(_outgoingRiver + 128));
		} else {
			p_writer.Write ((byte)0);
		}

		int roadFlags = 0;
		for (int i = 0; i < _roads.Length; ++i) {
			if (_roads [i]) {
				roadFlags |= 1 << i;
			}
		}
		p_writer.Write((byte) roadFlags);
	}

	public void Load(BinaryReader p_reader) {
		_terrainTypeIndex = p_reader.ReadByte ();
		_elevation = p_reader.ReadByte ();
		RefreshPosition ();

		_waterLevel = p_reader.ReadByte ();
		_urbanLevel = p_reader.ReadByte ();
		_farmLevel = p_reader.ReadByte ();
		_plantLevel = p_reader.ReadByte ();
		_specialFeatureIndex = p_reader.ReadByte ();
		_isWalled = p_reader.ReadBoolean ();

		byte riverData = p_reader.ReadByte ();
		if (riverData > 128) {
			_hasIncomingRiver = true;
			_incomingRiver = (ENUM_HexDirection)(riverData - 128);
		} else {
			_hasIncomingRiver = false;
		}

		riverData = p_reader.ReadByte ();
		if (riverData > 128) {
			_hasOutgoingRiver = true;
			_outgoingRiver = (ENUM_HexDirection)(riverData - 128);
		} else {
			_hasOutgoingRiver = false;
		}

		int roadFlags = p_reader.ReadByte ();
		for (int i = 0; i < _roads.Length; ++i) {
			_roads [i] = (roadFlags & (1 << i)) != 0;
		}

		Refresh ();
	}

	#endregion
}
