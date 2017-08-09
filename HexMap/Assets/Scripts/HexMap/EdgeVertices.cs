using UnityEngine;

public struct EdgeVertices {
	public Vector3 v1, v2, v3, v4;

	public EdgeVertices (Vector3 p_corner1, Vector3 p_corner2) {
		v1 = p_corner1;
		v2 = Vector3.Lerp (p_corner1, p_corner2, 1f / 3f);
		v3 = Vector3.Lerp (p_corner1, p_corner2, 2f / 3f);
		v4 = p_corner2;
	}

	public static EdgeVertices TerraceLerp(EdgeVertices p_a, EdgeVertices p_b, int step)
	{
		EdgeVertices result;
		result.v1 = HexMetrics.TerraceLerp (p_a.v1, p_b.v1, step);
		result.v2 = HexMetrics.TerraceLerp (p_a.v2, p_b.v2, step);
		result.v3 = HexMetrics.TerraceLerp (p_a.v3, p_b.v3, step);
		result.v4 = HexMetrics.TerraceLerp (p_a.v4, p_b.v4, step);

		return result;
	}
}
