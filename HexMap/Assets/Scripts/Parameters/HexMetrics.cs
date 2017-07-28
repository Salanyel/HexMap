using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexMetrics {
	public const float _outerRadius = 10f;
	public const float _innerRadius = _outerRadius * 0.866025404f;

	public const float _solidFactor = 0.75f;
	public const float _blendFactor = 1 - _solidFactor;

	static Vector3[] _corners = {
		new Vector3 (0f, 0f, _outerRadius),
		new Vector3 (_innerRadius, 0f, 0.5f * _outerRadius),
		new Vector3 (_innerRadius, 0f, -0.5f * _outerRadius),
		new Vector3 (0f, 0f, -_outerRadius),
		new Vector3 (-_innerRadius, 0f, -0.5f * _outerRadius),
		new Vector3 (-_innerRadius, 0f, 0.5f * _outerRadius),
		new Vector3 (0f, 0f, _outerRadius),
	};

	public static Vector3 GetFirstCorner(ENUM_HexDirection p_direction) {
		return _corners [(int)p_direction];
	}

	public static Vector3 GetSecondCorner(ENUM_HexDirection p_direction) {
		return _corners [(int)p_direction + 1];
	}

	public static Vector3 GetFirstSolidCorner(ENUM_HexDirection p_direction) {
		return _corners [(int)p_direction] * _solidFactor;
	}

	public static Vector3 GetSecondSolidCorner(ENUM_HexDirection p_direction) {
		return _corners [(int)p_direction + 1] * _solidFactor;
	}

	public static Vector3 GetBridge(ENUM_HexDirection p_direction) {
		return (_corners[(int)p_direction] + _corners[(int) p_direction + 1]) * 0.5f * _blendFactor;
	}
}