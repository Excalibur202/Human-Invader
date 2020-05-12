using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour {
    [SerializeField] float damage;
    [SerializeField] Collider col;

    Queue<Collider> ignoredColliders = new Queue<Collider> ();

    void Start () {
        if (!col)
            col = transform.GetComponent<Collider> ();

        if (!col) {
            print ("ERROR! HURTBOX SCRIPT DID NOT FIND A COLLIDER");
            transform.gameObject.SetActive (false);
        }
    }

    void OnTriggerEnter (Collider other) {

        // Damage if object has a health controller
        HealthController healthController = other.GetComponent<HealthController> ();
        if (healthController != null) {
            healthController.Damage (damage);
        }

        // Ignore that collider from now on, attacks should only hit once
        Physics.IgnoreCollision (other, col, true);
        ignoredColliders.Enqueue (other);
    }

    // Needs to be called when the attack ends
    public void Reset () {

        // Roll back ignored collisions
        while (ignoredColliders.Count > 0) {
            Physics.IgnoreCollision (ignoredColliders.Dequeue (), col, false);
        }
    }
}