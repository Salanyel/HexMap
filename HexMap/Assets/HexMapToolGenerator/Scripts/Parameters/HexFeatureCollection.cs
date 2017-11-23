using UnityEngine;

[System.Serializable]
public struct HexFeatureCollection {
	public Transform[] _prefabs;

	public Transform Pick(float p_choice) {
		return _prefabs [(int)(p_choice * _prefabs.Length)];
	}
}
