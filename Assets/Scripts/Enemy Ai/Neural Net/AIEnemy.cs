using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEnemy : MonoBehaviour
{
    public NeuralNetwork nNet;
    MapGenerator map;
    int sizeX;
    int sizeY;
    char[,] mapArea;
    
    float[] nNInput;
    public float[] nNOutput;
    

    Vector2 last2DPos = Vector2.zero;
    Vector2 pos2D;

    public Vector2 outputAxis = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!map)
            map = MapGenerator.instance;
    }

    public void UpdateAI(float deltaTime)
    {
        if (map)
        {
            //Vision
            UpdateNNVision();
            nNOutput = nNet.Eval(nNInput);
            outputAxis = GetAIAxis(nNOutput);
        }
    }

    //AI Vision
    public void SetVisionArea(int sizeX, int sizeY)
    {
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        nNInput = new float[sizeX * sizeY];
    }

    private void UpdateNNVision()
    {
        pos2D = new Vector2((int)this.gameObject.transform.position.x, (int)this.gameObject.transform.position.z);

        if (last2DPos != pos2D || map.navMesh.refreshAreas)
        {
            mapArea = map.navMesh.GetArea(sizeX, sizeY, this.gameObject.transform.position);
            int auxIndex = 0;
            foreach (char unit in mapArea)
            {
                switch (unit)
                {
                    case 'g':
                        nNInput[auxIndex] = 1f;
                        break;

                    case 's':
                        nNInput[auxIndex] = 1f;
                        break;

                    case 'w':
                        nNInput[auxIndex] = 2f;
                        break;

                    case 'o':
                        nNInput[auxIndex] = 2f;
                        break;

                    case 'p':
                        nNInput[auxIndex] = 3f;
                        break;

                    default:
                        nNInput[auxIndex] = 0f;
                        break;
                }
                auxIndex++;
            }
            last2DPos = pos2D;
        }
    }

    private Vector2 GetAIAxis(float[] nNOutputs)
    {
        if(nNOutputs.Length >= 4)
        {
            Vector2 axis = Vector2.zero;
            axis.x += NeuralNetworkMath.sigmoidToAxis(nNOutputs[0]) + NeuralNetworkMath.sigmoidToAxis(nNOutputs[1]);
            axis.y += NeuralNetworkMath.sigmoidToAxis(nNOutputs[2]) + NeuralNetworkMath.sigmoidToAxis(nNOutputs[3]);
            return axis.normalized;
        }
        return Vector2.zero;
    }

    


}
