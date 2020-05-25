using UnityEngine;

public class Mine : MonoBehaviour {
    enum MineState {
        Flying,
        Landed,
        Triggered
    }
    MineState mineState;

    [SerializeField] GameObject oneShotAudioPlayer;
    [SerializeField] GameObject triggerSphere;
    [SerializeField] GameObject triggerLaser;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip beepSound;
    [SerializeField] AudioClip explosionSound;
    [SerializeField] float damage = 30;
    [SerializeField] float explosionRadius = 3;
    [SerializeField] float explosionDelay = 0.5f;
    [SerializeField] float slowdownMultiplier = 0.3f;
    [SerializeField] float slowdownDuration = 3;

    Material triggerSphereMat;
    Material triggerLaserMat;

    Vector3 finalPosition;
    Quaternion finalRotation;
    Rigidbody finalBody;
    float lifetime;
    float beeper;
    float beepDelay;

    public void SetDestination (Rigidbody rigidbody, Vector3 position, Quaternion rotation) {
        finalBody = rigidbody;
        finalPosition = position;
        finalRotation = rotation;
    }

    void FixedUpdate () {
        lifetime += Time.fixedDeltaTime;

        switch (mineState) {
            case MineState.Flying:
                float distance = Vector3.Distance (finalPosition, transform.position);

                if (distance < 1f) {
                    lifetime = 0;
                    mineState = MineState.Landed;

                    // Set to intended position and try to connect it to collided object if possible
                    transform.position = finalPosition;
                    transform.rotation = finalRotation;
                    var fj = gameObject.AddComponent<FixedJoint> ();
                    if (finalBody) {
                        fj.connectedBody = finalBody;
                    }

                    // Set custom hologram material variables
                    triggerSphereMat = triggerSphere.GetComponent<MeshRenderer> ().material;
                    triggerSphereMat.SetVector ("_UpVector", transform.up);
                    triggerSphereMat.SetFloat ("_Invert", 1);
                    triggerLaserMat = triggerLaser.GetComponent<MeshRenderer> ().material;
                    triggerLaserMat.SetFloat ("_EdgeWidth", 0);
                    triggerLaserMat.SetFloat ("_Range", 1);
                    triggerLaserMat.SetFloat ("_Offset", 10);

                    triggerSphere.SetActive (true);
                } else {
                    // Spin mine during airtime
                    transform.Rotate (new Vector3 (1500 * Time.fixedDeltaTime, 0, 0));

                    if (lifetime > 15)
                        Trigger ();
                }
                break;

            case MineState.Landed:
                float currentRange = triggerSphereMat.GetFloat ("_Range");
                float currentWidth = triggerSphereMat.GetFloat ("_EdgeWidth");

                // Continue trigger sphere fade-in animation if hasnt completed yet
                if (currentRange < 1) {
                    currentRange += 1f * Time.fixedDeltaTime;

                    // On completing the sphere animation
                    if (currentRange >= 1) {
                        currentRange = 1;
                        triggerSphere.GetComponent<SphereCollider> ().enabled = true;
                    }
                    triggerSphereMat.SetFloat ("_Range", currentRange);

                } else if (currentWidth > 0.03f) {
                    currentWidth -= 1f * Time.fixedDeltaTime;

                    // On completing the edge animation
                    if (currentWidth <= 0.03f) {
                        currentWidth = 0.03f;

                        triggerLaser.SetActive (true);
                    }
                    triggerSphereMat.SetFloat ("_EdgeWidth", currentWidth);

                } else if (triggerLaser.transform.localScale.y < 3) {
                    triggerLaser.transform.localScale += new Vector3 (0, 5 * Time.fixedDeltaTime, 0);
                    triggerLaser.transform.localPosition += new Vector3 (0, 5 * Time.fixedDeltaTime, 0);

                    // On completing the laser animation
                    if (triggerLaser.transform.localPosition.y >= 3.49f) {
                        triggerLaser.transform.localPosition = new Vector3 (0, 3.49f, 0);
                        triggerLaser.transform.localScale = new Vector3 (0.1f, 3, 0.1f);
                    }
                }
                break;

            case MineState.Triggered:
                if (beeper >= 4)
                    Explode ();
                else if (lifetime >= beeper * beepDelay) {
                    Beep ();
                    beeper++;
                }
                break;
        }
    }

    public void Trigger () {
        if (mineState != MineState.Triggered) {
            mineState = MineState.Triggered;
            lifetime = 0;
            beeper = 0;
            beepDelay = explosionDelay / 3;
        }
    }

    public void Beep () {
        audioSource.PlayOneShot (beepSound);
    }

    // Finds and damages everything within range that has a HealthController on it
    void Explode () {
        Util.PlaySound(explosionSound, transform.position);

        Collider[] objectsInRange = Physics.OverlapSphere (transform.position, explosionRadius);
        foreach (Collider col in objectsInRange) {
            // Draw debug line to each affected target
            Debug.DrawLine (transform.position, col.transform.position, Color.red, 2);

            // Damage if object has a health controller
            HealthController healthController = col.GetComponent<HealthController> ();
            if (healthController) {
                healthController.Damage (damage);
            }

            // Apply slow effect if is an enemy
            BaseEnemy baseEnemy = col.GetComponent<BaseEnemy> ();
            if (baseEnemy) {
                baseEnemy.ApplyTemporarySlowdown (slowdownMultiplier, slowdownDuration);
            }

            // Trigger other mines in range
            if (col.gameObject.name == "TriggerSphere") {
                Mine mine = col.transform.parent.GetComponent<Mine> ();
                if (mine) {
                    mine.Trigger ();
                }
            }

            Destroy (transform.gameObject);
        }
    }
}