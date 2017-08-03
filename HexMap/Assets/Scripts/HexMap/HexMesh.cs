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
		Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(p_direction);
		Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(p_direction);

		AddTriangle (center, v1, v2);
		AddTriangleColor (p_cell.Color);

		if (p_direction <= ENUM_HexDirection.SE) {
			TriangulateConnection (p_direction, p_cell, v1, v2);
		}
	}

	void TriangulateEdgeTerraces(
		Vector3 p_beginLeft, Vector3 p_beginRight, HexCell p_beginCell,
		Vector3 p_endLeft, Vector3 p_endRight, HexCell p_endCell) 
	{
		Vector3 v3 = HexMetrics.TerraceLerp (p_beginLeft, p_endLeft, 1);
		Vector3 v4 = HexMetrics.TerraceLerp (p_beginRight, p_endRight, 1);
		Color c2 = HexMetrics.TerraceLerp (p_beginCell.Color, p_endCell.Color, 1);

		AddQuad (p_beginLeft, p_beginRight, v3, v4);
		AddQuadColor (p_beginCell.Color, c2);

		for (int i = 2; i < HexMetrics._terraceSteps; i++) {
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c2;

			v3 = HexMetrics.TerraceLerp (p_beginLeft, p_endLeft, i);
			v4 = HexMetrics.TerraceLerp (p_beginRight, p_endRight, i);
			c2 = HexMetrics.TerraceLerp (p_beginCell.Color, p_endCell.Color, i);

			AddQuad (v1, v2, v3, v4);
			AddQuadColor (c1, c2);
		}

		AddQuad (v3, v4, p_endLeft, p_endRight);
		AddQuadColor (c2, p_endCell.Color);
	}

	void TriangulateConnection(ENUM_HexDirection p_direction, HexCell p_cell, Vector3 p_v1, Vector3 p_v2) {
		HexCell neighbor = p_cell.GetNeighbor(p_direction);

		if (neighbor == null) {
			return;
		}

		Vector3 bridge = HexMetrics.GetBridge (p_direction);
		Vector3 v3 = p_v1 + bridge;
		Vector3 v4 = p_v2 + bridge;
		v3.y = v4.y = neighbor.Elevation * HexMetrics._elevationStep;

		if (p_cell.GetEdgeType (p_direction) == ENUM_HexEdgeType.Slope) {
			TriangulateEdgeTerraces (p_v1, p_v2, p_cell, v3, v4, neighbor);
		} else {
			AddQuad(p_v1, p_v2, v3, v4);
			AddQuadColor(p_cell.Color, neighbor.Color);
		}

		HexCell nextNeighbor = p_cell.GetNeighbor(p_direction.Next());

		if (p_direction <= ENUM_HexDirection.E && nextNeighbor != null) {
			Vector3 v5 = p_v2 + HexMetrics.GetBridge (p_direction.Next ());
			v5.y = nextNeighbor.Elevation * HexMetrics._elevationStep;

			if (p_cell.Elevation <= neighbor.Elevation) {
				if (p_cell.Elevation <= nextNeighbor.Elevation) {
					TriangulateCorner (p_v2, p_cell, v4, neighbor, v5, nextNeighbor);
				} else {
					TriangulateCorner (v5, nextNeighbor, p_v2, p_cell, v4, neighbor);
				} 
			}else if (neighbor.Elevation <= nextNeighbor.Elevation) {
				TriangulateCorner (v4, neighbor, v5, nextNeighbor, p_v2, p_cell);
			} else {
				TriangulateCorner (v5, nextNeighbor, p_v2, p_cell, v4, neighbor);
			}

			AddTriangle(p_v2, v4, v5);
			AddTriangleColor(p_cell.Color, neighbor.Color, nextNeighbor.Color);
		}
	}

	void TriangulateCorner (
		Vector3 p_bottom, HexCell p_bottomCell, 
		Vector3 p_left, HexCell p_leftCell,
		Vector3 p_right, HexCell p_rightCell
	) {
		ENUM_HexEdgeType leftEdgeType = p_bottomCell.GetEdgeType (p_leftCell);
		ENUM_HexEdgeType rightEdgeType = p_bottomCell.GetEdgeType (p_rightCell);

		if (leftEdgeType == ENUM_HexEdgeType.Slope) {
			if (rightEdgeType == ENUM_HexEdgeType.Slope) {
				TriangulateCornerTerraces (
					p_bottom, p_bottomCell, p_left, p_leftCell, p_right, p_rightCell);
				return;
			}
			if (rightEdgeType == ENUM_HexEdgeType.Flat) {
				TriangulateCornerTerraces(
					p_left, p_leftCell, p_right, p_rightCell, p_bottom, p_bottomCell
				);
				return;
			}

			TriangulateCornerTerracesCliff (p_right, p_rightCell, p_bottom, p_bottomCell, p_left, p_leftCell);
			return;
		}

		if (rightEdgeType == ENUM_HexEdgeType.Slope) {
			if (leftEdgeType == ENUM_HexEdgeType.Flat) {
				TriangulateCornerTerraces(p_right, p_rightCell, p_bottom, p_bottomCell, p_left, p_leftCell);
				return;
			}
			TriangulateCornerCliffTerraces (p_bottom, p_bottomCell, p_left, p_leftCell, p_right, p_rightCell);
			return;
		}

		if (p_leftCell.GetEdgeType (p_rightCell) == ENUM_HexEdgeType.Slope) {
			if (p_leftCell.Elevation < p_rightCell.Elevation) {
				TriangulateCornerCliffTerraces (p_right, p_rightCell, p_bottom, p_bottomCell, p_left, p_leftCell);
			} else {
				TriangulateCornerTerracesCliff (p_left, p_leftCell, p_right, p_rightCell, p_bottom, p_bottomCell);
			}
		} 

		AddTriangle (p_bottom, p_left, p_right);
		AddTriangleColor (p_bottomCell.Color, p_leftCell.Color, p_rightCell.Color);
	}

	void TriangulateCornerTerraces(
		Vector3 p_begin, HexCell p_beginCell,
		Vector3 p_left, HexCell p_leftCell,
		Vector3 p_right, HexCell p_rightCell
	) {
		Vector3 v3 = HexMetrics.TerraceLerp(p_begin, p_left, 1);
		Vector3 v4 = HexMetrics.TerraceLerp(p_begin, p_right, 1);
		Color c3 = HexMetrics.TerraceLerp(p_beginCell.Color, p_leftCell.Color, 1);
		Color c4 = HexMetrics.TerraceLerp(p_beginCell.Color, p_rightCell.Color, 1);

		AddTriangle(p_begin, v3, v4);
		AddTriangleColor(p_beginCell.Color, c3, c4);

		for (int i = 2; i < HexMetrics._terraceSteps; i++) {
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c3;
			Color c2 = c4;
			v3 = HexMetrics.TerraceLerp(p_begin, p_left, i);
			v4 = HexMetrics.TerraceLerp(p_begin, p_right, i);
			c3 = HexMetrics.TerraceLerp(p_beginCell.Color, p_leftCell.Color, i);
			c4 = HexMetrics.TerraceLerp(p_beginCell.Color, p_rightCell.Color, i);

			AddQuad(v1, v2, v3, v4);
			AddQuadColor(c1, c2, c3, c4);
		}
	}

	void TriangulateCornerTerracesCliff(
		Vector3 p_begin, HexCell p_beginCell,
		Vector3 p_left, HexCell p_leftCell,
		Vector3 p_right, HexCell p_rightCell
	) {
		float b = 1f / (p_rightCell.Elevation - p_beginCell.Elevation);
		Vector3 boundary = Vector3.Lerp (p_begin, p_right, b);
		Color boundaryColor = Color.Lerp (p_beginCell.Color, p_rightCell.Color, b);
		if (b < 0) {
			b = -b;
		}

		TriangulateBoundaryTriangle (
			p_begin, p_beginCell, p_left, p_leftCell, boundary, boundaryColor);

		if (p_leftCell.GetEdgeType (p_rightCell) == ENUM_HexEdgeType.Slope) {
			TriangulateBoundaryTriangle (p_left, p_leftCell, p_right, p_rightCell, boundary, boundaryColor);
		} else {
			AddTriangle (p_left, p_right, boundary);
			AddTriangleColor (p_leftCell.Color, p_rightCell.Color, boundaryColor);
		}
	}

	void TriangulateCornerCliffTerraces(	
		Vector3 p_begin, HexCell p_beginCell,
		Vector3 p_left, HexCell p_leftCell,
		Vector3 p_right, HexCell p_rightCell
	) {
		float b = 1f / (p_leftCell.Elevation - p_beginCell.Elevation);
		Vector3 boundary = Vector3.Lerp (p_begin, p_left, b);
		Color boundaryColor = Color.Lerp (p_beginCell.Color, p_leftCell.Color, b);
		if (b < 0) {
			b = -b;
		}

		TriangulateBoundaryTriangle (
			p_right, p_rightCell, p_begin, p_beginCell, boundary, boundaryColor);
			
		if (p_leftCell.GetEdgeType (p_rightCell) == ENUM_HexEdgeType.Slope) {
			TriangulateBoundaryTriangle (p_left, p_leftCell, p_right, p_rightCell, boundary, boundaryColor);
		} else {
			AddTriangle (p_left, p_right, boundary);
			AddTriangleColor (p_leftCell.Color, p_rightCell.Color, boundaryColor);
		}
	}

	void TriangulateBoundaryTriangle(
		Vector3 p_begin, HexCell p_beginCell,
		Vector3 p_left, HexCell p_leftCell,
		Vector3 p_boundary, Color p_boundaryColor
	) {
		Vector3 v2 = HexMetrics.TerraceLerp (p_begin, p_left, 1);
		Color c2 = HexMetrics.TerraceLerp (p_beginCell.Color, p_leftCell.Color, 1);

		AddTriangle (p_begin, v2, p_boundary);
		AddTriangleColor (p_beginCell.Color, c2, p_boundaryColor);

		for (int i = 2; i < HexMetrics._terraceSteps; i++) {
			Vector3 v1 = v2;
			Color c1 = c2;
			v2 = HexMetrics.TerraceLerp (p_begin, p_left, i);
			c2 = HexMetrics.TerraceLerp (p_beginCell.Color, p_leftCell.Color, i);
			AddTriangle (v1, v2, p_boundary);
			AddTriangleColor (c1, c2, p_boundaryColor);
		}

		AddTriangle (v2, p_left, p_boundary);
		AddTriangleColor (c2, p_leftCell.Color, p_boundaryColor);
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