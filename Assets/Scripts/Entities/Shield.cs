using UnityEngine;

public class Shield : MonoBehaviour {
    [SerializeField] HealthController healthController;
    [SerializeField] MeshRenderer meshRenderer;
    Material material;
    Color baseColor;

    void Start () {
        if (!meshRenderer || !healthController)
            OnDie ();
        else {
            material = meshRenderer.material;
            baseColor = material.color;
        }
    }

    public void OnDamage () {
        float healthPercentage = healthController.healthPoints / healthController.healthPointsMax;
        material.color = baseColor * healthPercentage + Color.red * (1 - healthPercentage);
    }

    public void OnDie () {
        GameObject shieldGuy = transform.parent.gameObject;
        shieldGuy.GetComponent<BaseEnemy> ().moveSpeed *= 3;
        shieldGuy.GetComponent<HealthController> ().healthPointsMax *= 2;
        shieldGuy.GetComponent<HealthController> ().healthPoints *= 2;
        Destroy (transform.gameObject);
    }
}