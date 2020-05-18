using UnityEngine;

public class Keycard_Pickup : MonoBehaviour {

    AudioClip pickupSound;

    void OnTriggerEnter (Collider other) {
        Util.GetPlayer ().GetComponent<PlayerAbilityController> ().keycards++;
        Util.PlaySound (pickupSound);
        Destroy (gameObject);
    }
}