using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
//using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Animator animator;
    //Player movement
    [Header("Movement")]
    public CharacterController characterController;
    public float maxJogSpeed = 1.8f;
    public float jogAcceleration = 1f;
    public float sprintSpeed = 3f;
    public float sprintAcceletarion = 1f;
    public float deacceleration = 3f;
    public float currentSpeed = 0;
    public bool sprinting = false;
    public float rotationSpeed = 15f;

    //Gravity Vars
    [Header("Jumping/Gravity")]
    [Range(0, 25)]
    public float jumpVelocity = 3f;
    [Range(0, 5)]
    public float lowJumpMultiplier = 1;
    [Range(0, 5)]
    public float fallMultiplier = 1;
    [Range(0, 99)]
    public float characterGravityAcceleration = -9.80665f;


    [Header("Shooting")]
    public Transform gunBarrel;
    public float damage = 23f;
    public float shotsPerSec = 5f;
    private float fireRate = 0.2f;
    private float nextFire = 0f;
    public GameObject shotTrailPrefab;
    [SerializeField]
    private int cachedBulletCounts = 5;
    private Queue<ShotTrail> shotsCache;
    public float bloomAmmount = 0.5f;
    public float currentBloom = 0.0f;
    public float bloomDecayPerSec = 1.0f;
    public float maxBloom = 5f;
    public Light muzzleLight;
    public float muzzleLightIntensity = 3f;
    public float muzzleLightDecay = 5f;
    private float currentMuzzle = 0f;



    //Camera Rotation Vars
    [Header("Camera")]
    public bool useCamera = true;
    public GameObject cameraPrefab;
    public float cameraHeight = 1f;
    public float cameraSideOffset = 1f;
    public float cameraBackOffset = 1f;
    [Range(0.01f, 0.5f)]
    public float cameraHitOffset = 0.01f;
    public float defaultFov = 70f;
    public float aimFov = 55f;
    public float fovSwitchSpeed = 30f;
    private float currentShake = 0f;
    private Vector3 shakePosition = Vector3.zero;


    public float mouseSensibility = 1f;
    public float cameraYaw = 0;
    public float cameraPitch = 0;
    [Range(0, 90)]
    public float maxPitch = 45;
    [Range(-90, 0)]
    public float minPitch = -45;



    //Private stuff
    private float horizontalInput = 0;
    private float verticalInput = 0;
    private Vector3 cameraForward = Vector3.forward;
    private Vector3 cameraRight = Vector3.right;
    private float characterYaw = 0;
    private Vector3 keyboardInput = Vector3.zero;
    private Vector3 characterMotion = Vector3.zero;
    private Vector3 lastCharacterMotion = Vector3.zero;
    private float gravityVelocity = 0;
    private float gravityScale = 1f;
    private Vector3 gravityMotion = Vector3.zero;
    private float direction = 0;
    private Vector3 characterDir = Vector3.zero;
    private Transform cameraTransform;
    private bool aiming = false;
    private Camera camComp;


    private void Awake()
    {
        if (!characterController)
        {
            Debug.LogWarning("Trying to get character controller... Please asign one next time.");
            characterController = GetComponent<CharacterController>() as CharacterController;
            if (characterController == null)
                Debug.LogError("Couldn't get Character Controller...");
        }
        if (!animator)
        {
            Debug.Log("No animator assigned.");
        }
        if (!cameraPrefab)
        {
            Debug.LogWarning("No camera prefab found. Please add one if the default doesn't suit.");
            GameObject go = new GameObject("Default Camera");
            camComp = go.AddComponent<Camera>();
            cameraTransform = go.transform;
        }
        else
        {
            GameObject go = Instantiate(cameraPrefab);
            cameraTransform = go.transform;
        }

        fireRate = 1f / shotsPerSec;

        CacheShots();

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        #region Player Inputs
        cameraPitch -= Input.GetAxisRaw("Mouse Y") * mouseSensibility;
        cameraYaw += Input.GetAxisRaw("Mouse X") * mouseSensibility;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        #endregion

        

        #region Keyboard Camera Relative Direction
            cameraForward = cameraTransform.transform.forward;
        cameraRight = cameraTransform.transform.right;
        
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraRight.Normalize();
        cameraForward.Normalize();
        
        keyboardInput = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;
        #endregion

        Debug.DrawLine(transform.position, transform.position + keyboardInput, Color.blue, Time.deltaTime);

        #region Sprinting?
        if (Input.GetKey(KeyCode.LeftShift) && verticalInput > 0)
            sprinting = true;
        else
            sprinting = false;
        #endregion


        if (keyboardInput != Vector3.zero)
            characterDir = Vector3.Lerp(characterDir, keyboardInput,Time.deltaTime*5f);

        if (Input.GetMouseButton(1))
            aiming = true;
        else
            aiming = false;

        if (Input.GetMouseButton(0) && aiming)
            Shoot();

        //if (aiming)
        //    characterDir = new Vector3(cameraTransform.forward.x, cameraTransform.forward.y, cameraTransform.forward.z);


        Debug.DrawLine(transform.position, transform.position + characterDir, Color.yellow, Time.deltaTime);
        

        //Debug.Log(direction);

        #region Acceleration / Deacceleration
        if (horizontalInput == 0 && verticalInput == 0)
            currentSpeed -= deacceleration * Time.deltaTime;
        else
            currentSpeed += jogAcceleration * Time.deltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0, (sprinting) ? sprintSpeed : maxJogSpeed);
        #endregion

        
        //Stop from controlling motion while on air
        if (characterController.isGrounded)
            characterMotion = characterDir * currentSpeed * Time.deltaTime;
        else
            characterMotion = lastCharacterMotion;


        #region Gravity & Jump

        gravityMotion = Vector3.down;

        if (characterController.isGrounded)
            gravityVelocity = 0f;

        if (Input.GetKeyDown(KeyCode.Space) && characterController.isGrounded)
            gravityVelocity = jumpVelocity;


        if (gravityVelocity < 0)
            gravityScale = fallMultiplier;
        else if (gravityVelocity > 0 && Input.GetKey(KeyCode.Space))
            gravityScale = lowJumpMultiplier;
        else
            gravityScale = 1;


        gravityVelocity += characterGravityAcceleration * gravityScale * Time.deltaTime;
        gravityMotion = Vector3.up * gravityVelocity * Time.deltaTime;

        #endregion
        //Apply final movement

        if (aiming)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(cameraTransform.forward.x, cameraTransform.forward.y * 0, cameraTransform.forward.z), Vector3.up), Time.deltaTime * rotationSpeed);

        else if (currentSpeed > 0 )
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(characterDir), Time.deltaTime*rotationSpeed);

        

        characterController.Move(characterMotion + gravityMotion);
        lastCharacterMotion = characterMotion;

       // if (keyboardInput != Vector3.zero)
        direction =  Mathf.Lerp(direction, Vector3.SignedAngle(transform.forward, characterDir.normalized, Vector3.up), Time.deltaTime * rotationSpeed);
        
        if (animator)
        {

            animator.SetFloat("Direction", -direction);

            if (keyboardInput.magnitude > 0)
                animator.SetBool("IsAccelerating", true);
            else
                animator.SetBool("IsAccelerating", false);
            if (currentSpeed > 0)
                animator.SetFloat("Speed", 90f);
            else
                animator.SetFloat("Speed", 0f);


            if (characterController.isGrounded)
                animator.SetBool("InAir", false);
            else
                animator.SetBool("InAir", true);
        }


        UpdateShooting();
        

      // Cursor.lockState = CursorLockMode.Locked;

    }

    

    public void LateUpdate()
    {
        UpdateAnimator();
        UpdateCameraMovement();
        UpdateFov();
    }


    private float rawAnimationYaw = 0;
    private float rawAnimationPitch = 0;
    private float animationYaw = 0;
    private float animationPitch = 0;
    private float animationOffsetSpeed = 10;

    private void UpdateAnimator()
    {
        if (!animator)
            return;

        rawAnimationPitch = -cameraPitch;
        rawAnimationYaw = Vector3.SignedAngle(transform.forward, cameraForward, Vector3.up); //cameraYaw - characterYaw;

        animationPitch = Mathf.Lerp(animationPitch, rawAnimationPitch, Time.deltaTime * animationOffsetSpeed);
        animationYaw = Mathf.Lerp( animationYaw, rawAnimationYaw, Time.deltaTime * animationOffsetSpeed);

        animator.SetFloat("Pitch", animationPitch);
        animator.SetFloat("Yaw", animationYaw);

        animator.SetBool("Combat", aiming);
    }

    private void UpdateCameraMovement()
    {
        cameraTransform.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        
        Vector3 targetPosition = transform.position - (cameraTransform.forward * cameraBackOffset) + (Vector3.up*cameraHeight) + (cameraTransform.right*cameraSideOffset);
        Debug.DrawLine(transform.position, targetPosition, Color.red, Time.deltaTime);
       
        Vector3 cdir = (-(cameraTransform.forward * cameraBackOffset) + (Vector3.up * cameraHeight) + (cameraTransform.right * cameraSideOffset));
        RaycastHit hit;

        if(Physics.Raycast(transform.position,cdir.normalized,out hit, cdir.magnitude,LayerMask.GetMask("Obstacle")))
            targetPosition = transform.position + (cdir.normalized * (hit.distance - cameraHitOffset));


        targetPosition += shakePosition;


        cameraTransform.position = targetPosition;

        Debug.DrawLine(transform.position, targetPosition,Color.green,Time.deltaTime);
    }

    public void UpdateShooting()
    {
        shakePosition = Vector3.Lerp(shakePosition, Vector3.zero, Time.deltaTime * 5f);
        currentBloom -= Mathf.Pow((Time.time - (nextFire - fireRate)), 2) * Time.deltaTime * bloomDecayPerSec;
        currentBloom = Mathf.Clamp(currentBloom, 0, maxBloom);

        currentMuzzle -= muzzleLightDecay * Time.deltaTime;
        currentMuzzle = Mathf.Clamp(currentMuzzle, 0, muzzleLightIntensity);

        muzzleLight.intensity = currentMuzzle;
    }



    private Vector3 rawShootingPoint = Vector3.zero;
    private Vector3 barrelShootingDirection = Vector3.zero;
    private Vector3 lastHitPoint = Vector3.zero;
    private RaycastHit cameraShootHit;
    private RaycastHit [] playerShootHits;
    private HealthController hc;
    private float currentDamage;
    

    private void Shoot()
    {
        if (Time.time <= nextFire)
            return;

        if(!gunBarrel)
        {
            Debug.Log("Gun barrel transform missing. Please provide it.");
            return;
        }

        currentDamage = damage;

        rawShootingPoint = cameraTransform.position + (cameraTransform.forward * 1000f);

        if(Physics.Raycast(cameraTransform.position,cameraTransform.forward,out cameraShootHit, 1000f)){
            rawShootingPoint = cameraShootHit.point;
        }

        Debug.DrawLine(cameraTransform.position, rawShootingPoint,Color.red,1f);

        barrelShootingDirection = (rawShootingPoint - gunBarrel.position).normalized;
        barrelShootingDirection = (Quaternion.AngleAxis(UnityEngine.Random.Range(-bloomAmmount, bloomAmmount)*currentBloom, UnityEngine.Random.onUnitSphere) * barrelShootingDirection);

        currentBloom += bloomAmmount;

        shakePosition = UnityEngine.Random.insideUnitSphere * 0.05f;

        playerShootHits = Physics.RaycastAll(gunBarrel.position, barrelShootingDirection, 1000f);

        lastHitPoint = gunBarrel.position + (barrelShootingDirection * 1000f);

        currentMuzzle = muzzleLightIntensity;

        for (int i = 0; i < playerShootHits.Length; i++)
        {
            hc = playerShootHits[i].collider.gameObject.GetComponent<HealthController>() as HealthController;
            lastHitPoint = playerShootHits[i].point;

            if (hc != null)
            {
                hc.Damage(currentDamage);
                currentDamage -= 10f;

                if (currentDamage <= 0)
                    break;
            }
            else break;
        }

        Debug.DrawLine(gunBarrel.position, lastHitPoint, Color.green, 1f);

        GetNextShotTrail().Setup(gunBarrel.position, lastHitPoint);

        nextFire = Time.time + fireRate;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private float rawFov = 0;
    private float currentFov = 90;

    private void UpdateFov()
    {
        rawFov = Mathf.Clamp(rawFov, aimFov, defaultFov);
        if (!aiming)
            rawFov += fovSwitchSpeed * Time.deltaTime;
        else
            rawFov -= fovSwitchSpeed * Time.deltaTime;

        rawFov = Mathf.Clamp(rawFov, aimFov, defaultFov );


        currentFov = Mathf.Lerp(currentFov,rawFov,Time.deltaTime*10);
        //camComp.fieldOfView = currentFov;
    }

    private void CacheShots()
    {
        shotsCache = new Queue<ShotTrail>();

        for(int i = 0; i < cachedBulletCounts; i++)
        {
            GameObject go = Instantiate(shotTrailPrefab);
            shotsCache.Enqueue(go.GetComponent<ShotTrail>());
        }
    }

    private ShotTrail GetNextShotTrail()
    {
        ShotTrail st = shotsCache.Dequeue();
        shotsCache.Enqueue(st);
        return st;
    }
}
