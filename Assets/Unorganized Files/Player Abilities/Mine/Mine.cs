using UnityEngine;

public class Mine : MonoBehaviour {
    Vector3 finalPosition;
    bool landed;
    Quaternion finalRotation;
    Rigidbody finalBody;
    float lifetime;

    public void SetDestination (Rigidbody rigidbody, Vector3 position, Quaternion rotation) {
        finalBody = rigidbody;
        finalPosition = position;
        finalRotation = rotation;
    }

    void FixedUpdate () {
        lifetime += Time.fixedDeltaTime;

        if (!landed) {
            float distance = Vector3.Distance (finalPosition, transform.position);
            if (distance < 1f) {
                landed = true;

                transform.position = finalPosition;
                transform.rotation = finalRotation;

                var fj = gameObject.AddComponent<FixedJoint> ();
                fj.connectedBody = finalBody;
            } else {
                transform.Rotate (new Vector3 (1000 * Time.fixedDeltaTime, 0, 0));

                if (lifetime > 15)
                    Destroy (transform.root.gameObject);
            }
        }
    }
}