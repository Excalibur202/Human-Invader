using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class NeuralNetwork
{
    NeuralNode[,] hidenNodes; // not public 
    NeuralNode[] outputNodes;
    //public float[] inputVec;
    //public float[] outputVec;
    public float fitness;
    float[] weightsZero;

    public NeuralNetwork()
    { }

    //Constructor
    public NeuralNetwork(int hidenNColumns, int hidenNRows, int nInputs, int nOutputs/*, float[] inputNodesRef*/)
    {
        hidenNodes = new NeuralNode[hidenNColumns, hidenNRows];

        outputNodes = new NeuralNode[nOutputs];
        //inputVec = inputNodesRef;

        //Input weights
        for (int rowIndex = 0; rowIndex < hidenNodes.GetLength(1); rowIndex++)
        {
            hidenNodes[0, rowIndex] = new NeuralNode(0, new float[nInputs/*inputVec.Length*/]);
            hidenNodes[0, rowIndex].activated = true;////////
        }
        hidenNodes[0, 0].activated = true;

        //HidenNodes weights
        for (int columnIndex = 1; columnIndex < hidenNodes.GetLength(0); columnIndex++)
            for (int rowIndex = 0; rowIndex < hidenNodes.GetLength(1); rowIndex++)
            {
                hidenNodes[columnIndex, rowIndex] = new NeuralNode(0, new float[hidenNodes.GetLength(1)]);
                hidenNodes[columnIndex, rowIndex].activated = true;////////
            }
                

        //Output weithrs
        for (int outputNodeIndex = 0; outputNodeIndex < outputNodes.Length; outputNodeIndex++)
        {
            outputNodes[outputNodeIndex] = new NeuralNode(0, new float[hidenNodes.GetLength(1)]);
            outputNodes[outputNodeIndex].activated = true;
        }

        weightsZero = new float[hidenNodes.GetLength(1)];
        for (int weight = 0; weight < weightsZero.Length; weight++)
            weightsZero[weight] = 0;
    }

    //Evaluate
    public float[] Eval(float[] inputVec)
    {
        Matrix calcMatrix;
        float[] outputVecAux;
        bool rowWhitActivatedNodes = false;
        //Hiden Nodes
        float[] bias = new float[hidenNodes.GetLength(1)];
        outputVecAux = inputVec;//new float[inputVec.Length]; SEE LATER Use deepClone?

        for (int column = 0; column < hidenNodes.GetLength(0); column++)
        {
            //calcMatrix = new Matrix(hidenNodes.GetLength(1), outputVecAux.Length);
            calcMatrix = new Matrix(outputVecAux.Length, hidenNodes.GetLength(1));
            rowWhitActivatedNodes = false;

            for (int row = 0; row < hidenNodes.GetLength(1); row++)
            {
                if (hidenNodes[column, row].activated)
                {
                    rowWhitActivatedNodes = true;
                    break;
                }
            }

            if (rowWhitActivatedNodes)
            {
                for (int row = 0; row < hidenNodes.GetLength(1); row++)
                {
                    if (hidenNodes[column, row].activated)
                    {
                        calcMatrix.AddRow(hidenNodes[column, row].weights);
                        bias[row] = hidenNodes[column, row].bias;
                    }
                    else
                    {
                        weightsZero = new float[outputVecAux.Length];
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

    //Mutate NeuralNetwork
    public void MutateNeuralNetwork(float weightMutationRate, float biasMutationRate, float activationProb)
    {
        var rand = new Random();
        foreach (NeuralNode neuralNode in hidenNodes)
        {
            if (!neuralNode.activated)
            {
                if ((float)(rand.NextDouble()) <= activationProb)
                    neuralNode.activated = true;
            }
        }

        foreach (NeuralNode neuralNode in hidenNodes)
        {
            if (neuralNode.activated)
            {
                neuralNode.bias += (float)((rand.NextDouble() * 2) - 1) * biasMutationRate;

                for (int weightIndex = 0; weightIndex < neuralNode.weights.Length; weightIndex++)
                    neuralNode.weights[weightIndex] += (float)((rand.NextDouble() * 2) - 1) * weightMutationRate;
            }
        }

        foreach (NeuralNode neuralNode in outputNodes)
        {
            if (neuralNode.activated)
            {
                neuralNode.bias += (float)((rand.NextDouble() * 2) - 1) * biasMutationRate;

                for (int weightIndex = 0; weightIndex < neuralNode.weights.Length; weightIndex++)
                    neuralNode.weights[weightIndex] += (float)((rand.NextDouble() * 2) - 1) * weightMutationRate;
            }
        }
    }
}
[Serializable]
public class NeuralNode
{
    public float bias = 0;
    public float[] weights;

    public bool activated = false;

    public NeuralNode() { }
    public NeuralNode(float bias, float[] weights)
    {
        this.bias = bias;
        this.weights = weights;

        //for (int weightIndex = 0; weightIndex < weights.Length; weightIndex++)
        //    weights[weightIndex] = 0;
    }
}