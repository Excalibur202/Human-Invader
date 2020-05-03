using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class MapGenerator : MonoBehaviour
{
    public static MapGenerator instance;

    //Spawned Prefabs
    public List<RoomInfo> spawnedRooms = new List<RoomInfo>();

    public NavMesh navMesh;

    //Available Prefabs
    [SerializeField]
    private List<GameObject> basicRooms = new List<GameObject>();
    [SerializeField]
    private List<GameObject> spawnRooms = new List<GameObject>();
    [SerializeField]
    private List<GameObject> cmdRooms = new List<GameObject>();
    [SerializeField]
    private List<int> cmdRoomsProb = new List<int>();
    [SerializeField]
    private List<GameObject> doors = new List<GameObject>();
    [SerializeField]
    private GameObject consolePrefab;
    [SerializeField]
    private GameObject sectorDoor;
    [SerializeField]
    private GameObject sectorRoomEntrance;


    //Random Seed
    [SerializeField]
    private int randSeed = 0;
    public bool seedOnOff;

    //Colision Map Info
    public int mapSizeX;
    public int mapSizeY;

    //Sector Info
    public int nSectors;
    public int minRoomsPSector;
    public int maxRoomsPSector;

    [SerializeField]
    public Transform playerTransform;

    public GameObject cubeTestObstacles;
    public GameObject cubeTestWalls;
    public GameObject cubeTestGround;


    private void Awake()
    {
        if (instance)
        {
            Destroy(this);
        }
        instance = this;
    }

    private void Start()
    {
        transform.tag = "MapGen";
        navMesh = new NavMesh(mapSizeX, mapSizeY);
        int lastSector = -1;

        //First exit
        spawnedRooms.Add(new RoomInfo(this.gameObject));
        spawnedRooms[0].exitPoints.Add(new PrefabExit(this.gameObject.transform, 'o'));

        //Generate map
        GenerateMap(mapSizeX, mapSizeY, minRoomsPSector, maxRoomsPSector, nSectors,
        basicRooms, spawnRooms,
        cmdRooms, cmdRoomsProb/*, halls*/,
         doors, spawnedRooms, randSeed);

        //Rearrange SubSectors
        RearrangeSubSectors();

        //Get map geometry
        navMesh.BuildNavMeshMap(spawnedRooms);

        //Get map border
        navMesh.GetMapBorder(true, true);



        ////Debug navMesh
        //for (int x = 0; x < mapSizeX; x++)
        //    for (int y = 0; y < mapSizeY; y++)
        //    {

        //        if (navMesh.GetPosChar(x, y) == 'p')
        //            Instantiate(cubeTestObstacles, new Vector3(x - 0.5f, 1, y - 0.5f), cubeTestObstacles.transform.rotation);
        //        else if (navMesh.GetPosChar(x, y) == 'w')
        //            Instantiate(cubeTestWalls, new Vector3(x - 0.5f, 1, y - 0.5f), cubeTestObstacles.transform.rotation);
        //        else if (navMesh.GetPosChar(x, y) == 'g')
        //            Instantiate(cubeTestGround, new Vector3(x - 0.5f, 1, y - 0.5f), cubeTestObstacles.transform.rotation);
        //    }



        Transform roomConsoleTransform;
        GameObject auxTerminal;
        GameObject auxDoor;
        foreach (RoomInfo room in spawnedRooms)
        {
            //Close doors
            foreach (PrefabExit exit in room.exitPoints)
            {
                if (exit.exitState == 'c' || exit.exitState == 'o')
                    CloseExit(exit.exitTransform, doors);
            }

            //Sector
            if (room.sector != lastSector)
            {
                //Spawn Sector Terminals
                if (room.basePrefab != null)
                {
                    roomConsoleTransform = room.prefab.GetComponent<RoomEntrance>().consoleTransform;
                    auxTerminal = Instantiate(consolePrefab, roomConsoleTransform.position, roomConsoleTransform.rotation);
                    auxTerminal.transform.SetParent(room.prefab.transform);
                }

                //Spawn Sector Doors
                auxDoor = Instantiate(sectorDoor, room.prefab.transform.position + sectorDoor.transform.position, room.prefab.transform.rotation);
                auxDoor.transform.SetParent(room.prefab.transform);
                room.sectorDoor = auxDoor;

                lastSector = room.sector;
            }
            else
            {
                auxDoor = Instantiate(sectorRoomEntrance, room.prefab.transform.position, room.prefab.transform.rotation);
                auxDoor.transform.SetParent(room.prefab.transform);
            }
        }
        playerTransform.position = spawnedRooms[spawnedRooms.Count - 1].prefab.GetComponent<RoomEntrance>().playerSpawnPoint.position;
    }

    private bool GenerateMap
        (int mapSizeX, int mapSizeY, int minRoomsPSector, int maxRoomsPSector, int nSectors,
         List<GameObject> basicRooms, List<GameObject> spawnRooms,
         List<GameObject> cmdRooms, List<int> cmdRoomsProb/*, List<GameObject> halls*/,
         List<GameObject> doors, List<RoomInfo> spawnedRooms, int randSeed = 69)
    {

        //Sector Path
        List<RoomInfo> sectorPath = new List<RoomInfo>();

        //Mapa de colisoes
        ColisionMap colisionMap = new ColisionMap(mapSizeX, mapSizeY);

        //N de prefabs que faltam para o mapa estar completo
        int nRooms = 1;

        //Sector
        bool sectorFirstRoom = false; ;
        int thisSector = -1;
        char thisSubSector = (char)64;
        int sectorRoomCount = 0;
        int canRemoveSector = 0;
        //int lastSector = 0;

        //Pode gerar um corredor?
        bool canSpawnHall = false;

        //A ultima peça foi um corredor?
        bool lastWasHall = false;
        bool spawnCommandRoom = true;

        //Random
        if (seedOnOff)//Seed On/Off
            Random.InitState(randSeed);
        int randExit;
        GameObject nextPrefab; // Este rand é obtido a partir da funçao NextPrefab()

        //Prefab Spawnado
        GameObject spawnedRoom;

        //Guarda o script entrada do selected prefab
        RoomEntrance roomEntrance;

        //Gerar Mapa
        while (true)
        {

            //Caso o sector nao consiga continuar
            if (thisSector > 0 && sectorPath.Count == 1 && !sectorPath[0].OpenExits)
            {
                if (canRemoveSector == sectorPath[0].sector)
                {
                    if (sectorRoomCount < minRoomsPSector)
                    {
                        if (RemoveSector(sectorPath[0].sector, ref colisionMap))
                        {

                            if (!SelectSector(spawnedRooms[spawnedRooms.Count - 1].sector, spawnedRooms, sectorPath))
                            {
                                SpawnSpawnRoom(spawnedRooms, ref colisionMap, spawnRooms);
                                return true;
                            }

                            thisSector = spawnedRooms[spawnedRooms.Count - 1].sector;
                            nSectors++;
                            nRooms = 0;
                        }
                    }
                    else nRooms = 0;
                }
            }


            //Get nRooms p/Sector
            if (nRooms <= 0 && nSectors > 0)
            {
                thisSector++;
                nRooms = Random.Range(minRoomsPSector, maxRoomsPSector + 1);
                nSectors--;
                sectorRoomCount = 0;
                sectorFirstRoom = true;
                thisSubSector = (char)64;
                canSpawnHall = false;
            }

            //Caso o sector nao tenha saidas
            if (sectorPath.Count == 1 && sectorPath[0].sector > 0 && !sectorPath[0].OpenExits)
                if (!SelectSector(sectorPath[0].sector - 1, spawnedRooms, sectorPath))
                {
                    SpawnSpawnRoom(spawnedRooms, ref colisionMap, spawnRooms);
                    return true;
                }
            if (sectorPath.Count == 1 && sectorPath[0].sector == 0 && !sectorPath[0].OpenExits)
            {
                SpawnSpawnRoom(spawnedRooms, ref colisionMap, spawnRooms);
                return true;
            }

            //Pode continuar a gerar?
            if (nRooms > 0 && spawnedRooms.Count != 0)
            {//Sim

                ////A peça antes do spawn room nao pode ser um corredor
                if (nRooms == 2 && nSectors <= 0)
                    canSpawnHall = false;


                //Obter o proximo prefab
                if (spawnCommandRoom)
                {
                    nextPrefab = cmdRooms[ListRand(cmdRoomsProb, cmdRooms)];
                    spawnCommandRoom = false;
                }
                else
                    nextPrefab = NextPrefab(basicRooms/*, halls*/, ref canSpawnHall, ref thisSubSector);

                //O prefab tem saidas?
                if ((spawnedRooms.Count == 1) || sectorPath[sectorPath.Count - 1].OpenExits)
                {

                    if (spawnedRooms.Count == 1)
                    {
                        randExit = spawnedRooms[spawnedRooms.Count - 1].GetRandExit;
                    }
                    else
                    {

                        //Escolher uma saida aleatoria
                        randExit = sectorPath[sectorPath.Count - 1].GetRandExit;
                        while (sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitState == 'l')
                        {
                            if (sectorPath[sectorPath.Count - 1].exitPoints[randExit].linkedRoom.OpenExits)
                            {
                                sectorPath.Add(sectorPath[sectorPath.Count - 1].exitPoints[randExit].linkedRoom);
                            }
                            else
                            {
                                sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitState = 'x';
                                if (!sectorPath[sectorPath.Count - 1].OpenExits)
                                {
                                    if (sectorPath.Count == 1)
                                    {
                                        if (sectorPath[0].sector > 0)
                                        {
                                            if (!SelectSector(sectorPath[0].sector - 1, spawnedRooms, sectorPath))
                                            {
                                                SpawnSpawnRoom(spawnedRooms, ref colisionMap, spawnRooms);
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            SpawnSpawnRoom(spawnedRooms, ref colisionMap, spawnRooms);
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        sectorPath[sectorPath.Count - 1].lastExitPoints[sectorPath[sectorPath.Count - 1].thisEntranceIndex].exitState = 'x';
                                        sectorPath.RemoveAt(sectorPath.Count - 1);
                                    }
                                }
                            }

                            if (sectorPath.Count == 1 && sectorPath[0].sector == 0)
                            {
                                SpawnSpawnRoom(spawnedRooms, ref colisionMap, spawnRooms);
                                return true;
                            }
                            if (sectorPath.Count >= 1)
                                randExit = sectorPath[sectorPath.Count - 1].GetRandExit;

                        }
                    }

                    if (spawnedRooms.Count == 1)
                        colisionMap.MapColision(randExit, nextPrefab, spawnedRooms.Count - 1, spawnedRooms);

                    //Há colisao?
                    if (sectorPath.Count > 0 && colisionMap.MapColision(randExit, nextPrefab, sectorPath.Count - 1, sectorPath))
                    {//Sim

                        if (sectorPath[sectorPath.Count - 1].OpenExits)
                            sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitState = 'c';

                        while (!sectorPath[sectorPath.Count - 1].OpenExits && sectorPath.Count > 1)
                        {
                            sectorPath[sectorPath.Count - 1].lastExitPoints[sectorPath[sectorPath.Count - 1].thisEntranceIndex].exitState = 'x';
                            sectorPath.RemoveAt(sectorPath.Count - 1);
                        }

                        while (sectorPath.Count > 1)
                            sectorPath.RemoveAt(1);
                    }
                    else
                    {//Caso nao exista colisao

                        //Gerar Prefab
                        if (sectorPath.Count > 0) //Sectores
                        {
                            spawnedRoom = Instantiate(nextPrefab, sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitTransform.position,
                                sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitTransform.rotation);

                            roomEntrance = spawnedRoom.GetComponent<RoomEntrance>();

                            //Adiciona a informaçao do prefab a lista de prefabs gerados
                            spawnedRooms.Add(new RoomInfo(spawnedRoom, nextPrefab, roomEntrance.exitPoints, roomEntrance.roomDimension.transform.lossyScale,
                                roomEntrance.rightCorner.transform.position, spawnedRoom.transform, roomEntrance.spawnEnemies, thisSector, thisSubSector));

                            spawnedRooms[spawnedRooms.Count - 1].lastExitPoints = sectorPath[sectorPath.Count - 1].exitPoints;
                            spawnedRooms[spawnedRooms.Count - 1].thisEntranceIndex = randExit;
                            sectorPath[sectorPath.Count - 1].exitPoints[randExit].linkedRoom = spawnedRooms[spawnedRooms.Count - 1];


                            if (sectorFirstRoom)
                            {

                                sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitState = 's';
                                sectorPath.Clear();
                                sectorPath.Add(spawnedRooms[spawnedRooms.Count - 1]);
                                sectorFirstRoom = false;
                                canRemoveSector = thisSector;
                            }
                            else
                                sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitState = 'l';

                            while (sectorPath.Count > 1)
                                sectorPath.RemoveAt(1);

                        }
                        else
                        {//Inicio do mapa
                            spawnedRoom = Instantiate(nextPrefab, spawnedRooms[spawnedRooms.Count - 1].exitPoints[randExit].exitTransform.position,
                                spawnedRooms[spawnedRooms.Count - 1].exitPoints[randExit].exitTransform.rotation);

                            //Obter entrada
                            roomEntrance = spawnedRoom.GetComponent<RoomEntrance>();

                            ////Fazer a ligaçao entre quartos
                            spawnedRooms[spawnedRooms.Count - 1].exitPoints[randExit].exitState = 's';

                            //Adiciona a informaçao do prefab a lista de prefabs gerados
                            spawnedRooms.Add(new RoomInfo(spawnedRoom, nextPrefab, roomEntrance.exitPoints, roomEntrance.roomDimension.transform.lossyScale,
                                roomEntrance.rightCorner.transform.position, spawnedRoom.transform, roomEntrance.spawnEnemies, -1, '*'));

                            spawnedRooms[spawnedRooms.Count - 1].lastExitPoints = spawnedRooms[spawnedRooms.Count - 2].exitPoints;
                            spawnedRooms[spawnedRooms.Count - 1].thisEntranceIndex = randExit;
                            spawnedRooms[spawnedRooms.Count - 2].exitPoints[randExit].linkedRoom = spawnedRooms[spawnedRooms.Count - 1];

                            sectorPath.Add(spawnedRooms[spawnedRooms.Count - 1]);
                            sectorFirstRoom = false;
                        }

                        ////Info para o caso de existir colisao depois de um corredor
                        lastWasHall = !canSpawnHall;
                        sectorRoomCount++;
                        thisSubSector++;
                        nRooms--;
                    }
                }
                else
                {
                    //Caso o prefab seleccionado nao tenha saidas
                    canSpawnHall = false;

                    if (sectorPath.Count > 1)
                        sectorPath[sectorPath.Count - 1].lastExitPoints[sectorPath[sectorPath.Count - 1].thisEntranceIndex].exitState = 'x';

                    while (!sectorPath[sectorPath.Count - 1].OpenExits && sectorPath.Count > 1)
                    {
                        sectorPath[sectorPath.Count - 1].lastExitPoints[sectorPath[sectorPath.Count - 1].thisEntranceIndex].exitState = 'x';
                        sectorPath.RemoveAt(sectorPath.Count - 1);
                    }

                    while (sectorPath.Count > 1)
                        sectorPath.RemoveAt(1);
                }
            }
            else
            {
                SpawnSpawnRoom(spawnedRooms, ref colisionMap, spawnRooms);
                return true;
            }
        }
    }

    private GameObject NextPrefab(List<GameObject> availableRooms/*, List<GameObject> availableHalls*/, ref bool canSpawnHall, ref char thisSubSector)
    {
        return availableRooms[Random.Range(0, availableRooms.Count)];
    }

    private int ListRand(List<int> probs, List<GameObject> objList)
    {
        int rand = 0;
        int total = 0;

        foreach (int prob in probs)
            total += prob;

        rand = Random.Range(0, total + 1);

        total = 0;
        int indexObstacle = -1;
        foreach (int prob in probs)
            if (rand >= total)
            {
                total += prob;
                indexObstacle++;
            }

        return indexObstacle;
    }

    private void CloseExit(Transform exit, List<GameObject> doors)
    {
        if (doors.Count > 0)
        {
            int randDoor = Random.Range(0, doors.Count);

            GameObject auxDoor = Instantiate(doors[randDoor], exit.position + doors[randDoor].transform.position, exit.rotation);
            auxDoor.transform.SetParent(exit);
        }
    }

    private bool RemoveSector(int sector, ref ColisionMap colisionMap)
    {
        bool sectorRemoved = false;

        if (sector > 0)
            for (int roomIndex = spawnedRooms.Count - 1; roomIndex > 0; roomIndex--)
            {
                if (spawnedRooms[roomIndex].sector == sector)
                {
                    colisionMap.RemoveFromMapColision(spawnedRooms[roomIndex]);
                    spawnedRooms[roomIndex].lastExitPoints[spawnedRooms[roomIndex].thisEntranceIndex].exitState = 'c';
                    Destroy(spawnedRooms[roomIndex].prefab);
                    spawnedRooms.RemoveAt(roomIndex);
                    roomIndex = spawnedRooms.Count;
                    sectorRemoved = true;
                }
            }

        return sectorRemoved;
    }

    private bool SelectSector(int sector, List<RoomInfo> spawnedRooms, List<RoomInfo> sectorPath)
    {
        foreach (RoomInfo room in spawnedRooms)
        {
            if (room.sector == sector && room.subSector == (char)64)
            {
                sectorPath.Clear();
                sectorPath.Add(room);
                return true;
            }
        }
        return false;
    }

    private void SpawnSpawnRoom(List<RoomInfo> spawnedRooms, ref ColisionMap colisionMap, List<GameObject> spawnRooms)
    {
        List<RoomInfo> sectorPath = new List<RoomInfo>();
        GameObject spawnedRoom;
        GameObject nextRoom = spawnRooms[0];
        int randExit = 0;


        SelectSector(spawnedRooms[spawnedRooms.Count - 1].sector, spawnedRooms, sectorPath);

        while (true)
            if (sectorPath[sectorPath.Count - 1].OpenExits)
            {
                randExit = sectorPath[sectorPath.Count - 1].GetRandExit;
                if (sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitState == 'l')
                    sectorPath.Add(sectorPath[sectorPath.Count - 1].exitPoints[randExit].linkedRoom);
                else
                {
                    if (colisionMap.MapColision(randExit, nextRoom, sectorPath.Count - 1, sectorPath))
                    {//Ha colisao

                        sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitState = 'c';

                        while (!sectorPath[sectorPath.Count - 1].OpenExits && sectorPath.Count > 1)
                        {
                            sectorPath[sectorPath.Count - 1].lastExitPoints[sectorPath[sectorPath.Count - 1].thisEntranceIndex].exitState = 'x';
                            sectorPath.RemoveAt(sectorPath.Count - 1);
                        }

                        while (sectorPath.Count > 1)
                            sectorPath.RemoveAt(1);
                    }
                    else
                    {//Nao ha colisao
                        spawnedRoom = Instantiate(nextRoom, sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitTransform.position, sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitTransform.rotation);

                        RoomEntrance roomEntrance = spawnedRoom.GetComponent<RoomEntrance>();

                        //Adiciona a informaçao do prefab a lista de prefabs gerados
                        spawnedRooms.Add(new RoomInfo(spawnedRoom, nextRoom, roomEntrance.exitPoints, roomEntrance.roomDimension.transform.lossyScale,
                            roomEntrance.rightCorner.transform.position, spawnedRoom.transform, roomEntrance.spawnEnemies, -2, '*'));

                        spawnedRooms[spawnedRooms.Count - 1].lastExitPoints = sectorPath[sectorPath.Count - 1].exitPoints;
                        spawnedRooms[spawnedRooms.Count - 1].thisEntranceIndex = randExit;
                        sectorPath[sectorPath.Count - 1].exitPoints[randExit].linkedRoom = spawnedRooms[spawnedRooms.Count - 1];
                        spawnedRooms[spawnedRooms.Count - 1].lastExitPoints[randExit].exitState = 'x';
                        break;
                    }
                }
            }
            else
            {
                if (sectorPath.Count == 1)
                {
                    List<PrefabExit> lastExits = sectorPath[0].lastExitPoints;
                    int lastExitIndex = sectorPath[0].thisEntranceIndex;
                    RemoveSector(sectorPath[0].sector, ref colisionMap);

                    spawnedRoom = Instantiate(nextRoom, lastExits[lastExitIndex].exitTransform.position, lastExits[lastExitIndex].exitTransform.rotation);

                    RoomEntrance roomEntrance = spawnedRoom.GetComponent<RoomEntrance>();

                    //Adiciona a informaçao do prefab a lista de prefabs gerados
                    spawnedRooms.Add(new RoomInfo(spawnedRoom, nextRoom, roomEntrance.exitPoints, roomEntrance.roomDimension.transform.lossyScale,
                        roomEntrance.rightCorner.transform.position, spawnedRoom.transform, roomEntrance.spawnEnemies, -2, '*'));

                    spawnedRooms[spawnedRooms.Count - 1].lastExitPoints = sectorPath[sectorPath.Count - 1].exitPoints;
                    spawnedRooms[spawnedRooms.Count - 1].thisEntranceIndex = randExit;
                    sectorPath[sectorPath.Count - 1].exitPoints[randExit].linkedRoom = spawnedRooms[spawnedRooms.Count - 1];

                    lastExits[lastExitIndex].exitState = 'l';
                    break;
                }
            }
    }

    private void RearrangeSubSectors()
    {
        char subSector = '@';
        List<RoomInfo> sectorPath = new List<RoomInfo>();

        bool test = false;
        bool first = true;

        SelectSector(spawnedRooms[spawnedRooms.Count - 2].sector, spawnedRooms, sectorPath);

        while (true)
        {
            if (sectorPath[sectorPath.Count - 1].OpenSubSectors)
            {
                if (sectorPath[sectorPath.Count - 1].LinkedExitsCount > 1)
                {
                    subSector++;
                    first = false;
                    int exit = sectorPath[sectorPath.Count - 1].GetRandSubSectorPath;
                    sectorPath[sectorPath.Count - 1].exitPoints[exit].exitState = '+';
                    sectorPath.Add(sectorPath[sectorPath.Count - 1].exitPoints[exit].linkedRoom);
                    sectorPath[sectorPath.Count - 1].subSector = subSector;
                }
                else if (sectorPath[sectorPath.Count - 1].LinkedExitsCount == 1)
                {
                    if (sectorPath[sectorPath.Count - 1].Cross >= 1 || test || first)
                    {
                        subSector++;
                        test = false;
                        first = false;
                    }

                    int exit = sectorPath[sectorPath.Count - 1].GetRandSubSectorPath;
                    sectorPath[sectorPath.Count - 1].exitPoints[exit].exitState = '-';
                    sectorPath.Add(sectorPath[sectorPath.Count - 1].exitPoints[exit].linkedRoom);
                    sectorPath[sectorPath.Count - 1].subSector = subSector;

                }
            }
            else
            {
                if (sectorPath[sectorPath.Count - 1].sector == 0 && sectorPath.Count == 1)
                    break;

                if (sectorPath.Count > 1)
                {
                    sectorPath.RemoveAt(sectorPath.Count - 1);
                    test = true;
                }
                else
                {
                    subSector = '@';
                    SelectSector(sectorPath[0].sector - 1, spawnedRooms, sectorPath);
                    test = false;
                }
            }
        }

    }

}