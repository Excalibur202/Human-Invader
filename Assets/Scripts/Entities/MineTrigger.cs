using UnityEngine;

public class MineTrigger : MonoBehaviour {
    [SerializeField] Mine mine;

    private void OnTriggerEnter (Collider other) {
        mine.Trigger ();
    }
}