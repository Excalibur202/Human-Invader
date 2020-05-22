using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapToTexture : MonoBehaviour
{
    public static MapToTexture instance;
    bool shaderReady = false;

    public bool aIVision = false;

    //Player Transform
    public Transform playerTransform;

    //Textures
    private Texture2D mapTexture;
    private Texture2D fogTexture;

    //Materials
    public Material mapMat;
    public Material debugMapMat;
    public Material debugFogMat;

    //Colors
    Color pixelColor;
    public Color wallColor;
    public Color borderColor;
    public Color groundColor;
    public Color obstacleColor;
    public Color backgroundColor;
    public Color sectorDoorColor;

    //Map Info
    int sizeX;
    int sizeY;
    char[,] navMap;
    public int size;
    public int border;
    private MapGenerator map;
    private GeneticSelection GenSelect;

    ////////////////////////////////////////////////////////////
    public char[,] mapChar;

    private void Awake()
    {
        if (instance)
        {
            Destroy(this);
        }
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        map = MapGenerator.instance;
        GenSelect = GeneticSelection.instance;

    }

    // Update is called once per frame
    void Update()
    {
        if (!shaderReady && aIVision)
            playerTransform = map.aIEnemyTraining.transform;
        

        if (playerTransform)
        {
            if (!shaderReady)
            {
                if (aIVision)
                    shaderReady = InitializeMapAI();
                else
                    shaderReady = InitializeMap();
            }
            else
            {
                if (aIVision)
                    UpdateAIShaderInfo();
                else
                    UpdateShaderInfo();//Updates player pos in shader
            }  
        }
    }


    public bool InitializeMapAI()
    {
        if (map.navMesh != null)
        {
            sizeX = GenSelect.aIVisionSizeX;
            sizeY = GenSelect.aIVisionSizeY;

            //Debug Materials
            if (debugMapMat) debugMapMat.mainTexture = mapTexture;
            if (debugFogMat) debugFogMat.mainTexture = fogTexture;

            //FogTexture
            fogTexture = new Texture2D((border * 2) + (sizeX * size), (border * 2) + (sizeY * size));
            fogTexture.wrapMode = TextureWrapMode.Clamp;
            SetTextureColor(fogTexture, Color.white);
            CenterOfTexture(fogTexture, Color.white, sizeX, sizeY, size, border);
            fogTexture.Apply();

            //MapTexture
            mapTexture = new Texture2D((border * 2) + (sizeX * size), (border * 2) + (sizeY * size));
            mapTexture.wrapMode = TextureWrapMode.Clamp;
            SetTextureColor(mapTexture, borderColor);


            //Send info to shader
            mapMat.SetTexture("_MainTex", mapTexture);
            mapMat.SetTexture("_SecondaryTex", fogTexture);
            mapMat.SetVector("_PlayerPos", playerTransform.position);
            mapMat.SetVector("_FuncInfo", new Vector3(sizeX, sizeY, 0));

            //Draw Spawn
            DrawSector(-2);

            return true;
        }
        return false;
    }

    public void UpdateAIShaderInfo()
    {

        navMap =  map.aIEnemyTraining.GetComponent<AIEnemy>().mapArea;
        if(navMap!= null)
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
                            pixelColor = Color.black;
                            break;
                        case 'a':
                            pixelColor = Color.red;
                            break;
                        case 's':
                            pixelColor = sectorDoorColor;
                            break;
                        default:
                            pixelColor = backgroundColor;
                            break;
                    }
                    SetTextureColor(border, size, x, y, pixelColor, mapTexture);
                }
            mapTexture.Apply();
        }
        

        //Update player pos
        mapMat.SetVector("_PlayerPos", playerTransform.position);
    }

    /*Init*/
    public bool InitializeMap()
    {
        if (map.navMesh != null)
        {
            //Get map size
            navMap = map.navMesh.navMeshMap;
            sizeX = navMap.GetLength(0);
            sizeY = navMap.GetLength(1);

            //Debug Materials
            if (debugMapMat) debugMapMat.mainTexture = mapTexture;
            if (debugFogMat) debugFogMat.mainTexture = fogTexture;

            //FogTexture
            fogTexture = new Texture2D((border * 2) + (sizeX * size), (border * 2) + (sizeY * size));
            fogTexture.wrapMode = TextureWrapMode.Clamp;
            SetTextureColor(fogTexture, Color.white);
            CenterOfTexture(fogTexture, Color.black, sizeX, sizeY, size, border);
            fogTexture.Apply();

            //MapTexture
            mapTexture = new Texture2D((border * 2) + (sizeX * size), (border * 2) + (sizeY * size));
            mapTexture.wrapMode = TextureWrapMode.Clamp;
            SetTextureColor(mapTexture, borderColor);
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
                            pixelColor = groundColor;
                            break;
                        case 's':
                            pixelColor = sectorDoorColor;
                            break;
                        default:
                            pixelColor = backgroundColor;
                            break;
                    }
                    SetTextureColor(border, size, x, y, pixelColor, mapTexture);
                }
            mapTexture.Apply();

            //Send info to shader
            mapMat.SetTexture("_MainTex", mapTexture);
            mapMat.SetTexture("_SecondaryTex", fogTexture);
            mapMat.SetVector("_PlayerPos", playerTransform.position);
            mapMat.SetVector("_FuncInfo", new Vector3(sizeX, sizeY, 0));

            //Draw Spawn
            DrawSector(-2);

            return true;
        }
        return false;
    }

    /*Updates shader info*/
    public void UpdateShaderInfo()
    {
        //Update player pos
        mapMat.SetVector("_PlayerPos", playerTransform.position);

    }

    /*Set texture main color (with border)*/
    public static void SetTextureColor(int border, int size, int x, int y, Color pxColor, Texture2D texture)
    {
        Vector2 modiefiedPos = new Vector2(border + (x * size), border + (y * size));

        for (int deltaX = (int)modiefiedPos.x; deltaX < modiefiedPos.x + size; deltaX++)
            for (int deltaY = (int)modiefiedPos.y; deltaY < modiefiedPos.y + size; deltaY++)
            {
                texture.SetPixel(deltaX, deltaY, pxColor);
            }
    }

    /*Set texture main color (without border)*/
    private void SetTextureColor(Texture2D texture, Color borderColor)
    {
        for (int x = 0; x < texture.width; x++)
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, borderColor);
            }
    }

    /*Paints a quad area in texture*/
    public void CenterOfTexture(Texture2D texture, Color color, int sizeX, int sizeY, int size, int border)
    {
        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeY; y++)
            {
                SetTextureColor(border, size, x, y, color, texture);
            }
    }

    /*Draws a sector from the map in the texture*/
    public bool DrawSector(int sector)
    {
        bool sectorDrown = false;
        foreach (RoomInfo room in map.spawnedRooms)
            if (room.sector == sector)
            {
                map.navMesh.RoomToTexture(fogTexture, room, size, border);
                room.downloaded = true;
                sectorDrown = true;
            }

        return sectorDrown;
    }
}
