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

	public static HexCoordinates FromPosition (Vector3 p_position) {
		float x = p_position.x / (HexMetrics._innerRadius * 2f);
		float y = -x;
		float offset = p_position.z / (HexMetrics._outerRadius * 3f);

		x -= offset;
		y -= offset;

		int iX = Mathf.RoundToInt(x);
		int iY = Mathf.RoundToInt(y);
		int iZ = Mathf.RoundToInt(-x -y);

		if (iX + iY + iZ != 0) {
			float dX = Mathf.Abs(x - iX);
			float dY = Mathf.Abs(y - iY);
			float dZ = Mathf.Abs(-x -y - iZ);

			if (dX > dY && dX > dZ) {
				iX = -iY - iZ;
			}
			else if (dZ > dY) {
				iZ = -iX - iY;
			}
		}

		return new HexCoordinates(iX, iZ);
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