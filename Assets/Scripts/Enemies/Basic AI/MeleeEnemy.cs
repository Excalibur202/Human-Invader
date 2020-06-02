using System;
using System.Collections.Generic;
using PlayerStates;
using UnityEngine;

public class MeleeEnemy : BaseEnemy
{
    enum AggroAction
    {
        None,
        MeleeAttack,
    }
    AggroAction aggroAction = AggroAction.None;

    // Serialized Options
    [SerializeField] GameObject stick;
    Vector3 defaultStickPos;

    // General attack variables
    int attackStage = -1;

    // AI Variables
    Dictionary<int, Action> aiActions = new Dictionary<int, Action>();

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        defaultStickPos = stick.transform.localPosition;

        Action chaseAttack = () =>
        {
            strafing = false;
        };

        Action strafeAttack = () =>
        {
            strafing = true;
        };

        aiActions.Add((int)PlayerAction.Nothing, chaseAttack);
        aiActions.Add((int)PlayerAction.Attack, strafeAttack);
        aiActions.Add((int)PlayerAction.UsingAbility1, chaseAttack);
        aiActions.Add((int)PlayerAction.UsingAbility2, chaseAttack);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimers();

        UpdateAI();
        MeleeAttack();
    }


    void UpdateAI()
    {
        // Run basic chase and pathfinding AI from base class
        base.Update();

        switch (currentBehavior)
        {
            case Behavior.aggro:
                AggroAI();
                break;
        }
    }

    void AggroAI()
    {
        EvalLDT(aiActions);

        if (attackStage == -1 && canSeePlayer)
        {
            if (playerAngleFromForward < 5 && playerSqrDistance < Util.Square(2f))
            {
                aggroAction = AggroAction.MeleeAttack;
                attackStage = 0;
            }
        }
    }

    void MeleeAttack()
    {
        switch (attackStage)
        {
            case 0:
                stick.transform.localPosition += new Vector3(0, 0, 5 * Time.deltaTime);

                if (stick.transform.localPosition.z >= 2)
                    attackStage = 1;
                break;

            case 1:

                stick.transform.localPosition -= new Vector3(0, 0, 5 * Time.deltaTime);

                if (stick.transform.localPosition.z <= 0)
                    attackStage = 2;
                break;

            case 2:
                stick.transform.localPosition = defaultStickPos;
                stick.GetComponent<Hurtbox>().Reset();

                aggroAction = AggroAction.None;
                attackStage = -1;
                break;
        }
    }

    // Timers that should be updated using Time.deltaTime every Update() go here
    void UpdateTimers() { }
}