using UnityEngine;

public class HexFeatureManager : MonoBehaviour {

	#region Variables

	public Transform _featurePrefab;

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

	public void AddFeature(Vector3 p_position) {
		HexHash hash = HexMetrics.SampleHasGrid (p_position);

		//Percentage density for the features
		if (hash._a > +0.5f) {
			return;
		}

		Transform instance = Instantiate (_featurePrefab);
		p_position.y = instance.localScale.y * 0.5f;
		instance.localPosition = HexMetrics.Perturb(p_position);
		instance.localRotation = Quaternion.Euler (0f, 360f * hash._b, 0f);
		instance.SetParent (_container, false);
	}

	#endregion
}
