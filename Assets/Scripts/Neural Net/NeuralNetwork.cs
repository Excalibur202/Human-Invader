using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class NeuralNetwork
{
    Node[,] hidenNodes; // not public 
    Node[] outputNodes;
    //public float[] inputVec;
    //public float[] outputVec;
    float[] weightsZero;




    //Constructor
    public NeuralNetwork(int hidenNColumns, int hidenNRows, int nInputs, int nOutputs/*, float[] inputNodesRef*/)
    {
        hidenNodes = new Node[hidenNColumns, hidenNRows];

        outputNodes = new Node[nOutputs];
        //inputVec = inputNodesRef;

        //Input weights
        for (int rowIndex = 0; rowIndex < hidenNodes.GetLength(1); rowIndex++)
            hidenNodes[0, rowIndex] = new Node(0, new float[nInputs/*inputVec.Length*/]);

        //HidenNodes weights
        for (int columnIndex = 1; columnIndex < hidenNodes.GetLength(0); columnIndex++)
            for (int rowIndex = 0; rowIndex < hidenNodes.GetLength(1); rowIndex++)
                hidenNodes[columnIndex, rowIndex] = new Node(0, new float[hidenNodes.GetLength(1)]);

        //Output weithrs
        for (int outputNodeIndex = 0; outputNodeIndex < outputNodes.Length; outputNodeIndex++)
            outputNodes[outputNodeIndex] = new Node(0, new float[hidenNodes.GetLength(1)]);

        weightsZero = new float[hidenNodes.GetLength(1)];
        for (int weight = 0; weight < weightsZero.Length; weight++)
            weightsZero[weight] = 0;

        //Rand NeuralNet
        RandNeuralNetwork();
    }

    //Evaluate
    public float[] Eval(float[] inputVec)//alterar para eval geral
    {
        Matrix calcMatrix;
        float[] outputVecAux;

        //Hiden Nodes
        float[] bias = new float[hidenNodes.GetLength(1)];
        outputVecAux = inputVec;//new float[inputVec.Length]; SEE LATER



        for (int column = 0; column < hidenNodes.GetLength(0); column++)
        {
            //calcMatrix = new Matrix(hidenNodes.GetLength(1), outputVecAux.Length);
            calcMatrix = new Matrix(outputVecAux.Length, hidenNodes.GetLength(1));

            for (int row = 0; row < hidenNodes.GetLength(1); row++)
            {
                if (hidenNodes[column, row].activated)
                {
                    calcMatrix.AddRow(hidenNodes[column, row].weights);
                    bias[row] = hidenNodes[column, row].bias;
                }
                else
                {
                    calcMatrix.AddRow(weightsZero);
                    bias[row] = 0;
                }
            }

            outputVecAux = (column > 0) ? (calcMatrix * outputVecAux) : (calcMatrix * inputVec);

            for (int index = 0; index < outputVecAux.Length; index++)
            {
                outputVecAux[index] += bias[index];
                outputVecAux[index] = NeuralNetworkMath.Sigmoid(outputVecAux[index]);
            }
        }

        //Output Nodes
        //calcMatrix = new Matrix(outputNodes.Length, outputVecAux.Length);
        calcMatrix = new Matrix(outputVecAux.Length, outputNodes.Length);
        bias = new float[outputNodes.Length];
        for (int row = 0; row < outputNodes.Length; row++)
        {
            calcMatrix.AddRow(outputNodes[row].weights);
            bias[row] = outputNodes[row].bias;
        }
        outputVecAux = (calcMatrix * outputVecAux);

        for (int index = 0; index < outputVecAux.Length; index++)
        {
            outputVecAux[index] += bias[index];
            outputVecAux[index] = NeuralNetworkMath.Sigmoid(outputVecAux[index]);
        }
        //outputVec = outputVecAux;
        return outputVecAux;
    }

    //Randomize NeuralNetwork
    void RandNeuralNetwork()
    {
        var rand = new Random();

        foreach (Node neuralNode in hidenNodes)
        {
            if(neuralNode.activated)
            {
                neuralNode.bias = (float)((rand.NextDouble() * 2) - 1);

                for (int weightIndex = 0; weightIndex < neuralNode.weights.Length; weightIndex++)
                    neuralNode.weights[weightIndex] = (float)((rand.NextDouble() * 2) - 1);
            }
        }

        foreach (Node neuralNode in outputNodes)
        {
            if (neuralNode.activated)
            {
                neuralNode.bias = (float)((rand.NextDouble() * 2) - 1);

                for (int weightIndex = 0; weightIndex < neuralNode.weights.Length; weightIndex++)
                    neuralNode.weights[weightIndex] = (float)((rand.NextDouble() * 2) - 1);
            }
        }
    }


}

public class Node
{

    public float bias = 0;
    public float[] weights;

    public bool activated = false;

    public Node() { }
    public Node(float bias, float[] weights)
    {
        this.bias = bias;
        this.weights = weights;
    }
}