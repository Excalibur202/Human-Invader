using System.Collections;
using System.Collections.Generic;
using SaveLoad;
using UnityEngine;

public class GeneticSelection : MonoBehaviour
{
    public static GeneticSelection instance;
    MapGenerator map;
    NeuralNetwork[] population;
    List<NeuralNetwork> topNNetHistory = new List<NeuralNetwork>();

    private AIEnemy aIEnemy;
    private BaseEnemy baseAI;

    [HideInInspector]
    public int aIVisionSizeX = 21;
    [HideInInspector]
    public int aIVisionSizeY = 21;

    int populationCount = 25; // Can only be 20
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
    int hiddenLayerMaxColumns = 0;
    [SerializeField]
    int hiddenLayerMaxRows = 0;
    [SerializeField]
    int aIReward = 0;

    float timer = 0;
    public int selectedNeuralNet = -1;
    bool nextNNet = true;
    int neuralGeneration;

    private void Awake()
    {
        //Set instance
        if (instance)
        {
            Destroy(this);
        }
        instance = this;
    }

    void Start()
    {
        if (RestartPopulation)
        {
            neuralGeneration = 0;

            //Atribuir nova populaçao
            population = new NeuralNetwork[populationCount];
            for (int neuralNetIndex = 0; neuralNetIndex < population.Length; neuralNetIndex++)
            {
                //Nova Neural Net
                population[neuralNetIndex] = new NeuralNetwork(hiddenLayerMaxColumns, hiddenLayerMaxRows, inputVecLength, outputVecLength);
                //Fazer mutaçao inicial
                population[neuralNetIndex].MutateNeuralNetwork(startWeightMutationRate, startBiasMutationRate, startNeuronActivationProb);
            }


            ////Fazer mutaçao inicial
            //foreach (NeuralNetwork neuralNet in population)
            //    neuralNet.MutateNeuralNetwork(startWeightMutationRate, startBiasMutationRate, startNeuronActivationProb);

        }
        else
        {
            population = population.LoadBinary("Assets\\AIData\\NeuralData", "NeuralNetworkPopulation");
            topNNetHistory = topNNetHistory.LoadBinary("Assets\\AIData\\NeuralData", "TopGenerationLogs");
            neuralGeneration = population[0].generation;
        }
        if (MapGenerator.instance)
            map = MapGenerator.instance;

        GetAIMapInfo();

        print("Current Generation #: " + neuralGeneration);
    }

    // Update is called once per frame
    void Update()
    {
        /*Simulation*/
        if (simulate && aIEnemy && population.Length > 0) //do we want to simulate?
        {
            //next neural net?
            if (nextNNet) // Ver se da
            {
                if (!(++selectedNeuralNet < population.Length))
                { //Restart Simulation
                    print("------------------------------------------------------------------------------");
                    print("Restart Simulation");

                    neuralGeneration++;
                    //Generation related info
                    print("Current Generation #: " + neuralGeneration);

                    print("Flatten Fitness");
                    //Flatten fitness
                    foreach (NeuralNetwork nNet in population)
                    {
                        if (nNet.lastFitness != 0)
                            nNet.fitness = MyMath.CalculateAverage(nNet.fitness, nNet.lastFitness);
                        nNet.lastFitness = nNet.fitness;
                        nNet.generation = neuralGeneration;
                    }

                    print("Sort population by fitness (Max -> Min)");
                    //Sort fitness Max-Min
                    QuickSortByFitness(population, 0, population.Length - 1);

                    print("Saving Generation Top 5 NNets");
                    AddToTopNNet(population, 5);
                    //DebugFitnessValues();

                    if (topNNetHistory.Count > 0)
                    {
                        print("Highest fitness so far: " + topNNetHistory[0].fitness);
                    }

                    float[] fitnessArray = new float[population.Length];
                    for (int i = 0; i < population.Length; i++) fitnessArray[i] = population[i].fitness;
                    print("Average generation fitness: " + MyMath.CalculateAverage(fitnessArray));

                    print("Mutation & Fusion");
                    int fusionIndex = 10;
                    for (int bestFitnessIndex = 0; bestFitnessIndex < 5; bestFitnessIndex++)
                    {
                        //Mutation
                        NeuralNetwork auxNNet = population[bestFitnessIndex].DeepClone();
                        auxNNet.MutateNeuralNetwork(weightMutationRate, biasMutationRate, neuronActivationProb);
                        population[bestFitnessIndex + 5] = auxNNet;

                        //Fusion
                        for (int i = bestFitnessIndex + 1; i < 5; i++)
                        {
                            population[fusionIndex++] = NeuralNetwork.FuseNeuralNetwork(population[bestFitnessIndex], population[i]);
                        }
                    }

                    //Random NNets
                    for (int i = 20; i < 25; i++)
                    {
                        population[i] = new NeuralNetwork(hiddenLayerMaxColumns, hiddenLayerMaxRows, inputVecLength, outputVecLength);
                        population[i].MutateNeuralNetwork(startWeightMutationRate, startBiasMutationRate, startNeuronActivationProb);
                    }

                    print("Reset NNet index");
                    selectedNeuralNet = 0;
                    print("------------------------------------------------------------------------------");
                }

                print("Simulating NNet: " + (selectedNeuralNet + 1));
                population[selectedNeuralNet].fitness = 0;
                //aIEnemy.nNet = population[selectedNeuralNet];
                map.RestartSimulation();
                GetAIMapInfo(selectedNeuralNet);
                nextNNet = false;
            }

            //Continuar a dar update a simulacao?
            if (timer < simulationTime) //Sim
            {
                timer += Time.deltaTime; //Start timer
                aIEnemy.UpdateAI(baseAI.canSeePlayer ? 1 : 0); // 1 means yes
                aIEnemy.nNet.fitness += Fitness(baseAI.canSeePlayer, Mathf.Sqrt(baseAI.playerSqrDistance), aIReward);
            }
            else //Nao
            {
                //Reset timer & next NNet
                timer = 0;
                nextNNet = true;
                print("Fitness: " + (aIEnemy.nNet.fitness));
            }
        }
    }

    private void OnApplicationQuit() //stoping the program
    {
        //save population
        population.SaveBinary("Assets\\AIData\\NeuralData", "NeuralNetworkPopulation");
        //Save top  NNet
        topNNetHistory.SaveBinary("Assets\\AIData\\NeuralData", "TopGenerationLogs");
    }

    /*Calculates the fitness of a Neural Net*/
    private float Fitness(bool canSee, float distance, float cantSeeReward)
    {
        float fitness = 0;
        distance = (distance < 3.1f) ? 0 : distance;

        if (canSee)
            fitness = distance * Time.deltaTime;
        else
            fitness = cantSeeReward * distance * Time.deltaTime;

        return fitness;
    }

    private void GetAIMapInfo(int populatioNNetIndex = 0)
    {

        aIEnemy = map.aIEnemyTraining.GetComponent<AIEnemy>();
        aIEnemy.SetVisionArea(aIVisionSizeX, aIVisionSizeY);
        aIEnemy.nNet = population[populatioNNetIndex];

        baseAI = map.enemyAI.GetComponent<BaseEnemy>();

    }

    private void DebugFitnessValues()
    {
        print("////////////////////////////////////////////////////////////");
        for (int i = 0; i < population.Length; i++)
            print(population[i].fitness);
        print("////////////////////////////////////////////////////////////");
    }

    private void AddToTopNNet(NeuralNetwork[] population, int nToSave)
    {
        //Obter os "nToSave" melhores de cada geraçao
        for (int i = 0; i < nToSave; i++)
            topNNetHistory.Add(population[i].DeepClone());
        QuickSortByFitness(topNNetHistory, 0, topNNetHistory.Count - 1);

        //Top 100
        while (topNNetHistory.Count > 50)
            topNNetHistory.RemoveAt(topNNetHistory.Count - 1);
    }
    //NotMine ///////////////////////////////////////////////////////////////////////////https://www.w3resource.com/csharp-exercises/searching-and-sorting-algorithm/searching-and-sorting-algorithm-exercise-9.php
    private static void QuickSortByFitness(NeuralNetwork[] arr, int left, int right)
    {
        if (left < right)
        {
            int pivot = Partition(arr, left, right);

            if (pivot > 1)
            {
                QuickSortByFitness(arr, left, pivot - 1);
            }
            if (pivot + 1 < right)
            {
                QuickSortByFitness(arr, pivot + 1, right);
            }
        }
    }
    private static int Partition(NeuralNetwork[] arr, int left, int right)
    {
        float pivot = arr[left].fitness;
        while (true)
        {
            while (arr[left].fitness > pivot)
            {
                left++;
            }

            while (arr[right].fitness < pivot)
            {
                right--;
            }

            if (left < right)
            {
                if (arr[left].fitness == arr[right].fitness) return right;

                NeuralNetwork temp = arr[left];
                arr[left] = arr[right];
                arr[right] = temp;
            }
            else
            {
                return right;
            }
        }
    }

    private static void QuickSortByFitness(List<NeuralNetwork> arr, int left, int right)
    {
        if (left < right)
        {
            int pivot = Partition(arr, left, right);

            if (pivot > 1)
            {
                QuickSortByFitness(arr, left, pivot - 1);
            }
            if (pivot + 1 < right)
            {
                QuickSortByFitness(arr, pivot + 1, right);
            }
        }
    }
    private static int Partition(List<NeuralNetwork> arr, int left, int right)
    {
        float pivot = arr[left].fitness;
        while (true)
        {
            while (arr[left].fitness > pivot)
            {
                left++;
            }

            while (arr[right].fitness < pivot)
            {
                right--;
            }

            if (left < right)
            {
                if (arr[left].fitness == arr[right].fitness) return right;

                NeuralNetwork temp = arr[left];
                arr[left] = arr[right];
                arr[right] = temp;
            }
            else
            {
                return right;
            }
        }
    }

}