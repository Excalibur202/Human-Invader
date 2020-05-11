using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SoundMenu : MonoBehaviour
{
    public InputField gameValue;
    public InputField masterValue;
    public InputField soundValue;
    public InputField voiceValue;
    
    public Slider gameVolume;
    public Slider masterVolume;
    public Slider soundVolume;
    public Slider voiceVolume;
   
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

    }

    
}
