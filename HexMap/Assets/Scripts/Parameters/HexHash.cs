using UnityEngine;

public struct HexHash {

	#region Variables

	public float _a, _b, _c;

	#endregion

	#region Methods

	public static HexHash Create() {
		HexHash hash;
		hash._a = Random.value * 0.999f;
		hash._b = Random.value * 0.999f;
		hash._c = Random.value * 0.999f;

		return hash;
	}

	#endregion
}
