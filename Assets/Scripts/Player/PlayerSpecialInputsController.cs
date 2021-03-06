﻿using UnityEngine;
internal class SpecialInputs
{
    internal bool InputtedSomething = false;
    internal bool Use = false;
    internal bool Ability1 = false;
    internal bool Ability2 = false;
    internal bool Map = false;
    internal bool Pause = false;
}

public class PlayerSpecialInputsController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerAbilityCaster abilities;
    [SerializeField] Camera cam;
    [SerializeField] TerminalController connectedTerminal;
    [SerializeField] Material mapTexture;

    public UnityEngine.UI.Text keycardCount;
    public UnityEngine.UI.Button exitButton;
    public GameObject keyCardIcon;
    public int keycards;
    public GameObject map;
    public GameObject crosshair;
    public GameObject pause;
    public GameObject HUD;
    SpecialInputs inputs = new SpecialInputs();
    bool mapActive;
    float mapScale;
    public float mapScaleSpeed;
    public float maxZoom;
    public float minZoom;
    [SerializeField] float scale, dragX, dragY;

    void Start()
    {
        keycards = 0;
        // Disable ability inputs if no abilities to work with
        if (!abilities)
            enabled = false;

        if (!cam)
            cam = (Camera)GameObject.FindObjectOfType(typeof(Camera));
    }

    void Update()
    {
        if (!connectedTerminal && !mapActive && !inputs.Pause)
        {
            playerController.enabled = true;

            UpdateSpecialInputs();
            KeyCardCount();

            if (inputs.InputtedSomething)
            {
                if (inputs.Ability1)
                {
                    abilities.UseAbility(1);
                }
                if (inputs.Ability2)
                {
                    abilities.UseAbility(2);
                }
                if (inputs.Use)
                {
                    Interact();
                }
                if (inputs.Map)
                {
                    ActiveMap(true);
                }
                if (inputs.Pause)
                {
                    PauseGame();
                }

            }
        }
        else
        {
            if (connectedTerminal)
            {
                if (playerController.enabled)
                {
                    playerController.enabled = false;

                    // Move the player to be diagonal to the left of the terminal
                    playerController.transform.root.transform.position = new Vector3(
                        connectedTerminal.transform.parent.position.x + connectedTerminal.transform.parent.forward.x * 0.5f + connectedTerminal.transform.parent.right.x * 0.5f,
                        playerController.transform.root.transform.position.y,
                        connectedTerminal.transform.parent.position.z + connectedTerminal.transform.parent.forward.z * 0.5f + connectedTerminal.transform.parent.right.z * 0.5f) * 1.0f; /*+*/

                    // Rotate the player to face the terminal
                    playerController.transform.root.forward = Util.V3setY(connectedTerminal.transform.parent.position - playerController.transform.root.position, 0).normalized;
                }

                // Smoothly move the camera to be diagonal to the right of the terminal
                cam.transform.position = Vector3.Lerp(
                    cam.transform.position,
                    connectedTerminal.transform.position * 1.0f +
                    connectedTerminal.transform.up * 1f +
                    connectedTerminal.transform.right * 0.5f,
                    Time.fixedDeltaTime);

                // Smoothly rotate the camera to face the terminal
                cam.transform.forward = Vector3.Lerp(
                    cam.transform.forward,
                    (connectedTerminal.transform.position - cam.transform.position).normalized,
                    Time.fixedDeltaTime);

                UpdateTerminalInputs();
            }
            else if (mapActive)
            {
                if (playerController.enabled)
                {
                    playerController.enabled = false;
                }
                crosshair.SetActive(false);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                dragX = Mathf.Clamp((Input.mousePosition.x / Screen.width) * 2 - 1, -1.0f, 1.0f);
                dragY = Mathf.Clamp((Input.mousePosition.y / Screen.height) * 2 - 1, -1.0f, 1.0f);

                //if (Input.GetMouseButtonDown(0))
                //{
                OnMouseDrag();
                //}


                if (Input.mouseScrollDelta.y > 0) // forward
                {
                    mapScale += (mapScale < minZoom) ? mapScaleSpeed : 0;
                }
                if (Input.mouseScrollDelta.y < 0) // backwards
                {
                    mapScale -= (mapScale > maxZoom) ? mapScaleSpeed : 0;
                }

                ScaleMap(mapScale);

                if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Escape))
                {
                    Cursor.visible = false;
                    crosshair.SetActive(true);
                    ActiveMap(false);
                }
            }
            else if (inputs.Pause)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGame();
                }
            }

        }
    }

    void UpdateTerminalInputs()
    {
        if (Input.inputString.Length > 0)
            connectedTerminal.AddStringToInput(Input.inputString);

        if (Input.GetKeyDown(KeyCode.Return))
            connectedTerminal.ParseInput();

        if (Input.GetKeyDown(KeyCode.Escape))
            connectedTerminal = null;
    }

    void UpdateSpecialInputs()
    {
        ClearInputs();

        // Ability 1
        if (Input.GetKeyDown(OptionsManager.instance.data.keysbinds["Ability1"]))
        {
            inputs.Ability1 = true;
            inputs.InputtedSomething = true;
        }

        if (Input.GetKeyDown(OptionsManager.instance.data.keysbinds["Map"]))
        {
            inputs.Map = true;
            inputs.InputtedSomething = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            inputs.Pause = true;
            inputs.InputtedSomething = true;
        }

        // // Ability 2
        // if (Input.GetKeyDown (KeyCode.R)) {
        //     inputs.Ability2 = true;
        //     inputs.InputtedSomething = true;
        // } 

        // Use key
        if (Input.GetKeyDown(OptionsManager.instance.data.keysbinds["Ability2"]))
        {
            inputs.Use = true;
            inputs.InputtedSomething = true;
        }
    }

    void Interact()
    {
        RaycastHit raycastHit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out raycastHit, 3, LayerMask.GetMask("Interactable")))
        {
            if (raycastHit.transform.tag.Equals("Terminal"))
                connectedTerminal = (TerminalController)raycastHit.transform.GetComponentInChildren(typeof(TerminalController));
            if (connectedTerminal.logStrings.Count == 0)
                connectedTerminal.Initialize();
        }

        return;
    }

    void ClearInputs()
    {
        inputs.InputtedSomething = false;
        inputs.Ability1 = false;
        inputs.Ability2 = false;
        inputs.Use = false;
        inputs.Map = false;
        inputs.Pause = false;
    }

    void ActiveMap(bool state)
    {
        mapActive = state;
        map.SetActive(state);
    }

    void PauseGame()
    {
        if (Time.timeScale == 1)
        {
            Time.timeScale = 0;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            playerController.enabled = false;

            HUD.SetActive(false);
            pause.SetActive(true);
        }
        else
        {

            HUD.SetActive(true);
            pause.SetActive(false);
            Time.timeScale = 1;
            playerController.enabled = true;
        }
    }

    void DragMap(float dragX, float dragY)
    {
        mapTexture.SetFloat("_TiltX", dragX);
        mapTexture.SetFloat("_TiltY", dragY);
    }

    void ScaleMap(float scale)
    {
        mapTexture.SetFloat("_Scale", scale);
    }

    void KeyCardCount()
    {
        if(keycards <= 0)
        {
            keyCardIcon.SetActive(false);
        }
        else
        {
            keyCardIcon.SetActive(true);
            keycardCount.text = "x" + keycards;
        }
    }

    private void OnMouseDrag()
    {
        DragMap(dragX, dragY);
        //mapTexture.SetFloat("_TiltX", dragX);
        //mapTexture.SetFloat("_TiltY", dragY);
    }
}