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

	#endregion

	#region Methods

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

	void Refresh() {
		if (_chunk) {
			_chunk.Refresh ();
			for (int i = 0; _neighbors.Length; ++i) {
				HexCell neighbor = _neighbors [i];
				if (neighbor != null & neighbor._chunk != _chunk) {
					neighbor.Chunk.Refresh ();
				}
			}
		}
	}

	#endregion
}
