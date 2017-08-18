﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ENUM_HexEdgeType {
	Flat, 
	Slope, 
	Cliff
}

public static class HexMetrics {

	#region Variables

	public const float _outerRadius = 10f;
	public const float _innerRadius = _outerRadius * 0.866025404f;

	public const float _elevationStep = 3f;
	public const int _terracesPerSlope = 2;
	public const int _terraceSteps = _terracesPerSlope * 2 + 1;
	public const float _horizontalTerraceStepSize = 1f / _terraceSteps;
	public const float _verticalTerraceStepSize = 1f / (_terracesPerSlope + 1);

	public const float _solidFactor = 0.82f;
	public const float _blendFactor = 1 - _solidFactor;

	public const float _cellPerturbStrengh = 0f;//3f;
	public const float _cellPerturbElevation = 1.5f;
	public const float _noiseScale = 0.003f;
	public static Texture2D _noiseSource;

	public const int _chunkSizeX = 5;
	public const int _chunkSizeZ = 5;

	public const float _streamBedElevationOffset = -1f;

	static Vector3[] _corners = {
		new Vector3 (0f, 0f, _outerRadius),
		new Vector3 (_innerRadius, 0f, 0.5f * _outerRadius),
		new Vector3 (_innerRadius, 0f, -0.5f * _outerRadius),
		new Vector3 (0f, 0f, -_outerRadius),
		new Vector3 (-_innerRadius, 0f, -0.5f * _outerRadius),
		new Vector3 (-_innerRadius, 0f, 0.5f * _outerRadius),
		new Vector3 (0f, 0f, _outerRadius),
	};

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

	public static Vector3 GetBridge(ENUM_HexDirection p__direction) {
		return (_corners[(int)p__direction] + _corners[(int) p__direction + 1]) * _blendFactor;
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

	#endregion
}