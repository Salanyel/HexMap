using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapCamera : MonoBehaviour {

	#region Variables

	[SerializeField]
	float _stickMinZoom;

	[SerializeField]
	float _stickMaxZoom;

	[SerializeField]
	float _swivelMinZoom;

	[SerializeField]
	float _swivelMaxZoom;

	Transform _swivel;
	Transform _stick;
	float _zoom = 1f;

	#endregion

	#region Unity_methods

	void Awake() {
		_swivel = transform.GetChild (0);
		_stick = _swivel.GetChild (0);
	}

	void Update() {
		float zoomDelta = Input.GetAxis ("Mouse ScrollWheel");
		if (zoomDelta != 0f) {
			AdjustZoom (zoomDelta);
		}
	}

	#endregion

	#region Methods

	void AdjustZoom(float p_zoomDelta) {
		_zoom = Mathf.Clamp01 (_zoom + p_zoomDelta);

		float distance = Mathf.Lerp (_stickMinZoom, _stickMaxZoom, _zoom);
		_stick.localPosition = new Vector3 (0f, 0f, distance);

		float angle = Mathf.Lerp(_swivelMinZoom, _swivelMaxZoom, _zoom);
		_swivel.localRotation = Quaternion.Euler (angle, 0f, 0f);
	}

	#endregion
}
