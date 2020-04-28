using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapToTexture : MonoBehaviour
{
    public bool openMap = false;
    Texture2D mapTexture;
    MapGenerator map;
    public Material mapMaterial;
    Color pixelColor;
    public Color wallColor;
    public Color playerColor;
    public Color obstacleColor;
    public Color groundColor;
    public Color backgroundColor;
    int sizeX;
    int sizeY;
    char[,] navMap;
    bool navMeshReady = false;

    // Start is called before the first frame update
    void Start()
    {
        map = MapGenerator.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!navMeshReady)
        {
            if (map.navMesh != null)
            {
                navMap = map.navMesh.navMeshMap;
                sizeX = navMap.GetLength(0);
                sizeY = navMap.GetLength(1);

                mapTexture = new Texture2D(sizeX, sizeY);
                mapMaterial.mainTexture = mapTexture;
                navMeshReady = true;
            }
        }
        else
        {
            if(openMap)
            {
                for (int x = 0; x < sizeX; x++)
                    for (int y = 0; y < sizeY; y++)
                    {
                        switch (navMap[x, y])
                        {
                            case 'w':
                                pixelColor = wallColor;
                                break;
                            case 'g':
                                pixelColor = groundColor;
                                break;
                            case 'o':
                                pixelColor = obstacleColor;
                                break;
                            case 'p':
                                pixelColor = playerColor;
                                break;
                            default:
                                pixelColor = backgroundColor;
                                break;
                        }

                        mapTexture.SetPixel(x, y, pixelColor);
                    }

                mapTexture.Apply();
                openMap = false;
            }
           
        }
    }
}
