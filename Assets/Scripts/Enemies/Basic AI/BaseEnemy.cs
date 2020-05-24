using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Basic enemy class with pathfinding, player tracking and behavior states
public class BaseEnemy : MonoBehaviour
{
    protected enum Behavior
    {
        passive,
        aggro,
        waiting,
        returning
    }

    [SerializeField] protected Behavior currentBehavior;
    [SerializeField] protected float ground_Y;
    [SerializeField] protected float aboveGround_Y;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float aggroRange;
    [SerializeField] protected float health;

    protected CharacterController charCtrl;
    protected GameObject player;
    protected float defaultMoveSpeed;
    public bool hasKeycard;

    // Movement and navigation
    public bool canSeePlayer;
    protected Vector3 playerLastSightedAt;
    public float playerSqrDistance;
    protected List<Vector2> recentPos = new List<Vector2>();
    protected List<Vector2> waypoints = new List<Vector2>();

    // Strafing behavior
    protected bool strafing;
    bool strafeRight;
    float timerStrafe;

    // Damage effect
    Material damageEffectMaterial;
    bool damageEffectActive;
    bool damagedRecently;

    Coroutine slowdownCoroutine;

    protected void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        charCtrl = transform.GetComponentInChildren<CharacterController>();

        currentBehavior = Behavior.passive;
        defaultMoveSpeed = moveSpeed;

        transform.position = Util.V3setY(transform.position, ground_Y + aboveGround_Y);

        // Waypoint pathing system
        // Adds spawn location as the "home base", the first travel waypoint
        waypoints.Add(Util.V3toV2(transform.position));
        recentPos.Add(Util.V3toV2(transform.position));
        StartCoroutine(PositionLibrarian());
        StartCoroutine(WaypointPathMaker());

        StartCoroutine(AggroScanner());
    }

    public void Reset()
    {
        transform.position = Util.V2toV3(waypoints[0], ground_Y + aboveGround_Y);

        waypoints = new List<Vector2>();
        recentPos = new List<Vector2>();
        waypoints.Add(Util.V3toV2(transform.position));
        recentPos.Add(Util.V3toV2(transform.position));
        StartCoroutine(PositionLibrarian());
        StartCoroutine(WaypointPathMaker());

        StartCoroutine(AggroScanner());
    }

    protected void Update()
    {
        if (!player)
            player = GameObject.FindGameObjectWithTag("Player");

        playerSqrDistance = Util.SqrDistance(transform.position, player.transform.position);

        // AI is only able to see the player at double the aggro range
        if (playerSqrDistance <= Util.Square(aggroRange * 2))
        {
            RaycastHit raycastHit = Util.RayFromTo(transform.position, player.transform.position, LayerMask.GetMask("Obstacle", "Player"), aggroRange * 2);

            if (raycastHit.collider && raycastHit.collider.CompareTag("Player"))
            {
                canSeePlayer = true;
                playerLastSightedAt = player.transform.position;
            }
            else
            {
                canSeePlayer = false;
            }
        }
        else
            canSeePlayer = false;

        // Different updates depending on behavior
        switch (currentBehavior)
        {
            case (Behavior.aggro):
                ChaseAI();
                break;

            case (Behavior.returning):
                ReturnAI();
                break;
        }
    }

    void ChaseAI()
    {
        timerStrafe += Time.deltaTime;

        if (canSeePlayer)
        {
            MoveTowards(playerLastSightedAt);
        }

        // Move to the last place player was seen
        else if (Util.SqrDistance(transform.position, playerLastSightedAt, true) > Util.Square(0.25f))
        {
            MoveTowards(playerLastSightedAt);

            if (!damagedRecently && playerSqrDistance > Util.Square(aggroRange * 2))
            {
                StartCoroutine(ReturnToSpawn());
            }
        }
        // If already standing where the player was last seen
        else
        {
            StartCoroutine(ReturnToSpawn());
        }
    }

    // Iterates through the waypoints vector list in the direction lastIndex -> 0
    // With the exception of the first waypoint, deletes each waypoint as it reaches them
    void ReturnAI()
    {
        // If there are still waypoints beyond the home base
        if (waypoints.Count > 1)
        {

            // Approach next waypoint
            if (Util.SqrDistance(waypoints[waypoints.Count - 1], Util.V3toV2(transform.position)) > Util.Square(0.1f))
            {
                MoveTowards(Util.V2toV3(waypoints[waypoints.Count - 1], ground_Y + aboveGround_Y), 5);
            }

            // Delete waypoint on arrival
            else
            {
                waypoints.RemoveAt(waypoints.Count - 1);
            }
        }

        // If haven't arrived at the home base yet
        else if (Util.SqrDistance(waypoints[0], Util.V3toV2(transform.position)) > Util.Square(1f))
        {
            MoveTowards(Util.V2toV3(waypoints[0], transform.position.y), 10);
        }

        // On reaching the home base
        else
        {
            // Set HP back to full
            GetComponent<HealthController>().ResetHP();

            currentBehavior = Behavior.passive;
        }
    }

    // Move and rotate towards a direction with options for strafing and distance limit
    protected void MoveTowards(Vector3 moveTarget, float forwardMovementAngleLimit = 30, bool strafing = false, int limitDistance = 0)
    {
        if (moveSpeed > 0)
        {
            Vector3 movementVector = new Vector3();
            Quaternion targetRotation = new Quaternion();

            float distanceFromTarget = Util.SqrDistance(transform.position, moveTarget, true);

            // Rotate towards the position
            targetRotation = Quaternion.LookRotation(new Vector3(moveTarget.x - transform.position.x, 0, moveTarget.z - transform.position.z));

            // Rotate faster as get closer to target
            float smooth = Mathf.Clamp(1000 / Vector2.Distance(Util.V3toV2(moveTarget), Util.V3toV2(transform.position)), 150, 800);
            transform.rotation = (Quaternion.RotateTowards(transform.rotation, targetRotation, smooth * Time.deltaTime));

            float angleToTarget = Vector2.Angle(Util.V3toV2(transform.forward), Util.V3toV2(moveTarget - transform.position));

            if (angleToTarget <= forwardMovementAngleLimit)
            {
                if (limitDistance <= 0)
                {
                    movementVector += transform.forward;
                }
                else
                {
                    if (distanceFromTarget < limitDistance * 0.9f)
                        movementVector -= transform.forward;

                    else if (distanceFromTarget > limitDistance * 1.1f)
                        movementVector += transform.forward;
                }
            }

            /////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////
            // EXPERIMENTAL ARMS
            Ray leftArm = new Ray(transform.position, transform.forward - transform.right);
            Ray rightArm = new Ray(transform.position, transform.forward + transform.right);
            if (Physics.Raycast(leftArm, 2, LayerMask.GetMask("Obstacle")))
            {
                strafing = true;
                strafeRight = true;
                timerStrafe = 0;
            }
            else if (Physics.Raycast(rightArm, 2, LayerMask.GetMask("Obstacle")))
            {
                strafing = true;
                strafeRight = false;
                timerStrafe = 0;
            }
            else
                strafing = false;
            /////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////

            if (strafing)
            {
                if (timerStrafe >= 0.8f)
                {
                    if (UnityEngine.Random.Range(0, 2) == 0)
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

            Move(movementVector);
        }
    }

    // Move towards a direction (no rotation)
    protected void Move(Vector3 moveVector)
    {
        moveVector = Util.V3setY(moveVector, 0).normalized;
        moveVector *= moveSpeed * Time.deltaTime;
        charCtrl.Move(moveVector);
        transform.position = Util.V3setY(transform.position, ground_Y + aboveGround_Y);
    }

    protected void EvalLDT(Dictionary<int, Action> aiActions)
    {
        //LDTV2Manager.instance.lDT.Eval (aiActions);
    }

    // Used when the enemy loses track of the player
    // Restarts the aggro checker, and after a few seconds swaps to return behavior
    IEnumerator ReturnToSpawn()
    {
        currentBehavior = Behavior.waiting;

        StartCoroutine(AggroScanner());

        yield return new WaitForSeconds(3);

        // Make sure the way back is as efficient as possible
        for (int i = waypoints.Count - 1; i > 0; i--)
            if (i == waypoints.Count - 1)
                WaypointPathMinimizer(i);

        if (currentBehavior == Behavior.waiting)
        {
            currentBehavior = Behavior.returning;
        }
    }

    // Scan for the player with line of sight
    IEnumerator AggroScanner()
    {
        while (true)
        {

            if (damagedRecently)
            {
                currentBehavior = Behavior.aggro;
                break;
            }

            // If player is within aggro range
            else if (playerSqrDistance < Util.Square(aggroRange))
            {

                RaycastHit raycastHit = Util.RayFromTo(transform.position, player.transform.position, LayerMask.GetMask("Obstacle", "Player"), aggroRange);

                if (raycastHit.collider && raycastHit.collider.CompareTag("Player"))
                {
                    playerLastSightedAt = player.transform.position;

                    // Switch to aggro behavior
                    currentBehavior = Behavior.aggro;

                    // Break out of the aggro check loop
                    break;
                }
            }

            // Retry aggro scan after X seconds
            yield return new WaitForSeconds(0.5f);
        }
    }

    // Keeps recent enemy positions, used when deciding where to set a new waypoint
    IEnumerator PositionLibrarian()
    {
        while (true)
        {
            // Don't keep more than X most recent ones
            while (recentPos.Count > 10)
                recentPos.RemoveAt(0);

            // If the enemy has moved X distance from the most recent position stored
            if (Util.SqrDistance(recentPos[recentPos.Count - 1], Util.V3toV2(transform.position)) > Util.Square(1f))
            {
                // Store that new position
                recentPos.Add(Util.V3toV2(transform.position));
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    // Lays down a series of waypoints that the enemy can follow to return to spawn
    IEnumerator WaypointPathMaker()
    {
        while (true)
        {
            int lastWaypoint = waypoints.Count - 1;

            // If the enemy has moved away from last waypoint
            if (Util.SqrDistance(waypoints[lastWaypoint], Util.V3toV2(transform.position)) > Util.Square(0.5f))
            {

                // Cast a ray from the last waypoint to the enemy
                RaycastHit raycastHit = Util.RayFromTo(Util.V2toV3(waypoints[lastWaypoint], ground_Y + aboveGround_Y), transform.position, LayerMask.GetMask("Obstacle"));

                // If there was an obstacle on the way
                if (raycastHit.collider)
                {
                    int recentPosIndex;

                    // Go through the recent positions list from the most recent to the oldest
                    for (recentPosIndex = recentPos.Count - 1; recentPosIndex >= 0; recentPosIndex--)
                    {

                        // Cast a ray from the last waypoint to the position
                        raycastHit = Util.RayFromTo(waypoints[lastWaypoint], recentPos[recentPosIndex], ground_Y + aboveGround_Y, LayerMask.GetMask("Obstacle"));

                        // If there was no obstacles on the way
                        if (!raycastHit.collider)
                        {
                            // Add the position as a new waypoint
                            waypoints.Add(recentPos[recentPosIndex]);

                            // Check for possible waypoint optimizations
                            if (waypoints.Count > 2)
                                WaypointPathMinimizer(waypoints.Count - 1);

                            break;
                        }
                    }

                    // If for some reason couldn't find a visible recent position, reset waypoints and set the current position as a new home
                    // Should hopefully never reach this state, this is a safeguard
                    if (recentPosIndex == -1)
                    {
                        print("had to reset waypoint list! \n recent pos index at the time: " + recentPosIndex + "\n waypoints at the time: " + waypoints);
                        waypoints = new List<Vector2>();
                        waypoints.Add(Util.V3toV2(transform.position));

                    }
                }
            }

            // Only run every x seconds to make it more lightweight
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Find and delete redundant waypoints inbetween home and a given waypoint
    void WaypointPathMinimizer(int indexWP)
    {
        if (waypoints.Count > 1)
        {
            RaycastHit raycastHit;

            for (int i = 0; i < indexWP - 1; i++)
            {
                // Raycast from the inputed waypoint to the current waypoint
                raycastHit = Util.RayFromTo(waypoints[indexWP], waypoints[i], transform.position.y, LayerMask.GetMask("Obstacle"));

                // If the last waypoint can see this earlier waypoint (raycast did not collide with a map object)
                if (!raycastHit.collider)
                {
                    // Remove each waypoint inbetween the current waypoint and the index waypoint
                    waypoints.RemoveRange(i + 1, indexWP - i - 1);

                    break;
                }
            }
        }

        return;
    }

    // Manages the visual indication of damage
    IEnumerator DamageEffect()
    {
        if (!damageEffectActive && damageEffectMaterial)
        {
            damageEffectActive = true;

            var mr = GetComponentsInChildren<MeshRenderer>();
            List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
            List<Material> oldMaterials = new List<Material>();

            foreach (MeshRenderer r in mr)
            {
                meshRenderers.Add(r);
                oldMaterials.Add(r.material);

                r.material = damageEffectMaterial;
            }

            yield return new WaitForSeconds(0.2f);

            while (meshRenderers.Count > 0)
            {
                meshRenderers[0].material = oldMaterials[0];
                meshRenderers.RemoveAt(0);
                oldMaterials.RemoveAt(0);
            }

            damageEffectActive = false;
        }
    }

    // Makes the enemy temporarily chase the player regardless of distance
    IEnumerator RageMode()
    {
        damagedRecently = true;

        yield return new WaitForSeconds(5);

        damagedRecently = false;

        StopCoroutine(RageMode());
    }

    // Triggers behavior and visual changes that should occur on receiving damage
    // Called everytime the enemy is receives damage
    public void onDamage()
    {

        // If hit by damage and not already enraged, enable rage mode
        if (!damagedRecently)
            StartCoroutine(RageMode());

        // Restart rage mode if already enraged
        else
        {
            StopCoroutine(RageMode());
            StartCoroutine(RageMode());
        }

        StartCoroutine(DamageEffect());
    }

    public void Die()
    {
        if (hasKeycard)
        {
            // Spawn a keycard and make it jump a little in some direction
            GameObject keycard = Instantiate(
                Resources.Load("Keycard") as GameObject,
                transform.position,
                Quaternion.Euler(Util.RndRange(0, 1), Util.RndRange(0, 1), Util.RndRange(0, 1)));

            keycard.GetComponent<Rigidbody>().AddForce(5 * new Vector3(Util.RndRange(0, 1), Util.RndRange(0, 1), Util.RndRange(0, 1)), ForceMode.VelocityChange);
            keycard.GetComponent<Rigidbody>().AddTorque(10 * new Vector3(Util.RndRange(0, 1), Util.RndRange(0, 1), Util.RndRange(0, 1)), ForceMode.VelocityChange);
        }

        //// DEATH ANIMATION?
        Destroy(gameObject);

        return;
    }

    // Applies a % slowdown for X seconds, replacing an ongoing slowdown effect if active  
    public void ApplyTemporarySlowdown(float slowdownMultiplier, float duration)
    {
        if (slowdownCoroutine != null)
            StopCoroutine(slowdownCoroutine);

        slowdownCoroutine = StartCoroutine(TemporarySlowdownEffect(slowdownMultiplier, duration));
    }

    IEnumerator TemporarySlowdownEffect(float slowdownMultiplier, float duration)
    {
        moveSpeed = defaultMoveSpeed * slowdownMultiplier;
        yield return new WaitForSeconds(duration);
        moveSpeed = defaultMoveSpeed;
    }
}