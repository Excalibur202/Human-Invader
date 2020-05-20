using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void StartLoad()
    {
        StartCoroutine(StartLoading());
    }

    private IEnumerator StartLoading()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("LoadingScreen");
    }

    void QuitButton()
    {
        Application.Quit();
    }
}
