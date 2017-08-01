using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

	#region Variables

	[SerializeField]
	int _width = 6;

	[SerializeField]
	int _height = 6;

	[SerializeField]
	HexCell _cellPrefab;

	[SerializeField]
	Text _cellLabelPrefab;

	[SerializeField]
	Color _defaultColor = Color.white;

	Canvas _gridCanvas;
	HexMesh _hexMesh;

	public int Width {
		get { return _width; }
	}

	public int Height {
		get { return _height; }
	}

	HexCell[] _cells;

	#endregion

	#region Unity_Methods

	void Awake() {
		_gridCanvas = GetComponentInChildren<Canvas>();
		_hexMesh = GetComponentInChildren<HexMesh> ();

		_cells = new HexCell[_height * _width];

		for (int z = 0, i = 0; z < _height; ++z) {
			for (int x = 0; x < _width; ++x) {
				CreateCell (x, z, i++);
			}
		}
	}

	void Start() {
		_hexMesh.Triangulate (_cells);
	}

	#endregion

	#region Methods

	public HexCell GetCell (Vector3 p_position) {
		p_position = transform.InverseTransformPoint (p_position);
		HexCoordinates coordinates = HexCoordinates.FromPosition (p_position);
		int index = coordinates.X + coordinates.Z * Width + coordinates.Z / 2;
		return _cells [index];
	}

	public void Refresh() {
		_hexMesh.Triangulate (_cells);
	}

	void CreateCell(int p_x, int p_z, int p_i) {
		Vector3 position;
		position.x = (p_x + p_z * 0.5f - p_z / 2) * (HexMetrics._innerRadius * 2f);
		position.y = 0f;
		position.z = p_z * (HexMetrics._outerRadius * 1.5f);

		HexCell cell = _cells[p_i] = Instantiate<HexCell> (_cellPrefab);
		cell.transform.SetParent (transform, false);
		cell.transform.localPosition = position;
		cell.Coordinates = HexCoordinates.FromOffsetCoordinates (p_x, p_z);
		cell.Color = _defaultColor;

		//Save neighbors
		if (p_x > 0) {
			cell.SetNeighbor(ENUM_HexDirection.W, _cells[p_i - 1]);
		}

		if (p_z > 0) {
			if ((p_z & 1) == 0) {
				cell.SetNeighbor (ENUM_HexDirection.SE, _cells [p_i - _width]);

				if (p_x > 0) {
					cell.SetNeighbor (ENUM_HexDirection.SW, _cells [p_i - _width - 1]);
				}
			} else {
				cell.SetNeighbor (ENUM_HexDirection.SW, _cells [p_i - _width]);

				if (p_x < _width - 1) {
					cell.SetNeighbor (ENUM_HexDirection.SE, _cells [p_i - _width + 1]);
				}
			}
		}

		//Display
		Text label = Instantiate<Text>(_cellLabelPrefab);
		label.rectTransform.SetParent (_gridCanvas.transform, false);
		label.rectTransform.anchoredPosition = new Vector2 (position.x, position.z);
		label.text = cell.Coordinates.ToStringOnSeparateLines ();
		cell.UIRect = label.rectTransform;

	}

	#endregion
}