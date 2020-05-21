using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
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

    public void GoDie(){
        SceneManager.LoadScene("Menu");
    }
}
