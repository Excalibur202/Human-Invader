using System.Collections;
using System.Collections.Generic;
using System;

using Random = UnityEngine.Random;

[Serializable]
public class NeuralNetwork
{
    public NeuralNode[,] hiddenNodes; // not public 
    public NeuralNode[] outputNodes;
    public float fitness,lastFitness = 0;
    float[] weightsZero;
    public int generation;

    public NeuralNetwork()
    { }

    //Constructor
    public NeuralNetwork(int hiddenNColumns, int hiddenNRows, int nInputs, int nOutputs/*, float[] inputNodesRef*/)
    {
        hiddenNodes = new NeuralNode[hiddenNColumns, hiddenNRows];
        outputNodes = new NeuralNode[nOutputs];

        //Input weights
        for (int rowIndex = 0; rowIndex < hiddenNodes.GetLength(1); rowIndex++)
        {
            hiddenNodes[0, rowIndex] = new NeuralNode(0, new float[nInputs/*inputVec.Length*/]);
            hiddenNodes[0, rowIndex].activated = true;////////
        }
        hiddenNodes[0, 0].activated = true;

        //HidenNodes weights
        for (int columnIndex = 1; columnIndex < hiddenNodes.GetLength(0); columnIndex++)
            for (int rowIndex = 0; rowIndex < hiddenNodes.GetLength(1); rowIndex++)
                hiddenNodes[columnIndex, rowIndex] = new NeuralNode(0, new float[hiddenNodes.GetLength(1)]);

        //Output weithrs
        for (int outputNodeIndex = 0; outputNodeIndex < outputNodes.Length; outputNodeIndex++)
        {
            outputNodes[outputNodeIndex] = new NeuralNode(0, new float[hiddenNodes.GetLength(1)]);
            outputNodes[outputNodeIndex].activated = true;
        }

        weightsZero = new float[hiddenNodes.GetLength(1)];
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
        float[] bias = new float[hiddenNodes.GetLength(1)];
        outputVecAux = inputVec;//new float[inputVec.Length]; SEE LATER Use deepClone?

        for (int column = 0; column < hiddenNodes.GetLength(0); column++)
        {
            //calcMatrix = new Matrix(hidenNodes.GetLength(1), outputVecAux.Length);
            calcMatrix = new Matrix(outputVecAux.Length, hiddenNodes.GetLength(1));
            rowWhitActivatedNodes = false;

            for (int row = 0; row < hiddenNodes.GetLength(1); row++)
            {
                if (hiddenNodes[column, row].activated)
                {
                    rowWhitActivatedNodes = true;
                    break;
                }
            }

            if (rowWhitActivatedNodes)
            {
                for (int row = 0; row < hiddenNodes.GetLength(1); row++)
                {
                    if (hiddenNodes[column, row].activated)
                    {
                        calcMatrix.AddRow(hiddenNodes[column, row].weights);
                        bias[row] = hiddenNodes[column, row].bias;
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
        foreach (NeuralNode neuralNode in hiddenNodes)
        {
            if (!neuralNode.activated)
            {
                if ((float)(Random.value) <= activationProb)
                    neuralNode.activated = true;
            }
        }

        foreach (NeuralNode neuralNode in hiddenNodes)
        {
            if (neuralNode.activated)
            {
                neuralNode.bias += (float)((Random.value * 2) - 1) * biasMutationRate;

                for (int weightIndex = 0; weightIndex < neuralNode.weights.Length; weightIndex++)
                    neuralNode.weights[weightIndex] += (float)((Random.value * 2) - 1) * weightMutationRate;
            }
        }

        foreach (NeuralNode neuralNode in outputNodes)
        {
            if (neuralNode.activated)
            {
                neuralNode.bias += (float)((Random.value * 2) - 1) * biasMutationRate;

                for (int weightIndex = 0; weightIndex < neuralNode.weights.Length; weightIndex++)
                    neuralNode.weights[weightIndex] += (float)((Random.value * 2) - 1) * weightMutationRate;
            }
        }
    }

    //Fuse 2 NeuralNetwork
    public static NeuralNetwork FuseNeuralNetwork(NeuralNetwork nNA, NeuralNetwork nNB)
    {
        NeuralNetwork nNResult;
        if (nNA != null && nNB != null)
        {
            nNResult = nNA.DeepClone();
            for (int column = 0; column < nNResult.hiddenNodes.GetLength(0); column++)
                for (int row = 0; row < nNResult.hiddenNodes.GetLength(1); row++)
                {
                    if (nNA.hiddenNodes[column, row].activated && nNB.hiddenNodes[column, row].activated)
                    {
                        nNResult.hiddenNodes[column, row].bias = MyMath.CalculateAverage(nNA.hiddenNodes[column, row].bias, nNB.hiddenNodes[column, row].bias); //Byas

                        for (int indexWeight = 0; indexWeight < nNResult.hiddenNodes[column, row].weights.Length; indexWeight++)
                            nNResult.hiddenNodes[column, row].weights[indexWeight] = MyMath.CalculateAverage(nNA.hiddenNodes[column, row].weights[indexWeight], nNB.hiddenNodes[column, row].weights[indexWeight]);//Weights
                    }
                    else if (nNB.hiddenNodes[column, row].activated)
                    {
                        nNResult.hiddenNodes[column, row].activated = nNB.hiddenNodes[column, row].activated;//Activation
                        nNResult.hiddenNodes[column, row].bias = nNB.hiddenNodes[column, row].bias; //Byas

                        for (int indexWeight = 0; indexWeight < nNResult.hiddenNodes[column, row].weights.Length; indexWeight++)
                            nNResult.hiddenNodes[column, row].weights[indexWeight] = nNB.hiddenNodes[column, row].weights[indexWeight];//Weights
                    }
                }
            return nNResult;
        }
        return null;
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