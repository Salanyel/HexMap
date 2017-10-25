using UnityEngine;

public class HexFeatureManager : MonoBehaviour {

	#region Variables

	public HexFeatureCollection[] _urbanCollections;

	Transform _container;

	#endregion

	#region Methods

	public void Clear() { 
		if (_container) {
			Destroy (_container.gameObject);
		}

		_container = new GameObject ("Features Container").transform;
		_container.SetParent (transform, false);
	}

	public void Apply() { }

	public void AddFeature(HexCell p_cell, Vector3 p_position) {
		HexHash hash = HexMetrics.SampleHasGrid (p_position);

		Transform prefab = PickPrefab (p_cell.UrbanLevel, hash._a, hash._b);
		if (!prefab) {
			return;
		}

		Transform instance = Instantiate (prefab);
		p_position.y = instance.localScale.y * 0.5f;
		instance.localPosition = HexMetrics.Perturb(p_position);
		instance.localRotation = Quaternion.Euler (0f, 360f * hash._c, 0f);
		instance.SetParent (_container, false);
	}

	Transform PickPrefab(int p_level, float p_hash, float p_choice) {
		if (p_level > 0) {
			float[] thresholds = HexMetrics.GetFeatureThresholds (p_level - 1);
			for (int i = 0; i < thresholds.Length; ++i) {
				if (p_hash < thresholds [i]) {
					return _urbanCollections[i].Pick(p_choice);
				}
			}
		}

		return null;
	}

	#endregion
}
