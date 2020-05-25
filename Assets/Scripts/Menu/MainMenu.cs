using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    //public void StartLoad()
    //{
    //    //StartCoroutine(StartLoading());
    //}

    public void StartLoad()
    {
        SceneManager.LoadScene("LoadingScreen");
    }

    void QuitButton()
    {
        Application.Quit();
    }
}
