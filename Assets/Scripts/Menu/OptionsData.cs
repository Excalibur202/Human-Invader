using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class OptionsData 
{
    public float masterVolume, gameVolume, soundVolume, voiceVolume;

    public OptionsData()
    {
        masterVolume = 50f;
        gameVolume = 50f;
        soundVolume = 50f;
        voiceVolume = 50f;
    }

    
    
}
