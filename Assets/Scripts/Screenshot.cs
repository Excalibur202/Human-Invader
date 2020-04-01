using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Screenshot : MonoBehaviour
{
    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Insert))
        {
            string date = System.DateTime.Now.ToString();
            date = date.Replace("/", "-");
            date = date.Replace(" ", "_");
            date = date.Replace(":", "-");
            ScreenCapture.CaptureScreenshot("C:\\Users\\Ronaldo\\Pictures\\"+date+".png");
        }
    }
}
