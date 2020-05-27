using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityCaster : MonoBehaviour {
    [SerializeField] Camera cam;

    // Serialized variables
    [SerializeField] Transform mineOrigin;
    [SerializeField] GameObject minePrefab;
    [SerializeField] GameObject mineHologramPrefab;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] float mineSpeed = 20;
    [SerializeField] float mineMaxCooldown = 1;

    List<Vector3> points;
    GameObject mineHologram;
    RaycastHit mineRaycastHit;
    float mineCooldown = 0;
    bool usingAbility;

    void Start () {
        if (!cam)
            cam = (Camera) GameObject.FindObjectOfType (typeof (Camera));
        if (!minePrefab) {
            print ("Error! No mine prefab.");
            return;
        }
    }

    void Update () {
        if (mineCooldown > 0) {
            mineCooldown -= Time.deltaTime;
            if (mineCooldown < 0)
                mineCooldown = 0;
        }
    }

    internal void UseAbility (int abilityKey) {
        if (!usingAbility) {
            usingAbility = true;

            print ("Using ability number " + abilityKey);
            switch (abilityKey) {
                case 1:
                    if (mineCooldown == 0)
                        StartCoroutine(SoldierMine());
                    else
                        usingAbility = false;
                    break;
                default:
                    usingAbility = false;
                    return;
            }
        }
    }

    private IEnumerator SoldierMine () {
        if (!mineHologram && mineHologramPrefab)
            mineHologram = Instantiate (mineHologramPrefab);
        else {
            print ("Error, no mine hologram prefab!");
            StopCoroutine (SoldierMine ());
        }

        RaycastHit rayHit;
        Vector3 throwDirection = cam.transform.forward;

        while (Input.GetKey (KeyCode.F)) {
            if (Physics.Raycast (cam.transform.position, cam.transform.forward, out rayHit, 9999, LayerMask.GetMask ("Obstacle")))
                throwDirection = (rayHit.point - mineOrigin.position).normalized;
            else
                throwDirection = cam.transform.forward;

            DrawMineTracer (mineOrigin.position, throwDirection, mineSpeed);

            yield return new WaitForSeconds (Time.smoothDeltaTime);
        }
        lineRenderer.enabled = false;

        GameObject mine = Instantiate (minePrefab);
        mine.GetComponent<Mine> ().SetDestination (mineRaycastHit.rigidbody, mineHologram.transform.position, mineHologram.transform.rotation);
        mine.transform.position = mineOrigin.position;
        Rigidbody mineRigidbody = (Rigidbody) mine.GetComponent (typeof (Rigidbody));
        mineRigidbody.velocity = throwDirection * mineSpeed;

        // comment lines 54-55 and uncomment this for full-auto mine shooting
        //     yield return new WaitForSeconds (Time.smoothDeltaTime);
        // }

        mineHologram.SetActive (false);
        usingAbility = false;
        
        mineCooldown = mineMaxCooldown;
    }

    private IEnumerator SoldierRadar () { throw new NotImplementedException (); }

    private void DrawMineTracer (Vector3 origin, Vector3 direction, float speed) {
        int stepSeconds = 3;
        Vector3 velocity = direction * speed;

        points = new List<Vector3> ();
        points.Add (origin);
        points.Add (origin + velocity * stepSeconds * Time.fixedDeltaTime);

        // Calculate points along the mine's predicted path until colliding with the map or going too far
        while (points.Count < 100 &&
            !Physics.SphereCast (new Ray (points[points.Count - 2], points[points.Count - 1] - points[points.Count - 2]),
                0.1f,
                Vector3.Distance (points[points.Count - 2], points[points.Count - 1]),
                LayerMask.GetMask ("Obstacle"))) {
            velocity += Physics.gravity * 3 * Time.fixedDeltaTime;
            points.Add (points[points.Count - 1] + velocity * stepSeconds * Time.fixedDeltaTime);
        }

        // If it collided with the floor, shift the last point to where it collided
        if (points.Count < 100) {
            Physics.SphereCast (
                new Ray (
                    points[points.Count - 2],
                    points[points.Count - 1] - points[points.Count - 2]),
                0.1f,
                out mineRaycastHit,
                Vector3.Distance (points[points.Count - 2], points[points.Count - 1]),
                LayerMask.GetMask ("Obstacle"));
            points[points.Count - 1] = mineRaycastHit.point;

            // Place a mine hologram on the point of contact
            if (mineHologram) {
                mineHologram.SetActive (true);
                mineHologram.transform.position = points[points.Count - 1];
                mineHologram.transform.rotation = Quaternion.LookRotation (mineRaycastHit.normal);
                mineHologram.transform.Rotate (new Vector3 (90, 0, 0));
            } else
                mineHologram.SetActive (false);
        }

        // Draw lines between the points
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions (points.ToArray ());

        lineRenderer.enabled = true;
    }
}