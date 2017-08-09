using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

	Mesh _hexMesh;
	List<Vector3> _vertices;
	List<Color> _colors;
	List<int> _triangles;

	MeshCollider meshCollider;

	void Awake () {
		GetComponent<MeshFilter>().mesh = _hexMesh = new Mesh();
		meshCollider = gameObject.AddComponent<MeshCollider>();
		_hexMesh.name = "Hex Mesh";
		_vertices = new List<Vector3>();
		_colors = new List<Color>();
		_triangles = new List<int>();
	}

	public void Triangulate (HexCell[] p_cells) {
		_hexMesh.Clear();
		_vertices.Clear();
		_colors.Clear();
		_triangles.Clear();
		for (int i = 0; i < p_cells.Length; i++) {
			Triangulate(p_cells[i]);
		}
		_hexMesh.vertices = _vertices.ToArray();
		_hexMesh.colors = _colors.ToArray();
		_hexMesh.triangles = _triangles.ToArray();
		_hexMesh.RecalculateNormals();
		meshCollider.sharedMesh = _hexMesh;
	}

	void Triangulate (HexCell p_cell) {
		for (ENUM_HexDirection d = ENUM_HexDirection.NE; d <= ENUM_HexDirection.NW; d++) {
			Triangulate(d, p_cell);
		}
	}

	void Triangulate (ENUM_HexDirection p_direction, HexCell p_cell) {
		Vector3 center = p_cell.Position;
		EdgeVertices e = new EdgeVertices (
			center + HexMetrics.GetFirstSolidCorner(p_direction),
			center + HexMetrics.GetSecondSolidCorner(p_direction)
         	);
		TriangulateEdgeFan (center, e, p_cell.Color);

		if (p_direction <= ENUM_HexDirection.SE) {
			TriangulateConnection(p_direction, p_cell, e);
		}
	}

	void TriangulateConnection (
		ENUM_HexDirection p_direction, HexCell p_cell, EdgeVertices p_e) {

		HexCell neighbor = p_cell.GetNeighbor(p_direction);
		if (neighbor == null) {
			return;
		}

		Vector3 bridge = HexMetrics.GetBridge(p_direction);
		bridge.y = neighbor.Position.y - p_cell.Position.y;
		EdgeVertices e2 = new EdgeVertices (p_e.v1 + bridge, p_e.v4 + bridge);

		if (p_cell.GetEdgeType(p_direction) == ENUM_HexEdgeType.Slope) {
			TriangulateEdgeTerraces(p_e, p_cell, e2, neighbor);
		}
		else {
			TriangulateEdgeStrip (p_e, p_cell.Color, e2, neighbor.Color);
		}

		HexCell nextNeighbor = p_cell.GetNeighbor(p_direction.Next());
		if (p_direction <= ENUM_HexDirection.E && nextNeighbor != null) {
			Vector3 v5 = p_e.v4 + HexMetrics.GetBridge(p_direction.Next());
			v5.y = nextNeighbor.Position.y;

			if (p_cell.Elevation <= neighbor.Elevation) {
				if (p_cell.Elevation <= nextNeighbor.Elevation) {
					TriangulateCorner(p_e.v4, p_cell, e2.v4, neighbor, v5, nextNeighbor);
				}
				else {
					TriangulateCorner(v5, nextNeighbor, p_e.v4, p_cell, e2.v4, neighbor);
				}
			}
			else if (neighbor.Elevation <= nextNeighbor.Elevation) {
				TriangulateCorner(e2.v4, neighbor, v5, nextNeighbor, p_e.v4, p_cell);
			}
			else {
				TriangulateCorner(v5, nextNeighbor, p_e.v4, p_cell, e2.v4, neighbor);
			}
		}
	}

	void TriangulateCorner (
		Vector3 p_bottom, HexCell p_bottomCell,
		Vector3 p_left, HexCell p_leftCell,
		Vector3 p_right, HexCell p_rightCell
	) {
		ENUM_HexEdgeType p_leftEdgeType = p_bottomCell.GetEdgeType(p_leftCell);
		ENUM_HexEdgeType p_rightEdgeType = p_bottomCell.GetEdgeType(p_rightCell);

		if (p_leftEdgeType == ENUM_HexEdgeType.Slope) {
			if (p_rightEdgeType == ENUM_HexEdgeType.Slope) {
				TriangulateCornerTerraces(
					p_bottom, p_bottomCell, p_left, p_leftCell, p_right, p_rightCell
				);
			}
			else if (p_rightEdgeType == ENUM_HexEdgeType.Flat) {
				TriangulateCornerTerraces(
					p_left, p_leftCell, p_right, p_rightCell, p_bottom, p_bottomCell
				);
			}
			else {
				TriangulateCornerTerracesCliff(
					p_bottom, p_bottomCell, p_left, p_leftCell, p_right, p_rightCell
				);
			}
		}
		else if (p_rightEdgeType == ENUM_HexEdgeType.Slope) {
			if (p_leftEdgeType == ENUM_HexEdgeType.Flat) {
				TriangulateCornerTerraces(
					p_right, p_rightCell, p_bottom, p_bottomCell, p_left, p_leftCell
				);
			}
			else {
				TriangulateCornerCliffTerraces(
					p_bottom, p_bottomCell, p_left, p_leftCell, p_right, p_rightCell
				);
			}
		}
		else if (p_leftCell.GetEdgeType(p_rightCell) == ENUM_HexEdgeType.Slope) {
			if (p_leftCell.Elevation < p_rightCell.Elevation) {
				TriangulateCornerCliffTerraces(
					p_right, p_rightCell, p_bottom, p_bottomCell, p_left, p_leftCell
				);
			}
			else {
				TriangulateCornerTerracesCliff(
					p_left, p_leftCell, p_right, p_rightCell, p_bottom, p_bottomCell
				);
			}
		}
		else {
			AddTriangle(p_bottom, p_left, p_right);
			AddTriangleColor(p_bottomCell.Color, p_leftCell.Color, p_rightCell.Color);
		}
	}

	void TriangulateEdgeTerraces (
		EdgeVertices p_begin, HexCell p_beginCell,
		EdgeVertices p_end, HexCell p_endCell
	) {
		EdgeVertices e2 = EdgeVertices.TerraceLerp (p_begin, p_end, 1);
		Color c2 = HexMetrics.TerraceLerp(p_beginCell.Color, p_endCell.Color, 1);

		TriangulateEdgeStrip (p_begin, p_beginCell.Color, e2, c2);

		for (int i = 2; i < HexMetrics._terraceSteps; i++) {
			EdgeVertices e1 = e2;
			Color c1 = c2;
			e2 = EdgeVertices.TerraceLerp (p_begin, p_end, 1);
			c2 = HexMetrics.TerraceLerp(p_beginCell.Color, p_endCell.Color, i);
			TriangulateEdgeStrip (e1, c1, e2, c2);
		}

		TriangulateEdgeStrip (e2, c2, p_end, p_endCell.Color);
	}

	void TriangulateCornerTerraces (
		Vector3 p_begin, HexCell p_p_beginCell,
		Vector3 p_left, HexCell p_leftCell,
		Vector3 p_right, HexCell p_rightCell
	) {
		Vector3 v3 = HexMetrics.TerraceLerp(p_begin, p_left, 1);
		Vector3 v4 = HexMetrics.TerraceLerp(p_begin, p_right, 1);
		Color c3 = HexMetrics.TerraceLerp(p_p_beginCell.Color, p_leftCell.Color, 1);
		Color c4 = HexMetrics.TerraceLerp(p_p_beginCell.Color, p_rightCell.Color, 1);

		AddTriangle(p_begin, v3, v4);
		AddTriangleColor(p_p_beginCell.Color, c3, c4);

		for (int i = 2; i < HexMetrics._terraceSteps; i++) {
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c3;
			Color c2 = c4;
			v3 = HexMetrics.TerraceLerp(p_begin, p_left, i);
			v4 = HexMetrics.TerraceLerp(p_begin, p_right, i);
			c3 = HexMetrics.TerraceLerp(p_p_beginCell.Color, p_leftCell.Color, i);
			c4 = HexMetrics.TerraceLerp(p_p_beginCell.Color, p_rightCell.Color, i);
			AddQuad(v1, v2, v3, v4);
			AddQuadColor(c1, c2, c3, c4);
		}

		AddQuad(v3, v4, p_left, p_right);
		AddQuadColor(c3, c4, p_leftCell.Color, p_rightCell.Color);
	}

	void TriangulateCornerTerracesCliff (
		Vector3 p_begin, HexCell p_p_beginCell,
		Vector3 p_left, HexCell p_leftCell,
		Vector3 p_right, HexCell p_rightCell
	) {
		float b = 1f / (p_rightCell.Elevation - p_p_beginCell.Elevation);
		if (b < 0) {
			b = -b;
		}
		Vector3 p_boundary = Vector3.Lerp(p_begin, p_right, b);
		Color p_boundaryColor = Color.Lerp(p_p_beginCell.Color, p_rightCell.Color, b);

		TriangulateBoundaryTriangle(
			p_begin, p_p_beginCell, p_left, p_leftCell, p_boundary, p_boundaryColor
		);

		if (p_leftCell.GetEdgeType(p_rightCell) == ENUM_HexEdgeType.Slope) {
			TriangulateBoundaryTriangle(
				p_left, p_leftCell, p_right, p_rightCell, p_boundary, p_boundaryColor
			);
		}
		else {
			AddTriangle(p_left, p_right, p_boundary);
			AddTriangleColor(p_leftCell.Color, p_rightCell.Color, p_boundaryColor);
		}
	}

	void TriangulateCornerCliffTerraces (
		Vector3 p_begin, HexCell p_p_beginCell,
		Vector3 p_left, HexCell p_leftCell,
		Vector3 p_right, HexCell p_rightCell
	) {
		float b = 1f / (p_leftCell.Elevation - p_p_beginCell.Elevation);
		if (b < 0) {
			b = -b;
		}
		Vector3 p_boundary = Vector3.Lerp(p_begin, p_left, b);
		Color p_boundaryColor = Color.Lerp(p_p_beginCell.Color, p_leftCell.Color, b);

		TriangulateBoundaryTriangle(
			p_right, p_rightCell, p_begin, p_p_beginCell, p_boundary, p_boundaryColor
		);

		if (p_leftCell.GetEdgeType(p_rightCell) == ENUM_HexEdgeType.Slope) {
			TriangulateBoundaryTriangle(
				p_left, p_leftCell, p_right, p_rightCell, p_boundary, p_boundaryColor
			);
		}
		else {
			AddTriangle(p_left, p_right, p_boundary);
			AddTriangleColor(p_leftCell.Color, p_rightCell.Color, p_boundaryColor);
		}
	}

	void TriangulateBoundaryTriangle (
		Vector3 p_begin, HexCell p_p_beginCell,
		Vector3 p_left, HexCell p_leftCell,
		Vector3 p_boundary, Color p_boundaryColor
	) {
		Vector3 v2 = HexMetrics.TerraceLerp(p_begin, p_left, 1);
		Color c2 = HexMetrics.TerraceLerp(p_p_beginCell.Color, p_leftCell.Color, 1);

		AddTriangle(p_begin, v2, p_boundary);
		AddTriangleColor(p_p_beginCell.Color, c2, p_boundaryColor);

		for (int i = 2; i < HexMetrics._terraceSteps; i++) {
			Vector3 v1 = v2;
			Color c1 = c2;
			v2 = HexMetrics.TerraceLerp(p_begin, p_left, i);
			c2 = HexMetrics.TerraceLerp(p_p_beginCell.Color, p_leftCell.Color, i);
			AddTriangle(v1, v2, p_boundary);
			AddTriangleColor(c1, c2, p_boundaryColor);
		}

		AddTriangle(v2, p_left, p_boundary);
		AddTriangleColor(c2, p_leftCell.Color, p_boundaryColor);
	}

	void TriangulateEdgeFan(Vector3 p_center, EdgeVertices p_edge, Color p_color) {
		AddTriangle (p_center, p_edge.v1, p_edge.v2);
		AddTriangleColor (p_color);
		AddTriangle (p_center, p_edge.v2, p_edge.v3);
		AddTriangleColor (p_color);
		AddTriangle (p_center, p_edge.v3, p_edge.v4);
		AddTriangleColor (p_color);
	}

	void TriangulateEdgeStrip(EdgeVertices p_e1, Color p_c1, EdgeVertices p_e2, Color p_c2) {
		AddQuad (p_e1.v1, p_e1.v2, p_e2.v1, p_e2.v2);
		AddQuadColor (p_c1, p_c2);
		AddQuad (p_e1.v2, p_e1.v3, p_e2.v2, p_e2.v3);
		AddQuadColor (p_c1, p_c2);
		AddQuad (p_e1.v3, p_e1.v4, p_e2.v3, p_e2.v4);
		AddQuadColor (p_c1, p_c2);
	}

	void AddTriangle (Vector3 p_v1, Vector3 p_v2, Vector3 p_v3) {
		int vertexIndex = _vertices.Count;
		_vertices.Add(Perturb(p_v1));
		_vertices.Add(Perturb(p_v2));
		_vertices.Add(Perturb(p_v3));
		_triangles.Add(vertexIndex);
		_triangles.Add(vertexIndex + 1);
		_triangles.Add(vertexIndex + 2);
	}

	void AddTriangleColor (Color p_color) {
		_colors.Add(p_color);
		_colors.Add(p_color);
		_colors.Add(p_color);
	}

	void AddTriangleColor (Color p_c1, Color p_c2, Color p_c3) {
		_colors.Add(p_c1);
		_colors.Add(p_c2);
		_colors.Add(p_c3);
	}

	void AddQuad (Vector3 p_v1, Vector3 p_v2, Vector3 p_v3, Vector3 p_v4) {
		int vertexIndex = _vertices.Count;
		_vertices.Add(Perturb(p_v1));
		_vertices.Add(Perturb(p_v2));
		_vertices.Add(Perturb(p_v3));
		_vertices.Add(Perturb(p_v4));
		_triangles.Add(vertexIndex);
		_triangles.Add(vertexIndex + 2);
		_triangles.Add(vertexIndex + 1);
		_triangles.Add(vertexIndex + 1);
		_triangles.Add(vertexIndex + 2);
		_triangles.Add(vertexIndex + 3);
	}

	void AddQuadColor (Color p_c1, Color p_c2) {
		_colors.Add(p_c1);
		_colors.Add(p_c1);
		_colors.Add(p_c2);
		_colors.Add(p_c2);
	}

	void AddQuadColor (Color p_c1, Color p_c2, Color p_c3, Color p_c4) {
		_colors.Add(p_c1);
		_colors.Add(p_c2);
		_colors.Add(p_c3);
		_colors.Add(p_c4);
	}

	Vector3 Perturb(Vector3 p_position) {
		Vector4 sample = HexMetrics.SampleNoise (p_position);
		p_position.x += (sample.x * 2f - 1f) * HexMetrics._cellPerturbStrengh;
		//p_position.y += (sample.y * 2f - 1f) * HexMetrics._cellPerturbStrengh;
		p_position.z += (sample.z * 2f - 1f) * HexMetrics._cellPerturbStrengh;
		return p_position;
	}
}