using System;
using System.Collections;
using System.Collections.Generic;
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


    //Camera Rotation Vars
    [Header("Camera")]
    public GameObject cameraPrefab;
    public float cameraHeight = 1f;
    public float cameraSideOffset = 1f;
    public float cameraBackOffset = 1f;
    [Range(0.01f, 0.5f)]
    public float cameraHitOffset = 0.01f;

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
            go.AddComponent<Camera>();
            cameraTransform = go.transform;
        }
        else
        {
            GameObject go = Instantiate(cameraPrefab);
            cameraTransform = go.transform;
        }


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




        Debug.DrawLine(transform.position, transform.position + characterDir, Color.yellow, Time.deltaTime);
        

        //Debug.Log(direction);

        

        #region Acceleration / Deacceleration
        if (horizontalInput == 0 && verticalInput == 0)
            currentSpeed -= deacceleration * Time.deltaTime;
        else
            currentSpeed += jogAcceleration * Time.deltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0, (sprinting) ? sprintSpeed : maxJogSpeed);
        #endregion

        

        if (animator)
        {
            if (keyboardInput.magnitude > 0)
                animator.SetBool("IsAccelerating", true);
            else
                animator.SetBool("IsAccelerating", false);
            if (currentSpeed > 0)
                animator.SetFloat("Speed", 90f);
            else
                animator.SetFloat("Speed",0f);


            


            if (characterController.isGrounded)
                animator.SetBool("InAir", false);
            else
                animator.SetBool("InAir", true);
        }

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
        if (currentSpeed > 0)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(characterDir), Time.deltaTime*roationSpeed);
            //characterYaw = Mathf.Lerp(characterYaw, cameraYaw, Time.deltaTime * 10);

        characterController.Move(characterMotion + gravityMotion);
        lastCharacterMotion = characterMotion;

       // if (keyboardInput != Vector3.zero)
        direction =  Mathf.Lerp(direction, Vector3.SignedAngle(transform.forward, characterDir.normalized, Vector3.up), Time.deltaTime * roationSpeed);
        animator.SetFloat("Direction", -direction);
    }

    

    public void LateUpdate()
    {
        UpdateAnimator();
        UpdateCameraMovement();
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
    }

    private void UpdateCameraMovement()
    {
        cameraTransform.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);



        Vector3 targetPosition = transform.position - (cameraTransform.forward * cameraBackOffset) + (Vector3.up*cameraHeight) + (cameraTransform.right*cameraSideOffset);
        Debug.DrawLine(transform.position, targetPosition, Color.red, Time.deltaTime);
       
        
        Vector3 cdir = (-(cameraTransform.forward * cameraBackOffset) + (Vector3.up * cameraHeight) + (cameraTransform.right * cameraSideOffset));
        RaycastHit hit;

        if(Physics.Raycast(transform.position,cdir.normalized,out hit, cdir.magnitude))
            targetPosition = transform.position + (cdir.normalized * (hit.distance - cameraHitOffset));
        


        cameraTransform.position = targetPosition;

        Debug.DrawLine(transform.position, targetPosition,Color.green,Time.deltaTime);
    }


}
