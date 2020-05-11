using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyBindManager : MonoBehaviour
{
    private Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();

    public Text forward, back, left, right, jump, attack, ability1, ability2;

    private GameObject currentKey;

    private void Start()
    {
        keys.Add("Forward", KeyCode.W);
        keys.Add("Back", KeyCode.S);
        keys.Add("Left", KeyCode.A);
        keys.Add("Right", KeyCode.D);
        keys.Add("Jump", KeyCode.Space);
        keys.Add("Attack", KeyCode.Mouse0);
        keys.Add("Ability1", KeyCode.F);
        keys.Add("Ability2", KeyCode.Q);
    }

    void Update()
    {
        if(Input.GetKeyDown(keys["Forward"]))
        {

        }
        if (Input.GetKeyDown(keys["Back"]))
        {

        }
        if (Input.GetKeyDown(keys["Left"]))
        {

        }
        if (Input.GetKeyDown(keys["Right"]))
        {

        }
        if (Input.GetKeyDown(keys["Jump"]))
        {

        }
        if (Input.GetKeyDown(keys["Attack"]))
        {

        }
        if (Input.GetKeyDown(keys["Ability1"]))
        {

        }
        if (Input.GetKeyDown(keys["Ability2"]))
        {

        }
    }
}
