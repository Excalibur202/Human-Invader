using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Basic enemy class with behavior states but no 
public class BaseEnemy : MonoBehaviour {
    public enum Behavior {
        passive,
        aggro,
        waiting,
        returning
    }

    [SerializeField] Behavior currentBehavior;
    [SerializeField] float ground_Y;
    [SerializeField] float aboveGround_Y;
    [SerializeField] float moveSpeed;
    [SerializeField] float aggroRange;
    [SerializeField] float health;

    CharacterController charCtrl;
    GameObject player;
    float defaultMoveSpeed;

    // Movement and navigation
    [SerializeField] bool canSeePlayer;
    Vector3 playerLastSightedAt;
    float playerSqrDistance;
    List<Vector3> recentPositions = new List<Vector3> ();
    List<Vector3> waypoints = new List<Vector3> ();

    // Strafing behavior
    float timerStrafe;
    bool strafeRight;

    // Damage effect
    Material damageEffectMaterial;
    bool beingDamaged;
    bool damagedRecently;

    void Start () {
        player = GameObject.FindGameObjectWithTag ("Player");
        charCtrl = transform.GetComponentInChildren<CharacterController> ();

        currentBehavior = Behavior.passive;
        defaultMoveSpeed = moveSpeed;

        // Waypoint pathing system
        StartCoroutine (PositionLibrarian ());
        StartCoroutine (WaypointPathMaker ());

        StartCoroutine (AggroScanner ());
    }

    void Update () {
        // Always stick to same Y position above ground
        if (transform.position.y != ground_Y + aboveGround_Y)
            transform.position = UtilF.V3setY (transform.position, ground_Y + aboveGround_Y);

        playerSqrDistance = Vector3.SqrMagnitude (transform.position - player.transform.position);
        canSeePlayer = false;

        // Different updates depending on behavior
        switch (currentBehavior) {
            case (Behavior.passive):
                break;

            case (Behavior.aggro):
                AggroAI ();
                break;

            case (Behavior.returning):
                ReturnAI ();
                break;

            case (Behavior.waiting):
                break;
        }

    }

    void AggroAI () {
        // Get player info if within the double of aggro range
        if (playerSqrDistance < UtilF.SquareOf (aggroRange * 2)) {
            RaycastHit raycastHit;
            Physics.Raycast (new Ray (transform.position, player.transform.position - transform.position), out raycastHit, UtilF.SquareOf (aggroRange), LayerMask.GetMask ("Default", "Player"));

            if (raycastHit.collider && raycastHit.collider.CompareTag ("Player")) {
                canSeePlayer = true;
                playerLastSightedAt = player.transform.position;
            }
        } else
            // If further than double of aggro range
            if (!damagedRecently) {
                StartCoroutine (ReturnToSpawn (playerLastSightedAt));
                return;
            }

        timerStrafe += Time.deltaTime;

        MoveTowards (playerLastSightedAt);
    }

    // Iterates through the waypoints vector list in the direction lastIndex -> 0
    // With the exception of the first waypoint, deletes each waypoint as it reaches them
    void ReturnAI () {
        // If there are still waypoints beyond the home base
        if (waypoints.Count > 1) {
            // Approach next waypoint
            if (Vector2.Distance (UtilF.V3toV2 (waypoints[waypoints.Count - 1]), UtilF.V3toV2 (transform.position)) > 0.1f)
                MoveTowards (waypoints[waypoints.Count - 1], 10);

            // Delete waypoint on arrival
            else
                waypoints.RemoveAt (waypoints.Count - 1);
        }

        // If haven't arrived at the home base yet
        else if (Vector2.Distance (UtilF.V3toV2 (waypoints[0]), UtilF.V3toV2 (transform.position)) > 1f) {
            MoveTowards (waypoints[0], 10);
        }

        // On reaching the home base
        else {
            // Set HP back to full
            GetComponent<HealthController> ().ResetHP ();

            currentBehavior = Behavior.passive;
        }
    }

    // Used when the enemy loses track of the player
    // Restarts the aggro checker, and after a few seconds swaps to return behavior
    IEnumerator ReturnToSpawn (Vector3 lastSeenPlayerPosAtWaitTime) {
        currentBehavior = Behavior.waiting;

        StartCoroutine (AggroScanner ());

        yield return new WaitForSeconds (3);

        if (currentBehavior == Behavior.waiting) {
            currentBehavior = Behavior.returning;
        }
    }

    // Move and rotate towards a direction with options for strafing and distance limit
    void MoveTowards (Vector3 moveTarget, float forwardMovementAngleLimit = 30, bool strafing = false, int limitDistance = 0) {
        if (moveSpeed > 0) {
            Vector3 movementVector = new Vector3 ();
            Quaternion targetRotation = new Quaternion ();

            float distanceFromTarget = Vector3.SqrMagnitude (transform.position - moveTarget);

            // Rotate towards the position
            targetRotation = Quaternion.LookRotation (
                new Vector3 (moveTarget.x - transform.position.x,
                    0,
                    moveTarget.z - transform.position.z)
            );

            // Rotate faster as get closer to target
            float smooth = Mathf.Clamp (1000 / Vector2.Distance (UtilF.V3toV2 (moveTarget), UtilF.V3toV2 (transform.position)), 150, 800);
            transform.rotation = (Quaternion.RotateTowards (transform.rotation, targetRotation, smooth * Time.deltaTime));

            float angleToTarget = Vector2.Angle (UtilF.V3toV2 (transform.forward), UtilF.V3toV2 (moveTarget - transform.position));

            if (angleToTarget <= forwardMovementAngleLimit) {
                if (limitDistance <= 0) {
                    movementVector += transform.forward;
                } else {
                    if (distanceFromTarget < limitDistance * 0.9f)
                        movementVector -= transform.forward;

                    else if (distanceFromTarget > limitDistance * 1.1f)
                        movementVector += transform.forward;
                }
            }

            if (strafing) {
                if (timerStrafe >= 0.8f) {
                    if (UnityEngine.Random.Range (0, 2) == 0)
                        strafeRight = false;
                    else
                        strafeRight = true;
                    timerStrafe = 0;
                };

                if (strafeRight)
                    movementVector += transform.right * 0.5f;
                else
                    movementVector -= transform.right * 0.5f;
            }

            Move (movementVector.normalized);
        }
    }

    // Move towards a direction with out rotation
    void Move (Vector3 moveVector) {
        moveVector = UtilF.V3setY (moveVector, 0).normalized;
        moveVector *= moveSpeed * Time.deltaTime;
        charCtrl.Move (moveVector);
    }

    // Scan for the player with line of sight
    IEnumerator AggroScanner () {
        while (true) {
            if (damagedRecently) {
                currentBehavior = Behavior.aggro;
                break;
            }

            // If player is within aggro range
            else if (playerSqrDistance < UtilF.SquareOf (aggroRange)) {
                RaycastHit raycastHit;

                // Cast a ray from the enemy to the player
                if (Physics.Raycast (new Ray (transform.position, player.transform.position - transform.position), out raycastHit, aggroRange, LayerMask.GetMask ("Obstacle", "Player"))) {
                    if (raycastHit.collider.CompareTag ("Player")) {
                        playerLastSightedAt = player.transform.position;

                        // Switch to aggro behavior
                        currentBehavior = Behavior.aggro;

                        // Break out of the aggro check loop
                        break;
                    }
                }
            }

            // Retry aggro scan after X seconds
            yield return new WaitForSeconds (0.25f);
        }
    }

    // Keeps recent enemy positions, used when deciding where to set a new waypoint
    IEnumerator PositionLibrarian () {
        recentPositions.Add (transform.position);

        while (true) {
            // Don't keep more than X most recent ones
            while (recentPositions.Count > 10)
                recentPositions.RemoveAt (0);

            // If the enemy has moved X distance from the most recent position stored
            if (Vector3.SqrMagnitude (recentPositions[recentPositions.Count - 1] - transform.position) > 1f) {
                // Store that new position
                recentPositions.Add (transform.position);
            }

            yield return new WaitForSeconds (0.1f);
        }
    }

    // Lays down a series of waypoints that the enemy can follow to return to spawn
    IEnumerator WaypointPathMaker () {
        // Add spawn location as the first travel waypoint
        waypoints.Add (UtilF.V3setY (transform.position, ground_Y + aboveGround_Y));

        while (true) {
            int lastWaypoint = waypoints.Count - 1;

            // If the enemy has moved away from last waypoint
            if (Vector3.SqrMagnitude (waypoints[lastWaypoint] - transform.position) > 0.25f) {
                RaycastHit raycastHit;

                // Cast a ray from the last waypoint towards the enemy
                Physics.Raycast (new Ray (waypoints[lastWaypoint], transform.position - waypoints[lastWaypoint]), out raycastHit, Vector3.Distance (transform.position, waypoints[lastWaypoint]), 1);

                // If the last waypoint can't see the enemy (collided with a map object)
                if (raycastHit.collider) {
                    // Go through the recent positions list until one is in sight of the last waypoint
                    int recentPosIndex = recentPositions.Count - 1;
                    while (recentPosIndex >= 0) {
                        Physics.Raycast (new Ray (waypoints[lastWaypoint], recentPositions[recentPosIndex] - waypoints[lastWaypoint]), out raycastHit, Vector3.Distance (waypoints[lastWaypoint], recentPositions[recentPosIndex]), 1);

                        // If the last waypoint can see this recent position (raycast did not collide with anything)
                        if (!raycastHit.collider) {
                            // Add position as a new waypoint
                            waypoints.Add (recentPositions[recentPosIndex]);

                            // Check for possible waypoint optimizations
                            if (waypoints.Count > 2)
                                WaypointPathMinimizer (waypoints.Count - 1);

                            break;
                        }

                        // Check an older position
                        recentPosIndex--;
                    }

                    // If for some reason couldn't find a visible recent position, reset waypoints and set current position as a new home
                    // Should hopefully never reach this state, this is a safeguard
                    if (recentPosIndex == -1) {
                        print ("reset waypoint list! recent pos index at the time: " + recentPosIndex);
                        waypoints = new List<Vector3> ();
                        waypoints.Add (transform.position);

                    }
                }
            }

            // Only run every x seconds to make it more lightweight
            yield return new WaitForSeconds (0.1f);
        }
    }

    // Find and delete redundant waypoints inbetween home and a given waypoint
    void WaypointPathMinimizer (int indexWP) {
        if (waypoints.Count > 2) {
            RaycastHit raycastHit;

            for (int i = 0; i < indexWP - 1; i++) {
                // Raycast from the inputed waypoint to the current waypoint
                Physics.Raycast (new Ray (waypoints[indexWP], waypoints[i]), out raycastHit, Vector3.Distance (waypoints[indexWP], waypoints[i]), 1);

                // If the last waypoint can see this earlier waypoint (raycast did not collide with a map object)
                if (!raycastHit.collider) {
                    // Remove each waypoint inbetween the current waypoint and the index waypoint
                    waypoints.RemoveRange (i + 1, indexWP - i - 1);

                    break;
                }
            }
        }

        return;
    }

    // Manages the visual indication of damage
    IEnumerator DamageEffect () {
        if (!beingDamaged && damageEffectMaterial) {
            beingDamaged = true;

            var mr = GetComponentsInChildren<MeshRenderer> ();
            List<MeshRenderer> meshRenderers = new List<MeshRenderer> ();
            List<Material> oldMaterials = new List<Material> ();

            foreach (MeshRenderer r in mr) {
                meshRenderers.Add (r);
                oldMaterials.Add (r.material);

                r.material = damageEffectMaterial;
            }

            yield return new WaitForSeconds (0.2f);

            while (meshRenderers.Count > 0) {
                meshRenderers[0].material = oldMaterials[0];
                meshRenderers.RemoveAt (0);
                oldMaterials.RemoveAt (0);
            }

            beingDamaged = false;
        }
    }

    // Makes the enemy temporarily chase the player regardless of distance
    IEnumerator RageMode () {
        damagedRecently = true;

        yield return new WaitForSeconds (5);

        damagedRecently = false;

        StopCoroutine (RageMode ());
    }

    // Triggers behavior and visual changes that should occur on receiving damage
    // Called everytime the enemy is receives damage
    public void onDamage () {
        // If hit by damage and not already enraged, enable rage mode
        if (!damagedRecently)
            StartCoroutine (RageMode ());

        // Restart rage mode if already enraged
        else {
            StopCoroutine (RageMode ());
            StartCoroutine (RageMode ());
        }

        StartCoroutine (DamageEffect ());
    }

    public void Die () {
        //// DEATH ANIMATION?

        Destroy (transform);

        return;
    }

}