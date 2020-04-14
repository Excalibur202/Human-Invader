using System.Collections;
using UnityEngine;

internal class InputKey {
    internal bool InputtedSomething = false;
    internal bool Use = false;
    internal bool Ability1 = false;
    internal bool Ability2 = false;
}

public class PlayerInputController : MonoBehaviour {
    InputKey inputs = new InputKey ();
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerAbilities abilities;
    [SerializeField] Camera cam;
    [SerializeField] TerminalController connectedTerminal;

    void Start () {
        // Disable ability inputs if no abilities to work with
        if (!abilities)
            enabled = false;

        if (!cam)
            cam = (Camera) GameObject.FindObjectOfType (typeof (Camera));
    }

    void Update () {
        if (!connectedTerminal) {
            //playerController.enabled = true;

            UpdateInputs ();

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
            }

        } else {
            //playerController.enabled = false;

            UpdateTerminalInputs ();
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

    void UpdateInputs () {
        ClearInputs ();

        // Ability 1
        if (Input.GetKeyDown (KeyCode.F)) {
            inputs.Ability1 = true;
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
        }

        return;
    }

    void ClearInputs () {
        inputs.InputtedSomething = false;
        inputs.Ability1 = false;
        inputs.Ability2 = false;
        inputs.Use = false;
    }
}