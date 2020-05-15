using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapToTexture : MonoBehaviour
{
    public static MapToTexture instance;
    public Texture2D texture2D;
    public int size;
    public int border;
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
    public Material matTest2;
    //public Material blitMat;
    Color pixelColor;
    public Color wallColor;
    public Color obstacleColor;
    public Color groundColor;
    public Color backgroundColor;
    public Color sectorDoorColor;
    public Color borderColor;
    int sizeX;
    int sizeY;
    public int area2D;
    public float radius;
    char[,] navMap;
    bool navMeshReady = false;
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
                fogTexture = new Texture2D((border * 2) + (sizeX * size), (border * 2) + (sizeY * size));
                texture2D = new Texture2D((border * 2) + (sizeX * size), (border * 2) + (sizeY * size));

                mapChar = new char[sizeX, sizeY];


                mapMaterial.mainTexture = texture2D;
                matTest2.mainTexture = fogTexture;
                //playerPos = new Vector3(playerTransform.position.x, playerTransform.position.z,0);
                mapTexture.wrapMode = TextureWrapMode.Clamp;
                fogTexture.wrapMode = TextureWrapMode.Clamp;
                texture2D.wrapMode = TextureWrapMode.Clamp;

                SetTextureColor(texture2D, borderColor);
                SetTextureColor(fogTexture, Color.white);

                //matTest.SetTexture("_MainTex", mapTexture);
                matTest.SetTexture("_MainTex", texture2D);
                matTest.SetTexture("_SecondaryTex", fogTexture);

                matTest.SetVector("_PlayerPos", playerTransform.position);

                //matTest.SetVector("_MapScale", new Vector3(sizeX,sizeY, 0));

                matTest.SetVector("_FuncInfo", new Vector3(sizeX, sizeY, 0));
                Debug.Log(MyMath.GetSlope(sizeX, 0.5f));
                //matTest.SetVector("_PlayerPos", playerPos);
                CenterOfTexture(fogTexture, Color.black, sizeX, sizeY, size, border);
                fogTexture.Apply();
                //texture2D.Apply();
                navMeshReady = true;

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

                        //mapTexture.SetPixel(x, y, pixelColor);
                        SetTextureColor(border, size, x, y, pixelColor, texture2D);
                    }

                //mapTexture.Apply();
                texture2D.Apply();

                // openMap = true;
            }
        }
        else
        {
            //playerPos = new Vector3(playerTransform.position.x,playerTransform.position.z,0);
            //matTest.SetVector("_PlayerPos", playerPos);

            //Graphics.Blit(mapTexture, mapRender, blitMat);

            //blitMat.mainTexture = mapRender;
            matTest.SetVector("_PlayerPos", playerTransform.position);
            playerPos2D = Util.V3toV2(playerTransform.position);
            //for (int x = ((int)playerPos2D.x - area2D); x < ((int)playerPos2D.x + area2D); x++)
            //    for (int y = ((int)playerPos2D.y - area2D); y < ((int)playerPos2D.y + area2D); y++)
            //    {
            //        if (map.navMesh.InMapRange(x, y))
            //            if (Util.SqrDistance(new Vector2(x, y),playerPos2D) < Util.Square(radius))
            //                fogTexture.SetPixel(x, y, Color.white);

            //    }

            //fogTexture.Apply();

            //foreach (RoomInfo roomInfo in map.spawnedRooms)
            //    if (!roomInfo.drawed)
            //        if (Util.SqrDistance(playerPos2D, Util.V3toV2(roomInfo.prefab.transform.position)) < Util.Square(17f))
            //        {
            //            map.navMesh.RoomToTexture(fogTexture, roomInfo, size, border);
            //            roomInfo.drawed = true;
            //        }

            //if (openMap)
            //{


            //    openMap = false;
            //}

        }
    }

    public static void SetTextureColor(int border, int size, int x, int y, Color pxColor, Texture2D texture)
    {

        Vector2 modiefiedPos = new Vector2(border + (x * size), border + (y * size));

        for (int deltaX = (int)modiefiedPos.x; deltaX < modiefiedPos.x + size; deltaX++)
            for (int deltaY = (int)modiefiedPos.y; deltaY < modiefiedPos.y + size; deltaY++)
            {
                texture.SetPixel(deltaX, deltaY, pxColor);
            }
    }

    private void SetTextureColor(Texture2D texture, Color borderColor)
    {

        for (int x = 0; x < texture.width; x++)
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, borderColor);
            }
    }

    public void CenterOfTexture(Texture2D texture, Color color, int sizeX, int sizeY, int size, int border)
    {
        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeY; y++)
            {
                SetTextureColor(border, size, x, y, color, texture);
            }
    }

    public bool DrawSector(int sector)
    {
        bool sectorDrown = false;
        foreach (RoomInfo room in map.spawnedRooms)
            if (room.sector == sector)
            {
                map.navMesh.RoomToTexture(fogTexture, room, size, border);
                sectorDrown = true;
            }

        return sectorDrown;
    }

}
