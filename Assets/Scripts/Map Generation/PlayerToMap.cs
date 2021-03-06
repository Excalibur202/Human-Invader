﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToMap : MonoBehaviour {
    MapGenerator map;
    Vector2 last2DPos = Vector2.zero;
    char lastChar;
    Vector2 pos2D;
    [SerializeField]
    int renderDistance = 0;

    // Update is called once per frame
    void Update () {
        if (!map)
            map = MapGenerator.instance;
        else {
            map.navMesh.refreshAreas = false; //so pode ser usado se so existir um enimigo
            pos2D = new Vector2 ((int) this.gameObject.transform.position.x + 1, (int) this.gameObject.transform.position.z + 1);

            //Debug.Log(map.navMesh.GetPosChar(pos2D));
            if (map.navMesh.InMapRange (pos2D) && (map.navMesh.GetPosChar (pos2D) == 'g' || (map.navMesh.GetPosChar (pos2D) == 's'))) {
                if (last2DPos != pos2D) {
                    //player in map
                    map.navMesh.refreshAreas = true;
                    if (map.navMesh.GetPosChar (last2DPos) == 'p')
                        map.navMesh.SetPosChar (last2DPos, lastChar/*'g'*/);

                    lastChar = map.navMesh.GetPosChar(pos2D);
                    map.navMesh.SetPosChar (pos2D, 'p');

                    //Room activation
                    foreach (RoomInfo room in map.spawnedRooms) {
                        if (room.entranceTransform != null)
                            if (Util.SqrDistance (this.gameObject.transform.position, room.GetRoomEntrance().roomDimension.transform.position, true) < Util.Square (renderDistance))
                                room.prefab.SetActive (true);
                            else
                                room.prefab.SetActive (false);
                    }
                    last2DPos = pos2D;
                    
                }

                if (map.spawnedEnemies.Count > 0)
                    for (int enemyIndex = 0; enemyIndex < map.spawnedEnemies.Count; enemyIndex++) {
                        if (!map.spawnedEnemies[enemyIndex])
                            map.spawnedEnemies.RemoveAt (enemyIndex--);
                        else {
                            if (Util.SqrDistance (this.gameObject.transform.position, map.spawnedEnemies[enemyIndex].transform.position, true) < Util.Square (renderDistance)) {
                                if (!map.spawnedEnemies[enemyIndex].activeInHierarchy) {
                                    map.spawnedEnemies[enemyIndex].SetActive (true);
                                    map.spawnedEnemies[enemyIndex].GetComponent<BaseEnemy> ().Reset ();
                                }
                            } else {
                                map.spawnedEnemies[enemyIndex].SetActive (false);
                            }

                        }
                    }
            }

        }

    }
}