using UnityEngine;

public class Keycard_Pickup : MonoBehaviour {

    [SerializeField] AudioClip pickupSound;

    void OnTriggerEnter (Collider other) {
        if (other.tag == "Player") {
            Util.GetPlayer ().GetComponent<PlayerSpecialInputsController> ().keycards++;
            Util.PlaySound (pickupSound);
            Destroy (gameObject);
        }
    }
}