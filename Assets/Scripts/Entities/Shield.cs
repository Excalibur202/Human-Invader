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
        var shieldHC = shieldGuy.GetComponent<HealthController> ();
        var shieldBE = shieldGuy.GetComponent<BaseEnemy> ();

        shieldBE.defaultMoveSpeed *= 3;
        if (!shieldBE.isSlowed)
            shieldBE.moveSpeed = shieldBE.defaultMoveSpeed;

        shieldHC.healthPointsMax *= 2;
        shieldHC.healthPoints *= 2;
        Destroy (transform.gameObject);
    }
}