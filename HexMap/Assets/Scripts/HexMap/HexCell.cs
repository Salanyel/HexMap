using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour {

	#region Variables

	[SerializeField]
	HexCoordinates _coordinates;

	[SerializeField]
	Color _color;

	[SerializeField]
	int _elevation = int.MinValue;

	[SerializeField]
	RectTransform _uiRect;

	[SerializeField]
	HexCell[] _neighbors;

	[SerializeField]
	HexGridChunk _chunk;

	bool _hasIncomingRiver;
	bool _hasOutgoingRiver;
	ENUM_HexDirection _incomingRiver;
	ENUM_HexDirection _outgoingRiver;

	public Vector3 Position {
		get { return transform.localPosition; }
	}

	public HexCoordinates Coordinates {
		get { return _coordinates; }
		set { _coordinates = value; }
	}

	public Color Color {
		get { return _color; }
		set { 
			if (_color == value) {
				return;
			}
			_color = value;
			Refresh ();
		}
	}

	public int Elevation {
		get { return _elevation; }
		set { 
			if (_elevation == value) {
				return;
			}

			_elevation = value;
			Vector3 position = transform.localPosition;
			position.y = value * HexMetrics._elevationStep;
			position.y += (HexMetrics.SampleNoise (position).y * 2f - 1f) * HexMetrics._cellPerturbElevation;
			transform.localPosition = position;

			Vector3 uiPosition = _uiRect.localPosition;
			uiPosition.z = -position.y;
			_uiRect.localPosition = uiPosition;

			//Manage the rivers behaviour
			if (_hasOutgoingRiver && _elevation < GetNeighbor (_outgoingRiver)._elevation) {
				RemoveOutgoingRiver ();
			}

			if (_hasIncomingRiver && _elevation > GetNeighbor (_incomingRiver)._elevation) {
				RemoveIncomingRiver ();
			}

			Refresh ();
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
		get { return(_elevation + HexMetrics._riverSurfaceElevationOffset) * HexMetrics._elevationStep; }
	}

	#endregion

	#region Methods

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

	public ENUM_HexEdgeType GetEdgeType(ENUM_HexDirection p_direction) {
		return HexMetrics.GetEdgeType(_elevation, _neighbors[(int) p_direction].Elevation);
	}

	public ENUM_HexEdgeType GetEdgeType(HexCell p_cell) {
		return HexMetrics.GetEdgeType (_elevation, p_cell.Elevation);
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
		if (!neighbor || _elevation < neighbor.Elevation) {
			return;
		}

		RemoveOutgoingRiver ();
		if (_hasIncomingRiver && _incomingRiver == p_direction) {
			RemoveIncomingRiver ();
		}

		_hasOutgoingRiver = true;
		_outgoingRiver = p_direction;
		RefreshSelfOnly ();

		neighbor.RemoveIncomingRiver ();
		neighbor._hasIncomingRiver = true;
		neighbor._incomingRiver = p_direction.Opposite ();
		neighbor.RefreshSelfOnly ();
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

	#endregion
}
