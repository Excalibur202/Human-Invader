using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMesh
{
    char[,] navMeshMap;// 'g'-ground; 'w'-wall; 'e'-enemy;
    int mapSizeX;
    int mapSizeY;
    public bool refreshAreas = false;// so pode ser usado se so existir um enimigo


    public NavMesh(int mapSizeX, int mapSizeY)
    {
        this.navMeshMap = new char[mapSizeX, mapSizeY];
        this.mapSizeX = mapSizeX;
        this.mapSizeY = mapSizeY;
    }

    public bool InMapRange(int x, int y)
    {
        return (x >= 0 && x < mapSizeX && y >= 0 && x < mapSizeY) ? true : false;
    }
    public bool InMapRange(Vector2 vec)
    {
        return ((int)vec.x >= 0 && (int)vec.x < mapSizeX && (int)vec.y >= 0 && (int)vec.y < mapSizeY) ? true : false;
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

    public int GetDimentionLength(int dimention)
    {
        return navMeshMap.GetLength(dimention);
    }

    public void BuildNavMeshMap(List<RoomInfo> rooms)
    {
        RoomEntrance roomInfo;
        GameObject roomObj;

        Vector2 entrance2DForward;

        Vector2 up = new Vector3(0, 1);
        Vector2 down = new Vector3(0, -1);
        Vector2 right = new Vector3(1, 0);
        Vector2 left = new Vector3(-1, 0);

        int deltaX;
        int deltaY;

        /*  */
        foreach (RoomInfo room in rooms)
        {
            roomObj = room.prefab;
            roomInfo = roomObj.GetComponent<RoomEntrance>();

            if (roomInfo)
            {
                entrance2DForward = new Vector2((int)roomObj.transform.forward.normalized.x, (int)roomObj.transform.forward.normalized.z);

                for (int navMeshBoxIndex = 0; navMeshBoxIndex < roomInfo.navMeshObjs.Count; navMeshBoxIndex++)
                {
                    Transform navMeshBox = roomInfo.navMeshObjs[navMeshBoxIndex].transform;
                    int box2DScaleX = Mathf.RoundToInt(navMeshBox.lossyScale.x);
                    int box2DScaleY = Mathf.RoundToInt(navMeshBox.lossyScale.z);

                    Transform boxRightCorner = roomInfo.navMeshRightCorners[navMeshBoxIndex].transform;
                    int boxRightCorner2DPosX = Mathf.RoundToInt(boxRightCorner.position.x);
                    int boxRightCorner2DPosY = Mathf.RoundToInt(boxRightCorner.position.z);


                    if (entrance2DForward == up)//Exit Up
                    {
                        deltaX = (boxRightCorner2DPosX - box2DScaleX);
                        deltaY = (boxRightCorner2DPosY - box2DScaleY);

                        for (int x = boxRightCorner2DPosX; x > deltaX; x--)
                            for (int y = boxRightCorner2DPosY; y > deltaY; y--)
                                if (InMapRange(x, y))
                                    navMeshMap[x, y] = 'g';
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
                                    navMeshMap[x, y] = 'g';
                    }
                    else if (entrance2DForward == right)//Exit Right
                    {
                        boxRightCorner2DPosY = boxRightCorner2DPosY + 1;
                        deltaX = (boxRightCorner2DPosX - box2DScaleY);
                        deltaY = (boxRightCorner2DPosY + box2DScaleX);

                        for (int x = boxRightCorner2DPosX; x > deltaX; x--)
                            for (int y = boxRightCorner2DPosY; y < deltaY; y++)
                                if (InMapRange(x, y))
                                    navMeshMap[x, y] = 'g';
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
                                    navMeshMap[x, y] = 'g';
                    }
                }
            }
        }
    }

    public char[,] GetMapBorder(bool changeOriginalMap, bool maintainGroundInfo)
    {
        char[,] wallMap = new char[mapSizeX, mapSizeY];

        for (int posX = 0; posX < mapSizeX; posX++)
            for (int posY = 0; posY < mapSizeY; posY++)
            {
                if (navMeshMap[posX, posY] == 'g')
                    if ((posX > 0 && posX < mapSizeX - 1) && (posY > 0 && posY < mapSizeY - 1))//Esta no limite do mapa?
                    {//Nao
                        if (navMeshMap[posX - 1, posY] != 'g' || navMeshMap[posX + 1, posY] != 'g' || //vertical
                            navMeshMap[posX, posY - 1] != 'g' || navMeshMap[posX, posY + 1] != 'g' || //horizontal
                            navMeshMap[posX + 1, posY + 1] != 'g' || navMeshMap[posX - 1, posY - 1] != 'g' || navMeshMap[posX - 1, posY + 1] != 'g' || navMeshMap[posX + 1, posY - 1] != 'g') //diagonal
                            wallMap[posX, posY] = 'w';
                        else if (maintainGroundInfo)
                            wallMap[posX, posY] = 'g';
                    }
                    else //Sim
                        wallMap[posX, posY] = 'w';
            }

        if (changeOriginalMap)
            navMeshMap = wallMap;

        return wallMap;
    }

    public char[,] GetArea(int sizeX, int sizeY, Vector3 position)
    {
        char[,] mapArea = new char[sizeX, sizeY];

        Vector2 rightCorner = new Vector2((int)((int)(sizeX * 0.5f) + position.x), (int)((int)(sizeY * 0.5f) + position.z));

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

}
