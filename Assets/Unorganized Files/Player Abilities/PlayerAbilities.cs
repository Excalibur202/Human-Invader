using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour {
    List<Vector3> points;
    Camera cam;
    [SerializeField] Transform mineOrigin;
    [SerializeField] GameObject minePrefab;
    [SerializeField] GameObject mineHologramPrefab;
    [SerializeField] float mineSpeed = 20;
    GameObject mineHologram;
    bool usingAbility = false;

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

    void DrawMineTracer (Vector3 origin, Vector3 direction, float speed) {
        float maxTracerDistance = 50;
        Vector3 velocity = direction * speed;

        points = new List<Vector3> ();
        points.Add (origin);
        points.Add (origin + velocity * Time.fixedDeltaTime);

        // Calculate points along the mine's predicted path until colliding with the map or going too far
        while (Vector3.Distance (origin, points[points.Count - 1]) < maxTracerDistance &&
            !Physics.Raycast (
                points[points.Count - 2],
                points[points.Count - 1] - points[points.Count - 2],
                Vector3.Distance (points[points.Count - 2], points[points.Count - 1]),
                LayerMask.GetMask ("Obstacle"))) {
            velocity += Physics.gravity * Time.fixedDeltaTime;
            points.Add (points[points.Count - 1] + velocity * Time.fixedDeltaTime);
        }

        // If it collided with the floor, shift the last point to where it collided
        RaycastHit raycastHit;
        if (Vector3.Distance (origin, points[points.Count - 1]) < maxTracerDistance) {
            Physics.Raycast (points[points.Count - 2],
                points[points.Count - 1] - points[points.Count - 2],
                out raycastHit,
                Vector3.Distance (points[points.Count - 2], points[points.Count - 1]),
                LayerMask.GetMask ("Obstacle"));
            points[points.Count - 1] = raycastHit.point;

            // Place a mine hologram on the point of contact
            if (mineHologram) {
                mineHologram.SetActive (true);
                mineHologram.transform.position = points[points.Count - 1];
                mineHologram.transform.rotation = Quaternion.LookRotation (raycastHit.normal);
                mineHologram.transform.Rotate (new Vector3 (90, 0, 0));
            }
            else
                mineHologram.SetActive(false);
        }

        // Draw lines between the points
        for (int i = 0; i < points.Count - 1; i++) {
            Debug.DrawLine (points[i], points[i + 1], Color.green);
        }
    }

    private IEnumerator SoldierMine () {
        if (!mineHologram && mineHologramPrefab)
            mineHologram = Instantiate (mineHologramPrefab);
        if (!cam)
            cam = (Camera) GameObject.FindObjectOfType (typeof (Camera));

        while (Input.GetKey (KeyCode.E)) {
            DrawMineTracer (mineOrigin.position, cam.transform.forward, mineSpeed);

            yield return new WaitForSeconds (Time.smoothDeltaTime);
        }

        var mine = Instantiate(minePrefab);
        mine.transform.position = mineOrigin.position;
        var mineRigidbody = (Rigidbody)mine.GetComponent(typeof(Rigidbody));
        mineRigidbody.velocity = cam.transform.forward * mineSpeed;

        mineHologram.SetActive (false);
        usingAbility = false;

        yield return null;
    }

    private IEnumerator SoldierRadar () { throw new NotImplementedException (); }
    private IEnumerator KnightSlash () { throw new NotImplementedException (); }
    private IEnumerator KnightBash () { throw new NotImplementedException (); }
}