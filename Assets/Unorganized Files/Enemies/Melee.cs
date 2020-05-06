using System;
using System.Collections.Generic;
using PlayerStates;
using UnityEngine;

public class Melee : BaseEnemy {
	enum AggroAction {
		none,
		attack,
		ability1,
	}
	AggroAction aggroAction = AggroAction.none;

	// Serialized Options
	[SerializeField] float ability1CooldownMax;
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
			case AggroAction.none:
				UpdateAI ();
				break;

			case AggroAction.ability1:
				Ability1 ();
				break;

			case AggroAction.attack:
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
			/*
			if (ability1Cooldown == 0 && playerSqrDistance < Util.Square (4f)) {
				aggroAction = AggroAction.attack;
				attackStage = 0;
			}
			*/
			if (playerSqrDistance < Util.Square (2f)) {
				aggroAction = AggroAction.attack;
				attackStage = 0;
			}
		}
	}

	void Ability1 () {
		switch (attackStage) {
			case 0:
				break;
		}
	}

	void MeleeAttack () {
		switch (attackStage) {
			case 0:
				stick.transform.localPosition += new Vector3 (0, 0, 4 * Time.deltaTime);

				if (stick.transform.localPosition.z >= 2)
					attackStage = 1;
				break;

			case 1:

				stick.transform.localPosition -= new Vector3 (0, 0, 4 * Time.deltaTime);

				if (stick.transform.localPosition.z <= 0)
					attackStage = 2;
				break;

			case 2:
				stick.transform.localPosition = Vector3.zero;
				aggroAction = AggroAction.none;
				break;
		}
	}

	void UpdateTimers () {
		if (aggroAction == AggroAction.ability1) {
			attackStopwatch += Time.deltaTime;
		}

		if (ability1Cooldown > 0) {
			ability1Cooldown -= Time.deltaTime;

			if (ability1Cooldown <= 0) {
				ability1Cooldown = 0;
			}
		}
	}
}