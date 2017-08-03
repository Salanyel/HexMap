using UnityEngine.EventSystems;
using UnityEngine;

public class HexMapEditor : MonoBehaviour {

	#region Variables 

	[SerializeField] 
	Color[] _colors;

	HexGrid _hexGrid;
	Color _activeColor;
	int _activeElevation;

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
			EditCell(_hexGrid.GetCell(hit.point));
		}
	}

	public void EditCell(HexCell p_cell) {
		p_cell.Color = _activeColor;
		p_cell.Elevation = _activeElevation;
		_hexGrid.Refresh ();
	}

	public void SelectColor(int p_index) {
		_activeColor = _colors [p_index];
	}

	public void SetElevation(float p_elevation) {
		_activeElevation = (int) p_elevation;
	}

	#endregion
}
