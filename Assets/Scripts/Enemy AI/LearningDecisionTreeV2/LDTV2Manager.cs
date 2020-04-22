using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerStates;



public class LDTV2Manager : MonoBehaviour
{
    public static LDTV2Manager instance;
    public LDTV2 lDT = new LDTV2();
    private int[] realTimeStates = new int[3];
    private int lastAction = (int)PlayerAction.Nothing;

    public bool recreateDecisionTree;
    public bool learning;
    public bool reset;
    public bool debugTable;


    //public bool state;
    //public bool state2;



    private void Awake()
    {
        if (instance)
            Destroy(this);

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (reset)
        {
            lDT.AddColumn("Health");
            lDT.AddColumn("Ability");
            /*__________________________*/

            lDT.AddColumn("Actions");
            lDT.AddColumn("Duplicates");

            recreateDecisionTree = true;
        }
        else
        {
            lDT.LoadTable("Assets\\AIData\\LDTreeData", "TableData");
            lDT.LoadDecisionTree("Assets\\AIData\\LDTreeData", "DTree");
        }

        if (debugTable)
            lDT.DebugTable();

        if (recreateDecisionTree)
            lDT.CreateDecisionTree();

    }

    // Update is called once per frame
    void Update()
    {
        lDT.RefreshStates(realTimeStates);

        //Learning
        if (learning && lastAction != realTimeStates[(int)StateIndex.Action])
        {
            lDT.AddRow(realTimeStates);
            lastAction = realTimeStates[(int)StateIndex.Action];
        }
    }

    private void OnApplicationQuit()
    {
        lDT.SaveTable("Assets\\AIData\\LDTreeData", "TableData");
        lDT.SaveDecisionTree("Assets\\AIData\\LDTreeData", "DTree");
    }

    /*______________________________________________________________________________________*/

    public void SetState(StateIndex state, int value)
    {
        realTimeStates[(int)state] = value;
    }



}
