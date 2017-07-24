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

	void CreateCell(int p_x, int p_z, int p_i) {
		Vector3 position;
		position.x = (p_x + p_z * 0.5f - p_z / 2) * (HexMetrics._innerRadius * 2f);
		position.y = 0f;
		position.z = p_z * (HexMetrics._outerRadius * 1.5f);

		HexCell Cell = _cells[p_i] = Instantiate<HexCell> (_cellPrefab);
		Cell.transform.SetParent (transform, false);
		Cell.transform.localPosition = position;

		//Display
		Text label = Instantiate<Text>(_cellLabelPrefab);
		label.rectTransform.SetParent (_gridCanvas.transform, false);
		label.rectTransform.anchoredPosition = new Vector2 (position.x, position.z);
		label.text = p_x.ToString () + "\n" + p_z.ToString ();

	}

	#endregion
}