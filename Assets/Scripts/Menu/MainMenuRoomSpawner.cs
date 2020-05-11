using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuRoomSpawner : MonoBehaviour
{
    public GameObject[] roomsToSpawn;
    public Camera mainMenuCamera;
    public Transform player;
    private Vector3 spot;
    private GameObject appear;
    private int random;
    public int posY = 0;
    void Start()
    {
        posY = 1;
        random = Random.Range(0, roomsToSpawn.Length);

        appear = Instantiate(roomsToSpawn[random],this.transform.position, this.transform.rotation);

        spot = appear.transform.position;

        spot.y += posY;

        mainMenuCamera = Instantiate(mainMenuCamera, spot, appear.transform.rotation);
    }
}
