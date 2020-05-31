using UnityEngine;

public class Shield : MonoBehaviour {
    [SerializeField] HealthController healthController;
    [SerializeField] MeshRenderer shieldRenderer;
    [SerializeField] MeshRenderer faceRenderer;
    [SerializeField] Material madFace;

    Material happyFace;
    Color baseColor;

    void Start () {
        if (!shieldRenderer || !healthController)
            OnDie ();
        else {
            happyFace = shieldRenderer.material;
            baseColor = happyFace.color;
        }
    }

    public void OnDamage () {
        float healthPercentage = healthController.healthPoints / healthController.healthPointsMax;
        happyFace.color = baseColor * healthPercentage + Color.red * (1 - healthPercentage);
    }

    public void OnDie () {
        GameObject shieldGuy = transform.parent.gameObject;

        if (faceRenderer)
            faceRenderer.material = madFace;

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