using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_01 : MonoBehaviour
{

    //	public GameObject Wing_Right;
    //	public GameObject Wing_Left;
    List<GameObject> nearbyCreatures = new List<GameObject>();
    bool open = false;
    public Animation WingLeft;
    public Animation WingRight;

    private void Update()
    {
        if (open)
        {
            for (int i = 0; i < nearbyCreatures.Count; i++)
            {
                if (!nearbyCreatures[i]) nearbyCreatures.RemoveAt(i--);
            }
            if (nearbyCreatures.Count == 0) CloseDoor();
        }
            
    }

    void OnTriggerEnter(Collider c)
    {
        if (nearbyCreatures.Count == 0) OpenDoor();

        nearbyCreatures.Add(c.gameObject);
    }

    void OnTriggerExit(Collider c)
    {
        nearbyCreatures.Remove(c.gameObject);
    }

    private void OpenDoor()
    {
        GetComponent<AudioSource>().Play();
        WingLeft["door_01_wing_left"].speed = 1;
        WingRight["door_01_wing_right"].speed = 1;
        WingLeft.Play();
        WingRight.Play();
        open = true;
    }

    private void CloseDoor()
    {
        GetComponent<AudioSource>().Play();
        WingLeft["door_01_wing_left"].time = WingLeft["door_01_wing_left"].length;
        WingRight["door_01_wing_right"].time = WingRight["door_01_wing_right"].length;
        WingLeft["door_01_wing_left"].speed = -1;
        WingRight["door_01_wing_right"].speed = -1;
        WingLeft.Play();
        WingRight.Play();
        open = false;
    }




}
