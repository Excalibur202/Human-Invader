using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading;

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
    public char scene = 'g';
    AsyncOperation scenes;

    void Start()
    {
        StartCoroutine(LoadAsyncOperation());
    }

    IEnumerator LoadAsyncOperation()
    {
        AsyncOperation sceneLoader;
        float timer = 0;

        switch(scene)
        {
            case 'g':
                sceneLoader = SceneManager.LoadSceneAsync("Game");
                
                break;
            case 'm':
                sceneLoader = SceneManager.LoadSceneAsync("Menu");
                
                break;
            default:
                sceneLoader = SceneManager.LoadSceneAsync("Menu");
                break;
        }

        
        sceneLoader.allowSceneActivation = false;

        //Debug.Log("eu estou aqui");

        while (!sceneLoader.isDone)
        {
            timer += Time.deltaTime;

            
            if (sceneLoader.progress >= 0.9f && timer >= 2)
            {


                //sceneLoader.allowSceneActivation = true;

            }
            yield return new WaitForEndOfFrame();
        }
    }
}
