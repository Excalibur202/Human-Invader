using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityCaster : MonoBehaviour {
    public List<Vector3> points;
    [SerializeField] Camera cam;
    [SerializeField] Transform mineOrigin;
    [SerializeField] GameObject minePrefab;
    [SerializeField] GameObject mineHologramPrefab;
    [SerializeField] float mineSpeed = 20;
    RaycastHit mineRaycastHit;
    GameObject mineHologram;
    bool usingAbility = false;

    void Start () {
        if (!cam)
            cam = (Camera) GameObject.FindObjectOfType (typeof (Camera));
    }

    internal void UseAbility (int abilityKey) {
        if (!usingAbility) {
            usingAbility = true;

            print ("Using ability number " + abilityKey);
            switch (abilityKey) {
                case 1:
                    StartCoroutine (SoldierMine ());
                    break;
                case 2:
                    StartCoroutine (SoldierRadar ());
                    break;
                case 3:
                    StartCoroutine (KnightSlash ());
                    break;
                case 4:
                    StartCoroutine (KnightBash ());
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

        while (Input.GetKey (KeyCode.F)) {
            DrawMineTracer (mineOrigin.position, cam.transform.forward, mineSpeed);

            yield return new WaitForSeconds (Time.smoothDeltaTime);
        }

        GameObject mine = Instantiate (minePrefab);
        mine.GetComponent<Mine> ().SetDestination (mineRaycastHit.rigidbody, mineHologram.transform.position, mineHologram.transform.rotation);
        mine.transform.position = mineOrigin.position;
        Rigidbody mineRigidbody = (Rigidbody) mine.GetComponent (typeof (Rigidbody));
        mineRigidbody.velocity = cam.transform.forward * mineSpeed;

        // comment lines 54-55 and uncomment this for fun times autofire
        //     yield return new WaitForSeconds (Time.smoothDeltaTime);
        // }

        mineHologram.SetActive (false);
        usingAbility = false;

        yield return null;
    }

    private IEnumerator SoldierRadar () { throw new NotImplementedException (); }

    private IEnumerator KnightSlash () { throw new NotImplementedException (); }

    private IEnumerator KnightBash () { throw new NotImplementedException (); }

    private void DrawMineTracer (Vector3 origin, Vector3 direction, float speed) {
        Vector3 velocity = direction * speed;

        points = new List<Vector3> ();
        points.Add (origin);
        points.Add (origin + velocity * 3 * Time.fixedDeltaTime);

        // Calculate points along the mine's predicted path until colliding with the map or going too far
        while (points.Count < 100 &&
            !Physics.SphereCast (new Ray (points[points.Count - 2], points[points.Count - 1] - points[points.Count - 2]),
                0.1f,
                Vector3.Distance (points[points.Count - 2], points[points.Count - 1]),
                LayerMask.GetMask ("Obstacle"))) {
            velocity += Physics.gravity * 3 * Time.fixedDeltaTime;
            points.Add (points[points.Count - 1] + velocity * 3 * Time.fixedDeltaTime);
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
        for (int i = 0; i < points.Count - 1; i++) {
            Debug.DrawLine (points[i], points[i + 1], Color.green);
        }
    }
}