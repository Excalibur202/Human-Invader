using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public void StartLoad()
    {
        SceneManager.LoadScene("Luis");
    }

    public void ExitPause()
    {
        Time.timeScale = 1;
    }

    private IEnumerator StartLoading()
    {
        yield return new WaitForSeconds(0.5f);
        
    }

}
