using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Audio;
using SaveLoad;

public class SoundMenu : MonoBehaviour
{
    OptionsData optionsData;
    public bool testSave = false;
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
        LoadOptionsData();
    }

    void Update()
    {
        ValuesChange(masterVolume.value,gameVolume.value,soundVolume.value,voiceVolume.value);

        if(testSave)
        {
            SaveOptionsData();
            testSave = false;
        }
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

    private void GetValuesFromOptionsData()
    {
        masterVolume.value = optionsData.masterVolume;
        gameVolume.value = optionsData.gameVolume;
        soundVolume.value = optionsData.soundVolume;
        voiceVolume.value = optionsData.voiceVolume;
    }

    private void SetValuesFromOptionsData()
    {
        optionsData.masterVolume = masterVolume.value;
        optionsData.gameVolume = gameVolume.value;
        optionsData.soundVolume = soundVolume.value;
        optionsData.voiceVolume = voiceVolume.value;
    }

    private void LoadOptionsData()
    {
        optionsData = optionsData.LoadBinary("Assets\\SettingsData", "SettingsData");
        if (optionsData == null)
            optionsData = new OptionsData();

        GetValuesFromOptionsData();
        
    }

    public void SaveOptionsData()
    {
        SetValuesFromOptionsData();

        optionsData.SaveBinary("Assets\\SettingsData", "SettingsData");
    }

    //public void SetVolume(float sliderValue)
    //{
    //    audioMixer.SetFloat("Master", (((sliderValue * 0.01f) - 1) * 80));
    //}
}
