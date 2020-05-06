using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapToTexture : MonoBehaviour
{
    //public Texture2D texture2D;
    //public RenderTexture mapRender;
    public bool openMap = false;
    public Transform playerTransform;
    private Vector3 playerPos;
    private Vector2 playerPos2D;
    Texture2D mapTexture;
    Texture2D fogTexture;
    MapGenerator map;
    public Material mapMaterial;
    public Material matTest;
    //public Material blitMat;
    Color pixelColor;
    public Color wallColor;
    public Color playerColor;
    public Color obstacleColor;
    public Color groundColor;
    public Color backgroundColor;
    int sizeX;
    int sizeY;
    public int area2D;
    public float radius;
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
                fogTexture = new Texture2D(sizeX, sizeY);
                //texture2D = new Texture2D(sizeX,sizeY);

                for (int x = 0; x < sizeX; x++)
                    for (int y = 0; y < sizeY; y++)
                    {
                        fogTexture.SetPixel(x, y, Color.black);
                        //texture2D.SetPixel(x, y, Color.white);
                    }
                mapMaterial.mainTexture = mapTexture;
                //playerPos = new Vector3(playerTransform.position.x, playerTransform.position.z,0);
                mapTexture.wrapMode = TextureWrapMode.Clamp;
                fogTexture.wrapMode = TextureWrapMode.Clamp;
                matTest.SetTexture("_MainTex", mapTexture);
                matTest.SetTexture("_SecondaryTex", fogTexture);
                //matTest.SetVector("_PlayerPos", playerPos);
                fogTexture.Apply();
                //texture2D.Apply();
                navMeshReady = true;
                openMap = true;
            }
        }
        else
        {
            //playerPos = new Vector3(playerTransform.position.x,playerTransform.position.z,0);
            //matTest.SetVector("_PlayerPos", playerPos);
            
            //Graphics.Blit(mapTexture, mapRender, blitMat);
            
            //blitMat.mainTexture = mapRender;
            
            playerPos2D = Util.V3toV2(playerTransform.position);

            for (int x = ((int)playerPos2D.x - area2D); x < ((int)playerPos2D.x + area2D); x++)
                for (int y = ((int)playerPos2D.y - area2D); y < ((int)playerPos2D.y + area2D); y++)
                {
                    if (map.navMesh.InMapRange(x, y))
                        if (Util.SqrDistance(new Vector2(x, y),playerPos2D) < Util.Square(radius))
                            fogTexture.SetPixel(x, y, Color.white);
                }
            
            if (openMap)
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
                fogTexture.Apply();
                openMap = false;
            }

        }
    }
}
