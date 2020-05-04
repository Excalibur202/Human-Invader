using System.Collections;
using UnityEngine;

public class Melee : BaseEnemy {
	enum AggroAction {
		none,
		attack,
		ability1,
	}
	AggroAction aggroAction = AggroAction.none;

	[SerializeField] float ability1CooldownMax;
	float ability1Cooldown;
	float attackTimer;

	// Start is called before the first frame update
	void Start () {
		base.Start ();
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

		if (canSeePlayer) {

			if (ability1Cooldown == 0 && playerSqrDistance < Util.Square (2f)) {
				ability1Cooldown = ability1CooldownMax;

				aggroAction = AggroAction.ability1;
			}

			if (playerSqrDistance < Util.Square (1f)) {
				aggroAction = AggroAction.attack;
			}
		}
	}

	void Ability1 () {

	}

	void MeleeAttack () {

	}

	void UpdateTimers () {
		if (attackTimer > 0) {
			attackTimer -= Time.deltaTime;
			if (attackTimer < 0) {
				attackTimer = 0;
			}
		}

		if (ability1Cooldown > 0) {
			ability1Cooldown -= Time.deltaTime;
			if (ability1Cooldown < 0) {
				ability1Cooldown = 0;
			}
		}
	}
}