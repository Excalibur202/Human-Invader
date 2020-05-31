using System;
using System.Collections.Generic;
using PlayerStates;
using UnityEngine;

public class MeleeEnemy : BaseEnemy {
	enum AggroAction {
		None,
		MeleeAttack,
	}
	AggroAction aggroAction = AggroAction.None;

	// Serialized Options
	//[SerializeField] float ability1CooldownMax;
	[SerializeField] GameObject stick;

	// General attack variables
	Vector2 attackDirection;
	float attackStopwatch;
	int attackStage;

	// Unique variables
	float ability1Cooldown;
	Vector3 stickDefaultPosition;
	Quaternion stickDefaultRotation;

	// AI Variables
	Dictionary<int, Action> aiActions = new Dictionary<int, Action> ();

	// Start is called before the first frame update
	void Start () {
		base.Start ();

		stickDefaultPosition = stick.transform.localPosition;
		stickDefaultRotation = stick.transform.localRotation;

		Action chaseAttack = () => {
			strafing = false;
		};

		Action strafeAttack = () => {
			strafing = true;
		};

		aiActions.Add ((int) PlayerAction.Nothing, chaseAttack);
		aiActions.Add ((int) PlayerAction.Attack, strafeAttack);
		aiActions.Add ((int) PlayerAction.UsingAbility1, chaseAttack);
		aiActions.Add ((int) PlayerAction.UsingAbility2, chaseAttack);
	}

	// Update is called once per frame
	void Update () {
		UpdateTimers ();

		switch (aggroAction) {
			case AggroAction.None:
				UpdateAI ();
				break;

			case AggroAction.MeleeAttack:
				MeleeAttack ();
				break;
		}
	}

	void UpdateAI () {
		// Run basic chase and pathfinding AI from base class
		base.Update ();

		switch (currentBehavior) {
			case Behavior.aggro:
				AggroAI ();
				break;
		}
	}

	void AggroAI () {
		EvalLDT (aiActions);

		if (canSeePlayer) {
			if (playerSqrDistance < Util.Square (1f)) {
				aggroAction = AggroAction.MeleeAttack;
				attackStage = 0;
			}
		}
	}

	void MeleeAttack () {
		switch (attackStage) {
			case 0:
				stick.transform.localPosition += new Vector3 (0, 0, 12 * Time.deltaTime);

				if (stick.transform.localPosition.z >= 2)
					attackStage = 1;
				break;

			case 1:

				stick.transform.localPosition -= new Vector3 (0, 0, 12 * Time.deltaTime);

				if (stick.transform.localPosition.z <= 0)
					attackStage = 2;
				break;

			case 2:
				stick.transform.localPosition = Vector3.zero;
				stick.GetComponent<Hurtbox> ().Reset ();

				aggroAction = AggroAction.None;
				break;
		}
	}

	// Timers that should be updated using Time.deltaTime every Update() go here
	void UpdateTimers () { }
}