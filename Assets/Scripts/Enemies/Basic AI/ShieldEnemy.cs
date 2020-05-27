using System;
using System.Collections.Generic;
using PlayerStates;
using UnityEngine;

public class ShieldEnemy : BaseEnemy {
	enum AggroAction {
		None,
		MeleeAttack,
	}
	AggroAction aggroAction = AggroAction.None;

	// Serialized vars
	[SerializeField] GameObject tongue;
	[SerializeField] GameObject shield;

	// Base attack variables
	Vector2 attackDirection;
	float attackStopwatch = 0;
	int attackStage;

	// Tongue attack variables
	Vector3 tongueStartPosition;
	Vector3 tongueStartScale;
	Vector3 tongueEndPosition;
	Vector3 tongueEndScale;
	const float tongueDuration = 0.5f;

	// AI Variables
	Dictionary<int, Action> aiActions = new Dictionary<int, Action> ();

	// Start is called before the first frame update
	void Start () {
		base.Start ();

		tongueStartPosition = tongue.transform.localPosition;
		tongueStartScale = tongue.transform.localScale;
		tongueEndPosition = new Vector3 (tongueStartPosition.x, 0.18f, 1.5f);
		tongueEndScale = new Vector3 (tongueStartScale.x, tongueStartScale.y, 2.5f);

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

		if (canSeePlayer && Vector2.Angle (Util.V3toV2 (transform.forward), Util.V3toV2 (player.transform.position - transform.position)) < 3) {
			if (playerSqrDistance < Util.Square (2f)) {
				aggroAction = AggroAction.MeleeAttack;
				attackStage = 0;
				attackStopwatch = 0;
			}
		}
	}

	void MeleeAttack () {
		// starting values: position.y = 0.58 | position.z = 0   | scale.z = 0 
		// final values:    position.y = 0.18 | position.z = 1.5 | scale.z = 2.5

		float lerpVal = 2 * attackStopwatch / tongueDuration;
		tongue.transform.localPosition = Vector3.Lerp (tongueStartPosition, tongueEndPosition, lerpVal);
		tongue.transform.localScale = Vector3.Lerp (tongueStartScale, tongueEndScale, lerpVal);

		switch (attackStage) {
			case 0:
				attackStopwatch += Time.deltaTime;
				if (attackStopwatch >= tongueDuration / 2) {
					attackStage = 1;
				}
				break;
			case 1:
				attackStopwatch -= Time.deltaTime;
				if (attackStopwatch <= 0)
					attackStage = 2;
				break;
			case 2:
				tongue.GetComponent<Hurtbox> ().Reset ();
				aggroAction = AggroAction.None;
				break;
		}
	}

	// Timers that should be updated using Time.deltaTime every Update() go here
	void UpdateTimers () { }
}