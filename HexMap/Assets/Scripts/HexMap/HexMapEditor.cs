using UnityEngine.EventSystems;
using UnityEngine;

public class HexMapEditor : MonoBehaviour {

	#region Variables 

	enum ENUM_OptionalToggle {
		Ignore,
		Yes,
		No
	}

	[SerializeField] 
	Color[] _colors;

	HexGrid _hexGrid;
	Color _activeColor;

	int _activeElevation;
	int _activeWaterLevel;
	int _activeUrbanLevel;
	int _activeFarmLevel;
	int _activePlantLevel;

	bool _canApplyColor;
	bool _canApplyElevation = true;
	bool _canApplyWaterLevel = true;
	bool _canApplyUrbanLevel = false;
	bool _canApplyFarmLevel = false;
	bool _canApplyPlantLevel = false;

	int _brushSize = 0;
	ENUM_OptionalToggle _riverMode;
	ENUM_OptionalToggle _roadMode;
	ENUM_OptionalToggle _walledMode;

	bool _isDrag;
	ENUM_HexDirection _dragDirection;
	HexCell _previousCell;

	#endregion

	#region Unity_Methods

	void Awake() {
		_hexGrid = FindObjectOfType<HexGrid> ();

		SelectColor (0);
	}

	void Update() {
		if (Input.GetMouseButton (0) && !EventSystem.current.IsPointerOverGameObject ()) {
			HandleInput ();
		} else {
			_previousCell = null;
		}
	}

	#endregion

	#region Methods

	void HandleInput() {
		Ray inputRay = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (inputRay, out hit)) {
			HexCell currentCell = _hexGrid.GetCell (hit.point);
			if (_previousCell && _previousCell != currentCell) {
				ValidateDrag (currentCell);
			} else {
				_isDrag = false;
			}

			EditCells (currentCell);
			_previousCell = currentCell;
		} else {
			_previousCell = null;
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

			if (_canApplyWaterLevel) {
				p_cell.WaterLevel = _activeWaterLevel;
			}

			if (_canApplyUrbanLevel) {
				p_cell.UrbanLevel = _activeUrbanLevel;
			}

			if (_canApplyFarmLevel) {
				p_cell.FarmLevel = _activeFarmLevel;
			}

			if (_canApplyPlantLevel) {
				p_cell.PlantLevel = _activePlantLevel;
			}

			if (_walledMode != ENUM_OptionalToggle.Ignore) {
				p_cell.Walled = _walledMode == ENUM_OptionalToggle.Yes;
			}

			if (_riverMode == ENUM_OptionalToggle.No) {
				p_cell.RemoveRiver ();
			} else if (_isDrag && _riverMode == ENUM_OptionalToggle.Yes) {
				HexCell otherCell = p_cell.GetNeighbor (_dragDirection.Opposite ());
				if (otherCell) {
					_previousCell.SetOutgoingRiver (_dragDirection);
				}
			}

			if (_roadMode == ENUM_OptionalToggle.No) {
				p_cell.RemoveRoads ();
			} else if (_isDrag && _roadMode == ENUM_OptionalToggle.Yes) {
				HexCell otherCell = p_cell.GetNeighbor (_dragDirection.Opposite ());
				if (otherCell) {
					otherCell.AddRoad(_dragDirection);
				}
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

	void ValidateDrag(HexCell p_cell) {
		for (_dragDirection = ENUM_HexDirection.NE; _dragDirection <= ENUM_HexDirection.NW; ++_dragDirection) {
			if (_previousCell.GetNeighbor (_dragDirection) == p_cell) {
				_isDrag = true;
				return;
			}
		}

		_isDrag = false;
	}

	public void SelectColor(int p_index) {
		_canApplyColor = p_index >= 0;
		if (_canApplyColor) {
			_activeColor = _colors [p_index];
		}
	}

	public void SetWaterLevel(float p_level) {
		_activeWaterLevel = (int) p_level;
	}

	public void SetElevation(float p_elevation) {
		_activeElevation = (int) p_elevation;
	}

	public void SetUrbanLevel(float p_urbanLevel) {
		_activeUrbanLevel = (int)p_urbanLevel;
	}

	public void SetFarmLevel(float p_farmLevel) {
		_activeFarmLevel = (int)p_farmLevel;
	}

	public void SetPlantLevel(float p_plantLevel) {
		_activePlantLevel = (int)p_plantLevel;
	}

	public void SetCanApplyElevation(bool p_can) {
		_canApplyElevation = p_can;
	}

	public void SetCanApplyWaterLevel(bool p_can) {
		_canApplyWaterLevel = p_can;
	}

	public void SetCanApplyUrbanLevel(bool p_can) {
		_canApplyUrbanLevel = p_can;
	}

	public void SetCanApplyFarmLevel(bool p_can) {
		_canApplyFarmLevel = p_can;
	}

	public void SetCanApplyPlantLevel(bool p_can) {
		_canApplyPlantLevel = p_can;
	}

	public void SetBrushSize(float p_size) {
		_brushSize = (int)p_size;
	}

	public void ShowUI(bool p_isVisible) {
		_hexGrid.ShowUI (p_isVisible);
	}

	public void SetRiverMode(int p_mode) {
		_riverMode = (ENUM_OptionalToggle) p_mode;
	}

	public void SetRoadMode(int p_mode) {
		_roadMode = (ENUM_OptionalToggle) p_mode;
	}

	public void SetWalledMode(int p_mode) {
		_walledMode = (ENUM_OptionalToggle) p_mode;
	}

	#endregion
}
