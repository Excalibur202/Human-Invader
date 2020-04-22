﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToMap : MonoBehaviour
{
    MapGenerator map;
    Vector2 last2DPos = Vector2.zero;
    Vector2 pos2D;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!map)
            map = MapGenerator.instance;
        else
        {
            map.navMesh.refreshAreas = false; // so pode ser usado se so existir um enimigo
            pos2D = new Vector2((int)this.gameObject.transform.position.x + 1, (int)this.gameObject.transform.position.z + 1);
            if (map.navMesh.InMapRange(pos2D) && map.navMesh.GetPosChar(pos2D) == 'g')
                if (last2DPos != pos2D)
                {
                    //player in map
                    map.navMesh.refreshAreas = true;
                    if (map.navMesh.GetPosChar(last2DPos) == 'e')
                        map.navMesh.SetPosChar(last2DPos, 'g');
                    map.navMesh.SetPosChar(pos2D, 'e');
                    
                    //Room activation
                    foreach (RoomInfo room in map.spawnedRooms)
                    {
                        if (room.entranceTransform != null)
                            if (Vector3.Distance(this.gameObject.transform.position, room.entranceTransform.position) < 50)
                                room.prefab.SetActive(true);
                            else
                                room.prefab.SetActive(false);
                    }

                    last2DPos = pos2D;
                }
        }
    }
}