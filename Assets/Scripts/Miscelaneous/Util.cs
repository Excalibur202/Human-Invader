using UnityEngine;

public static class Util {
    // Convert a 3D vector to 2D, dropping the Y value
    public static Vector2 V3toV2 (Vector3 vector3) {
        return (new Vector2 (vector3.x, vector3.z));
    }

    // Convert a 2D vector to 3D, with a specified Y value
    public static Vector3 V2toV3 (Vector2 vector2, float y) {
        return (new Vector3 (vector2.x, y, vector2.y));
    }

    // Set a vector's y value
    public static Vector3 V3setY (Vector3 vector3, float y) {
        return (new Vector3 (vector3.x, y, vector3.z));
    }

    // Because its nicer to the eye than arbitrary multiplications
    public static float Square (float a) { return (a * a); }
    public static int SquareOf (int a) { return (a * a); }

    // Makes the objective of getting the SqrMagnitude from the subtraction of two vectors more clear
    public static float SqrDistance (Vector2 v1, Vector2 v2) { return Vector2.SqrMagnitude (v1 - v2); }
    public static float SqrDistance (Vector3 v1, Vector3 v2, bool ignoreY = false) { return ignoreY ? Vector2.SqrMagnitude (Util.V3toV2 (v1 - v2)) : Vector3.SqrMagnitude (v1 - v2); }
}