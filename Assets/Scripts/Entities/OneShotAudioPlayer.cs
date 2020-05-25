using UnityEngine;

public class OneShotAudioPlayer : MonoBehaviour {

    [SerializeField] AudioSource audioSource;

    void Delete () {
        Destroy (transform.gameObject);
    }

    public void Play (AudioClip audioClip, float volume = 1, bool positionalAudio = false) {
        audioSource.volume = volume;

        if (positionalAudio)
            audioSource.spatialBlend = 0.5f;
        else
            audioSource.spatialBlend = 0;

        audioSource.PlayOneShot (audioClip);
        Invoke ("Delete", audioClip.length);
    }
}