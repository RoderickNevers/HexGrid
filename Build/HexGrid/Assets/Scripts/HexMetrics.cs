using System;
using System.Collections.Generic;

using UnityEngine;

public static class HexMetrics
{
	public readonly static float outerRadius = 10f;
	public readonly static float innerRadius = outerRadius * Mathf.Sqrt(3) / 2;

	public readonly static List<Vector3> corners = new()
	{
		new Vector3(0f, 0f, outerRadius),
		new Vector3(innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(0f, 0f, -outerRadius),
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
	};

    public static Vector3 GetFirstCorner(HexDirection direction)
    {
        return corners[(int)direction];
    }

    public static Vector3 GetSecondCorner(HexDirection direction)
    {
        return corners[(int)direction + 1];
    }
}