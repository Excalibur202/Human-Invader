using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TerminalController tc;

    // Update is called once per frame
    void Update()
    {
        if (tc.input.text.Length > 0 && Input.GetKey(KeyCode.Return))
        {
            tc.ParseInput();
        }
    }
}