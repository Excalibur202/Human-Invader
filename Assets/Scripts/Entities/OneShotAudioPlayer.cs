using UnityEngine;

public class OneShotAudioPlayer : MonoBehaviour {

    GameObject player;
    [SerializeField] AudioSource audioSource;
    AudioClip audioClip;
    float volume;
    bool positionalAudio;
    bool looping;

    void Delete () {
        if (looping)
        {
            Util.PlaySound(audioClip, transform.position, volume, looping);
        }
        Destroy (transform.gameObject);
    }

    void Update()
    {
        if (looping)
            transform.position = player.transform.position;
    }

    public void Play (AudioClip audioClip, float volume = 1, bool positionalAudio = false, bool looping = false) {
        audioSource.volume = volume;
        this.audioClip = audioClip;
        this.volume = volume;
        this.positionalAudio = positionalAudio;
        this.looping = looping;
        player = Util.GetPlayer();

        if (positionalAudio)
            audioSource.spatialBlend = 0.5f;
        else
            audioSource.spatialBlend = 0;

        audioSource.PlayOneShot (audioClip);
        Invoke ("Delete", audioClip.length);
    }
}