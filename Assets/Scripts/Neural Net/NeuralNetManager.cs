using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveLoad;

public class NeuralNetManager : MonoBehaviour
{
    public float[] inputVec = new float[4];
    NeuralNetwork neuralNetwork;
    // Start is called before the first frame update
    void Start()
    {
        neuralNetwork = new NeuralNetwork(3, 3, 5, inputVec);

        //Save
        //neuralNetwork.SaveBinary();


        //DebugNeuralNet();
    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("///////////////////////////////////////////////////////////////////////////////");
            neuralNetwork.Eval();
            Debug.Log("Output Vec:");
            foreach (float outputValue in neuralNetwork.outputVec)
                Debug.Log(outputValue);
        }
    }

    //void DebugNeuralNet()
    //{
    //    foreach(Node neuralNode in neuralNetwork.hidenNodes)
    //    {
    //        Debug.Log("NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN");
    //        Debug.Log("Bias: "+ neuralNode.bias);
    //        Debug.Log("Weights: ");
    //        foreach (float weight in neuralNode.weights)
    //        Debug.Log(weight);
    //        Debug.Log("NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN");
    //    }
    //}
}
