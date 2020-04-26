using System;
using UnityEngine;

public static class UtilityF
{
    // Convert a 3D vector to 2D, losing the Y value
    public static Vector2 V3toV2(Vector3 vector3)
    {
        return (new Vector2(vector3.x, vector3.z));
    }

    // Convert a 2D vector to 3D, with a specified Y value
    public static Vector3 V2toV3(Vector2 vector2, float y)
    {
        return (new Vector3(vector2.x, y, vector2.y));
    }

    // Set a vector's y value
    public static Vector3 V3setY(Vector3 vector3, float y)
    {
        return (new Vector3(vector3.x, y, vector3.z));
    }

    // Efficient distance between two vector3s, returns distance between vectors^2
    public static float sqrDistance(Vector3 a, Vector3 b)
    {
        return (a - b).sqrMagnitude;
    }
}