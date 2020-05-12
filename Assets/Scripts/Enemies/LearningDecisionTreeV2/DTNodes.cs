using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
abstract class Node
{
    abstract public void Eval(int[] row, Dictionary<int, Action> aIActions);
    abstract public void ExplodeNode(List<Node> nodesToExplode);

}

class TableNode : Node
{
    Table table;
    List<Node> nodesToExplode;
    private List<Node> nextNodes = new List<Node>();
    private List<Node> lastNodes = new List<Node>();
    int thisNodeIndex = 0;
    
    public TableNode(Table table, List<Node> nodesToExplode, List<Node> lastNodes, int thisNodeIndex)
    {
        this.table = table;
        this.lastNodes = lastNodes;
        this.nodesToExplode = nodesToExplode;
        this.thisNodeIndex = thisNodeIndex;

        nodesToExplode.Add(this);
    }



    private float ColumnEntropy(int column)
    {
        double entropy = 0;
        float stateProb = 0;
        List<int> columnStates = table.GetColumnStates(column);

        if (columnStates != null)
            foreach (int state in columnStates)
            {
                stateProb = table.IndividualStateProbability(column, state);
                if (stateProb != 0)
                    entropy -= stateProb * (Math.Log(stateProb, 2));
            }

        return (float)entropy;
    }
    private float ArrayEntropy(int[] nOccurrences, int total)
    {
        double entropy = 0;
        float stateOcc = 0;

        foreach (int nOcc in nOccurrences)
        {
            stateOcc = (float)nOcc / total;
            if (stateOcc != 0 && total != 0)
                entropy -= stateOcc * (Math.Log(stateOcc, 2));
        }
        return (float)entropy;
    }
    private float InfoGain(int column)
    {
        int actionColumn = table.tableData.Count - 2;
        int tableRowCount = table.GetTableRowCount;
        float infoGain = ColumnEntropy(actionColumn);
        int indexAction = 0;

        List<int> columnStates = table.GetColumnStates(column);
        List<int> actionStates = table.GetColumnStates(actionColumn);

        foreach (int state in columnStates)
        {
            //Get actions per columnState
            indexAction = 0;
            int[] actionsCount = new int[table.GetNumberOfStates(actionColumn)];
            foreach (int action in actionStates)
            {
                for (int row = 0; row < tableRowCount; row++)
                {
                    if (table.tableData[table.GetColumnName(column)][row] == state && table.tableData[table.GetColumnName(actionColumn)][row] == action)
                        actionsCount[indexAction] += table.tableData[table.GetColumnName(table.tableData.Count - 2)][row];
                }
                indexAction++;
            }
            infoGain -= table.IndividualStateProbability(column, state) * ArrayEntropy(actionsCount, table.GetStateCount(column, state));
        }

        return infoGain;
    }
    private int IndexBestInfoGainColumn()
    {
        float bestInfoGain = 0;
        int bestInfoGainColumn = 0;
        float columnInfoGain = 0;

        if (table.tableData.Count > 0)
        {
            int ActionColumn = table.tableData.Count - 2;

            for (int column = 0; column < ActionColumn; column++)
            {
                columnInfoGain = InfoGain(column);
                if (bestInfoGain <= columnInfoGain)
                {
                    bestInfoGain = columnInfoGain;
                    bestInfoGainColumn = column;
                }
            }
            return bestInfoGainColumn;
        }
        return -1;
    }

    public override void ExplodeNode(List<Node> nodesToExplode)
    {
        

        float actionColumnEntropy = ColumnEntropy(table.tableData.Count - 2);

        if (!(actionColumnEntropy == 0) && table.tableData.Count > 2)
        {
            int bestInfoGainColumn = 0;

            bestInfoGainColumn = IndexBestInfoGainColumn();
            List<int> stateNames = table.GetColumnStates(bestInfoGainColumn);

            for (int state = 0; state < table.GetNumberOfStates(bestInfoGainColumn); state++)
            {
                nextNodes.Add(new TableNode(table.FilterTableByState(bestInfoGainColumn, stateNames[state]), nodesToExplode,nextNodes,state));
            }

            lastNodes[thisNodeIndex] = new DecisionNode(nextNodes, table.GetColumnStates(bestInfoGainColumn), bestInfoGainColumn);

            //Debug.Log("DecisionNode///////////////////////////////////////////////////////");
            //table.DebugTable();
        }
        else
        {
            //Create ActionNode
            int actionColumn = table.tableData.Count - 2;
            List<int> individualStateCounts = new List<int>();
            List<int> stateNames = table.GetColumnStates(actionColumn);
            foreach (int state in stateNames)
                individualStateCounts.Add(table.GetStateCount(actionColumn, state));

            lastNodes[thisNodeIndex] = new ActionNode(stateNames, individualStateCounts);
            //Debug.Log("ActionNode///////////////////////////////////////////////////////");
            //table.DebugTable();
        }

    }



    public override void Eval(int[] row, Dictionary<int, Action> aIActions)
    {
        Debug.Log(this);
    }
}

[Serializable]
class DecisionNode : Node
{
    private List<Node> nodes;
    private List<int> columnStates;
    public int bestInfoGainColumn;

    public DecisionNode(List<Node> nodes, List<int> columnStates, int bestInfoGainColumn)
    {
        this.nodes = nodes;
        this.columnStates = columnStates;
        this.bestInfoGainColumn = bestInfoGainColumn;
    }

    public override void Eval(int[] row, Dictionary<int, Action> aIActions)
    {
        int selectedNode = -1;
        int index = -1;
        if (row != null)
            foreach (int state in columnStates)
            {
                index++;
                if (state == row[bestInfoGainColumn])
                {
                    selectedNode = index;
                    break;
                }
            }

        if (selectedNode > -1)
        {
            int[] newRow = new int[row.Length - 1];

            int indexAux = 0;
            int newRowIndex = 0;

            foreach (int rowState in row)
            {
                if (indexAux != bestInfoGainColumn)
                {
                    newRow[newRowIndex] = rowState;
                    newRowIndex++;
                }
                indexAux++;
            }
            nodes[selectedNode].Eval(newRow, aIActions);
        }
        //else
        //{
        //    Debug.Log("Error: selectedNode = " + selectedNode);
        //}


    }

    public override void ExplodeNode(List<Node> nodesToExplode) { }
}

[Serializable]
class ActionNode : Node
{
    List<int> actionNames = new List<int>();
    List<int> actionCount = new List<int>();

    //Constructor
    public ActionNode(List<int> actionNames, List<int> actionCount)
    {
        this.actionNames = actionNames.DeepClone();
        this.actionCount = actionCount.DeepClone();
    }

    //Func
    public override void Eval(int[] row, Dictionary<int, Action> aIActions)
    {
        aIActions[actionNames[RandAction(actionCount)]]();
    }
    public override void ExplodeNode(List<Node> nodesToExplode) { }
    private int RandAction(List<int> probs)
    {
        int rand = 0;
        int total = 0;

        foreach (int prob in probs)
            total += prob;

        rand = Random.Range(0, total + 1);

        total = 0;
        int indexObstacle = -1;
        foreach (int prob in probs)
            if (rand >= total)
            {
                total += prob;
                indexObstacle++;
            }

        return indexObstacle;
    }
}