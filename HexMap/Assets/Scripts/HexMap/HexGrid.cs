using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

	#region Variables

	[SerializeField]
	int _chunkCountX = 4;

	[SerializeField]
	int _chunkCountZ = 3;

	[SerializeField]
	HexCell _cellPrefab;

	[SerializeField]
	Text _cellLabelPrefab;

	[SerializeField]
	Color _defaultColor = Color.white;

	[SerializeField]
	HexGridChunk _chunkPrefab;

	[SerializeField]
	Texture2D _noiseSource;

	int _cellCountX;
	int _cellCountZ;

	public int ChunkCountX {
		get { return _chunkCountX; }
	}

	public int ChunkCountZ {
		get { return _chunkCountZ; }
	}

	public int CellCountX {
		get { return _cellCountX; }
	}

	public int CellCountZ {
		get { return _cellCountZ; }
	}

	HexCell[] _cells;
	HexGridChunk[] _chunks;

	#endregion

	#region Unity_Methods

	void Awake() {
		HexMetrics._noiseSource = _noiseSource;

		_cellCountX = _chunkCountX * HexMetrics._chunkSizeX;
		_cellCountZ = _chunkCountZ * HexMetrics._chunkSizeZ;

		CreateChunks ();
		CreateCells ();
	}

	#endregion

	#region Methods

	void CreateChunks() {
		_chunks = new HexGridChunk[_chunkCountX * _chunkCountZ];

		for (int z = 0, i = 0; z < _chunkCountZ; ++z) {
			for (int x = 0; x < _chunkCountX; ++x) {
				HexGridChunk chunk = _chunks[i++] = Instantiate(_chunkPrefab);
				chunk.transform.SetParent(transform);
			}
		}
	}
	void CreateCells() {
		_cells = new HexCell[_cellCountZ * _cellCountX];

		for (int z = 0, i = 0; z < _cellCountZ; ++z) {
			for (int x = 0; x < _cellCountX; ++x) {
				CreateCell (x, z, i++);
			}
		}
	}

	public HexCell GetCell (Vector3 p_position) {
		p_position = transform.InverseTransformPoint (p_position);
		HexCoordinates coordinates = HexCoordinates.FromPosition (p_position);
		int index = coordinates.X + coordinates.Z * _cellCountX + coordinates.Z / 2;
		return _cells [index];
	}

	public HexCell GetCell(HexCoordinates p_coordinates) {
		int z = p_coordinates.Z;
		if (z < 0 || z >= _cellCountZ) {
			return null;
		}

		int x = p_coordinates.X + z / 2;
		if (x < 0 || x >= _cellCountX) {
			return null;
		}

		return _cells[x + z * _cellCountX];
	}

	void CreateCell(int p_x, int p_z, int p_i) {
		Vector3 position;
		position.x = (p_x + p_z * 0.5f - p_z / 2) * (HexMetrics._innerRadius * 2f);
		position.y = 0f;
		position.z = p_z * (HexMetrics._outerRadius * 1.5f);

		HexCell cell = _cells[p_i] = Instantiate<HexCell> (_cellPrefab);
		cell.transform.localPosition = position;
		cell.Coordinates = HexCoordinates.FromOffsetCoordinates (p_x, p_z);
		cell.Color = _defaultColor;

		//Save neighbors
		if (p_x > 0) {
			cell.SetNeighbor(ENUM_HexDirection.W, _cells[p_i - 1]);
		}

		if (p_z > 0) {
			if ((p_z & 1) == 0) {
				cell.SetNeighbor (ENUM_HexDirection.SE, _cells [p_i - _cellCountX]);

				if (p_x > 0) {
					cell.SetNeighbor (ENUM_HexDirection.SW, _cells [p_i - _cellCountX - 1]);
				}
			} else {
				cell.SetNeighbor (ENUM_HexDirection.SW, _cells [p_i - _cellCountX]);

				if (p_x < _cellCountX - 1) {
					cell.SetNeighbor (ENUM_HexDirection.SE, _cells [p_i - _cellCountX + 1]);
				}
			}
		}

		//Display
		Text label = Instantiate<Text>(_cellLabelPrefab);
		label.rectTransform.anchoredPosition = new Vector2 (position.x, position.z);
		label.text = cell.Coordinates.ToStringOnSeparateLines ();
		cell.UIRect = label.rectTransform;

		cell.Elevation = 0;

		AddCellToChunk (p_x, p_z, cell);
	}

	void AddCellToChunk(int p_x, int p_z, HexCell p_cell) {
		int chunkX = p_x / HexMetrics._chunkSizeX;
		int chunkZ = p_z / HexMetrics._chunkSizeZ;
		HexGridChunk chunk = _chunks[chunkX + chunkZ * _chunkCountX];

		int localX = p_x - chunkX * HexMetrics._chunkSizeX;
		int localZ = p_z - chunkZ * HexMetrics._chunkSizeZ;
		chunk.AddCell(localX + localZ * HexMetrics._chunkSizeX, p_cell);
	}

	#endregion
}