using UnityEngine;
using System.Collections.Generic;

namespace HexMapGenerator {

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

	#region Variables

	[SerializeField]
	bool _isUsingCollider;

	[SerializeField]
	bool _isUsingColors;

	[SerializeField]
	bool _isUsingUVCoordinates;

	[SerializeField]
	bool _isUsingUV2Coordinates;

	[SerializeField]
	bool _isUsingTerrainTypes;

	List<Vector3> vertices;
	List<Vector3> _terrainTypes;
	List<Color> colors;
	List<Vector2> uvs;
	List<Vector2> uvs2;
	List<int> triangles;


	Mesh hexMesh;
	MeshCollider meshCollider;

	public bool IsUsingCollider {
		get { return _isUsingCollider; }
		set { _isUsingCollider = value; }
	}

	#endregion

	#region Unity_Methods

	void Awake () {
		GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();

		if (_isUsingCollider) {
			meshCollider = gameObject.AddComponent<MeshCollider>();
		}

		hexMesh.name = "Hex Mesh";
	}

	#endregion

	#region Methods

	public void Clear() {
		hexMesh.Clear ();
		vertices = ListPool<Vector3>.Get ();

		if (_isUsingColors) {
			colors = ListPool<Color>.Get ();
		}

		if (_isUsingUVCoordinates) {
			uvs = ListPool<Vector2>.Get ();
		}

		if (_isUsingUV2Coordinates) {
			uvs2 = ListPool<Vector2>.Get ();
		}

		if (_isUsingTerrainTypes) {
			_terrainTypes = ListPool<Vector3>.Get();
		}

		triangles = ListPool<int>.Get ();
	}

	public void Apply() {
		hexMesh.SetVertices (vertices);
		ListPool<Vector3>.Add (vertices);
		hexMesh.SetColors (colors);

		if (_isUsingColors) {
			ListPool<Color>.Add (colors);
		}

		if (_isUsingUVCoordinates) {
			hexMesh.SetUVs (0, uvs);
			ListPool<Vector2>.Add (uvs);
		}

		if (_isUsingUV2Coordinates) {
			hexMesh.SetUVs (0, uvs2);
			ListPool<Vector2>.Add (uvs2);
		}

		if (_isUsingTerrainTypes) {
			hexMesh.SetUVs(2, _terrainTypes);
			ListPool<Vector3>.Add(_terrainTypes);
		}

		hexMesh.SetTriangles (triangles, 0);
		ListPool<int>.Add (triangles);
		hexMesh.RecalculateNormals ();

		if (_isUsingCollider) {
			meshCollider.sharedMesh = hexMesh;
		}
	}

	public void AddTriangle (Vector3 v1, Vector3 v2, Vector3 v3) {
		int vertexIndex = vertices.Count;
		vertices.Add(HexMetrics.Perturb(v1));
		vertices.Add(HexMetrics.Perturb(v2));
		vertices.Add(HexMetrics.Perturb(v3));
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}

	public void AddTriangleTerrainTypes (Vector3 types) {
		_terrainTypes.Add(types);
		_terrainTypes.Add(types);
		_terrainTypes.Add(types);
	}

	public void AddTriangleUnperturbed (Vector3 v1, Vector3 v2, Vector3 v3) {
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}

	public void AddTriangleColor (Color color) {
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
	}

	public void AddTriangleColor (Color c1, Color c2, Color c3) {
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
	}

	public void AddQuad (Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
		int vertexIndex = vertices.Count;
		vertices.Add(HexMetrics.Perturb(v1));
		vertices.Add(HexMetrics.Perturb(v2));
		vertices.Add(HexMetrics.Perturb(v3));
		vertices.Add(HexMetrics.Perturb(v4));
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 3);
	}

	public void AddQuadTerrainTypes (Vector3 types) {
		_terrainTypes.Add(types);
		_terrainTypes.Add(types);
		_terrainTypes.Add(types);
		_terrainTypes.Add(types);
	}

	public void AddQuadUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
		int vertexIndex = vertices.Count;
		vertices.Add (v1);
		vertices.Add (v2);
		vertices.Add (v3);
		vertices.Add (v4);
		triangles.Add (vertexIndex);
		triangles.Add (vertexIndex + 2);
		triangles.Add (vertexIndex + 1);
		triangles.Add (vertexIndex + 1);
		triangles.Add (vertexIndex + 2);
		triangles.Add (vertexIndex + 3);
	}

	public void AddQuadColor (Color color) {
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
	}

	public void AddQuadColor (Color c1, Color c2) {
		colors.Add(c1);
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c2);
	}

	public void AddQuadColor (Color c1, Color c2, Color c3, Color c4) {
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
		colors.Add(c4);
	}

	public void AddTriangleUV (Vector2 uv1, Vector2 uv2, Vector2 uv3) {
		uvs.Add (uv1);
		uvs.Add (uv2);
		uvs.Add (uv3);
	}

	public void AddQuadUV (Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4) {
		uvs.Add (uv1);
		uvs.Add (uv2);
		uvs.Add (uv3);
		uvs.Add (uv4);
	}

	public void AddQuadUV(float uMin, float uMax, float vMin, float vMax) {
		uvs.Add (new Vector2 (uMin, vMin));
		uvs.Add (new Vector2 (uMax, vMin));
		uvs.Add (new Vector2 (uMin, vMax));
		uvs.Add (new Vector2 (uMax, vMax));
	}

	public void AddTriangleUV2 (Vector2 uv1, Vector2 uv2, Vector2 uv3) {
		uvs2.Add (uv1);
		uvs2.Add (uv2);
		uvs2.Add (uv3);
	}

	public void AddQuadUV2 (Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4) {
		uvs2.Add (uv1);
		uvs2.Add (uv2);
		uvs2.Add (uv3);
		uvs2.Add (uv4);
	}

	public void AddQuadUV2(float uMin, float uMax, float vMin, float vMax) {
		uvs2.Add (new Vector2 (uMin, vMin));
		uvs2.Add (new Vector2 (uMax, vMin));
		uvs2.Add (new Vector2 (uMin, vMax));
		uvs2.Add (new Vector2 (uMax, vMax));
	}

	#endregion

}
}