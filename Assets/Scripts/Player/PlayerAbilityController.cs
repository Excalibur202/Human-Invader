using UnityEngine;
internal class AbilityInputs {
    internal bool InputtedSomething = false;
    internal bool Use = false;
    internal bool Ability1 = false;
    internal bool Ability2 = false;
    internal bool Map = false;
    internal bool mapdrag = false;
}

public class PlayerAbilityController : MonoBehaviour {
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerAbilityCaster abilities;
    [SerializeField] Camera cam;
    [SerializeField] TerminalController connectedTerminal;
    [SerializeField] Material mapTexture;

    public int keycards;
    public GameObject map;
    AbilityInputs inputs = new AbilityInputs ();
    bool mapActive;
    [SerializeField] float scale, dragX, dragY;

    void Start () {
        // Disable ability inputs if no abilities to work with
        if (!abilities)
            enabled = false;

        if (!cam)
            cam = (Camera) GameObject.FindObjectOfType (typeof (Camera));
    }

    void Update () {
        if (!connectedTerminal && !mapActive) {
            playerController.enabled = true;

            UpdateAbilityInputs();

            if (inputs.InputtedSomething) {
                if (inputs.Ability1) {
                    abilities.UseAbility (1);
                }
                if (inputs.Ability2) {
                    abilities.UseAbility (2);
                }
                if (inputs.Use) {
                    Interact ();
                }
                if (inputs.Map)
                {
                    ActiveMap(true);
                }
            }
        } else {
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
            else if(mapActive)
            {
                if (playerController.enabled)
                {
                    playerController.enabled = false;
                }

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                dragX = Mathf.Clamp((Input.mousePosition.x / Screen.width) * 2 - 1, -1.0f, 1.0f);
                dragY = Mathf.Clamp((Input.mousePosition.y / Screen.height) * 2 - 1, -1.0f, 1.0f);
                
                if (Input.GetMouseButtonDown(0))
                {
                    OnMouseDrag();
                }

                if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Escape))
                        ActiveMap(false);
                
            }
        }
    }

    void UpdateTerminalInputs () {
        if (Input.inputString.Length > 0)
            connectedTerminal.AddStringToInput (Input.inputString);

        if (Input.GetKeyDown (KeyCode.Return))
            connectedTerminal.ParseInput ();

        if (Input.GetKeyDown (KeyCode.Escape))
            connectedTerminal = null;
    }

    void UpdateAbilityInputs () {
        ClearInputs ();

        // Ability 1
        if (Input.GetKeyDown (KeyCode.F)) {
            inputs.Ability1 = true;
            inputs.InputtedSomething = true;
        }

        if(Input.GetKeyDown(KeyCode.M))
        {
            inputs.Map = true;
            inputs.InputtedSomething = true;
        }
        

        // // Ability 2
        // if (Input.GetKeyDown (KeyCode.R)) {
        //     inputs.Ability2 = true;
        //     inputs.InputtedSomething = true;
        // } 

        // Use key
        if (Input.GetKeyDown (KeyCode.E)) {
            inputs.Use = true;
            inputs.InputtedSomething = true;
        }
    }

    void Interact () {
        RaycastHit raycastHit;

        if (Physics.Raycast (cam.transform.position, cam.transform.forward, out raycastHit, 3, LayerMask.GetMask ("Interactable"))) {
            if (raycastHit.transform.tag.Equals ("Terminal"))
                connectedTerminal = (TerminalController) raycastHit.transform.GetComponentInChildren (typeof (TerminalController));
            if (connectedTerminal.logStrings.Count == 0)
                connectedTerminal.Initialize ();
        }

        return;
    }

    void ClearInputs () {
        inputs.InputtedSomething = false;
        inputs.Ability1 = false;
        inputs.Ability2 = false;
        inputs.Use = false;
        inputs.Map = false;
        inputs.mapdrag = false;
    }

    void ActiveMap(bool state)
    {
        mapActive = state;
        map.SetActive(state);
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

    private void OnMouseDrag()
    {
        DragMap(dragX, dragY);
        //mapTexture.SetFloat("_TiltX", dragX);
        //mapTexture.SetFloat("_TiltY", dragY);
    }
}