using UnityEngine.EventSystems;
using UnityEngine;

public class HexMapEditor : MonoBehaviour {

	#region Variables 

	[SerializeField] 
	Color[] _colors;

	HexGrid _hexGrid;
	Color _activeColor;
	int _activeElevation;
	bool _canApplyColor;
	bool _canApplyElevation = true;
	int _brushSize = 0;

	#endregion

	#region Unity_Methods

	void Awake() {
		_hexGrid = FindObjectOfType<HexGrid> ();

		SelectColor (0);
	}

	void Update() {
		if (Input.GetMouseButton (0) && !EventSystem.current.IsPointerOverGameObject()) {
			HandleInput ();
		}
	}

	#endregion

	#region Methods

	void HandleInput() {
		Ray inputRay = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (inputRay, out hit)) {
			EditCells(_hexGrid.GetCell(hit.point));
		}
	}

	public void EditCell(HexCell p_cell) {
		if (p_cell) {
			if (_canApplyColor) {
				p_cell.Color = _activeColor;
			}

			if (_canApplyElevation) {
				p_cell.Elevation = _activeElevation;
			}
		}
	}

	public void EditCells(HexCell p_center) {
		int centerX = p_center.Coordinates.X;
		int centerZ = p_center.Coordinates.Z;

		for (int r = 0, z = centerZ - _brushSize; z <= centerZ; ++z, ++r) {
			for (int x = centerX - r; x <= centerX + _brushSize; ++x) {
				EditCell (_hexGrid.GetCell (new HexCoordinates (x, z)));
			}
		}

		for (int r = 0, z = centerZ + _brushSize; z > centerZ; --z, ++r) {
			for (int x = centerX - _brushSize; x <= centerX + r; ++x) {
				EditCell (_hexGrid.GetCell (new HexCoordinates (x, z)));
			}
		}
	}

	public void SelectColor(int p_index) {
		_canApplyColor = p_index >= 0;
		if (_canApplyColor) {
			_activeColor = _colors [p_index];
		}
	}

	public void SetElevation(float p_elevation) {
		_activeElevation = (int) p_elevation;
	}

	public void SetCanApplyElevation(bool p_can) {
		_canApplyElevation = p_can;
	}

	public void SetBrushSize(float p_size) {
		_brushSize = (int)p_size;
	}

	public void ShowUI(bool p_isVisible) {
		_hexGrid.ShowUI (p_isVisible);
	}

	#endregion
}
