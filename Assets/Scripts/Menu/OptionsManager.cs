using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveLoad;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager instance;
    public OptionsData data = new OptionsData();
    
    private void Awake()
    {
        if (instance)
        {
            Destroy(this);
        }
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        data = data.LoadBinary("Assets\\SettingsData", "SettingsData");
    }

    public void SaveOptions()
    {
        data.SaveBinary("Assets\\SettingsData", "SettingsData");
    }

}
