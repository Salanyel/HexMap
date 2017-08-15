using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapCamera : MonoBehaviour {

	#region Variables

	[SerializeField]
	float _moveSpeedMinZoom;

	[SerializeField]
	float _moveSpeedMaxZoom;

	[SerializeField]
	float _stickMinZoom;

	[SerializeField]
	float _stickMaxZoom;

	[SerializeField]
	float _swivelMinZoom;

	[SerializeField]
	float _swivelMaxZoom;

	[SerializeField]
	HexGrid _grid;

	Transform _swivel;
	Transform _stick;
	float _zoom = 1f;

	#endregion

	#region Unity_methods

	void Awake() {
		_swivel = transform.GetChild (0);
		_stick = _swivel.GetChild (0);

		if (_grid == null) {
			_grid = FindObjectOfType<HexGrid> ();
		}
	}

	void Update() {
		ManageZoom ();
		ManageDisplacement ();
	}

	#endregion

	#region Methods

	void ManageZoom() {
		float zoomDelta = Input.GetAxis ("Mouse ScrollWheel");
		if (zoomDelta != 0f) {
			AdjustZoom (zoomDelta);
		}
	}

	void ManageDisplacement() {
		float xDelta = Input.GetAxis ("Horizontal");
		float zDelta = Input.GetAxis ("Vertical");

		if (xDelta != 0 || zDelta != 00) {
			AdjustPosition (xDelta, zDelta);
		}
	}

	void AdjustZoom(float p_zoomDelta) {
		_zoom = Mathf.Clamp01 (_zoom + p_zoomDelta);

		float distance = Mathf.Lerp (_stickMinZoom, _stickMaxZoom, _zoom);
		_stick.localPosition = new Vector3 (0f, 0f, distance);

		float angle = Mathf.Lerp(_swivelMinZoom, _swivelMaxZoom, _zoom);
		_swivel.localRotation = Quaternion.Euler (angle, 0f, 0f);
	}

	void AdjustPosition(float p_x, float p_z) {
		Vector3 direction = new Vector3 (p_x, 0f, p_z).normalized;
		float damping = Mathf.Max (Mathf.Abs (p_x), Mathf.Abs (p_z));
		float distance = Mathf.Lerp(_moveSpeedMinZoom, _moveSpeedMaxZoom, _zoom) * damping * Time.deltaTime;

		Vector3 position = transform.localPosition;
		position += direction * distance;
		transform.localPosition = ClampPosition(position);
	}

	Vector3 ClampPosition(Vector3 p_origin) {
		float xMax = (_grid.ChunkCountX * HexMetrics._chunkSizeX - 0.5f) * (2f * HexMetrics._innerRadius);
		p_origin.x = Mathf.Clamp (p_origin.x, 0f, xMax);

		float zMax = (_grid.ChunkCountZ * HexMetrics._chunkSizeZ - 1f) * (1.5f * HexMetrics._innerRadius);
		p_origin.z = Mathf.Clamp (p_origin.z, 0f, zMax);

		return p_origin;
	}

	#endregion
}
