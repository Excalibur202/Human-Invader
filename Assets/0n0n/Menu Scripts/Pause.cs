using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public MapGenerator map;

    void Start()
    {
        map = MapGenerator.instance;
        
    }

}
