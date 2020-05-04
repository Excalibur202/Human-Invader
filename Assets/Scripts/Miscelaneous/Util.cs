using UnityEngine;

public static class Util {
    // Convert a 3D vector to 2D, dropping the Y value
    public static Vector2 V3toV2 (Vector3 vector3) {
        return (new Vector2 (vector3.x, vector3.z));
    }

    // Convert a 2D vector to 3D, with a specifiable Y value
    public static Vector3 V2toV3 (Vector2 vector2, float y = 0) {
        return (new Vector3 (vector2.x, y, vector2.y));
    }

    // Set a 3D vector's y value
    public static Vector3 V3setY (Vector3 vector3, float y) {
        return (new Vector3 (vector3.x, y, vector3.z));
    }

    // Because its nicer to the eye than arbitrary multiplications
    public static float Square (float a) { return (a * a); }
    public static int SquareOf (int a) { return (a * a); }

    // Makes the objective of getting the SqrMagnitude from the subtraction of two vectors more clear
    public static float SqrDistance (Vector2 v1, Vector2 v2) { return Vector2.SqrMagnitude (v1 - v2); }
    public static float SqrDistance (Vector3 v1, Vector3 v2, bool ignoreY = false) { return ignoreY ? Vector2.SqrMagnitude (Util.V3toV2 (v1 - v2)) : Vector3.SqrMagnitude (v1 - v2); }

    // Cleaner physics raycast between two vectors
    public static RaycastHit RayFromTo (Vector3 from, Vector3 to, LayerMask layerMask, float rayLength = 0) {
        Ray ray = new Ray (from, to);

        if (rayLength == 0)
            rayLength = Vector3.Distance (from, to);

        RaycastHit raycastHit;
        Physics.Raycast (ray, out raycastHit, rayLength, layerMask);

        return raycastHit;
    }
    // Cleaner physics raycast between two 2D vectors which are converted to 3D vectors of a set Y value 
    public static RaycastHit RayFromTo (Vector2 from, Vector2 to, float height, LayerMask layerMask, float rayLength = 0) {
        Vector3 fromV3 = Util.V2toV3 (from, height);
        Vector3 toV3 = Util.V2toV3 (to, height);

        if (rayLength == 0)
            rayLength = Vector2.Distance (from, to);

        return RayFromTo(fromV3, toV3, layerMask, rayLength);
    }
}