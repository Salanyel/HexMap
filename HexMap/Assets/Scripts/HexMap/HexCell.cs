using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour {

	#region Variables

	[SerializeField]
	HexCoordinates _coordinates;

	[SerializeField]
	Color _color;

	public HexCoordinates Coordinates {
		get { return _coordinates; }
		set { _coordinates = value; }
	}

	public Color Color {
		get { return _color; }
		set { _color = value; }
	}

	#endregion
}
