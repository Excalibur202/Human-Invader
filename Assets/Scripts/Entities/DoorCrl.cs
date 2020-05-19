using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorCrl : MonoBehaviour
{
    public Animation WingLeft;
    public Animation WingRight;
    public bool unlocked = false;

    public void OpenDoor()
    {
        unlocked = true;
        GetComponent<AudioSource>().Play();

        WingLeft["door_01_wing_left"].speed = 1;
        WingRight["door_01_wing_right"].speed = 1;
        WingLeft.Play();
        WingRight.Play();

    }

    public void CloseDoor()
    {
        GetComponent<AudioSource>().Play();
        WingLeft["door_01_wing_left"].time = WingLeft["door_01_wing_left"].length;
        WingRight["door_01_wing_right"].time = WingRight["door_01_wing_right"].length;
        WingLeft["door_01_wing_left"].speed = -1;
        WingRight["door_01_wing_right"].speed = -1;
        WingLeft.Play();
        WingRight.Play();
    }
}
