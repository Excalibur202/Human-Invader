using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveLoad;

public class GeneticSelection : MonoBehaviour
{
    NeuralNetwork primeNeuralNet = new NeuralNetwork();
    NeuralNetwork[] population;

    public AIEnemy aIEnemy;

    [SerializeField]
    int aIVisionSizeX;
    [SerializeField]
    int aIVisionSizeY;
    
    [SerializeField]
    int populationCount = 0;
    [SerializeField]
    float simulationTime;
    [SerializeField]
    bool RestartPopulation = false;
    [SerializeField]
    bool simulate = false;


    [SerializeField]
    float startWeightMutationRate = 0f;
    [SerializeField]
    float startBiasMutationRate = 0f;
    [SerializeField]
    float startNeuronActivationProb = 0f;


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

    float timer = 0;
    int selectedNeuralNet = 0;
    
    void Start()
    {
        if (RestartPopulation)
        {
            //Atribuir nova populaçao
            population = new NeuralNetwork[populationCount];
            for (int neuralNetIndex = 0; neuralNetIndex < population.Length; neuralNetIndex++)
                population[neuralNetIndex] = new NeuralNetwork(hidenLayerMaxColumns, hidenLayerMaxRows, inputVecLength, outputVecLength);

            //Fazer mutaçao inicial
            foreach (NeuralNetwork neuralNet in population)
                neuralNet.MutateNeuralNetwork(startWeightMutationRate, startBiasMutationRate, startNeuronActivationProb);
        }
        else population = population.LoadBinary("Assets\\AIData\\NeuralData", "NeuralNetworkPopulation");


        aIEnemy.SetVisionArea(aIVisionSizeX, aIVisionSizeY);

        
    }

    // Update is called once per frame
    void Update()
    {

        /*Simulation*/
        if (simulate && aIEnemy && population.Length > 0)//do we want to simulate?
        {
            timer += Time.deltaTime;//Start timer
            aIEnemy.nNet = population[0];

            aIEnemy.UpdateAI(Time.deltaTime);




        }

    }






    private void OnApplicationQuit() //stoping the program
    {
        //save population
        population.SaveBinary("Assets\\AIData\\NeuralData", "NeuralNetworkPopulation");
        primeNeuralNet.SaveBinary("Assets\\AIData\\NeuralData", "PrimeNeuralNet");

    }
}
