using UnityEngine;

internal class InputKey {
    internal bool InputtedSomething = true;
    internal bool Ability1 = false;
    internal bool Ability2 = false;
}

public class PlayerInput : MonoBehaviour {
    InputKey inputs = new InputKey ();
    [SerializeField] PlayerAbilities abilities = new PlayerAbilities ();

    void Start () {
        // Disable ability inputs if no abilities to work with
        if (!abilities)
            enabled = false;
    }

    void Update () {
        UpdateInputs ();

        if (inputs.InputtedSomething) {
            // Trigger inputted abilities
            if (inputs.Ability1) {
                abilities.UseAbility (1);
            }
        }
    }

    void UpdateInputs () {
        inputs.InputtedSomething = false;

        // Ability 1
        if (Input.GetKeyDown (KeyCode.E)) {
            inputs.Ability1 = true;
            inputs.InputtedSomething = true;
        } else {
            inputs.Ability1 = false;
        }
    }
}