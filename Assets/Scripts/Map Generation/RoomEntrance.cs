using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEntrance : MonoBehaviour
{
    public List<Transform> exitPoints;

    public GameObject roomDimension;
    public Transform playerSpawnPoint;

    public Transform consoleTransform;
    public Transform rightCorner;
    
    public List<Transform> navMeshObjs;
    public List<Transform> navMeshRightCorners;

    public bool obstaclesActivation;
    public List<Transform> obstacles;
    public List<Transform> obstaclesRightCorners;

    public bool spawnEnemies;
    public List<Transform> enemies;
}
