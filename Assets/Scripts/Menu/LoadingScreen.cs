using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum ProgTrack
{
    MapGeneration = 0,
    SectorAttribution = 1
}

public static class ProgressTracker
{ 
    public static bool[] progressGeneral = new bool[2];
    
    public static float GetProgress()
    {
        int trueCount = 0;
        
        foreach(bool pro in progressGeneral)
            if(pro == true)
                trueCount++;

        return ((float)trueCount / progressGeneral.Length);
    }
}

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    public Slider gameProgress;
    public Text progressText;

    void Start()
    {
        StartCoroutine(LoadAsyncOperation());
    }

    IEnumerator LoadAsyncOperation()
    {
        AsyncOperation gamelevel = SceneManager.LoadSceneAsync("Game");
        gamelevel.allowSceneActivation = false;

        while (!gamelevel.isDone)
        {
            float progress = Mathf.Clamp01(gamelevel.progress / .9f);

            gameProgress.value = progress;

            progressText.text = progress * 100f + "%";
            //Debug.Log("" + o);
           
            if (progress == 1)
            {
                //for (int pro = 0; pro < ProgressTracker.progressGeneral.Length; pro++)
                //    ProgressTracker.progressGeneral[pro] = false;

                gamelevel.allowSceneActivation = true;

            }

            yield return new WaitForEndOfFrame();
        }
    }
}
