using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Audio;
public class SoundMenu : MonoBehaviour
{
    [Header("Game")]
    public Slider gameVolume;
    public InputField gameValue;
    [Header("Master")]
    public Slider masterVolume;
    public InputField masterValue;
    [Header("Sound Effect")]
    public Slider soundVolume;
    public InputField soundValue;
    [Header("Voice")]
    public Slider voiceVolume;
    public InputField voiceValue;
    [Header("Mixer")]
    public AudioMixer audioMixer;
    [Header("Disable Sound")]
    public Toggle disable;

    void Start()
    {
        masterVolume.value = 100f;
        gameVolume.value = 100f;
        soundVolume.value = 100f;
        voiceVolume.value = 100f;
    }

    void Update()
    {
        ValuesChange(masterVolume.value,gameVolume.value,soundVolume.value,voiceVolume.value);
    }

    public void ValuesChange(float masterCount, float gameCount, float soundCount, float voiceCount)
    {
        masterValue.text = masterCount.ToString();
        gameValue.text = gameCount.ToString();
        soundValue.text = soundCount.ToString();
        voiceValue.text = voiceCount.ToString();

        audioMixer.SetFloat("Master", (((masterCount * 0.01f) - 1) * 80));
        audioMixer.SetFloat("Game", (((gameCount * 0.01f) - 1) * 80));
        audioMixer.SetFloat("Effect", (((soundCount * 0.01f) - 1) * 80));
        audioMixer.SetFloat("Voice", (((voiceCount * 0.01f) - 1) * 80));

        if (disable.isOn)
            AudioListener.volume = 0f;
        else
            AudioListener.volume = 1f;
    }

    //public void SetVolume(float sliderValue)
    //{
    //    audioMixer.SetFloat("Master", (((sliderValue * 0.01f) - 1) * 80));
    //}
}
