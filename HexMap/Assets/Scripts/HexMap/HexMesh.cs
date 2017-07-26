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
	}

	void Triangulate(HexCell p_cell) {
		Vector3 center = p_cell.transform.localPosition;
		for (int i = 0; i < 6; ++i) {
			AddTriangle (
				center,
				center + HexMetrics._corners [i],
				center + HexMetrics._corners [i+1]
			);

			AddTriangleColor(p_cell.Color);

		}

		_meshCollider.sharedMesh = _hexMesh;
	}

	void AddTriangleColor(Color p_color) {
		_colors.Add (p_color);
		_colors.Add (p_color);
		_colors.Add (p_color);
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
