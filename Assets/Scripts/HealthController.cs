using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HealthController : MonoBehaviour {
    //[Header("Die event")]
    public UnityEvent die;
    public UnityEvent damage;

    public bool invulnerable = false;
    public bool oneHitKill = false;
    private bool dead = false;
    public bool dieDebug = false;
    [SerializeField]
    public float healthPoints;
    [HideInInspector]
    public float healthPointsMax;

    private void Awake () {
        healthPointsMax = healthPoints;

        if (gameObject.tag == "Player" && SceneManager.GetActiveScene().name != "Kikbow") {
            GameObject.Find ("HealthBar").GetComponent<Slider> ().maxValue = healthPointsMax;
            UpdatePlayerHPBar ();
        }
    }

    private void Update () {
        if (dieDebug && !dead)
            Die ();
    }

    public void Damage (float damage, bool ignoreInvulnerability = false) {
        if ((invulnerable && !ignoreInvulnerability) || dead)
            return;

        healthPoints -= damage;

        Damage ();

        if (healthPoints <= 0 || oneHitKill)
            Die ();
    }

    private void Damage () {
        damage.Invoke ();
    }

    private void Die () {
        dead = true;
        die.Invoke ();
        StartCoroutine (WaitToDie ());
    }

    public void ResetHP () {
        healthPoints = healthPointsMax;
    }

    public int HealthPoints () {
        return Mathf.CeilToInt (healthPoints);
    }

    private IEnumerator WaitToDie () {
        yield return new WaitForSeconds (1f);

        SceneManager.LoadScene ("Luis");
    }

    public void UpdatePlayerHPBar () {
        GameObject.Find ("HealthBar").GetComponent<Slider> ().value = -(healthPoints - healthPointsMax);
    }
}