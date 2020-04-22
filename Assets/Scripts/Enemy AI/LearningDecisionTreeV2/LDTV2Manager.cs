using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LDTV2Manager : MonoBehaviour
{
    public static LDTV2Manager singleton;
    public LDTV2 lDT = new LDTV2();

    private string[] realTimeStates = new string[3];

    public bool recreateDecisionTree;
    public bool learning;

    public bool state;
    public bool state2;



    private void Awake()
    {
        if (singleton)
            Destroy(gameObject);

        singleton = this;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        lDT.LoadTable("LDTreeData", "TableData");
        lDT.LoadDecisionTree("LDTreeData", "DTree");

        lDT.AddColumn("Health");
        lDT.AddColumn("Ability");


        lDT.AddColumn("Actions");//Actions
        lDT.AddColumn("Duplicates");//Dups

        lDT.AddRow(0, 0, 0);
        lDT.AddRow(0, 1, 1);
        lDT.AddRow(1, 0, 2);
        lDT.AddRow(1, 1, 3);

        lDT.DebugTable();

        if (recreateDecisionTree)
            lDT.CreateDecisionTree();

    }

    // Update is called once per frame
    void Update()
    {
        int actions = 0;
        int s1, s2;
        if (state)
            s1 = 1;
        else s1 = 0;

        if (state2)
            s2 = 1;
        else s2 = 0;

        lDT.RefreshStates(s1, s2, actions);



        if (learning)
            lDT.AddRow(s1, s2, actions);

    }

    private void OnApplicationQuit()
    {
        lDT.SaveTable("LDTreeData", "TableData");
        lDT.SaveDecisionTree("LDTreeData", "DTree");
    }
}
