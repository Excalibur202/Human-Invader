using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Animator mainMenu;
    public Animator titleMenu;

    void Start()
    {
        mainMenu.enabled = true;
        titleMenu.enabled = true;

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

    public void QuitButton()
    {
        Application.Quit();
    }
}
