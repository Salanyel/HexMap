using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexMetrics {
	public const float _outerRadius = 10f;
	public const float _innerRadius = _outerRadius * 0.866025404f;

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
}