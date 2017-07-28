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
	HexCell[] _neighbors;

	public HexCoordinates Coordinates {
		get { return _coordinates; }
		set { _coordinates = value; }
	}

	public Color Color {
		get { return _color; }
		set { _color = value; }
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

	#endregion
}
