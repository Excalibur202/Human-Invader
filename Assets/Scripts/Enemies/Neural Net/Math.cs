﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matrix
{
    float[][] matrix;
    int selectedRow = 0;
    int columns;

    public Matrix(int columns, int rows)
    {
        matrix = new float[rows][];
        this.columns = columns;
    }

    public int Rows
    { get { return this.matrix.Length; } }

    public int Columns
    { get { return this.columns; } }

    public float[][] Data
    { get { return this.matrix; } }

    public bool AddRow(float[] row)
    {
        if (selectedRow < matrix.Length && row.Length == columns)
        {
            matrix[selectedRow++] = row;
            return true;
        }
        return false;
    }

    //Multiply (Matrix * Vector)
    public static float[] operator *(Matrix matrix, float[] vector)
    {
        float[] resultVector = new float[matrix.Rows];
        int resultVectorIndex = 0;
        int matrixRowIndex = 0;
        float rowTotal = 0;
        foreach (float[] row in matrix.Data)
        {
            foreach (float value in vector)
                rowTotal += row[matrixRowIndex++] * value;
            matrixRowIndex = 0;
            resultVector[resultVectorIndex++] = rowTotal;
            rowTotal = 0;
        }
        return resultVector;
    }
}

public static class MyMath
{
    public static float GetSlope(float x, float y)
    {
        return y / x;
    }

    public static float CalculateAverage(params float[] values)
    {
        float sum = 0;
        for (int i = 0; i < values.Length; i++)
            sum += values[i];

        return (sum / values.Length);
    }
}

public static class NeuralNetworkMath
{
    public static float Sigmoid(double value)
    {
        float k = (float)Math.Exp(value);
        return (value > 20) ? 1 : (value < -20) ? 0 : k / (1.0f + k);
    }

    public static float sigmoidToAxis(float value)
    {
        float result = (value - 0.5f) * 2;
        return (result < 0) ? 0 : (result > 1) ? 1 : result;
    }
}
