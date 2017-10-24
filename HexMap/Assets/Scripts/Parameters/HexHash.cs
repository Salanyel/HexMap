using UnityEngine;

public struct HexHash {

	#region Variables

	public float _a, _b;

	#endregion

	#region Methods

	public static HexHash Create() {
		HexHash hash;
		hash._a = Random.value;
		hash._b = Random.value;

		return hash;
	}

	#endregion
}
