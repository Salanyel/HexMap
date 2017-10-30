using UnityEngine;

public class HexFeatureManager : MonoBehaviour {

	#region Variables

	[SerializeField]
	HexFeatureCollection[] _urbanCollections;

	[SerializeField]
	HexFeatureCollection[] _farmCollections;

	[SerializeField]
	HexFeatureCollection[] _plantCollections;

	[SerializeField]
	HexMesh _walls;

	Transform _container;

	#endregion

	#region Methods

	public void Clear () {
		if (_container) {
			Destroy(_container.gameObject);
		}
		_container = new GameObject("Features _container").transform;
		_container.SetParent(transform, false);

		_walls.Clear ();
	}

	public void Apply () {
		_walls.Apply ();
	}

	Transform PickPrefab (
		HexFeatureCollection[] collection,
		int level, float hash, float choice
	) {
		if (level > 0) {
			float[] thresholds = HexMetrics.GetFeatureThresholds(level - 1);
			for (int i = 0; i < thresholds.Length; i++) {
				if (hash < thresholds[i]) {
					return collection[i].Pick(choice);
				}
			}
		}
		return null;
	}

	public void AddFeature (HexCell cell, Vector3 position) {
		HexHash hash = HexMetrics.SampleHashGrid(position);
		Transform prefab = PickPrefab(
			_urbanCollections, cell.UrbanLevel, hash._a, hash._d
		);
		Transform otherPrefab = PickPrefab(
			_farmCollections, cell.FarmLevel, hash._b, hash._d
		);
		float usedHash = hash._a;
		if (prefab) {
			if (otherPrefab && hash._b < hash._a) {
				prefab = otherPrefab;
				usedHash = hash._b;
			}
		}
		else if (otherPrefab) {
			prefab = otherPrefab;
			usedHash = hash._b;
		}
		otherPrefab = PickPrefab(
			_plantCollections, cell.PlantLevel, hash._c, hash._d
		);
		if (prefab) {
			if (otherPrefab && hash._c < usedHash) {
				prefab = otherPrefab;
			}
		}
		else if (otherPrefab) {
			prefab = otherPrefab;
		}
		else {
			return;
		}

		Transform instance = Instantiate(prefab);
		position.y += instance.localScale.y * 0.5f;
		instance.localPosition = HexMetrics.Perturb(position);
		instance.localRotation = Quaternion.Euler(0f, 360f * hash._e, 0f);
		instance.SetParent(_container, false);
	}

	public void AddWall(EdgeVertices p_near, HexCell p_nearCell, EdgeVertices p_far, HexCell p_farCell) {
		if (p_nearCell.Walled != p_farCell.Walled) {
			AddWallSegment (p_near.v1, p_far.v1, p_near.v5, p_far.v5);
		}
	}

	public void AddWall(Vector3 p_c1, HexCell p_cell1, Vector3 p_c2, HexCell p_cell2, Vector3 p_c3, HexCell p_cell3) {
		if (p_cell1.Walled) {
			if (p_cell2.Walled) {
				if (!p_cell3.Walled) {
					AddWallSegment (p_c3, p_cell3, p_c1, p_cell1, p_c2, p_cell2);
				}
			} else if (p_cell3.Walled) {
				AddWallSegment (p_c2, p_cell2, p_c3, p_cell3, p_c1, p_cell1);
			} else {
				AddWallSegment (p_c1, p_cell1, p_c2, p_cell2, p_c3, p_cell3);
			}
		} else if (p_cell2.Walled) {
			if (p_cell3.Walled) {
				AddWallSegment (p_c1, p_cell1, p_c2, p_cell2, p_c3, p_cell3);
			} else {
				AddWallSegment (p_c2, p_cell2, p_c3, p_cell3, p_c1, p_cell1);
			} 
		} else if (p_cell3.Walled) {
			AddWallSegment (p_c3, p_cell3, p_c1, p_cell1, p_c2, p_cell2);
		}
	}

	void AddWallSegment(Vector3 p_nearLeft, Vector3 p_farLeft, Vector3 p_nearRight, Vector3 p_farRight) {
		Vector3 v1, v2, v3, v4;
		Vector3 left = Vector3.Lerp (p_nearLeft, p_farLeft, 0.5f);
		Vector3 right = Vector3.Lerp (p_nearRight, p_farRight, 0.5f);

		Vector3 leftThicknessOffset = HexMetrics.wallThicknessOffset (p_nearLeft, p_farLeft);
		Vector3 rightThicknessOffset = HexMetrics.wallThicknessOffset (p_nearRight, p_farRight);

		float leftTop = left.y + HexMetrics._wallHeight;
		float rightTop = right.y + HexMetrics._wallHeight;

		v1 = v3 = left - leftThicknessOffset;
		v2 = v4 = right - rightThicknessOffset;
		v3.y = leftTop;
		v4.y = rightTop;
		_walls.AddQuad(v1, v2, v3, v4);

		Vector3 t1 = v3, t2 = v4;

		v1 = v3 = left + leftThicknessOffset;
		v2 = v4 = right + rightThicknessOffset;
		v3.y = leftTop;
		v4.y = rightTop;
		_walls.AddQuad(v2, v1, v4, v3);

		_walls.AddQuad(t1, t2, v3, v4);
	}

	void AddWallSegment(Vector3 p_pivot, HexCell p_pivotCell, Vector3 p_left, HexCell p_leftCell, Vector3 p_right, HexCell p_rightCell) {
		AddWallSegment (p_pivot, p_left, p_pivot, p_right);
	}

	#endregion
}