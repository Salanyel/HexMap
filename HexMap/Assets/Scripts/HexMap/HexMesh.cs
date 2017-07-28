using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

	#region Variables

	Mesh _hexMesh;
	MeshCollider _meshCollider;
	List<Vector3> _vertices;
	List<int> _triangles;
	List<Color> _colors;

	#endregion

	#region Unity_Methods

	void Awake() {
		GetComponent<MeshFilter> ().mesh = _hexMesh = new Mesh ();
		_meshCollider = gameObject.AddComponent<MeshCollider> ();

		_hexMesh.name = "Hex Mesh";
		_vertices = new List<Vector3> ();
		_triangles = new List<int> ();
		_colors = new List<Color> ();
	}

	#endregion

	#region Methods

	public void Triangulate(HexCell[] p_cells) {
		_hexMesh.Clear ();
		_vertices.Clear ();
		_triangles.Clear ();
		_colors.Clear ();

		for (int i = 0; i < p_cells.Length; ++i) {
			Triangulate (p_cells [i]);
		}

		_hexMesh.vertices = _vertices.ToArray ();
		_hexMesh.triangles = _triangles.ToArray ();
		_hexMesh.colors = _colors.ToArray ();

		_hexMesh.RecalculateNormals ();

		_meshCollider.sharedMesh = _hexMesh;
	}

	void Triangulate(HexCell p_cell) {
		for (ENUM_HexDirection d = ENUM_HexDirection.NE; d <= ENUM_HexDirection.NW; d++) {
			Triangulate (d, p_cell);
		}
	}

	void Triangulate(ENUM_HexDirection p_direction, HexCell p_cell) {
		Vector3 center = p_cell.transform.localPosition;

		AddTriangle (
			center,
			center + HexMetrics.GetFirstCorner(p_direction),
			center + HexMetrics.GetSecondCorner(p_direction)
		);

		HexCell prevNeighbor = p_cell.GetNeighbor (p_direction.Previous ()) ?? p_cell;
		HexCell neighbor = p_cell.GetNeighbor (p_direction) ?? p_cell;
		HexCell nextNeighbor = p_cell.GetNeighbor (p_direction.Next ()) ?? p_cell;

		Color edgeColor = (p_cell.Color + neighbor.Color) * 0.5f;
		AddTriangleColor (p_cell.Color, 
			(p_cell.Color + prevNeighbor.Color + neighbor.Color) / 3f,
			(p_cell.Color + neighbor.Color + nextNeighbor.Color) / 3f);
	}

	void AddTriangleColor(Color p_color) {
		_colors.Add (p_color);
		_colors.Add (p_color);
		_colors.Add (p_color);
	}

	void AddTriangleColor(Color p_c1, Color p_c2, Color p_c3) {
		_colors.Add (p_c1);
		_colors.Add (p_c2);
		_colors.Add (p_c3);
	}

	void AddTriangle(Vector3 p_v1, Vector3 p_v2, Vector3 p_v3) {
		int vertexIndex = _vertices.Count;
		_vertices.Add (p_v1);
		_vertices.Add (p_v2);
		_vertices.Add (p_v3);
		_triangles.Add (vertexIndex);
		_triangles.Add (vertexIndex + 1);
		_triangles.Add (vertexIndex + 2);
	}

	#endregion
}
