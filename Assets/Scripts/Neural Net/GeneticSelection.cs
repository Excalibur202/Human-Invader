using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveLoad;

public class GeneticSelection : MonoBehaviour
{
    NeuralNetwork primeNeuralNet = new NeuralNetwork();
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
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void OnDestroy() //stoping the program
    {
        //save population
        population.SaveBinary("Assets\\AIData\\NeuralData", "NeuralNetworkPopulation");
        primeNeuralNet.SaveBinary("Assets\\AIData\\NeuralData", "PrimeNeuralNet");

    }
}
