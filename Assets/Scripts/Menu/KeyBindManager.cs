using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SaveLoad;

public class KeyBindManager : MonoBehaviour
{
    private static KeyBindManager instance;

    public static KeyBindManager MyInstance
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType<KeyBindManager>();
            }

            return instance;
        }
    }

    OptionsManager optionsManager;

    //public Dictionary<string, KeyCode> keysbinds { get; set; }

    public Text forward, back, left, right, jump, attack, aim, ability1, ability2, map;

    private KeyCode keyCode;

    private GameObject currentKey;

    private string bindName;

    private void Start()
    {
        optionsManager = OptionsManager.instance;
    }

    void Update()
    {
       
    }

    void OnGUI()
    {
        if (OptionsManager.instance.data != null)
            GetKeyFromOptions();

        if(currentKey != null)
        {
            Event e = Event.current;

            if(e.isKey)
            {
                optionsManager.data.keysbinds[currentKey.name] = e.keyCode;
                currentKey.transform.GetChild(0).GetComponent<Text>().text = e.keyCode.ToString();
                currentKey = null;
            }
        }
    }

    public void Changekey(GameObject clicked)
    {
        currentKey = clicked;
    }

    public void GetKeyFromOptions()
    {
        forward.text = optionsManager.data.keysbinds["Forward"].ToString();
        back.text = optionsManager.data.keysbinds["Back"].ToString();
        left.text = optionsManager.data.keysbinds["Left"].ToString();
        right.text = optionsManager.data.keysbinds["Right"].ToString();
        jump.text = optionsManager.data.keysbinds["Jump"].ToString();
        attack.text = optionsManager.data.keysbinds["Attack"].ToString();
        aim.text = optionsManager.data.keysbinds["Aim"].ToString();
        ability1.text = optionsManager.data.keysbinds["Ability1"].ToString();
        ability2.text = optionsManager.data.keysbinds["Ability2"].ToString();
        map.text = optionsManager.data.keysbinds["Map"].ToString();
    }

    //public void SetKeyFromOptions()
    //{
    //    OnGUI();
    //}

    //private void LoadOptionsKeys()
    //{
    //    optionsData = optionsData.LoadBinary("Assets\\SettingsData", "SettingsData");
    //    if (optionsData == null)
    //        optionsData = new OptionsData();

    //    GetKeyFromOptions();
    //}

    //public void SaveOptionsKeys()
    //{
    //    SetKeyFromOptions();

    //    optionsData.SaveBinary("Assets\\SettingsData", "SettingsData");
    //}

    //public void BindKey(string key, KeyCode keyBind)
    //{
    //    Dictionary<string, KeyCode> currentdictionary = keysbinds;

    //    if(!currentdictionary.ContainsValue(keyBind))
    //    {
    //        currentdictionary.Add(key, keyBind);
    //    }
    //    else if(currentdictionary.ContainsValue(keyBind))
    //    {
    //        string mykey = currentdictionary.FirstOrDefault(x => x.Value == keyBind).Key;

    //        currentdictionary[mykey] = KeyCode.None;
    //    }

    //    currentdictionary[key] = keyBind;
    //    bindName = string.Empty;
    //}
}
