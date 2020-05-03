using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomInfo
{
    //Prefab info
    public GameObject basePrefab;
    public GameObject prefab;
    public Vector3 prefabScale;
    public Vector3 rightCornerPos;
    public Transform entranceTransform;

    //Room Exits
    public List<PrefabExit> exitPoints = new List<PrefabExit>();

    //Entrance
    public List<PrefabExit> lastExitPoints = new List<PrefabExit>();
    public int thisEntranceIndex;
    
    //Spawn enemies?
    public bool spawnEnemies;
    //public <EnemyClass> enemies
    
    //Sector Info
    public int sector;
    public char subSector;
    public GameObject sectorDoor;

    //______________________________________________________________________________

    public RoomInfo() { }

    public RoomInfo(GameObject prefab)
    {
        this.prefab = prefab;
    }

    public RoomInfo(
        GameObject prefab, GameObject basePrefab,
        List<Transform> exitPoints, Vector3 prefabScale,
        Vector3 rightCornerPos, Transform entranceTransform, bool spawnEnemies,
        int sector, char subSector)
    {
        this.prefab = prefab;
        this.basePrefab = basePrefab;
        foreach (Transform exit in exitPoints)
            this.exitPoints.Add(new PrefabExit(exit, 'o'));
        this.prefabScale = prefabScale;
        this.rightCornerPos = rightCornerPos;
        this.entranceTransform = entranceTransform;
        this.spawnEnemies = spawnEnemies;
        this.sector = sector;
        this.subSector = subSector;
    }

    public bool OpenExits
    {
        get
        {
            foreach (PrefabExit exit in exitPoints)
            {
                if (exit.exitState == 'o' || exit.exitState == 'l')
                    return true;
            }
            return false;
        }
    }
    public int GetNAvaulableExits
    {
        get
        {
            int nExits = 0;

            foreach (PrefabExit exit in exitPoints)
            {
                if (exit.exitState == 'o')
                    nExits++;
            }

            return nExits;
        }
    }
    public int GetRandExit
    {
        get
        {
            List<int> exitIndex = new List<int>();
            int index = 0;

            foreach (PrefabExit exit in exitPoints)
            {
                if (exit.exitState == 'o' || exit.exitState == 'l')
                    exitIndex.Add(index);

                index++;
            }

            int randIndexExit = Random.Range(0, exitIndex.Count);

            if (exitIndex.Count > 0)
                return exitIndex[randIndexExit];
            else
                return 0;

        }
    }
    public bool OpenSubSectors
    {
        get
        {
            foreach (PrefabExit exit in exitPoints)
            {
                if (exit.exitState == 'x' || exit.exitState == 'l')
                    if (exit.linkedRoom != null)
                        return true;
            }
            return false;
        }
    }
    public int GetRandSubSectorPath
    {
        get
        {
            List<int> exitIndex = new List<int>();
            int index = 0;

            foreach (PrefabExit exit in exitPoints)
            {
                if (exit.exitState == 'x' || exit.exitState == 'l')
                    if (exit.linkedRoom != null)
                        exitIndex.Add(index);

                index++;
            }

            int randIndexExit = Random.Range(0, exitIndex.Count);

            if (exitIndex.Count > 0)
                return exitIndex[randIndexExit];
            else
                return 0;

        }
    }
    public int LinkedExitsCount
    {
        get
        {
            int count = 0;

            foreach (PrefabExit exit in exitPoints)
            {
                if (exit.exitState == 'x' || exit.exitState == 'l')
                    if (exit.linkedRoom != null)
                        count++;
            }

            return count;
        }
    }
    public int Cross
    {
        get
        {
            int count = 0;

            foreach (PrefabExit exit in exitPoints)
            {
                if (exit.exitState == '+')
                    if (exit.linkedRoom != null)
                        count++;
            }

            return count;
        }
    }


    //public void DebugSector()
    //{
    //    RoomEntrance roomEntrance = prefab.GetComponent<RoomEntrance>();

    //    if (roomEntrance)
    //    {
    //        if (roomEntrance.textSector)
    //            roomEntrance.textSector.GetComponent<TextMesh>().text = sector.ToString() + subSector;

    //        if (roomEntrance.exitsText != null)
    //        {
    //            int index = 0;
    //            foreach (GameObject textObj in roomEntrance.exitsText)
    //            {
    //                textObj.GetComponent<TextMesh>().text = exitPoints[index].exitState.ToString();
    //                index++;
    //            }
    //        }
    //    }
    //}

}

public class PrefabExit
{
    public Transform exitTransform;
    public RoomInfo linkedRoom;
    public char exitState;// 'o'- Open 'c'-Close 'l'-link 's'- new sector "x" path with no exits
    public PrefabExit(Transform exit, char state)
    {
        exitTransform = exit;
        exitState = state;
    }
    public PrefabExit()
    { }

}

