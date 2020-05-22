using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMesh
{
    public char[,] navMeshMap;// 'g'-ground; 'w'-wall; 'p'-player; 'o'-obstacle; 's'-sector door;
    int mapSizeX;
    int mapSizeY;
    public bool refreshAreas = false;// so pode ser usado se so existir um player

    Vector2 up = new Vector3(0, 1);
    Vector2 down = new Vector3(0, -1);
    Vector2 right = new Vector3(1, 0);
    Vector2 left = new Vector3(-1, 0);

    public NavMesh(int mapSizeX, int mapSizeY)
    {
        this.navMeshMap = new char[mapSizeX, mapSizeY];
        this.mapSizeX = mapSizeX;
        this.mapSizeY = mapSizeY;
    }

    public bool InMapRange(int x, int y)
    {
        return ((x >= 0 && x < mapSizeX) && (y >= 0 && y < mapSizeY));
    }
    public bool InMapRange(Vector2 vec)
    {
        return (((int)vec.x >= 0 && (int)vec.x < mapSizeX) && ((int)vec.y >= 0 && (int)vec.y < mapSizeY));
    }

    public char GetPosChar(int x, int y)
    {
        return navMeshMap[x, y];
    }
    public char GetPosChar(Vector2 vec)
    {
        return navMeshMap[(int)vec.x, (int)vec.y];
    }

    public bool SetPosChar(int x, int y, char value)
    {
        if (InMapRange(x, y))
        {
            navMeshMap[x, y] = value;
            return true;
        }
        return false;
    }
    public bool SetPosChar(Vector2 vec, char value)
    {
        if (InMapRange(vec))
        {
            navMeshMap[(int)vec.x, (int)vec.y] = value;
            return true;
        }
        return false;
    }

    public char[,] GetMapBorder(bool changeOriginalMap, bool maintainGroundInfo, char[,] mapChar)
    {
        char[,] wallMap = new char[mapSizeX, mapSizeY];
        for (int posX = 0; posX < mapSizeX; posX++)
            for (int posY = 0; posY < mapSizeY; posY++)
            {
                if (mapChar[posX, posY] == 'g')
                {
                    if ((posX > 0 && posX < mapSizeX - 1) && (posY > 0 && posY < mapSizeY - 1))//Esta no limite do mapa?
                    {//Nao
                        for (int deltaX = -1; deltaX < 2; deltaX++)
                            for (int deltaY = -1; deltaY < 2; deltaY++)
                                if (mapChar[deltaX + posX, deltaY + posY] == '\0')
                                {
                                    wallMap[deltaX + posX, deltaY + posY] = 'w';
                                }

                        if (maintainGroundInfo)
                            wallMap[posX, posY] = mapChar[posX, posY];
                    }
                    else //Sim
                        wallMap[posX, posY] = 'w';
                }
                else
                    if (mapChar[posX, posY] != '\0')
                    wallMap[posX, posY] = mapChar[posX, posY];
            }

        if (changeOriginalMap)
            mapChar = wallMap;

        return mapChar;
    }

    public char[,] GetArea(int sizeX, int sizeY, Vector3 position)
    {
        char[,] mapArea = new char[sizeX, sizeY];

        Vector2 rightCorner = new Vector2((int)((int)(sizeX * 0.5f) + (position.x)), (int)((int)(sizeY * 0.5f) + (position.z)));

        int deltaX = ((int)rightCorner.x - sizeX);
        int deltaY = ((int)rightCorner.y - sizeY);

        int auxX = sizeX;
        int auxY;

        for (int x = (int)rightCorner.x; x > deltaX; x--)
        {
            auxX--;
            auxY = sizeY;
            for (int y = (int)rightCorner.y; y > deltaY; y--)
            {
                auxY--;
                if (!((int)(sizeX * 0.5f) + 1 == auxX && (int)(sizeY * 0.5f) + 1 == auxY))
                {
                    if (InMapRange(x, y))
                        mapArea[auxX, auxY] = navMeshMap[x, y];
                }
                else
                    mapArea[auxX, auxY] = 'a';
            }
        }


        return mapArea;
    }

    private char[,] SetMapChars(int box2DScaleX, int box2DScaleY, int boxRightCorner2DPosX, int boxRightCorner2DPosY, Vector2 entrance2DForward, char setToChar, char[,] mapChar)
    {
        int deltaX;
        int deltaY;
        if (entrance2DForward == up)//Exit Up
        {
            deltaX = (boxRightCorner2DPosX - box2DScaleX);
            deltaY = (boxRightCorner2DPosY - box2DScaleY);

            for (int x = boxRightCorner2DPosX; x > deltaX; x--)
                for (int y = boxRightCorner2DPosY; y > deltaY; y--)
                    if (InMapRange(x, y))
                        mapChar[x, y] = setToChar;
        }
        else if (entrance2DForward == down)//Exit Down
        {
            boxRightCorner2DPosX = boxRightCorner2DPosX + 1;
            boxRightCorner2DPosY = boxRightCorner2DPosY + 1;

            deltaX = (boxRightCorner2DPosX + box2DScaleX);
            deltaY = (boxRightCorner2DPosY + box2DScaleY);

            for (int x = boxRightCorner2DPosX; x < deltaX; x++)
                for (int y = boxRightCorner2DPosY; y < deltaY; y++)
                    if (InMapRange(x, y))
                        mapChar[x, y] = setToChar;
        }
        else if (entrance2DForward == right)//Exit Right
        {
            boxRightCorner2DPosY = boxRightCorner2DPosY + 1;
            deltaX = (boxRightCorner2DPosX - box2DScaleY);
            deltaY = (boxRightCorner2DPosY + box2DScaleX);

            for (int x = boxRightCorner2DPosX; x > deltaX; x--)
                for (int y = boxRightCorner2DPosY; y < deltaY; y++)
                    if (InMapRange(x, y))
                        mapChar[x, y] = setToChar;
        }
        else if (entrance2DForward == left)//Exit Left
        {
            boxRightCorner2DPosX = boxRightCorner2DPosX + 1;
            //Calculate delta relative to the mapArray
            deltaX = (boxRightCorner2DPosX + box2DScaleY);
            deltaY = (boxRightCorner2DPosY - box2DScaleX);

            for (int x = boxRightCorner2DPosX; x < deltaX; x++)
                for (int y = boxRightCorner2DPosY; y > deltaY; y--)
                    if (InMapRange(x, y))
                        mapChar[x, y] = setToChar;
        }

        return mapChar;
    }

    public int GetDimentionLength(int dimention)
    {
        return navMeshMap.GetLength(dimention);
    }

    public void RoomsToNavMesh(List<RoomInfo> rooms)
    {
        RoomEntrance roomInfo;
        GameObject roomObj;

        Vector2 entrance2DForward;

        foreach (RoomInfo room in rooms)
        {
            roomObj = room.prefab;
            roomInfo = roomObj.GetComponent<RoomEntrance>();

            if (roomInfo)
            {
                entrance2DForward = new Vector2((int)roomObj.transform.forward.normalized.x, (int)roomObj.transform.forward.normalized.z);

                for (int navMeshBoxIndex = 0; navMeshBoxIndex < roomInfo.navMeshObjs.Count; navMeshBoxIndex++)
                {
                    Transform navMeshBox = roomInfo.navMeshObjs[navMeshBoxIndex];
                    int box2DScaleX = Mathf.RoundToInt(navMeshBox.lossyScale.x);
                    int box2DScaleY = Mathf.RoundToInt(navMeshBox.lossyScale.z);

                    Transform boxRightCorner = roomInfo.navMeshRightCorners[navMeshBoxIndex];
                    int boxRightCorner2DPosX = Mathf.RoundToInt(boxRightCorner.position.x);
                    int boxRightCorner2DPosY = Mathf.RoundToInt(boxRightCorner.position.z);

                    //Ground
                    navMeshMap = SetMapChars(box2DScaleX, box2DScaleY, boxRightCorner2DPosX, boxRightCorner2DPosY, entrance2DForward, 'g', navMeshMap);
                }

                if (roomInfo.obstaclesActivation)
                {
                    for (int obstacleIndex = 0; obstacleIndex < roomInfo.obstacles.Count; obstacleIndex++)
                    {
                        Transform navMeshBox = roomInfo.obstacles[obstacleIndex];
                        int box2DScaleX = Mathf.RoundToInt(navMeshBox.lossyScale.x);
                        int box2DScaleY = Mathf.RoundToInt(navMeshBox.lossyScale.z);

                        Transform boxRightCorner = roomInfo.obstaclesRightCorners[obstacleIndex];
                        int boxRightCorner2DPosX = Mathf.RoundToInt(boxRightCorner.position.x);
                        int boxRightCorner2DPosY = Mathf.RoundToInt(boxRightCorner.position.z);

                        //Obstacles
                        navMeshMap = SetMapChars(box2DScaleX, box2DScaleY, boxRightCorner2DPosX, boxRightCorner2DPosY, entrance2DForward, 'o', navMeshMap);
                    }
                }
            }
        }
    }

    public char[,] RoomToNavMesh(char[,] mapChar, RoomInfo room)
    {
        Vector2 entrance2DForward;
        GameObject roomObj = room.prefab;
        RoomEntrance roomInfo = roomObj.GetComponent<RoomEntrance>();

        if (roomInfo)
        {
            entrance2DForward = new Vector2((int)roomObj.transform.forward.normalized.x, (int)roomObj.transform.forward.normalized.z);

            for (int navMeshBoxIndex = 0; navMeshBoxIndex < roomInfo.navMeshObjs.Count; navMeshBoxIndex++)
            {
                Transform navMeshBox = roomInfo.navMeshObjs[navMeshBoxIndex];
                int box2DScaleX = Mathf.RoundToInt(navMeshBox.lossyScale.x);
                int box2DScaleY = Mathf.RoundToInt(navMeshBox.lossyScale.z);

                Transform boxRightCorner = roomInfo.navMeshRightCorners[navMeshBoxIndex];
                int boxRightCorner2DPosX = Mathf.RoundToInt(boxRightCorner.position.x);
                int boxRightCorner2DPosY = Mathf.RoundToInt(boxRightCorner.position.z);

                //Ground
                navMeshMap = SetMapChars(box2DScaleX, box2DScaleY, boxRightCorner2DPosX, boxRightCorner2DPosY, entrance2DForward, 'g', navMeshMap);
            }

            if (roomInfo.obstaclesActivation)
            {
                for (int obstacleIndex = 0; obstacleIndex < roomInfo.obstacles.Count; obstacleIndex++)
                {
                    Transform navMeshBox = roomInfo.obstacles[obstacleIndex];
                    int box2DScaleX = Mathf.RoundToInt(navMeshBox.lossyScale.x);
                    int box2DScaleY = Mathf.RoundToInt(navMeshBox.lossyScale.z);

                    Transform boxRightCorner = roomInfo.obstaclesRightCorners[obstacleIndex];
                    int boxRightCorner2DPosX = Mathf.RoundToInt(boxRightCorner.position.x);
                    int boxRightCorner2DPosY = Mathf.RoundToInt(boxRightCorner.position.z);

                    //Obstacles
                    navMeshMap = SetMapChars(box2DScaleX, box2DScaleY, boxRightCorner2DPosX, boxRightCorner2DPosY, entrance2DForward, 'o', navMeshMap);
                }
            }
        }

        return mapChar;
    }

    public void RoomToTexture(Texture2D texture, RoomInfo room, int size, int border)
    {
        Vector2 entrance2DForward;
        GameObject roomObj = room.prefab;
        RoomEntrance roomInfo = roomObj.GetComponent<RoomEntrance>();

        if (roomInfo)
        {
            entrance2DForward = new Vector2((int)roomObj.transform.forward.normalized.x, (int)roomObj.transform.forward.normalized.z);

            for (int navMeshBoxIndex = 0; navMeshBoxIndex < roomInfo.navMeshObjs.Count; navMeshBoxIndex++)
            {
                Transform navMeshBox = roomInfo.navMeshObjs[navMeshBoxIndex];
                int box2DScaleX = Mathf.RoundToInt(navMeshBox.lossyScale.x);
                int box2DScaleY = Mathf.RoundToInt(navMeshBox.lossyScale.z);

                Transform boxRightCorner = roomInfo.navMeshRightCorners[navMeshBoxIndex];
                int boxRightCorner2DPosX = Mathf.RoundToInt(boxRightCorner.position.x);
                int boxRightCorner2DPosY = Mathf.RoundToInt(boxRightCorner.position.z);

                //Ground
                SetMapPixel(texture, Color.white, box2DScaleX, box2DScaleY, boxRightCorner2DPosX, boxRightCorner2DPosY, entrance2DForward, size, border);

                texture.Apply();
            }

        }
    }

    public void SetMapPixel(Texture2D texture, Color inputColor, int box2DScaleX, int box2DScaleY, int boxRightCorner2DPosX, int boxRightCorner2DPosY, Vector2 entrance2DForward, int size, int border)
    {
        int deltaX;
        int deltaY;
        if (entrance2DForward == up)//Exit Up
        {
            deltaX = (boxRightCorner2DPosX - box2DScaleX);
            deltaY = (boxRightCorner2DPosY - box2DScaleY);

            for (int x = boxRightCorner2DPosX; x > deltaX; x--)
                for (int y = boxRightCorner2DPosY; y > deltaY; y--)
                    if (InMapRange(x, y))
                        MapToTexture.SetTextureColor(border, size, x, y, inputColor, texture);
        }
        else if (entrance2DForward == down)//Exit Down
        {
            boxRightCorner2DPosX = boxRightCorner2DPosX + 1;
            boxRightCorner2DPosY = boxRightCorner2DPosY + 1;

            deltaX = (boxRightCorner2DPosX + box2DScaleX);
            deltaY = (boxRightCorner2DPosY + box2DScaleY);

            for (int x = boxRightCorner2DPosX; x < deltaX; x++)
                for (int y = boxRightCorner2DPosY; y < deltaY; y++)
                    if (InMapRange(x, y))
                        MapToTexture.SetTextureColor(border, size, x, y, inputColor, texture);
        }
        else if (entrance2DForward == right)//Exit Right
        {
            boxRightCorner2DPosY = boxRightCorner2DPosY + 1;
            deltaX = (boxRightCorner2DPosX - box2DScaleY);
            deltaY = (boxRightCorner2DPosY + box2DScaleX);

            for (int x = boxRightCorner2DPosX; x > deltaX; x--)
                for (int y = boxRightCorner2DPosY; y < deltaY; y++)
                    if (InMapRange(x, y))
                        MapToTexture.SetTextureColor(border, size, x, y, inputColor, texture);
        }
        else if (entrance2DForward == left)//Exit Left
        {
            boxRightCorner2DPosX = boxRightCorner2DPosX + 1;
            //Calculate delta relative to the mapArray
            deltaX = (boxRightCorner2DPosX + box2DScaleY);
            deltaY = (boxRightCorner2DPosY - box2DScaleX);

            for (int x = boxRightCorner2DPosX; x < deltaX; x++)
                for (int y = boxRightCorner2DPosY; y > deltaY; y--)
                    if (InMapRange(x, y))
                        MapToTexture.SetTextureColor(border, size, x, y, inputColor, texture);
        }
    }

    public void ObstaclesToNavMesh(NavMeshObstacle obstacles)
    {
        for (int i = 0; i < obstacles.obstacleTransform.Length;i++)
            ObstacleToNavMesh(obstacles.obstacleTransform[i], obstacles.obstacleRightCorner[i], obstacles.mapChar[i]);
    }
    private void ObstacleToNavMesh(Transform obstacleTransform, Transform obstacleRightCorner, char mapChar)
    {
        Vector2 obstacleForward = new Vector2((int)obstacleTransform.forward.normalized.x, (int)obstacleTransform.forward.normalized.z);

        int box2DScaleX = Mathf.RoundToInt(obstacleTransform.lossyScale.x);
        int box2DScaleY = Mathf.RoundToInt(obstacleTransform.lossyScale.z);

        int boxRightCorner2DPosX = Mathf.RoundToInt(obstacleRightCorner.position.x);
        int boxRightCorner2DPosY = Mathf.RoundToInt(obstacleRightCorner.position.z);

        navMeshMap = SetMapChars(box2DScaleX, box2DScaleY, boxRightCorner2DPosX, boxRightCorner2DPosY, obstacleForward, mapChar, navMeshMap);
    }
}
