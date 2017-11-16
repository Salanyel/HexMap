using UnityEngine;
using UnityEngine.UI;

public class HexGridChunk : MonoBehaviour {

	#region Variables

	[SerializeField]
	HexMesh _terrain;

	[SerializeField]
	HexMesh _rivers;

	[SerializeField]
	HexMesh _roads;

	[SerializeField]
	HexMesh _water;

	[SerializeField]
	HexMesh _waterShore;

	[SerializeField]
	HexMesh _estuaries;

	[SerializeField]
	HexFeatureManager _features;

	HexCell[] _cells;
	Canvas _gridCanvas;

	public HexMesh Terrain {
		get { return _terrain; }
		set { _terrain = value; }
	}

	public HexMesh Rivers {
		get { return _rivers; }
		set { _rivers = value; }
	}

	public HexFeatureManager Features {
		get { return _features; }
		set { _features = value; }
	}

	#endregion

	#region Unity_Methods

	void Awake() {
		_gridCanvas = GetComponentInChildren<Canvas> ();

		_cells = new HexCell[HexMetrics._chunkSizeX * HexMetrics._chunkSizeZ];
		ShowUI (false);
	}

	void LateUpdate() {
		Triangulate ();
		enabled = false;
	}

	#endregion

	#region Methods

	public void AddCell(int p_index, HexCell p_cell) {
		_cells [p_index] = p_cell;
		p_cell.Chunk = this;
		p_cell.transform.SetParent (transform, false);
		p_cell.UIRect.SetParent (_gridCanvas.transform, false);
	}

	public void Refresh() {
		enabled = true;
	}

	public void ShowUI(bool p_isVisible) {
		_gridCanvas.gameObject.SetActive (p_isVisible);
	}

	public void Triangulate () {
		_terrain.Clear ();
		_rivers.Clear ();
		_roads.Clear ();
		_water.Clear ();
		_waterShore.Clear ();
		_estuaries.Clear ();
		_features.Clear ();

		for (int i = 0; i < _cells.Length; i++) {
			Triangulate(_cells[i]);
		}

		_terrain.Apply ();
		_rivers.Apply ();
		_roads.Apply ();
		_water.Apply ();
		_waterShore.Apply ();
		_estuaries.Apply ();
		_features.Apply ();
	}

	void Triangulate (HexCell cell) {
		for (ENUM_HexDirection d = ENUM_HexDirection.NE; d <= ENUM_HexDirection.NW; d++) {
			Triangulate(d, cell);
		}

		if (!cell.IsUnderWater && !cell.HasRiver && !cell.HasRoads()) {
			_features.AddFeature (cell, cell.Position);
		}

	}

	void Triangulate (ENUM_HexDirection direction, HexCell cell) {
		Vector3 center = cell.Position;
		EdgeVertices e = new EdgeVertices(
			center + HexMetrics.GetFirstSolidCorner(direction),
			center + HexMetrics.GetSecondSolidCorner(direction)
		);

		if (cell.HasRiver) {
			if (cell.HasRiverThroughEdge(direction)) {
				e.v3.y = cell.StreamBedY;
				if (cell.HasRiverBeginOrEnd) {
					TriangulateWithRiverBeginOrEnd(direction, cell, center, e);
				}
				else {
					TriangulateWithRiver(direction, cell, center, e);
				}
			}
			else {
				TriangulateAdjacentToRiver(direction, cell, center, e);
			}
		}
		else {
			TriangulateWithoutRiver (direction, cell, center, e);

			if (!cell.IsUnderWater && !cell.HasRoadThroughEdge(direction)) {
				_features.AddFeature (cell, (center + e.v1 + e.v5) * (1f / 3f));
			}
		}

		if (direction <= ENUM_HexDirection.SE) {
			TriangulateConnection(direction, cell, e);
		}

		if (cell.IsUnderWater) {
			TriangulateWater (direction, cell, center);
		}
	}

	void TriangulateWithoutRiver(ENUM_HexDirection p_direction, HexCell p_cell, Vector3 p_center, EdgeVertices p_e) {
		TriangulateEdgeFan(p_center, p_e, p_cell.Color);

		if (p_cell.HasRoads()) {
			Vector2 interpolators = GetRoadInterpolators (p_direction, p_cell);
			TriangulateRoad (p_center, Vector3.Lerp (p_center, p_e.v1, interpolators.x), Vector3.Lerp (p_center, p_e.v5, interpolators.y), p_e, p_cell.HasRoadThroughEdge(p_direction));
		}
	}

	void TriangulateWater (ENUM_HexDirection direction, HexCell cell, Vector3 center) {
		center.y = cell.WaterSurfaceY;

		HexCell neighbor = cell.GetNeighbor(direction);
		if (neighbor != null && !neighbor.IsUnderWater) {
			TriangulateWaterShore(direction, cell, neighbor, center);
		}
		else {
			TriangulateOpenWater(direction, cell, neighbor, center);
		}
	}

	void TriangulateOpenWater (ENUM_HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center) {
		Vector3 c1 = center + HexMetrics.GetFirstWaterCorner(direction);
		Vector3 c2 = center + HexMetrics.GetSecondWaterCorner(direction);

		_water.AddTriangle(center, c1, c2);

		if (direction <= ENUM_HexDirection.SE && neighbor != null) {
			Vector3 bridge = HexMetrics.GetWaterBridge(direction);
			Vector3 e1 = c1 + bridge;
			Vector3 e2 = c2 + bridge;

			_water.AddQuad(c1, c2, e1, e2);

			if (direction <= ENUM_HexDirection.E) {
				HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
				if (nextNeighbor == null || !nextNeighbor.IsUnderWater) {
					return;
				}
				_water.AddTriangle(
					c2, e2, c2 + HexMetrics.GetWaterBridge(direction.Next())
				);
			}
		}
	}

	void TriangulateWaterShore (ENUM_HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center) {
		EdgeVertices e1 = new EdgeVertices(
			center + HexMetrics.GetFirstWaterCorner(direction),
			center + HexMetrics.GetSecondWaterCorner(direction)
		);
		_water.AddTriangle(center, e1.v1, e1.v2);
		_water.AddTriangle(center, e1.v2, e1.v3);
		_water.AddTriangle(center, e1.v3, e1.v4);
		_water.AddTriangle(center, e1.v4, e1.v5);

		Vector3 center2 = neighbor.Position;
		center2.y = center.y;
		EdgeVertices e2 = new EdgeVertices(
			center2 + HexMetrics.GetSecondSolidCorner(direction.Opposite()),
			center2 + HexMetrics.GetFirstSolidCorner(direction.Opposite())
		);

		if (cell.HasRiverThroughEdge (direction)) {
			TriangulateEstuary (e1, e2, cell.IncomingRiver == direction);
		} else {
			_waterShore.AddQuad (e1.v1, e1.v2, e2.v1, e2.v2);
			_waterShore.AddQuad (e1.v2, e1.v3, e2.v2, e2.v3);
			_waterShore.AddQuad (e1.v3, e1.v4, e2.v3, e2.v4);
			_waterShore.AddQuad (e1.v4, e1.v5, e2.v4, e2.v5);
			_waterShore.AddQuadUV (0f, 0f, 0f, 1f);
			_waterShore.AddQuadUV (0f, 0f, 0f, 1f);
			_waterShore.AddQuadUV (0f, 0f, 0f, 1f);
			_waterShore.AddQuadUV (0f, 0f, 0f, 1f);
		}

		HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (nextNeighbor != null) {
			Vector3 v3 = nextNeighbor.Position + (nextNeighbor.IsUnderWater ?
				HexMetrics.GetFirstWaterCorner(direction.Previous()) :
				HexMetrics.GetFirstSolidCorner(direction.Previous()));
			v3.y = center.y;
			_waterShore.AddTriangle(e1.v5, e2.v5, v3);
			_waterShore.AddTriangleUV(
				new Vector2(0f, 0f),
				new Vector2(0f, 1f),
				new Vector2(0f, nextNeighbor.IsUnderWater ? 0f : 1f)
			);
		}
	}

	void TriangulateEstuary (
		EdgeVertices e1, EdgeVertices e2, bool incomingRiver
	) {
		_waterShore.AddTriangle(e2.v1, e1.v2, e1.v1);
		_waterShore.AddTriangle(e2.v5, e1.v5, e1.v4);
		_waterShore.AddTriangleUV(
			new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f)
		);
		_waterShore.AddTriangleUV(
			new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f)
		);

		_estuaries.AddQuad(e2.v1, e1.v2, e2.v2, e1.v3);
		_estuaries.AddTriangle(e1.v3, e2.v2, e2.v4);
		_estuaries.AddQuad(e1.v3, e1.v4, e2.v4, e2.v5);

		_estuaries.AddQuadUV(
			new Vector2(0f, 1f), new Vector2(0f, 0f),
			new Vector2(1f, 1f), new Vector2(0f, 0f)
		);
		_estuaries.AddTriangleUV(
			new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(1f, 1f)
		);
		_estuaries.AddQuadUV(
			new Vector2(0f, 0f), new Vector2(0f, 0f),
			new Vector2(1f, 1f), new Vector2(0f, 1f)
		);

		if (incomingRiver) {
			_estuaries.AddQuadUV2(
				new Vector2(1.5f, 1f), new Vector2(0.7f, 1.15f),
				new Vector2(1f, 0.8f), new Vector2(0.5f, 1.1f)
			);
			_estuaries.AddTriangleUV2(
				new Vector2(0.5f, 1.1f),
				new Vector2(1f, 0.8f),
				new Vector2(0f, 0.8f)
			);
			_estuaries.AddQuadUV2(
				new Vector2(0.5f, 1.1f), new Vector2(0.3f, 1.15f),
				new Vector2(0f, 0.8f), new Vector2(-0.5f, 1f)
			);
		}
		else {
			_estuaries.AddQuadUV2(
				new Vector2(-0.5f, -0.2f), new Vector2(0.3f, -0.35f),
				new Vector2(0f, 0f), new Vector2(0.5f, -0.3f)
			);
			_estuaries.AddTriangleUV2(
				new Vector2(0.5f, -0.3f),
				new Vector2(0f, 0f),
				new Vector2(1f, 0f)
			);
			_estuaries.AddQuadUV2(
				new Vector2(0.5f, -0.3f), new Vector2(0.7f, -0.35f),
				new Vector2(1f, 0f), new Vector2(1.5f, -0.2f)
			);
		}
	}

	void TriangulateWaterfallInWater(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y1, float y2, float waterY) {
		v1.y = v2.y = y1;
		v3.y = v4.y = y2;
		v1 = HexMetrics.Perturb (v1);
		v2 = HexMetrics.Perturb (v2);
		v3 = HexMetrics.Perturb (v3);
		v4 = HexMetrics.Perturb (v4);

		float t = (waterY - y2) / (y1 - y2);
		v3 = Vector3.Lerp (v3, v1, t);
		v4 = Vector3.Lerp (v4, v2, t);

		_rivers.AddQuadUnperturbed(v1, v2, v3, v4);
		_rivers.AddQuadUV (0f, 1f, 0.8f, 1f);
	}

	void TriangulateAdjacentToRiver (
		ENUM_HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e
	) {
		if (cell.HasRoads()) {
			TriangulateRoadAdjacentToRiver (direction, cell, center, e);
		}

		if (cell.HasRiverThroughEdge(direction.Next())) {
			if (cell.HasRiverThroughEdge(direction.Previous())) {
				center += HexMetrics.GetSolidEdgeMiddle(direction) *
					(HexMetrics._innerToOuter * 0.5f);
			}
			else if (
				cell.HasRiverThroughEdge(direction.Previous2())
			) {
				center += HexMetrics.GetFirstSolidCorner(direction) * 0.25f;
			}
		}
		else if (
			cell.HasRiverThroughEdge(direction.Previous()) &&
			cell.HasRiverThroughEdge(direction.Next2())
		) {
			center += HexMetrics.GetSecondSolidCorner(direction) * 0.25f;
		}

		EdgeVertices m = new EdgeVertices(
			Vector3.Lerp(center, e.v1, 0.5f),
			Vector3.Lerp(center, e.v5, 0.5f)
		);

		TriangulateEdgeStrip(m, cell.Color, e, cell.Color);
		TriangulateEdgeFan(center, m, cell.Color);

		if (!cell.IsUnderWater && !cell.HasRoadThroughEdge(direction)) {
			_features.AddFeature (cell, (center + e.v1 + e.v5) * (1f / 3f));
		}
	}

	void TriangulateRoadAdjacentToRiver(ENUM_HexDirection p_direction, HexCell p_cell, Vector3 p_center, EdgeVertices p_e) {
		bool hasRoadThroughEdge = p_cell.HasRoadThroughEdge(p_direction);
		bool previousHasRiver = p_cell.HasRiverThroughEdge(p_direction.Previous());
		bool nextHasRiver = p_cell.HasRiverThroughEdge(p_direction.Next());
		Vector2 interpolators = GetRoadInterpolators(p_direction, p_cell);
		Vector3 roadCenter = p_center;

		if (p_cell.HasRiverBeginOrEnd) {
			roadCenter += HexMetrics.GetSolidEdgeMiddle(p_cell.RiverBeginOrEndDirection().Opposite()) * (1f / 3f);
		}
		else if (p_cell.IncomingRiver == p_cell.OutgoingRiver.Opposite()) {
			Vector3 corner;
			if (previousHasRiver) {
				if (
					!hasRoadThroughEdge &&
					!p_cell.HasRoadThroughEdge(p_direction.Next())
				) {
					return;
				}
				corner = HexMetrics.GetSecondSolidCorner(p_direction);
			}
			else {
				if (
					!hasRoadThroughEdge &&
					!p_cell.HasRoadThroughEdge(p_direction.Previous())
				) {
					return;
				}
				corner = HexMetrics.GetFirstSolidCorner(p_direction);
			}
			roadCenter += corner * 0.5f;
			if (p_cell.IncomingRiver == p_direction.Next() && (
				p_cell.HasRoadThroughEdge(p_direction.Next2()) ||
				p_cell.HasRoadThroughEdge(p_direction.Opposite())
			)) {
				_features.AddBridge(roadCenter, p_center - corner * 0.5f);
			}
			p_center += corner * 0.25f;
		}
		else if (p_cell.IncomingRiver == p_cell.OutgoingRiver.Previous()) {
			roadCenter -= HexMetrics.GetSecondCorner(p_cell.IncomingRiver) * 0.2f;
		}
		else if (p_cell.IncomingRiver == p_cell.OutgoingRiver.Next()) {
			roadCenter -= HexMetrics.GetFirstCorner(p_cell.IncomingRiver) * 0.2f;
		}
		else if (previousHasRiver && nextHasRiver) {
			if (!hasRoadThroughEdge) {
				return;
			}
			Vector3 offset = HexMetrics.GetSolidEdgeMiddle(p_direction) *
				HexMetrics._innerToOuter;
			roadCenter += offset * 0.7f;
			p_center += offset * 0.5f;
		}
		else {
			ENUM_HexDirection middle;
			if (previousHasRiver) {
				middle = p_direction.Next();
			}
			else if (nextHasRiver) {
				middle = p_direction.Previous();
			}
			else {
				middle = p_direction;
			}
			if (
				!p_cell.HasRoadThroughEdge(middle) &&
				!p_cell.HasRoadThroughEdge(middle.Previous()) &&
				!p_cell.HasRoadThroughEdge(middle.Next())
			) {
				return;
			}
			Vector3 offset = HexMetrics.GetSolidEdgeMiddle(middle);
			roadCenter += offset * 0.25f;
			if (
				p_direction == middle &&
				p_cell.HasRoadThroughEdge(p_direction.Opposite())
			) {
				_features.AddBridge(
					roadCenter,
					p_center - offset * (HexMetrics._innerToOuter * 0.7f)
				);
			}
		}

		Vector3 mL = Vector3.Lerp(roadCenter, p_e.v1, interpolators.x);
		Vector3 mR = Vector3.Lerp(roadCenter, p_e.v5, interpolators.y);
		TriangulateRoad(roadCenter, mL, mR, p_e, hasRoadThroughEdge);
		if (previousHasRiver) {
			TriangulateRoadEdge(roadCenter, p_center, mL);
		}
		if (nextHasRiver) {
			TriangulateRoadEdge(roadCenter, mR, p_center);
		}
	}

	void TriangulateRoadEdge (Vector3 p_center, Vector3 p_mL, Vector3 p_mR) {
		_roads.AddTriangle (p_center, p_mL, p_mR);
		_roads.AddTriangleUV (new Vector2 (1f, 0f), new Vector2 (0f, 0f), new Vector2 (0f, 0f));
	}

	Vector2 GetRoadInterpolators(ENUM_HexDirection p_direction, HexCell p_cell) {
		Vector2 interpolators;

		if (p_cell.HasRoadThroughEdge (p_direction)) {
			interpolators.x = interpolators.y = 0.5f;
		} else {
			interpolators.x = p_cell.HasRoadThroughEdge (p_direction.Previous ()) ? 0.5f : 0.25f;
			interpolators.y = p_cell.HasRoadThroughEdge (p_direction.Next ()) ? 0.5f : 0.25f;
		}

		return interpolators;
	}

	void TriangulateRoad(Vector3 p_center, Vector3 p_mL, Vector3 p_mR, EdgeVertices p_e, bool p_hasRoadThroughCellEdge) {
		if (p_hasRoadThroughCellEdge) {
			Vector3 mC = Vector3.Lerp (p_mL, p_mR, 0.5f);
			TriangulateRoadSegment (p_mL, mC, p_mR, p_e.v2, p_e.v3, p_e.v4);

			_roads.AddTriangle (p_center, p_mL, mC);
			_roads.AddTriangle (p_center, mC, p_mR);

			_roads.AddTriangleUV (new Vector2 (1f, 0f), new Vector2 (0f, 0f), new Vector2 (1f, 0f));
			_roads.AddTriangleUV (new Vector2 (1f, 0f), new Vector2 (1f, 0f), new Vector2 (0f, 0f));
		} else {
			TriangulateRoadEdge (p_center, p_mL, p_mR);
		}
	}

	void TriangulateRoadSegment(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6) {
		_roads.AddQuad (v1, v2, v4, v5);
		_roads.AddQuad (v2, v3, v5, v6);

		_roads.AddQuadUV (0f, 1f, 0f, 0f);
		_roads.AddQuadUV (1f, 0f, 0f, 0f);
	}

	void TriangulateWithRiverBeginOrEnd (ENUM_HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e) {
		EdgeVertices m = new EdgeVertices(
			Vector3.Lerp(center, e.v1, 0.5f),
			Vector3.Lerp(center, e.v5, 0.5f)
		);
		m.v3.y = e.v3.y;

		TriangulateEdgeStrip(m, cell.Color, e, cell.Color);
		TriangulateEdgeFan(center, m, cell.Color);

		if (!cell.IsUnderWater) {
			bool reversed = cell.HasIncomingRiver;
			TriangulateRiverQuad (m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed);
			center.y = m.v2.y = m.v4.y = cell.RiverSurfaceY;
			Rivers.AddTriangle (center, m.v2, m.v4);

			if (reversed) {
				_rivers.AddTriangleUV (new Vector2 (0.5f, 0.4f), new Vector2 (1f, 0.2f), new Vector2 (0f, 0.2f));
			} else {
				_rivers.AddTriangleUV (new Vector2 (0.5f, 0.4f), new Vector2 (0f, 0.6f), new Vector2 (1f, 0.6f));
			}
		}
	}

	void TriangulateWithRiver (
		ENUM_HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e
	) {
		Vector3 centerL, centerR;
		if (cell.HasRiverThroughEdge(direction.Opposite())) {
			centerL = center +
				HexMetrics.GetFirstSolidCorner(direction.Previous()) * 0.25f;
			centerR = center +
				HexMetrics.GetSecondSolidCorner(direction.Next()) * 0.25f;
		}
		else if (cell.HasRiverThroughEdge(direction.Next())) {
			centerL = center;
			centerR = Vector3.Lerp(center, e.v5, 2f / 3f);
		}
		else if (cell.HasRiverThroughEdge(direction.Previous())) {
			centerL = Vector3.Lerp(center, e.v1, 2f / 3f);
			centerR = center;
		}
		else if (cell.HasRiverThroughEdge(direction.Next2())) {
			centerL = center;
			centerR = center +
				HexMetrics.GetSolidEdgeMiddle(direction.Next()) *
				(0.5f * HexMetrics._innerToOuter);
		}
		else {
			centerL = center +
				HexMetrics.GetSolidEdgeMiddle(direction.Previous()) *
				(0.5f * HexMetrics._innerToOuter);
			centerR = center;
		}
		center = Vector3.Lerp(centerL, centerR, 0.5f);

		EdgeVertices m = new EdgeVertices(
			Vector3.Lerp(centerL, e.v1, 0.5f),
			Vector3.Lerp(centerR, e.v5, 0.5f),
			1f / 6f
		);
		m.v3.y = center.y = e.v3.y;

		TriangulateEdgeStrip(m, cell.Color, e, cell.Color);

		_terrain.AddTriangle(centerL, m.v1, m.v2);
		_terrain.AddTriangleColor(cell.Color);
		_terrain.AddQuad(centerL, center, m.v2, m.v3);
		_terrain.AddQuadColor(cell.Color);
		_terrain.AddQuad(center, centerR, m.v3, m.v4);
		_terrain.AddQuadColor(cell.Color);
		_terrain.AddTriangle(centerR, m.v4, m.v5);
		_terrain.AddTriangleColor(cell.Color);
	
		if (!cell.IsUnderWater) {
			bool reversed = cell.IncomingRiver == direction;
			TriangulateRiverQuad (centerL, centerR, m.v2, m.v4, cell.RiverSurfaceY, 0.4f, reversed);
			TriangulateRiverQuad (m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed);
		}
	}

	void TriangulateConnection (ENUM_HexDirection direction, HexCell cell, EdgeVertices e1) {
		HexCell neighbor = cell.GetNeighbor(direction);
		if (neighbor == null) {
			return;
		}

		Vector3 bridge = HexMetrics.GetBridge(direction);
		bridge.y = neighbor.Position.y - cell.Position.y;
		EdgeVertices e2 = new EdgeVertices(
			e1.v1 + bridge,
			e1.v5 + bridge
		);

		bool hasRiver = cell.HasRiverThroughEdge (direction);
		bool hasRoad = cell.HasRoadThroughEdge (direction);

		if (hasRiver) {
			e2.v3.y = neighbor.StreamBedY;

			if (!cell.IsUnderWater) {
				if (!neighbor.IsUnderWater) {
					TriangulateRiverQuad (e1.v2, e1.v4, e2.v2, e2.v4,
						cell.RiverSurfaceY, neighbor.RiverSurfaceY, 0.8f,
						cell.HasIncomingRiver && cell.IncomingRiver == direction);
				} else if (cell.Elevation > neighbor.WaterLevel) {
					TriangulateWaterfallInWater (e1.v2, e1.v4, e2.v2, e2.v4,
						cell.RiverSurfaceY, neighbor.RiverSurfaceY,
						neighbor.WaterSurfaceY);
				}
			} else if (!neighbor.IsUnderWater && neighbor.Elevation > cell.WaterLevel) {
				TriangulateWaterfallInWater (e2.v4, e2.v2, e1.v4, e1.v2,
					neighbor.RiverSurfaceY, cell.RiverSurfaceY,
					cell.WaterSurfaceY);
			}
		}

		if (cell.GetEdgeType(direction) == ENUM_HexEdgeType.Slope) {
			TriangulateEdgeTerraces(e1, cell, e2, neighbor, hasRoad);
		}
		else {
			TriangulateEdgeStrip(e1, cell.Color, e2, neighbor.Color, hasRoad);
		}

		_features.AddWall (e1, cell, e2, neighbor, hasRiver, hasRoad);

		HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (direction <= ENUM_HexDirection.E && nextNeighbor != null) {
			Vector3 v5 = e1.v5 + HexMetrics.GetBridge(direction.Next());
			v5.y = nextNeighbor.Position.y;

			if (cell.Elevation <= neighbor.Elevation) {
				if (cell.Elevation <= nextNeighbor.Elevation) {
					TriangulateCorner(
						e1.v5, cell, e2.v5, neighbor, v5, nextNeighbor
					);
				}
				else {
					TriangulateCorner(
						v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor
					);
				}
			}
			else if (neighbor.Elevation <= nextNeighbor.Elevation) {
				TriangulateCorner(
					e2.v5, neighbor, v5, nextNeighbor, e1.v5, cell
				);
			}
			else {
				TriangulateCorner(
					v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor
				);
			}
		}
	}

	void TriangulateCorner (
		Vector3 bottom, HexCell bottomCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {
		ENUM_HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
		ENUM_HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

		if (leftEdgeType == ENUM_HexEdgeType.Slope) {
			if (rightEdgeType == ENUM_HexEdgeType.Slope) {
				TriangulateCornerTerraces(
					bottom, bottomCell, left, leftCell, right, rightCell
				);
			}
			else if (rightEdgeType == ENUM_HexEdgeType.Flat) {
				TriangulateCornerTerraces(
					left, leftCell, right, rightCell, bottom, bottomCell
				);
			}
			else {
				TriangulateCornerTerracesCliff(
					bottom, bottomCell, left, leftCell, right, rightCell
				);
			}
		}
		else if (rightEdgeType == ENUM_HexEdgeType.Slope) {
			if (leftEdgeType == ENUM_HexEdgeType.Flat) {
				TriangulateCornerTerraces(
					right, rightCell, bottom, bottomCell, left, leftCell
				);
			}
			else {
				TriangulateCornerCliffTerraces(
					bottom, bottomCell, left, leftCell, right, rightCell
				);
			}
		}
		else if (leftCell.GetEdgeType(rightCell) == ENUM_HexEdgeType.Slope) {
			if (leftCell.Elevation < rightCell.Elevation) {
				TriangulateCornerCliffTerraces(
					right, rightCell, bottom, bottomCell, left, leftCell
				);
			}
			else {
				TriangulateCornerTerracesCliff(
					left, leftCell, right, rightCell, bottom, bottomCell
				);
			}
		}
		else {
			_terrain.AddTriangle(bottom, left, right);
			_terrain.AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
		}

		_features.AddWall (bottom, bottomCell, left, leftCell, right, rightCell);
	}

	void TriangulateEdgeTerraces (
		EdgeVertices begin, HexCell beginCell,
		EdgeVertices end, HexCell endCell,
		bool p_hasRoad
	) {
		EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
		Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

		TriangulateEdgeStrip(begin, beginCell.Color, e2, c2, p_hasRoad);

		for (int i = 2; i < HexMetrics._terraceSteps; i++) {
			EdgeVertices e1 = e2;
			Color c1 = c2;
			e2 = EdgeVertices.TerraceLerp(begin, end, i);
			c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);
			TriangulateEdgeStrip(e1, c1, e2, c2, p_hasRoad);
		}

		TriangulateEdgeStrip(e2, c2, end, endCell.Color, p_hasRoad);
	}

	void TriangulateCornerTerraces (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {
		Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
		Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
		Color c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);
		Color c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, 1);

		_terrain.AddTriangle(begin, v3, v4);
		_terrain.AddTriangleColor(beginCell.Color, c3, c4);

		for (int i = 2; i < HexMetrics._terraceSteps; i++) {
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c3;
			Color c2 = c4;
			v3 = HexMetrics.TerraceLerp(begin, left, i);
			v4 = HexMetrics.TerraceLerp(begin, right, i);
			c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
			c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, i);
			_terrain.AddQuad(v1, v2, v3, v4);
			_terrain.AddQuadColor(c1, c2, c3, c4);
		}

		_terrain.AddQuad(v3, v4, left, right);
		_terrain.AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
	}

	void TriangulateCornerTerracesCliff (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {
		float b = 1f / (rightCell.Elevation - beginCell.Elevation);
		if (b < 0) {
			b = -b;
		}
		Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(right), b);
		Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

		TriangulateBoundaryTriangle(
			begin, beginCell, left, leftCell, boundary, boundaryColor
		);

		if (leftCell.GetEdgeType(rightCell) == ENUM_HexEdgeType.Slope) {
			TriangulateBoundaryTriangle(
				left, leftCell, right, rightCell, boundary, boundaryColor
			);
		}
		else {
			_terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
			_terrain.AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
		}
	}

	void TriangulateCornerCliffTerraces (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {
		float b = 1f / (leftCell.Elevation - beginCell.Elevation);
		if (b < 0) {
			b = -b;
		}
		Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b);
		Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);

		TriangulateBoundaryTriangle(
			right, rightCell, begin, beginCell, boundary, boundaryColor
		);

		if (leftCell.GetEdgeType(rightCell) == ENUM_HexEdgeType.Slope) {
			TriangulateBoundaryTriangle(
				left, leftCell, right, rightCell, boundary, boundaryColor
			);
		}
		else {
			_terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
			_terrain.AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
		}
	}

	void TriangulateBoundaryTriangle (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 boundary, Color boundaryColor
	) {
		Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
		Color c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

		_terrain.AddTriangleUnperturbed(HexMetrics.Perturb(begin), v2, boundary);
		_terrain.AddTriangleColor(beginCell.Color, c2, boundaryColor);

		for (int i = 2; i < HexMetrics._terraceSteps; i++) {
			Vector3 v1 = v2;
			Color c1 = c2;
			v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, i));
			c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
			_terrain.AddTriangleUnperturbed(v1, v2, boundary);
			_terrain.AddTriangleColor(c1, c2, boundaryColor);
		}

		_terrain.AddTriangleUnperturbed(v2, HexMetrics.Perturb(left), boundary);
		_terrain.AddTriangleColor(c2, leftCell.Color, boundaryColor);
	}

	void TriangulateEdgeFan (Vector3 center, EdgeVertices edge, Color color) {
		_terrain.AddTriangle(center, edge.v1, edge.v2);
		_terrain.AddTriangleColor(color);
		_terrain.AddTriangle(center, edge.v2, edge.v3);
		_terrain.AddTriangleColor(color);
		_terrain.AddTriangle(center, edge.v3, edge.v4);
		_terrain.AddTriangleColor(color);
		_terrain.AddTriangle(center, edge.v4, edge.v5);
		_terrain.AddTriangleColor(color);
	}

	void TriangulateEdgeStrip (
		EdgeVertices e1, Color c1,
		EdgeVertices e2, Color c2,
		bool p_hasRoad = false
	) {
		_terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
		_terrain.AddQuadColor(c1, c2);
		_terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
		_terrain.AddQuadColor(c1, c2);
		_terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
		_terrain.AddQuadColor(c1, c2);
		_terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
		_terrain.AddQuadColor(c1, c2);

		if (p_hasRoad) {
			TriangulateRoadSegment (e1.v2, e1.v3, e1.v4, e2.v2, e2.v3, e2.v4);
		}
	}

	void TriangulateRiverQuad( Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y, float v, bool reversed) {
		TriangulateRiverQuad (v1, v2, v3, v4, y, y, v, reversed);
	}

	void TriangulateRiverQuad( Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y1, float y2, float v, bool reversed) {
		v1.y = v2.y = y1;
		v3.y = v4.y = y2;
		_rivers.AddQuad (v1, v2, v3, v4);

		if (reversed) {
			_rivers.AddQuadUV (1f, 0f, 0.8f - v, 0.6f - v);
		} else {
			_rivers.AddQuadUV (0f, 1f, v, v + 0.2f);
		}
	}

	#endregion
}