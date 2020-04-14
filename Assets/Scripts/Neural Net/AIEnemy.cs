using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEnemy : MonoBehaviour
{
    MapGenerator map;
    public int sizeX;
    public int sizeY;
    char[,] mapArea;
    float timer = 0;

    List<GameObject> areaDebug = new List<GameObject>();
    public GameObject cubeTest;
    public GameObject cubeTestAI;
    public GameObject cubeTestEnemy;

    Vector2 last2DPos = Vector2.zero;
    Vector2 pos2D;

    // Start is called before the first frame update
    void Start()
    { }

    // Update is called once per frame
    void Update()
    {
        if (!map)
            map = MapGenerator.instance;
        else
        {
           
            //timer += Time.deltaTime;
            pos2D = new Vector2((int)this.gameObject.transform.position.x, (int)this.gameObject.transform.position.z);

            if (last2DPos != pos2D || /*timer > 0.2f*/ map.navMesh.refreshAreas )
            {
                
                mapArea = map.navMesh.GetArea(sizeX, sizeY, this.gameObject.transform.position);

                while (areaDebug.Count > 0)
                {

                    Destroy(areaDebug[0]);
                    areaDebug.RemoveAt(0);
                }

                for (int posX = 0; posX < mapArea.GetLength(0); posX++)
                    for (int posY = 0; posY < mapArea.GetLength(1); posY++)
                    {
                        if (mapArea[posX, posY] == 'w')
                            areaDebug.Add(Instantiate(cubeTest, new Vector3(posX - 0.5f, 1, posY - 0.5f), cubeTest.transform.rotation));
                        else if (mapArea[posX, posY] == 'a')
                        {
                            if (cubeTestAI)
                                areaDebug.Add(Instantiate(cubeTestAI, new Vector3(posX - 0.5f, 1, posY - 0.5f), cubeTestAI.transform.rotation));
                        }
                        else if (mapArea[posX, posY] == 'e')
                        {
                            if (cubeTestEnemy)
                                areaDebug.Add(Instantiate(cubeTestEnemy, new Vector3(posX - 0.5f, 1, posY - 0.5f), cubeTestEnemy.transform.rotation));
                        }
                    }

                last2DPos = pos2D;
                timer = 0;
            }
        }
    }
}
