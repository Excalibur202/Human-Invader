using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveLoad;

public class GeneticSelection : MonoBehaviour
{
    
    NeuralNetwork[] population = new NeuralNetwork[20];
    
    [SerializeField]
    bool RestartPopulation = false;
    [SerializeField]
    float mutationRate = 0f;
    [SerializeField]
    int inputVecLength = 0;
    [SerializeField]
    int outputVecLength = 0;
    [SerializeField]
    int hidenLayerMaxColumns = 0;
    [SerializeField]
    int hidenLayerMaxRows = 0;
    

    void Start()
    {
        if (RestartPopulation)
        {
            //Atribuir nova populaçao
            for (int neuralNetIndex = 0; neuralNetIndex < population.Length; neuralNetIndex++)
                population[neuralNetIndex] = new NeuralNetwork(hidenLayerMaxColumns, hidenLayerMaxRows, inputVecLength, outputVecLength);
        }
        else population = population.LoadBinary("Assets\\AIData\\NeuralData", "NeuralNetworkPopulation");
        //DebugNeuralNet();
    }

    // Update is called once per frame
    void Update()
    {

        //if (Input.GetKeyDown(KeyCode.Space)) 
        //{
        //    Debug.Log("///////////////////////////////////////////////////////////////////////////////");
        //    neuralNetwork.Eval();
        //    Debug.Log("Output Vec:");
        //    foreach (float outputValue in neuralNetwork.outputVec)
        //        Debug.Log(outputValue);
        //}
    }


    private void OnApplicationQuit() //stoping the program
    {
        //save population
        population.SaveBinary("Assets\\AIData\\NeuralData", "NeuralNetworkPopulation");
    }

}
