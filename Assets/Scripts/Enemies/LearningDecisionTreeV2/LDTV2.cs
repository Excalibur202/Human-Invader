using SaveLoad;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LDTV2
{
    private List<Node> nodeToExplode = new List<Node>();
    Table table = new Table();
    List<Node> LDTRoot = new List<Node>();
    
    private int[] rowRealTimeStates;

    public LDTV2(){ }

    //Get
    public int GetColumnCount
    {
        get
        {
            return table.tableData.Count;
        }
    }
    public int GetTableRowCount
    {
        get
        {
            return table.GetTableRowCount;
        }
    }
    public int GetTotalRowCount
    {
        get
        {
            return table.GetTotalRowCount;
        }
    }
    
    //Func
    public void AddColumn(string columnName)
    { table.AddColumn(columnName); }
    public void AddRow(params int[] row)
    { table.AddRow(row); }
    public void CreateDecisionTree()
    {
        LDTRoot.Add(new TableNode(table, nodeToExplode,LDTRoot,0));
        for (int tableNode = 0; tableNode < nodeToExplode.Count + 1; tableNode++)
        {
            nodeToExplode[0].ExplodeNode(nodeToExplode);
            nodeToExplode.RemoveAt(0);
            tableNode = 0;
        }
    }
    public void RefreshStates(params int[] row)
    {
        this.rowRealTimeStates = row;
    }

    
    
    public void SaveTable(string folderPath, string fileName)
    {
        table.SaveBinary(folderPath, fileName);
    }
    public void LoadTable(string folderPath, string fileName)
    {
        table = table.LoadBinary(folderPath, fileName);
    }

    public void SaveDecisionTree(string folderPath, string fileName)
    {
        LDTRoot.SaveBinary(folderPath, fileName);
    }
    public void LoadDecisionTree(string folderPath, string fileName)
    {
        LDTRoot = LDTRoot.LoadBinary(folderPath, fileName);
    }

    public void Eval(Dictionary<int, Action> aiActions)
    {
        LDTRoot[0].Eval(rowRealTimeStates, aiActions);
    }

    public void DebugTable()
    {
        table.DebugTable();
    }
}










