using UnityEngine;

public class OneShotAudioPlayer : MonoBehaviour {

    [SerializeField] AudioSource audioSource;

    void Delete () {
        Destroy (transform.gameObject);
    }

    public void Play (AudioClip audioClip) {
        audioSource.PlayOneShot (audioClip);
        Invoke ("Delete", audioClip.length);
    }

}