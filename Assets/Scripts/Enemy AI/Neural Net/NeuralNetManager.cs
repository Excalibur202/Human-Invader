using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveLoad;

public class NeuralNetManager : MonoBehaviour
{
    public static NeuralNetManager instance;
    NeuralNetwork primeNeuralNet;

    private void Awake()
    {
        if (instance)
            Destroy(this);

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Get prime neural net
        primeNeuralNet = primeNeuralNet.LoadBinary("Assets\\AIData\\NeuralData", "PrimeNeuralNet");




    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
