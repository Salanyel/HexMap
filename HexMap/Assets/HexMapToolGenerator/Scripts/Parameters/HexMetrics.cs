using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMapGenerator {

public enum ENUM_HexEdgeType {
	Flat, 
	Slope, 
	Cliff
}

public static class HexMetrics {

    #region Variables

	public const float _outerRadius = 10f;
	public const float _innerRadius = _outerRadius * _outerToInner;

	public const float _elevationStep = 3f;
	public const int _terracesPerSlope = 2;
	public const int _terraceSteps = _terracesPerSlope * 2 + 1;
	public const float _horizontalTerraceStepSize = 1f / _terraceSteps;
	public const float _verticalTerraceStepSize = 1f / (_terracesPerSlope + 1);

	public const float _solidFactor = 0.82f;
	public const float _blendFactor = 1 - _solidFactor;

	public const float _waterFactor = 0.6f;
	public const float _waterBlendFactor = 1f - _waterFactor;
	public const float _cellPerturbStrengh = 4f;
	public const float _cellPerturbElevation = 1.5f;
	public const float _noiseScale = 0.003f;
	public static Texture2D _noiseSource;

	public const float _wallHeight = 3f;
	public const float _wallThickness = 0.75f;
	public const float _wallElevationOffset = _verticalTerraceStepSize;

	public const float _bridgeDesignLength = 7f;

	public const int _chunkSizeX = 5;
	public const int _chunkSizeZ = 5;

	public const float _streamBedElevationOffset = -1.75f;
	public const float _outerToInner = 0.866025404f;
	public const float _innerToOuter = 1f / _outerToInner;
	public const float _waterElevationOffset = -0.5f;
	 
	public const int _hashGridSize = 256;
	public const float _hashGridScale = 0.25f;

	static float[][] _featureThresholds = {
		new float[] { 0.0f, 0.0f, 0.4f },
		new float[] { 0.0f, 0.4f, 0.6f },
		new float[] { 0.4f, 0.6f, 0.8f }
	};
		
	static Vector3[] _corners = {
		new Vector3 (0f, 0f, _outerRadius),
		new Vector3 (_innerRadius, 0f, 0.5f * _outerRadius),
		new Vector3 (_innerRadius, 0f, -0.5f * _outerRadius),
		new Vector3 (0f, 0f, -_outerRadius),
		new Vector3 (-_innerRadius, 0f, -0.5f * _outerRadius),
		new Vector3 (-_innerRadius, 0f, 0.5f * _outerRadius),
		new Vector3 (0f, 0f, _outerRadius),
	};

	static HexHash[] _hashGrid;

	#endregion

	#region Methods

	public static Vector3 GetFirstCorner(ENUM_HexDirection p__direction) {
		return _corners [(int)p__direction];
	}

	public static Vector3 GetSecondCorner(ENUM_HexDirection p__direction) {
		return _corners [(int)p__direction + 1];
	}

	public static Vector3 GetFirstSolidCorner(ENUM_HexDirection p__direction) {
		return _corners [(int)p__direction] * _solidFactor;
	}

	public static Vector3 GetSecondSolidCorner(ENUM_HexDirection p__direction) {
		return _corners [(int)p__direction + 1] * _solidFactor;
	}

	public static Vector3 GetFirstWaterCorner(ENUM_HexDirection p_direction) {
		return _corners [(int)p_direction] * _waterFactor;
	}
	public static Vector3 GetSecondWaterCorner(ENUM_HexDirection p_direction) {
		return _corners [(int)p_direction + 1] * _waterFactor;
	}

	public static Vector3 GetBridge(ENUM_HexDirection p__direction) {
		return (_corners[(int)p__direction] + _corners[(int) p__direction + 1]) * _blendFactor;
	}

	public static Vector3 GetWaterBridge (ENUM_HexDirection p_direction) {
		return (_corners [(int)p_direction] + _corners [(int)p_direction + 1]) * _waterBlendFactor;
	}

	public static Vector3 TerraceLerp(Vector3 p_a, Vector3 p_b, int p_step) {
		float h = p_step * HexMetrics._horizontalTerraceStepSize;
		float v = ((p_step + 1) / 2) * HexMetrics._verticalTerraceStepSize;

		p_a.x += (p_b.x - p_a.x) * h;
		p_a.y += (p_b.y - p_a.y) * v;
		p_a.z += (p_b.z - p_a.z) * h;

		return p_a;
	}

	public static Color TerraceLerp(Color p_a, Color p_b, int p_step) {
		float h = p_step * HexMetrics._horizontalTerraceStepSize;
		return Color.Lerp (p_a, p_b, h);
	}

	public static ENUM_HexEdgeType GetEdgeType (int p_elevation1, int p_elevation2) {
		if (p_elevation1 == p_elevation2) {
			return ENUM_HexEdgeType.Flat;
		}

		int delta = p_elevation1 - p_elevation2;
		if (delta == -1 || delta == 1) {
			return ENUM_HexEdgeType.Slope;
		}

		return ENUM_HexEdgeType.Cliff;
	}

	public static Vector4 SampleNoise(Vector3 p_position) {
		return _noiseSource.GetPixelBilinear (p_position.x * HexMetrics._noiseScale, p_position.z * HexMetrics._noiseScale);
	}

	public static Vector3 GetSolidEdgeMiddle(ENUM_HexDirection p_direction) {
		return
			(_corners [(int)p_direction] + _corners [(int)p_direction + 1]) *
		(0.5f * _solidFactor);
	}

	public static Vector3 Perturb (Vector3 p_position) {
		Vector4 sample = SampleNoise(p_position);
		p_position.x += (sample.x * 2f - 1f) * _cellPerturbStrengh;
		p_position.z += (sample.z * 2f - 1f) * _cellPerturbStrengh;
		return p_position;
	}

	public static void InitializeHasGrid(int p_seed) {
		_hashGrid = new HexHash[_hashGridSize * _hashGridSize];

		Random.State currentState = Random.state;
		Random.InitState (p_seed);

		for (int i = 0; i < _hashGrid.Length; ++i) {
			_hashGrid [i] = HexHash.Create();
		}
		Random.state = currentState;
	}

	public static HexHash SampleHashGrid(Vector3 p_position) {
		int x = (int)(p_position.x * _hashGridScale) % _hashGridSize;
		if (x < 0) {
			x += _hashGridSize;
		}

		int z = (int)(p_position.z * _hashGridScale) % _hashGridSize;
		if (z < 0) {
			z += _hashGridSize;
		}

		return _hashGrid [x + z * _hashGridSize];
	}

	public static float[] GetFeatureThresholds(int p_level) {
		return _featureThresholds[p_level];
	}

	public static Vector3 wallThicknessOffset(Vector3 p_near, Vector3 p_far) {
		Vector3 offset;
		offset.x = p_far.x - p_near.x;
		offset.y = 0f;
		offset.z = p_far.z - p_near.z;
		return offset.normalized * (_wallThickness * 0.5f);
	}

	public static Vector3 WallLerp(Vector3 p_near, Vector3 p_far) {
		p_near.x += (p_far.x - p_near.x) * 0.5f;
		p_near.z += (p_far.z - p_near.z) * 0.5f;

		float v = p_near.y < p_far.y ? _wallElevationOffset : (1f - _wallElevationOffset);
		p_near.y += (p_far.y - p_near.y) * v;

		return p_near;
	}

	#endregion
}
}