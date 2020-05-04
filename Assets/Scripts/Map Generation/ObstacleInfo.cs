using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleInfo
{
    Transform obstacleTransform;
    Transform obstacleRightCorner;

    public ObstacleInfo(Transform obstacleTransform, Transform obstacleRightCorner)
    {
        this.obstacleTransform = obstacleTransform;
        this.obstacleRightCorner = obstacleRightCorner;
    }

    //Get/Set
    public Transform GetObstacleTransform
    {
        get
        {
            return obstacleTransform;
        }
    }
    public Transform GetRightCornerTransform
    {
        get
        {
            return obstacleRightCorner;
        }
    }
}
