using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveLoad;

public class GeneticSelection : MonoBehaviour
{
    NeuralNetwork primeNeuralNet = new NeuralNetwork();
    NeuralNetwork[] population;

    [SerializeField]
    int populationCount = 0;
    [SerializeField]
    bool RestartPopulation = false;
    

    [SerializeField]
    float weightMutationRate = 0f;
    [SerializeField]
    float biasMutationRate = 0f;
    [SerializeField]
    float neuronActivationProb = 0f;

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
            population = new NeuralNetwork[populationCount];
            for (int neuralNetIndex = 0; neuralNetIndex < population.Length; neuralNetIndex++)
                population[neuralNetIndex] = new NeuralNetwork(hidenLayerMaxColumns, hidenLayerMaxRows, inputVecLength, outputVecLength);
        }
        else population = population.LoadBinary("Assets\\AIData\\NeuralData", "NeuralNetworkPopulation");

        foreach (NeuralNetwork neuralNet in population)
            neuralNet.MutateNeuralNetwork(weightMutationRate, biasMutationRate, neuronActivationProb);
    }

    // Update is called once per frame
    void Update()
    {





    }


    private void OnApplicationQuit() //stoping the program
    {
        //save population
        population.SaveBinary("Assets\\AIData\\NeuralData", "NeuralNetworkPopulation");
        primeNeuralNet.SaveBinary("Assets\\AIData\\NeuralData", "PrimeNeuralNet");

    }
}
