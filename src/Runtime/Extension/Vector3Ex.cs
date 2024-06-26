using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Ex
{
	public static Vector3 ClampZ(this Vector3 val, float min, float max)
		=> new Vector3(val.x, val.y, Mathf.Clamp(val.z, min, max));

    public static bool IsSameDirectionAs(this Vector3 vector, Vector3 other)
    {
        return Mathf.Approximately(Vector3.Dot(vector.normalized, other.normalized), 1);
    }
}
