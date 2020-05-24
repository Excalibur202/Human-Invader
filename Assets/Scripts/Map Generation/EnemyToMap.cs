using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyToMap : MonoBehaviour
{
    MapGenerator map;
    Vector2 last2DPos = Vector2.zero;
    char lastChar;
    Vector2 pos2D;

    // Update is called once per frame
    void Update()
    {
        if (!map)
            map = MapGenerator.instance;
        else
        {
            map.navMesh.refreshAreas = false; //so pode ser usado se so existir um enimigo
            pos2D = new Vector2((int)this.gameObject.transform.position.x + 1, (int)this.gameObject.transform.position.z + 1);

            //Debug.Log(map.navMesh.GetPosChar(pos2D));
            if (map.navMesh.InMapRange(pos2D) && (map.navMesh.GetPosChar(pos2D) == 'g' || (map.navMesh.GetPosChar(pos2D) == 's')))
            {
                if (last2DPos != pos2D)
                {
                    //player in map
                    map.navMesh.refreshAreas = true;
                    if (map.navMesh.GetPosChar(last2DPos) == 'p')
                        map.navMesh.SetPosChar(last2DPos, lastChar);

                    lastChar = map.navMesh.GetPosChar(pos2D);
                    map.navMesh.SetPosChar(pos2D, 'p');
                    last2DPos = pos2D;

                }
            }

        }

    }
}
