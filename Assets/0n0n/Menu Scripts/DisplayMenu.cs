using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayMenu : MonoBehaviour
{
    [SerializeField]
    public Dropdown dropResolution;
    private Resolution[] resolutions;
    private int currentResolutionIndex = 0;
    private List<string> options = new List<string>();
    private string option;

    void Start()
    {
        resolutions = Screen.resolutions;

        for(int i = 0; i < resolutions.Length; i++)
        {
            option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if(resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        dropResolution.AddOptions(options);
        dropResolution.value = currentResolutionIndex;
        dropResolution.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[currentResolutionIndex];
        Screen.SetResolution(resolution.width,resolution.height,Screen.fullScreen);
    }
}
    /*
    private const string RESOLUTION_PREF_KEY = "resolution";

    [SerializeField]
    private Text resolutionText;
    private List<Resolution> resolutions;
    private int currentResolutionIndex = 0;

    void Start()
    {
        resolutions.AddRange(Screen.resolutions);
    }

    private void SetandApplyResolution(int newResolutionIndex)
    {
        currentResolutionIndex = newResolutionIndex;
        ApplyCurrentResolution();
    }

    private void ApplyCurrentResolution()
    {
        ApplyResolution(resolutions[currentResolutionIndex]);
    }

    private void ApplyResolution(Resolution resolution)
    {
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    private void SetResolution(Resolution resolution)
    {
        resolutionText.text = resolution.width + "x" + resolution.height;
    }

    public void SetNextResolution()
    {
        currentResolutionIndex = GetNextWrappedIndex(resolutions, currentResolutionIndex);
        SetResolution(resolutions[currentResolutionIndex]);
    }

    public void SetPreviousResolution()
    {
        currentResolutionIndex = GetPreviousWrappedIndex(resolutions, currentResolutionIndex);
        SetResolution(resolutions[currentResolutionIndex]);
    }

    private int GetNextWrappedIndex<T>(List<T> collection, int currentIndex)
    {
        if (collection.Count < 1) return 0;
        return (currentIndex + 1) % collection.Count;
    }

    private int GetPreviousWrappedIndex<T>(List<T> collection, int currentIndex)
    {
        if (collection.Count < 1) return 0;
        if ((currentIndex - 1) < 0) return collection.Count - 1;
        return (currentIndex - 1) % collection.Count;
    }

    public void ApplyChanges()
    {
        SetandApplyResolution(currentResolutionIndex);
    }
    */

