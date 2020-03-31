using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMainMenu : MonoBehaviour
{
    public Camera camera;
    public Transform[] views;
    public float transitionSpeed;
    private Transform currentView;
    private int viewCount = 0;

    void Update()
    {
        //views[0].transform.Rotate(0, Time.deltaTime * transitionSpeed, 0);
    }

    void LateUpdate()
    {
        
        //transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitionSpeed);

        //Vector3 currentAngle = new Vector3(Mathf.LerpAngle(transform.eulerAngles.x, transform.eulerAngles.x, Time.deltaTime * transitionSpeed),
        //                                   Mathf.LerpAngle(transform.eulerAngles.y, transform.eulerAngles.y, Time.deltaTime * transitionSpeed),
        //                                   Mathf.LerpAngle(transform.eulerAngles.z, transform.eulerAngles.z, Time.deltaTime * transitionSpeed));

        //transform.eulerAngles = currentAngle;

    }
}
