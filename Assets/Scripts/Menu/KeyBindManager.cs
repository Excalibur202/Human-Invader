using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public Dictionary<string, KeyCode> keysbinds { get; set; }
    
    public Text forward, back, left, right, jump, attack, aim, ability1, ability2, map;

    private KeyCode keyCode;

    private GameObject currentKey;

    private string bindName;

    private void Start()
    {

        keysbinds = new Dictionary<string, KeyCode>();
        

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

        forward.text = keysbinds["Forward"].ToString();
        back.text = keysbinds["Back"].ToString();
        left.text = keysbinds["Left"].ToString();
        right.text = keysbinds["Right"].ToString();
        jump.text = keysbinds["Jump"].ToString();
        attack.text = keysbinds["Attack"].ToString();
        aim.text = keysbinds["Aim"].ToString();
        ability1.text = keysbinds["Ability1"].ToString();
        ability2.text = keysbinds["Ability2"].ToString();
        map.text = keysbinds["Map"].ToString();
    }

    void OnGUI()
    {
        if(currentKey != null)
        {
            Event e = Event.current;

            if(e.isKey)
            {
                keysbinds[currentKey.name] = e.keyCode;
                currentKey.transform.GetChild(0).GetComponent<Text>().text = e.keyCode.ToString();
                currentKey = null;
            }
        }
    }

    public void Changekey(GameObject clicked)
    {
        currentKey = clicked;
    }

    public void BindKey(string key, KeyCode keyBind)
    {
        Dictionary<string, KeyCode> currentdictionary = keysbinds;

        if(!currentdictionary.ContainsValue(keyBind))
        {
            currentdictionary.Add(key, keyBind);
        }
        else if(currentdictionary.ContainsValue(keyBind))
        {
            string mykey = currentdictionary.FirstOrDefault(x => x.Value == keyBind).Key;

            currentdictionary[mykey] = KeyCode.None;
        }

        currentdictionary[key] = keyBind;
        bindName = string.Empty;
    }
}
