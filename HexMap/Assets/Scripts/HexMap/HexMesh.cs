﻿using System.Collections;
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
		Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(p_direction);
		Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(p_direction);

		AddTriangle (center, v1, v2);
		AddTriangleColor (p_cell.Color);

		Vector3 bridge = HexMetrics.GetBridge (p_direction);
		Vector3 v3 = v1 + bridge;
		Vector3 v4 = v2 + bridge;

		AddQuad (v1, v2, v3, v4);

		HexCell prevNeighbor = p_cell.GetNeighbor (p_direction.Previous ()) ?? p_cell;
		HexCell neighbor = p_cell.GetNeighbor (p_direction) ?? p_cell;
		HexCell nextNeighbor = p_cell.GetNeighbor (p_direction.Next ()) ?? p_cell;

		//The bridge is the square that directly link 2 hexes
		Color bridgeColor = (p_cell.Color + neighbor.Color) * 0.5f;
		AddQuadColor (p_cell.Color, bridgeColor);

		//Add two triangles to merge the color in the left triangular zone between 2 hexes and that reaches the others
		AddTriangle (v1, center + HexMetrics.GetFirstCorner (p_direction), v3);
		AddTriangleColor (
			p_cell.Color,
			(p_cell.Color + prevNeighbor.Color + neighbor.Color) / 3f,
			bridgeColor
		);

		AddTriangle(v2, v4, center + HexMetrics.GetSecondCorner(p_direction));
		AddTriangleColor(
			p_cell.Color,
			bridgeColor,
			(p_cell.Color + neighbor.Color + nextNeighbor.Color) / 3f
		);
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

	void AddQuad (Vector3 p_v1, Vector3 p_v2, Vector3 p_v3, Vector3 p_v4) {
		int vertexIndex = _vertices.Count;
		_vertices.Add(p_v1);
		_vertices.Add(p_v2);
		_vertices.Add(p_v3);
		_vertices.Add(p_v4);
		_triangles.Add(vertexIndex);
		_triangles.Add(vertexIndex + 2);
		_triangles.Add(vertexIndex + 1);
		_triangles.Add(vertexIndex + 1);
		_triangles.Add(vertexIndex + 2);
		_triangles.Add(vertexIndex + 3);
	}

	void AddQuadColor (Color p_c1, Color p_c2, Color p_c3, Color p_c4) {
		_colors.Add(p_c1);
		_colors.Add(p_c2);
		_colors.Add(p_c3);
		_colors.Add(p_c4);
	}

	void AddQuadColor(Color p_c1, Color p_c2) {
		_colors.Add (p_c1);
		_colors.Add (p_c1);
		_colors.Add (p_c2);
		_colors.Add (p_c2);
	}

	#endregion
}
