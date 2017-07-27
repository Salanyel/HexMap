using UnityEngine.EventSystems;
using UnityEngine;

public class HexMapEditor : MonoBehaviour {

	#region Variables 

	[SerializeField] 
	Color[] _colors;

	HexGrid _hexGrid;
	Color _activeColor;

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
			_hexGrid.ColorCell (hit.point, _activeColor);
			Debug.Log ("Touch");
		} else {
			Debug.Log ("No touch");
		}
	}

	public void SelectColor(int p_index) {
		_activeColor = _colors [p_index];
	}

	#endregion
}
