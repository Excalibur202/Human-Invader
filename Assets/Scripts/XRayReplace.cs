using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRayReplace : MonoBehaviour
{
    public Shader replacementShader;
    private void OnEnable()
    {
        GetComponent<Camera>().SetReplacementShader(replacementShader, "XRay");
    }
}
