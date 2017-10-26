using UnityEngine;

public class HexFeatureManager : MonoBehaviour {

	#region Variables

	[SerializeField]
	HexFeatureCollection[] _urbanCollections;

	[SerializeField]
	public HexFeatureCollection[] _farmCollections;

	[SerializeField]
	public HexFeatureCollection[] _plantCollections;

	Transform _container;

	#endregion

	#region Methods

	public void Clear () {
		if (_container) {
			Destroy(_container.gameObject);
		}
		_container = new GameObject("Features _container").transform;
		_container.SetParent(transform, false);
	}

	public void Apply () {}

	Transform PickPrefab (
		HexFeatureCollection[] collection,
		int level, float hash, float choice
	) {
		if (level > 0) {
			float[] thresholds = HexMetrics.GetFeatureThresholds(level - 1);
			for (int i = 0; i < thresholds.Length; i++) {
				if (hash < thresholds[i]) {
					return collection[i].Pick(choice);
				}
			}
		}
		return null;
	}

	public void AddFeature (HexCell cell, Vector3 position) {
		HexHash hash = HexMetrics.SampleHashGrid(position);
		Transform prefab = PickPrefab(
			_urbanCollections, cell.UrbanLevel, hash._a, hash._d
		);
		Transform otherPrefab = PickPrefab(
			_farmCollections, cell.FarmLevel, hash._b, hash._d
		);
		float usedHash = hash._a;
		if (prefab) {
			if (otherPrefab && hash._b < hash._a) {
				prefab = otherPrefab;
				usedHash = hash._b;
			}
		}
		else if (otherPrefab) {
			prefab = otherPrefab;
			usedHash = hash._b;
		}
		otherPrefab = PickPrefab(
			_plantCollections, cell.PlantLevel, hash._c, hash._d
		);
		if (prefab) {
			if (otherPrefab && hash._c < usedHash) {
				prefab = otherPrefab;
			}
		}
		else if (otherPrefab) {
			prefab = otherPrefab;
		}
		else {
			return;
		}

		Transform instance = Instantiate(prefab);
		position.y += instance.localScale.y * 0.5f;
		instance.localPosition = HexMetrics.Perturb(position);
		instance.localRotation = Quaternion.Euler(0f, 360f * hash._e, 0f);
		instance.SetParent(_container, false);
	}

	#endregion
}