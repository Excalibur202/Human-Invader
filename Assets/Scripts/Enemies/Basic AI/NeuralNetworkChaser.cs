using UnityEngine;

public class NeuralNetworkChaser : BaseEnemy {
    int timesRanStart = 0;

    // Start is called before the first frame update
    void Start () {
        base.Start ();
        
        canSeePlayer = true;
        currentBehavior = Behavior.aggro;
        playerLastSightedAt = player.transform.position;
    }

    void Update () {
        if (playerSqrDistance < Util.Square (2.9f)) {
            moveSpeed = 0;
        } else
            moveSpeed = defaultMoveSpeed;

        base.Update ();
    }
}