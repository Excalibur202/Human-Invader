using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColisionMap
{
    bool[,] mapArray; //Por a private
    int mapSizeX;
    int mapSizeY;

    public ColisionMap(int mapSizeX, int mapSizeY)
    {
        this.mapArray = new bool[mapSizeX, mapSizeY];
        this.mapSizeX = mapSizeX;
        this.mapSizeY = mapSizeY;
    }

    public bool MapColision(int exit, GameObject nextRoom, int selectedRoom,  List<RoomInfo> spawnedPrefabs)
    {
        Vector2 exit2DForward;
        Vector2 exit2DPos;

        Vector2 rightCornerRelative2DPos;
        Vector2 prefab2DScale;

        Vector2 up = new Vector3(0, 1);
        Vector2 down = new Vector3(0, -1);
        Vector2 right = new Vector3(1, 0);
        Vector2 left = new Vector3(-1, 0);



        //Exit info
        {
            Transform exitTransform = spawnedPrefabs[selectedRoom].exitPoints[exit].exitTransform;
            exit2DForward = new Vector2((int)exitTransform.forward.normalized.x, (int)exitTransform.forward.normalized.z);
            exit2DPos = new Vector2(Mathf.RoundToInt(exitTransform.position.x), Mathf.RoundToInt(exitTransform.position.z));
        }

        //Prefab info
        {
            RoomEntrance prefabInfo = nextRoom.GetComponent<RoomEntrance>();
            Vector3 rightCornerPos = prefabInfo.rightCorner.position;
            rightCornerRelative2DPos = new Vector2(Mathf.RoundToInt(rightCornerPos.x), Mathf.RoundToInt(rightCornerPos.z));
            prefab2DScale = new Vector2((int)prefabInfo.roomDimension.transform.lossyScale.x, (int)prefabInfo.roomDimension.transform.lossyScale.z);

        }


        int deltaX;
        int deltaY;

        if (exit2DForward == up)//Exit Up
        {
            Vector2 rightCornerArrayPos = new Vector2(exit2DPos.x + rightCornerRelative2DPos.x , exit2DPos.y + rightCornerRelative2DPos.y );
            deltaX = (int)(rightCornerArrayPos.x - prefab2DScale.x);
            deltaY = (int)(rightCornerArrayPos.y - prefab2DScale.y);

            for (int x = (int)rightCornerArrayPos.x; x > deltaX; x--)
            {
                for (int y = (int)rightCornerArrayPos.y; y > deltaY; y--)
                {
                    if (x > (mapSizeX - 1) || y > (mapSizeY - 1) || x < 0 || y < 0 || mapArray[x, y])
                    {
                        return true;
                    }
                }
            }

            for (int x = (int)rightCornerArrayPos.x; x > deltaX; x--)
            {
                for (int y = (int)rightCornerArrayPos.y; y > deltaY; y--)
                {
                    mapArray[x, y] = true;
                }
            }
        }
        else if (exit2DForward == down)//Exit Down
        {
            Vector2 rightCornerArrayPos = new Vector2(exit2DPos.x - rightCornerRelative2DPos.x, exit2DPos.y - rightCornerRelative2DPos.y);
            rightCornerArrayPos.x = rightCornerArrayPos.x + 1;
            rightCornerArrayPos.y = rightCornerArrayPos.y + 1;

            deltaX = (int)(rightCornerArrayPos.x + prefab2DScale.x);
            deltaY = (int)(rightCornerArrayPos.y + prefab2DScale.y);

            for (int x = (int)rightCornerArrayPos.x; x < deltaX; x++)
            {
                for (int y = (int)rightCornerArrayPos.y; y < deltaY; y++)
                {
                    if (x > (mapSizeX - 1) || y > (mapSizeY - 1) || x < 0 || y < 0 || mapArray[x, y])
                    {
                        return true;
                    }
                }
            }

            for (int x = (int)rightCornerArrayPos.x; x < deltaX; x++)
            {
                for (int y = (int)rightCornerArrayPos.y; y < deltaY; y++)
                {
                    mapArray[x, y] = true;
                }
            }

        }
        else if (exit2DForward == right)//Exit Right
        {
            Vector2 rightCornerArrayPos = new Vector2(exit2DPos.x + rightCornerRelative2DPos.y, exit2DPos.y - rightCornerRelative2DPos.x);
            rightCornerArrayPos.y = rightCornerArrayPos.y + 1;

            deltaX = (int)(rightCornerArrayPos.x - prefab2DScale.y);
            deltaY = (int)(rightCornerArrayPos.y + prefab2DScale.x);

            for (int x = (int)rightCornerArrayPos.x; x > deltaX; x--)
            {
                for (int y = (int)rightCornerArrayPos.y; y < deltaY; y++)
                {
                    if (x > (mapSizeX - 1) || y > (mapSizeY - 1) || x < 0 || y < 0 || mapArray[x, y])
                    {
                        return true;
                    }
                }
            }

            for (int x = (int)rightCornerArrayPos.x; x > deltaX; x--)
            {
                for (int y = (int)rightCornerArrayPos.y; y < deltaY; y++)
                {
                    mapArray[x, y] = true;
                }
            }
        }
        else if (exit2DForward == left)//Exit Left
        {

            //Get corner pos in the arrayMap
            Vector2 rightCornerArrayPos = new Vector2(exit2DPos.x - rightCornerRelative2DPos.y, exit2DPos.y + rightCornerRelative2DPos.x );
            rightCornerArrayPos.x = rightCornerArrayPos.x + 1;

            //Calculate delta relative to the mapArray
            deltaX = (int)(rightCornerArrayPos.x + prefab2DScale.y);
            deltaY = (int)(rightCornerArrayPos.y - prefab2DScale.x);

            //Colides?
            for (int x = (int)rightCornerArrayPos.x; x < deltaX; x++)
            {
                for (int y = (int)rightCornerArrayPos.y; y > deltaY; y--)
                {
                    if (x > (mapSizeX - 1) || y > (mapSizeY - 1) || x < 0 || y < 0 || mapArray[x, y])
                    {
                        return true;
                    }
                }
            }
            //If not coliding
            for (int x = (int)rightCornerArrayPos.x; x < deltaX; x++)
            {
                for (int y = (int)rightCornerArrayPos.y; y > deltaY; y--)
                {
                    mapArray[x, y] = true;
                }
            }
        }
        return false;
    }

    public bool RemoveFromMapColision(RoomInfo roomToRemove)
    {
        Vector2 exit2DForward;
        Vector2 exit2DPos;

        Vector2 rightCornerRelative2DPos;
        Vector2 prefab2DScale;

        Vector2 up = new Vector3(0, 1);
        Vector2 down = new Vector3(0, -1);
        Vector2 right = new Vector3(1, 0);
        Vector2 left = new Vector3(-1, 0);

        //Exit info
        {
            Transform exitTransform = roomToRemove.prefab.transform;
            exit2DForward = new Vector2((int)exitTransform.forward.normalized.x, (int)exitTransform.forward.normalized.z);
            exit2DPos = new Vector2(Mathf.RoundToInt(exitTransform.position.x), Mathf.RoundToInt(exitTransform.position.z));
        }

        //Prefab info
        {
            RoomEntrance prefabInfo = roomToRemove.basePrefab.GetComponent<RoomEntrance>();
            Vector3 rightCornerPos = prefabInfo.rightCorner.position;
            rightCornerRelative2DPos = new Vector2(Mathf.RoundToInt(rightCornerPos.x), Mathf.RoundToInt(rightCornerPos.z));
            prefab2DScale = new Vector2((int)prefabInfo.roomDimension.transform.lossyScale.x, (int)prefabInfo.roomDimension.transform.lossyScale.z);

        }

        int deltaX;
        int deltaY;

        if (exit2DForward == up)//Exit Up
        {
            Vector2 rightCornerArrayPos = new Vector2(exit2DPos.x + rightCornerRelative2DPos.x, exit2DPos.y + rightCornerRelative2DPos.y);

            deltaX = (int)(rightCornerArrayPos.x - prefab2DScale.x);
            deltaY = (int)(rightCornerArrayPos.y - prefab2DScale.y);
            
            for (int x = (int)rightCornerArrayPos.x; x > deltaX; x--)
            {
                for (int y = (int)rightCornerArrayPos.y; y > deltaY; y--)
                {
                    mapArray[x, y] = false;
                }
            }

        }
        else if (exit2DForward == down)//Exit Down
        {
            Vector2 rightCornerArrayPos = new Vector2(exit2DPos.x - rightCornerRelative2DPos.x, exit2DPos.y - rightCornerRelative2DPos.y);
            rightCornerArrayPos.x = rightCornerArrayPos.x + 1;
            rightCornerArrayPos.y = rightCornerArrayPos.y + 1;

            deltaX = (int)(rightCornerArrayPos.x + prefab2DScale.x);
            deltaY = (int)(rightCornerArrayPos.y + prefab2DScale.y);



            for (int x = (int)rightCornerArrayPos.x; x < deltaX; x++)
            {
                for (int y = (int)rightCornerArrayPos.y; y < deltaY; y++)
                {
                    mapArray[x, y] = false;
                }
            }

        }
        else if (exit2DForward == right)//Exit Right
        {
            Vector2 rightCornerArrayPos = new Vector2(exit2DPos.x + rightCornerRelative2DPos.y , exit2DPos.y - rightCornerRelative2DPos.x);
            rightCornerArrayPos.y = rightCornerArrayPos.y + 1;

            deltaX = (int)(rightCornerArrayPos.x - prefab2DScale.y);
            deltaY = (int)(rightCornerArrayPos.y + prefab2DScale.x);



            for (int x = (int)rightCornerArrayPos.x; x > deltaX; x--)
            {
                for (int y = (int)rightCornerArrayPos.y; y < deltaY; y++)
                {
                    mapArray[x, y] = false;
                }
            }
        }
        else if (exit2DForward == left)//Exit Left
        {

            //Get corner pos in the arrayMap
            Vector2 rightCornerArrayPos = new Vector2(exit2DPos.x - rightCornerRelative2DPos.y, exit2DPos.y + rightCornerRelative2DPos.x );
            rightCornerArrayPos.x = rightCornerArrayPos.x + 1;

            //Calculate delta relative to the mapArray
            deltaX = (int)(rightCornerArrayPos.x + prefab2DScale.y);
            deltaY = (int)(rightCornerArrayPos.y - prefab2DScale.x);
            
            //If not coliding
            for (int x = (int)rightCornerArrayPos.x; x < deltaX; x++)
            {
                for (int y = (int)rightCornerArrayPos.y; y > deltaY; y--)
                {
                    mapArray[x, y] = false;
                }
            }
        }
        return false;
    }


    public bool RemoveFromMapColision(int oneStepBackExit, int oneStepBackSpawnedPrefab, GameObject prefabToRemove,  List<RoomInfo> spawnedPrefabs)
    {
        Vector2 exit2DForward;
        Vector2 exit2DPos;

        Vector2 rightCornerRelative2DPos;
        Vector2 prefab2DScale;

        Vector2 up = new Vector3(0, 1);
        Vector2 down = new Vector3(0, -1);
        Vector2 right = new Vector3(1, 0);
        Vector2 left = new Vector3(-1, 0);

        //Exit info
        {
            Transform exitTransform = spawnedPrefabs[oneStepBackSpawnedPrefab].exitPoints[oneStepBackExit].exitTransform;
            exit2DForward = new Vector2((int)exitTransform.forward.normalized.x, (int)exitTransform.forward.normalized.z);
            exit2DPos = new Vector2(Mathf.RoundToInt(exitTransform.position.x), Mathf.RoundToInt(exitTransform.position.z));
        }

        //Prefab info
        {
            RoomEntrance prefabInfo = prefabToRemove.GetComponent<RoomEntrance>();
            Vector3 rightCornerPos = prefabInfo.rightCorner.position;
            rightCornerRelative2DPos = new Vector2(Mathf.RoundToInt(rightCornerPos.x), Mathf.RoundToInt(rightCornerPos.z));
            prefab2DScale = new Vector2((int)prefabInfo.roomDimension.transform.lossyScale.x, (int)prefabInfo.roomDimension.transform.lossyScale.z);

        }

        int deltaX;
        int deltaY;

        if (exit2DForward == up)//Exit Up
        {
            Vector2 rightCornerArrayPos = new Vector2(exit2DPos.x + rightCornerRelative2DPos.x - 1, exit2DPos.y + rightCornerRelative2DPos.y - 1);

            deltaX = (int)(rightCornerArrayPos.x - prefab2DScale.x);
            deltaY = (int)(rightCornerArrayPos.y - prefab2DScale.y);



            for (int x = (int)rightCornerArrayPos.x; x > deltaX; x--)
            {
                for (int y = (int)rightCornerArrayPos.y; y > deltaY; y--)
                {
                    mapArray[x, y] = false;
                }
            }

        }
        else if (exit2DForward == down)//Exit Down
        {
            Vector2 rightCornerArrayPos = new Vector2(exit2DPos.x - rightCornerRelative2DPos.x, exit2DPos.y - rightCornerRelative2DPos.y);

            deltaX = (int)(rightCornerArrayPos.x + prefab2DScale.x);
            deltaY = (int)(rightCornerArrayPos.y + prefab2DScale.y);



            for (int x = (int)rightCornerArrayPos.x; x < deltaX; x++)
            {
                for (int y = (int)rightCornerArrayPos.y; y < deltaY; y++)
                {
                    mapArray[x, y] = false;
                }
            }

        }
        else if (exit2DForward == right)//Exit Right
        {
            Vector2 rightCornerArrayPos = new Vector2(exit2DPos.x + rightCornerRelative2DPos.y - 1, exit2DPos.y - rightCornerRelative2DPos.x);

            deltaX = (int)(rightCornerArrayPos.x - prefab2DScale.y);
            deltaY = (int)(rightCornerArrayPos.y + prefab2DScale.x);



            for (int x = (int)rightCornerArrayPos.x; x > deltaX; x--)
            {
                for (int y = (int)rightCornerArrayPos.y; y < deltaY; y++)
                {
                    mapArray[x, y] = false;
                }
            }
        }
        else if (exit2DForward == left)//Exit Left
        {

            //Get corner pos in the arrayMap
            Vector2 rightCornerArrayPos = new Vector2(exit2DPos.x - rightCornerRelative2DPos.y, exit2DPos.y + rightCornerRelative2DPos.x - 1);

            //Calculate delta relative to the mapArray
            deltaX = (int)(rightCornerArrayPos.x + prefab2DScale.y);
            deltaY = (int)(rightCornerArrayPos.y - prefab2DScale.x);

            //If not coliding
            for (int x = (int)rightCornerArrayPos.x; x < deltaX; x++)
            {
                for (int y = (int)rightCornerArrayPos.y; y > deltaY; y--)
                {
                    mapArray[x, y] = false;
                }
            }
        }
        return false;
    }
    
}
