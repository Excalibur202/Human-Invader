using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class OptionsData 
{
    public float masterVolume, gameVolume, soundVolume, voiceVolume;

    public Dictionary<string, KeyCode> keysbinds = new Dictionary<string, KeyCode>();

    public OptionsData()
    {
        masterVolume = 50f;
        gameVolume = 50f;
        soundVolume = 50f;
        voiceVolume = 50f;
        BindKey("Forward", KeyCode.W);
        BindKey("Back", KeyCode.S);
        BindKey("Left", KeyCode.A);
        BindKey("Right", KeyCode.D);
        BindKey("Jump", KeyCode.Space);
        BindKey("Attack", KeyCode.Mouse0);
        BindKey("Aim", KeyCode.Mouse1);
        BindKey("Ability1", KeyCode.F);
        BindKey("Ability2", KeyCode.Q);
        BindKey("Map", KeyCode.M);
        BindKey("Pause", KeyCode.Escape);
    }

    public void BindKey(string key, KeyCode keyBind)
    {
        Dictionary<string, KeyCode> currentdictionary = keysbinds;

        if (!currentdictionary.ContainsValue(keyBind))
        {
            currentdictionary.Add(key, keyBind);
        }
        else if (currentdictionary.ContainsValue(keyBind))
        {
            string mykey = currentdictionary.FirstOrDefault(x => x.Value == keyBind).Key;

            currentdictionary[mykey] = KeyCode.None;
        }

        currentdictionary[key] = keyBind;
    }

}
