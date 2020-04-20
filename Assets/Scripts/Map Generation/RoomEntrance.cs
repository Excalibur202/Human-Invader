using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEntrance : MonoBehaviour
{
    public bool obstacles;
    public List<Transform> exitPoints;

    public GameObject roomDimension;

    public Transform consoleTransform;
    public Transform rightCorner;
    
    public List<GameObject> navMeshObjs;
    public List<Transform> navMeshRightCorners;
}
