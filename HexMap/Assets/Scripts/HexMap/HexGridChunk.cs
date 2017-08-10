using UnityEngine;
using UnityEngine.UI;

public class HexGridChunk : MonoBehaviour {

	#region Variables

	HexCell[] _cells;

	HexMesh _hexMesh;
	Canvas _gridCanvas;

	#endregion

	#region Unity_Methods

	void Awake() {
		_gridCanvas = GetComponentInChildren<Canvas> ();
		_hexMesh = GetComponentInChildren<HexMesh> ();

		_cells = new HexCell[HexMetrics._chunkSizeX * HexMetrics._chunkSizeZ];
	}

	void Start() {
		_hexMesh.Triangulate (_cells);
	}

	#endregion

	#region Methods

	public void AddCell(int p_index, HexCell p_cell) {
		_cells [p_index] = p_cell;
		p_cell.transform.SetParent (transform, false);
		p_cell.UIRect.SetParent (_gridCanvas.transform, false);
	}

	#endregion
}