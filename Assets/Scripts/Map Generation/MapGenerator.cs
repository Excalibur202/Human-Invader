using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour {
    public bool recreateMap = false;

    //Static instance
    public static MapGenerator instance;

    //Spawned Prefabs
    public List<RoomInfo> spawnedRooms = new List<RoomInfo> ();

    //Random Seed
    [SerializeField]
    private int randSeed = 0;
    public bool seedOnOff;

    [SerializeField]
    private GameObject sectorDoor;
    [SerializeField]
    private GameObject sectorRoomEntrance;

    //Colision Map Info
    public int mapSizeX;
    public int mapSizeY;

    //Map Info
    public int nSectors;
    public NavMesh navMesh;
    public int minRoomsPSector;
    public int maxRoomsPSector;
    [SerializeField]
    private List<GameObject> basicRooms = new List<GameObject> ();
    [SerializeField]
    private List<GameObject> spawnRooms = new List<GameObject> ();
    [SerializeField]
    private List<GameObject> cmdRooms = new List<GameObject> ();
    [SerializeField]
    private List<int> cmdRoomsProb = new List<int> ();
    [SerializeField]
    private List<GameObject> closedDoors = new List<GameObject> ();

    //Enemies
    public bool spawnEnemies = false;
    public int minEnemies;
    public int maxEnemies;
    [SerializeField]
    private List<GameObject> enemies = new List<GameObject> ();
    [SerializeField]
    private List<int> enemyProbs = new List<int> ();

    //AI Training
    public bool trainingAI = false;
    [SerializeField]
    private GameObject aIEnemyTraining;
    [SerializeField]
    private GameObject baseEnemyAI;

    //Player
    [SerializeField]
    public Transform playerTransform;

    private void Awake () {
        //Set instance
        if (instance) {
            Destroy (this);
        }
        instance = this;
    }

    private void Start () {
        transform.tag = "MapGen";
        if(trainingAI)
        {
            RestartSimulation();
        }else
        {
            //Generate map
            MapInit();

            //Set player pos (spawn room)
            SetPlayerPosToSpawnPos();
        }
       

    }

    private void Update () {
        //Recreate map
        if (recreateMap) {
            //AI training?
            if (trainingAI) {
                RestartSimulation ();
            } else {
                MapInit ();
                //Set player pos (spawn room)
                SetPlayerPosToSpawnPos ();
            }
            recreateMap = false;
        }
    }

    #region Map Generation Funcs
    //_____________________________________________________________________________________________________
    //Map Generation Funcs

    /*MapInit*/
    public void MapInit () {
        //Destroy map
        while (spawnedRooms.Count > 0) {
            if (spawnedRooms.Count != 1) {
                while (spawnedRooms[1].enemies.Count > 0) {
                    Destroy (spawnedRooms[1].enemies[0]);
                    spawnedRooms[1].enemies.RemoveAt (0);
                }
                Destroy (spawnedRooms[1].prefab);
                spawnedRooms.RemoveAt (1);
            } else
                spawnedRooms.RemoveAt (0);
        }

        navMesh = new NavMesh (mapSizeX, mapSizeY);
        int lastSector = -1;

        //First exit
        spawnedRooms.Add (new RoomInfo (this.gameObject));
        spawnedRooms[0].exitPoints.Add (new PrefabExit (this.gameObject.transform, 'o'));

        //Generate map
        GenerateMap (mapSizeX, mapSizeY, minRoomsPSector, maxRoomsPSector, nSectors,
            basicRooms, spawnRooms,
            cmdRooms, cmdRoomsProb /*, halls*/ ,
            closedDoors, spawnedRooms, randSeed);

        //Rearrange SubSectors
        RearrangeSubSectors ();

        //Get map geometry
        navMesh.RoomsToNavMesh (spawnedRooms);

        //Get map border
        navMesh.navMeshMap = navMesh.GetMapBorder (true, true, navMesh.navMeshMap);

        GameObject auxDoor;
        foreach (RoomInfo room in spawnedRooms) {
            CloseRoomExits (room);

            //Sector
            if (room.basePrefab != null) {
                if (room.sector != lastSector && room.sector >= -1) //first room of sector? not a spawn room?
                {
                    //Spawn Sector Doors
                    if (sectorDoor) {
                        auxDoor = Instantiate (sectorDoor, room.prefab.transform.position + sectorDoor.transform.position, room.prefab.transform.rotation);
                        auxDoor.transform.SetParent (room.prefab.transform);
                        navMesh.ObstacleToNavMesh (auxDoor.GetComponent<NavMeshObstacle> (), 's');
                        room.sectorDoor = auxDoor;
                    }

                    //Spawn Enemies
                    if ( /*room.sector >= -2 &&*/ room.sector >= 0 && spawnEnemies) {
                        SpawnEnemies (minEnemies, maxEnemies, enemies, enemyProbs, spawnedRooms, room.sector);
                        AssociateKeycards (room.sector);
                    }

                    lastSector = room.sector;
                } else {
                    if (sectorRoomEntrance) {
                        auxDoor = Instantiate (sectorRoomEntrance, room.prefab.transform.position, room.prefab.transform.rotation);
                        auxDoor.transform.SetParent (room.prefab.transform);
                    }
                }

                //Debug sectors
                room.GetRoomEntrance ().textMesh.text = "" + room.sector + room.subSector;
            }
        }

    }

    /*Generates Map*/
    private bool GenerateMap
        (int mapSizeX, int mapSizeY, int minRoomsPSector, int maxRoomsPSector, int nSectors,
            List<GameObject> basicRooms, List<GameObject> spawnRooms,
            List<GameObject> cmdRooms, List<int> cmdRoomsProb /*, List<GameObject> halls*/ ,
            List<GameObject> doors, List<RoomInfo> spawnedRooms, int randSeed = 69) {

            //Sector Path
            List<RoomInfo> sectorPath = new List<RoomInfo> ();

            //Mapa de colisoes
            ColisionMap colisionMap = new ColisionMap (mapSizeX, mapSizeY);

            //N de prefabs que faltam para o mapa estar completo
            int nRooms = 1;

            //Sector
            bool sectorFirstRoom = false;;
            int thisSector = -1;
            char thisSubSector = (char) 64;
            int sectorRoomCount = 0;
            int canRemoveSector = 0;

            //A ultima peça foi um corredor?
            bool spawnCommandRoom = true;

            //Random
            if (seedOnOff) //Seed On/Off
                Random.InitState (randSeed);
            int randExit;
            GameObject nextPrefab; // Este rand é obtido a partir da funçao NextPrefab()

            //Prefab Spawnado
            GameObject spawnedRoom;

            //Guarda o script entrada do selected prefab
            RoomEntrance roomEntrance;

            //Gerar Mapa
            while (true) {

                //Caso o sector nao consiga continuar
                if (thisSector > 0 && sectorPath.Count == 1 && !sectorPath[0].OpenExits) {
                    if (canRemoveSector == sectorPath[0].sector) {
                        if (sectorRoomCount < minRoomsPSector) {
                            if (RemoveSector (sectorPath[0].sector, ref colisionMap)) {
                                if (!SelectSector (spawnedRooms[spawnedRooms.Count - 1].sector, spawnedRooms, sectorPath)) {
                                    SpawnSpawnRoom (spawnedRooms, ref colisionMap, spawnRooms);
                                    return true;
                                }

                                thisSector = spawnedRooms[spawnedRooms.Count - 1].sector;
                                nSectors++;
                                nRooms = 0;
                            }
                        } else nRooms = 0;
                    }
                }
                //Get nRooms p/Sector
                if (nRooms <= 0 && nSectors > 0) {
                    thisSector++;
                    nRooms = Random.Range (minRoomsPSector, maxRoomsPSector + 1);
                    nSectors--;
                    sectorRoomCount = 0;
                    sectorFirstRoom = true;
                    thisSubSector = (char) 64;
                }
                //Caso o sector nao tenha saidas
                if (sectorPath.Count == 1 && sectorPath[0].sector > 0 && !sectorPath[0].OpenExits)
                    if (!SelectSector (sectorPath[0].sector - 1, spawnedRooms, sectorPath)) {
                        SpawnSpawnRoom (spawnedRooms, ref colisionMap, spawnRooms);
                        return true;
                    }
                if (sectorPath.Count == 1 && sectorPath[0].sector == 0 && !sectorPath[0].OpenExits) {
                    SpawnSpawnRoom (spawnedRooms, ref colisionMap, spawnRooms);
                    return true;
                }
                //Pode continuar a gerar?
                if (nRooms > 0 && spawnedRooms.Count != 0) { //Sim

                    //Obter o proximo prefab
                    if (spawnCommandRoom) {
                        nextPrefab = cmdRooms[ListRand (cmdRoomsProb)];
                        spawnCommandRoom = false;
                    } else
                        nextPrefab = NextPrefab (basicRooms);

                    //O prefab tem saidas?
                    if ((spawnedRooms.Count == 1) || sectorPath[sectorPath.Count - 1].OpenExits) {
                        if (spawnedRooms.Count == 1) {
                            randExit = spawnedRooms[spawnedRooms.Count - 1].GetRandExit;
                        } else {
                            //Escolher uma saida aleatoria
                            randExit = sectorPath[sectorPath.Count - 1].GetRandExit;
                            while (sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitState == 'l') {
                                if (sectorPath[sectorPath.Count - 1].exitPoints[randExit].linkedRoom.OpenExits) {
                                    sectorPath.Add (sectorPath[sectorPath.Count - 1].exitPoints[randExit].linkedRoom);
                                } else {
                                    sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitState = 'x';
                                    if (!sectorPath[sectorPath.Count - 1].OpenExits) {
                                        if (sectorPath.Count == 1) {
                                            if (sectorPath[0].sector > 0) {
                                                if (!SelectSector (sectorPath[0].sector - 1, spawnedRooms, sectorPath)) {
                                                    SpawnSpawnRoom (spawnedRooms, ref colisionMap, spawnRooms);
                                                    return true;
                                                }
                                            } else {
                                                SpawnSpawnRoom (spawnedRooms, ref colisionMap, spawnRooms);
                                                return true;
                                            }
                                        } else {
                                            sectorPath[sectorPath.Count - 1].lastExitPoints[sectorPath[sectorPath.Count - 1].thisEntranceIndex].exitState = 'x';
                                            sectorPath.RemoveAt (sectorPath.Count - 1);
                                        }
                                    }
                                }
                                if (sectorPath.Count >= 1)
                                    randExit = sectorPath[sectorPath.Count - 1].GetRandExit;

                            }
                        }

                        if (spawnedRooms.Count == 1)
                            colisionMap.MapColision (randExit, nextPrefab, spawnedRooms.Count - 1, spawnedRooms);

                        //Há colisao?
                        if (sectorPath.Count > 0 && colisionMap.MapColision (randExit, nextPrefab, sectorPath.Count - 1, sectorPath)) { //Sim

                            if (sectorPath[sectorPath.Count - 1].OpenExits)
                                sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitState = 'c';

                            while (!sectorPath[sectorPath.Count - 1].OpenExits && sectorPath.Count > 1) {
                                sectorPath[sectorPath.Count - 1].lastExitPoints[sectorPath[sectorPath.Count - 1].thisEntranceIndex].exitState = 'x';
                                sectorPath.RemoveAt (sectorPath.Count - 1);
                            }

                            while (sectorPath.Count > 1)
                                sectorPath.RemoveAt (1);
                        } else { //Caso nao exista colisao

                            //Gerar Prefab
                            if (sectorPath.Count > 0) //Sectores
                            {
                                spawnedRoom = Instantiate (nextPrefab, sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitTransform.position,
                                    sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitTransform.rotation);

                                roomEntrance = spawnedRoom.GetComponent<RoomEntrance> ();
                                roomEntrance.sector = thisSector;

                                //Adiciona a informaçao do prefab a lista de prefabs gerados
                                spawnedRooms.Add (new RoomInfo (spawnedRoom, nextPrefab, roomEntrance.exitPoints, roomEntrance.roomDimension.transform.lossyScale,
                                    roomEntrance.rightCorner.transform.position, spawnedRoom.transform, thisSector, thisSubSector));

                                spawnedRooms[spawnedRooms.Count - 1].lastExitPoints = sectorPath[sectorPath.Count - 1].exitPoints;
                                spawnedRooms[spawnedRooms.Count - 1].thisEntranceIndex = randExit;
                                sectorPath[sectorPath.Count - 1].exitPoints[randExit].linkedRoom = spawnedRooms[spawnedRooms.Count - 1];

                                if (sectorFirstRoom) {

                                    sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitState = 's';
                                    sectorPath.Clear ();
                                    sectorPath.Add (spawnedRooms[spawnedRooms.Count - 1]);
                                    sectorFirstRoom = false;
                                    canRemoveSector = thisSector;
                                } else
                                    sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitState = 'l';

                                while (sectorPath.Count > 1)
                                    sectorPath.RemoveAt (1);

                            } else { //Inicio do mapa
                                spawnedRoom = Instantiate (nextPrefab, spawnedRooms[spawnedRooms.Count - 1].exitPoints[randExit].exitTransform.position,
                                    spawnedRooms[spawnedRooms.Count - 1].exitPoints[randExit].exitTransform.rotation);

                                //Obter entrada
                                roomEntrance = spawnedRoom.GetComponent<RoomEntrance> ();

                                ////Fazer a ligaçao entre quartos
                                spawnedRooms[spawnedRooms.Count - 1].exitPoints[randExit].exitState = 's';

                                //Adiciona a informaçao do prefab a lista de prefabs gerados
                                spawnedRooms.Add (new RoomInfo (spawnedRoom, nextPrefab, roomEntrance.exitPoints, roomEntrance.roomDimension.transform.lossyScale,
                                    roomEntrance.rightCorner.transform.position, spawnedRoom.transform, -1, '*'));

                                spawnedRooms[spawnedRooms.Count - 1].lastExitPoints = spawnedRooms[spawnedRooms.Count - 2].exitPoints;
                                spawnedRooms[spawnedRooms.Count - 1].thisEntranceIndex = randExit;
                                spawnedRooms[spawnedRooms.Count - 2].exitPoints[randExit].linkedRoom = spawnedRooms[spawnedRooms.Count - 1];

                                sectorPath.Add (spawnedRooms[spawnedRooms.Count - 1]);
                                sectorFirstRoom = false;
                            }
                            sectorRoomCount++;
                            thisSubSector++;
                            nRooms--;
                        }
                    } else { //Caso o prefab seleccionado nao tenha saidas
                        if (sectorPath.Count > 1)
                            sectorPath[sectorPath.Count - 1].lastExitPoints[sectorPath[sectorPath.Count - 1].thisEntranceIndex].exitState = 'x';

                        while (!sectorPath[sectorPath.Count - 1].OpenExits && sectorPath.Count > 1) {
                            sectorPath[sectorPath.Count - 1].lastExitPoints[sectorPath[sectorPath.Count - 1].thisEntranceIndex].exitState = 'x';
                            sectorPath.RemoveAt (sectorPath.Count - 1);
                        }

                        while (sectorPath.Count > 1)
                            sectorPath.RemoveAt (1);
                    }
                } else {
                    SpawnSpawnRoom (spawnedRooms, ref colisionMap, spawnRooms);
                    return true;
                }
            }
        }

    /*Selects a rand object from a list*/
    private GameObject NextPrefab (List<GameObject> availableRooms) {
        return availableRooms[Random.Range (0, availableRooms.Count)];
    }

    /*Closes a exit with rand from list of doors*/
    private void CloseExit (Transform exit, List<GameObject> doors) {
        if (doors.Count > 0) {
            int randDoor = Random.Range (0, doors.Count);

            GameObject auxDoor = Instantiate (doors[randDoor], exit.position + doors[randDoor].transform.position, exit.rotation);
            auxDoor.transform.SetParent (exit);
            navMesh.ObstacleToNavMesh (auxDoor.GetComponent<NavMeshObstacle> (), 'w');

        }
    }

    /*Closes room exits*/
    private void CloseRoomExits (RoomInfo room) {
        //Close doors
        foreach (PrefabExit exit in room.exitPoints) {
            if (exit.exitState == 'c' || exit.exitState == 'o')
                CloseExit (exit.exitTransform, closedDoors);
        }
    }

    /*Removes a sector from the colision map*/
    private bool RemoveSector (int sector, ref ColisionMap colisionMap) {
        bool sectorRemoved = false;

        if (sector > 0)
            for (int roomIndex = spawnedRooms.Count - 1; roomIndex > 0; roomIndex--) {
                if (spawnedRooms[roomIndex].sector == sector) {
                    colisionMap.RemoveFromMapColision (spawnedRooms[roomIndex]);
                    spawnedRooms[roomIndex].lastExitPoints[spawnedRooms[roomIndex].thisEntranceIndex].exitState = 'c';
                    Destroy (spawnedRooms[roomIndex].prefab);
                    spawnedRooms.RemoveAt (roomIndex);
                    roomIndex = spawnedRooms.Count;
                    sectorRemoved = true;
                }
            }

        return sectorRemoved;
    }

    /*Selects the sector's main room and adds it to sector path*/
    private bool SelectSector (int sector, List<RoomInfo> spawnedRooms, List<RoomInfo> sectorPath) {
        foreach (RoomInfo room in spawnedRooms) {
            if (room.sector == sector && room.subSector == (char) 64) {
                sectorPath.Clear ();
                sectorPath.Add (room);
                return true;
            }
        }
        return false;
    }

    /*Spawns spawn room*/
    private void SpawnSpawnRoom (List<RoomInfo> spawnedRooms, ref ColisionMap colisionMap, List<GameObject> spawnRooms) {
        List<RoomInfo> sectorPath = new List<RoomInfo> ();
        GameObject spawnedRoom;
        GameObject nextRoom = spawnRooms[0];
        int randExit = 0;

        SelectSector (spawnedRooms[spawnedRooms.Count - 1].sector, spawnedRooms, sectorPath);

        while (true)
            if (sectorPath[sectorPath.Count - 1].OpenExits) {
                randExit = sectorPath[sectorPath.Count - 1].GetRandExit;
                if (sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitState == 'l')
                    sectorPath.Add (sectorPath[sectorPath.Count - 1].exitPoints[randExit].linkedRoom);
                else {
                    if (colisionMap.MapColision (randExit, nextRoom, sectorPath.Count - 1, sectorPath)) { //Ha colisao

                        sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitState = 'c';

                        while (!sectorPath[sectorPath.Count - 1].OpenExits && sectorPath.Count > 1) {
                            sectorPath[sectorPath.Count - 1].lastExitPoints[sectorPath[sectorPath.Count - 1].thisEntranceIndex].exitState = 'x';
                            sectorPath.RemoveAt (sectorPath.Count - 1);
                        }

                        while (sectorPath.Count > 1)
                            sectorPath.RemoveAt (1);
                    } else { //Nao ha colisao
                        spawnedRoom = Instantiate (nextRoom, sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitTransform.position, sectorPath[sectorPath.Count - 1].exitPoints[randExit].exitTransform.rotation);

                        RoomEntrance roomEntrance = spawnedRoom.GetComponent<RoomEntrance> ();

                        //Adiciona a informaçao do prefab a lista de prefabs gerados
                        spawnedRooms.Add (new RoomInfo (spawnedRoom, nextRoom, roomEntrance.exitPoints, roomEntrance.roomDimension.transform.lossyScale,
                            roomEntrance.rightCorner.transform.position, spawnedRoom.transform, -2, '*'));

                        spawnedRooms[spawnedRooms.Count - 1].lastExitPoints = sectorPath[sectorPath.Count - 1].exitPoints;
                        spawnedRooms[spawnedRooms.Count - 1].thisEntranceIndex = randExit;
                        sectorPath[sectorPath.Count - 1].exitPoints[randExit].linkedRoom = spawnedRooms[spawnedRooms.Count - 1];
                        spawnedRooms[spawnedRooms.Count - 1].lastExitPoints[randExit].exitState = 'x';
                        break;
                    }
                }
            }
        else {
            if (sectorPath.Count == 1) {
                List<PrefabExit> lastExits = sectorPath[0].lastExitPoints;
                int lastExitIndex = sectorPath[0].thisEntranceIndex;
                RemoveSector (sectorPath[0].sector, ref colisionMap);

                spawnedRoom = Instantiate (nextRoom, lastExits[lastExitIndex].exitTransform.position, lastExits[lastExitIndex].exitTransform.rotation);

                RoomEntrance roomEntrance = spawnedRoom.GetComponent<RoomEntrance> ();

                //Adiciona a informaçao do prefab a lista de prefabs gerados
                spawnedRooms.Add (new RoomInfo (spawnedRoom, nextRoom, roomEntrance.exitPoints, roomEntrance.roomDimension.transform.lossyScale,
                    roomEntrance.rightCorner.transform.position, spawnedRoom.transform, -2, '*'));

                spawnedRooms[spawnedRooms.Count - 1].lastExitPoints = sectorPath[sectorPath.Count - 1].exitPoints;
                spawnedRooms[spawnedRooms.Count - 1].thisEntranceIndex = randExit;
                sectorPath[sectorPath.Count - 1].exitPoints[randExit].linkedRoom = spawnedRooms[spawnedRooms.Count - 1];

                lastExits[lastExitIndex].exitState = 'l';
                break;
            }
        }
    }

    /*Corrects the layout of SubSectors */
    private void RearrangeSubSectors () {
        char subSector = '@';
        List<RoomInfo> sectorPath = new List<RoomInfo> ();

        bool test = false;
        bool first = true;

        SelectSector (spawnedRooms[spawnedRooms.Count - 2].sector, spawnedRooms, sectorPath);

        while (true) {
            if (sectorPath[sectorPath.Count - 1].OpenSubSectors) {
                if (sectorPath[sectorPath.Count - 1].LinkedExitsCount > 1) {
                    subSector++;
                    first = false;
                    int exit = sectorPath[sectorPath.Count - 1].GetRandSubSectorPath;
                    sectorPath[sectorPath.Count - 1].exitPoints[exit].exitState = '+';
                    sectorPath.Add (sectorPath[sectorPath.Count - 1].exitPoints[exit].linkedRoom);
                    sectorPath[sectorPath.Count - 1].subSector = subSector;
                } else if (sectorPath[sectorPath.Count - 1].LinkedExitsCount == 1) {
                    if (sectorPath[sectorPath.Count - 1].Cross >= 1 || test || first) {
                        subSector++;
                        test = false;
                        first = false;
                    }

                    int exit = sectorPath[sectorPath.Count - 1].GetRandSubSectorPath;
                    sectorPath[sectorPath.Count - 1].exitPoints[exit].exitState = '-';
                    sectorPath.Add (sectorPath[sectorPath.Count - 1].exitPoints[exit].linkedRoom);
                    sectorPath[sectorPath.Count - 1].subSector = subSector;

                }
            } else {
                if (sectorPath[sectorPath.Count - 1].sector == 0 && sectorPath.Count == 1)
                    break;

                if (sectorPath.Count > 1) {
                    sectorPath.RemoveAt (sectorPath.Count - 1);
                    test = true;
                } else {
                    subSector = '@';
                    SelectSector (sectorPath[0].sector - 1, spawnedRooms, sectorPath);
                    test = false;
                }
            }
        }

    }

    /*Spawns enemies*/
    private bool SpawnEnemies (int minEnemies, int maxEnemies, List<GameObject> enemies, List<int> enemyProbs, List<RoomInfo> spawnedRooms, int sector) {
        if (maxEnemies > 0 && enemies.Count > 0) {
            List<int> sectorRoomsI = new List<int> ();
            sectorRoomsI = GetRoomIndexesFromSectoSpawners (spawnedRooms, sector);

            if (sectorRoomsI.Count > 0) {
                int randRoom = 0;
                int randESpawner = 0;
                GameObject auxEnemy;
                GameObject auxBaseEnemy;
                Transform auxTransform;

                //Get the number of enemies for this sector
                int nEnemies = Random.Range (minEnemies, maxEnemies + 1);
                while (nEnemies > 0 && sectorRoomsI.Count > 0) {
                    //Select Room and Enemy Spawner
                    randRoom = Random.Range (0, sectorRoomsI.Count);
                    randESpawner = Random.Range (0, spawnedRooms[sectorRoomsI[randRoom]].GetRoomEntrance ().enemySpawners.Count);

                    auxTransform = spawnedRooms[sectorRoomsI[randRoom]].GetRoomEntrance ().enemySpawners[randESpawner];

                    auxBaseEnemy = weightedRandGO(enemies, enemyProbs);
                    auxEnemy = Instantiate (auxBaseEnemy, auxTransform.position, auxTransform.rotation);
                    auxEnemy.name = auxBaseEnemy.name;
                    //auxEnemy.transform.SetParent(spawnedRooms[sectorRoomsI[randRoom]].prefab.transform);// Ai n da
                    spawnedRooms[sectorRoomsI[randRoom]].enemies.Add (auxEnemy);

                    //Remove Spawner
                    spawnedRooms[sectorRoomsI[randRoom]].GetRoomEntrance ().enemySpawners.RemoveAt (randESpawner);

                    //Remove room from de the spawnable list (does it have spawners?)
                    if (spawnedRooms[sectorRoomsI[randRoom]].GetRoomEntrance ().enemySpawners.Count == 0)
                        sectorRoomsI.RemoveAt (randRoom);

                    nEnemies--;
                }
                return true;
            }
        }
        return false;
    }

    private bool SetPlayerPosToSpawnPos () {
        //Set player pos (spawn room)
        if (playerTransform) {
            playerTransform.position = spawnedRooms[spawnedRooms.Count - 1].prefab.GetComponent<RoomEntrance> ().playerSpawnPoint.position;
            return true;
        }
        return false;
    }

    private void AssociateKeycards (int sector) {
        List<int> roomsWithEnemies = GetRoomIndexesFromSectorEnemies (spawnedRooms, sector);

        if (roomsWithEnemies.Count > 0) {
            int randRoom = Random.Range (0, roomsWithEnemies.Count);

            int randEnemy = Random.Range (0, spawnedRooms[roomsWithEnemies[randRoom]].enemies.Count);

            spawnedRooms[roomsWithEnemies[randRoom]].enemies[randEnemy].GetComponent<BaseEnemy> ().hasKeycard = true;
        }

    }

    #endregion Map Generation Funcs

    #region AI Map (Simulation) Funcs
    //_____________________________________________________________________________________________________
    //Map Generation Funcs

    public void RestartSimulation () {
        MapInit ();
        SpawnAI ();
    }

    private bool SpawnAI () {
        GameObject auxEnemy;
        Transform auxTransform;

        int sectorCount = GetSectorCount ();
        int randSector = Random.Range (0, sectorCount);
        List<int> roomIndexes = GetRoomIndexesFromSectoSpawners (spawnedRooms, randSector);
        int randRoom = Random.Range (0, roomIndexes.Count);
        int randSpawner = 0;

        for (int i = 0; i < 2; i++) {
            if (spawnedRooms[roomIndexes[randRoom]].GetRoomEntrance ().enemySpawners.Count == 0)
                return false;

            randSpawner = Random.Range (0, spawnedRooms[roomIndexes[randRoom]].GetRoomEntrance ().enemySpawners.Count);

            auxTransform = spawnedRooms[roomIndexes[randRoom]].GetRoomEntrance ().enemySpawners[randSpawner];

            //Spawn
            auxEnemy = Instantiate ((i == 0) ? aIEnemyTraining : baseEnemyAI, auxTransform.position, auxTransform.rotation);
            spawnedRooms[roomIndexes[randRoom]].enemies.Add (auxEnemy);

            //Remove Spawner
            spawnedRooms[roomIndexes[randRoom]].GetRoomEntrance ().enemySpawners.RemoveAt (randSpawner);
        }
        return true;
    }

    #endregion AI Map (Simulation) Funcs

    #region Aux Funcs
    //_____________________________________________________________________________________________________
    //Aux Funcs

    /*Returns an index of list (weighted rand)*/
    private int ListRand (List<int> probs) {
        int rand = 0;
        int total = 0;

        foreach (int prob in probs)
            total += prob;

        rand = Random.Range (0, total + 1);

        total = 0;
        int indexObstacle = -1;
        foreach (int prob in probs)
            if (rand >= total) {
                total += prob;
                indexObstacle++;
            }

        return indexObstacle;
    }

    /*Weighted Rand (Game Object)*/
    private GameObject weightedRandGO (List<GameObject> objs, List<int> probs) {
        if (objs.Count > 0 && probs.Count > 0) {
            int sum = 0;
            int auxRand = 0;

            for (int i = 0; i < probs.Count; i++)
                sum += probs[i];

            auxRand = Random.Range (0, sum + 1);

            sum = 0;
            for (int i = 0; i < probs.Count; i++) {
                sum += probs[i];
                if (auxRand <= sum)
                    return objs[i];
            }
        }
        return null;
    }

    /*Returns the list of idexes of a sector*/
    public static List<int> GetRoomIndexesFromSector (List<RoomInfo> spawnedRooms, int sector) {
        List<int> sectorRoomsI = new List<int> ();
        for (int roomIndex = 1; roomIndex < spawnedRooms.Count; roomIndex++)
            if (spawnedRooms[roomIndex].sector == sector)
                sectorRoomsI.Add (roomIndex);

        return sectorRoomsI;
    }

    /*Get the index of the rooms with spawn points*/
    private List<int> GetRoomIndexesFromSectoSpawners (List<RoomInfo> spawnedRooms, int sector) {
        List<int> sectorRoomsI = new List<int> ();
        for (int roomIndex = 1; roomIndex < spawnedRooms.Count; roomIndex++)
            if (spawnedRooms[roomIndex].sector == sector && spawnedRooms[roomIndex].GetRoomEntrance ().enemySpawners.Count > 0)
                sectorRoomsI.Add (roomIndex);

        return sectorRoomsI;
    }

    /*Get the index of the rooms with enemies*/
    public List<int> GetRoomIndexesFromSectorEnemies (List<RoomInfo> spawnedRooms, int sector) {
        List<int> sectorRoomsI = new List<int> ();
        for (int roomIndex = 1; roomIndex < spawnedRooms.Count; roomIndex++)
            if (spawnedRooms[roomIndex].sector == sector && spawnedRooms[roomIndex].enemies.Count > 0)
                sectorRoomsI.Add (roomIndex);

        return sectorRoomsI;
    }

    //Get Sector Count
    public int GetSectorCount () {
        int sectorCount = 0;
        int lastSector = -1;
        foreach (RoomInfo room in spawnedRooms) {
            if (room.sector != lastSector && room.sector > -1) {
                sectorCount++;
                lastSector = room.sector;
            }
        }

        return sectorCount;
    }

    public void RefreshEnemies () {
        foreach (RoomInfo room in spawnedRooms) {
            for (int i = 0; i < room.enemies.Count; i++) {
                if (!room.enemies[i]) {
                    room.enemies.RemoveAt (i--);
                }
            }
        }
    }

    #endregion Aux Funcs
}