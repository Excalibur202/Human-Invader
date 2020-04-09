using UnityEngine;

public class Mine : MonoBehaviour {
    Collider col;

    // Start is called before the first frame update
    void Start () {
        col = (Collider) GetComponent (typeof (Collider));
    }

    void FixedUpdate () {
        if (col) // hasn't landed yet
            transform.Rotate (new Vector3 (1000 * Time.fixedDeltaTime, 0, 0));
    }

    // On touching an obstacle
    void OnCollisionEnter (Collision collision) {
        Destroy (col);

        Vector3 closestPoint = collision.collider.ClosestPoint (transform.position);

        RaycastHit raycastHit;
        Physics.Raycast (transform.position, closestPoint - transform.position, out raycastHit, 2, LayerMask.GetMask ("Obstacle"));

        // Set mine to the closest point on the obstacle and facing away from it
        transform.position = closestPoint;
        transform.rotation = Quaternion.LookRotation (raycastHit.normal);
        transform.Rotate (new Vector3 (90, 0, 0));

        var joint = gameObject.AddComponent<FixedJoint> ();
        joint.connectedBody = collision.rigidbody;
    }
}