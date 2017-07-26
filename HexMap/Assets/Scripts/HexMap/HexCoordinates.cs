using UnityEngine;

[System.Serializable]
public struct HexCoordinates {

	[SerializeField]
	private int _x, _z;

	public int X { 
		get {
			return _x;
		}
	}

	public int Z { 
		get { return _z; }
	}

	public int Y { 
		get { return -X - Z; }
	}

	public HexCoordinates (int p_x, int p_z) {
		_x = p_x;
		_z = p_z;
	}

	public static HexCoordinates FromOffsetCoordinates(int p_x, int p_z) {
		return new HexCoordinates (p_x - p_z / 2, p_z);
	}

	public override string ToString ()
	{
		return "(" + X.ToString () + ", " + Y.ToString() + ", " + Z.ToString () + ")";
	}

	public string ToStringOnSeparateLines ()
	{
		return X.ToString () + "\n" + Y.ToString () + "\n" + Z.ToString ();
	}
}