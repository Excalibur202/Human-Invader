using System.Collections;
using System.Collections.Generic;
//using UnityEngine;

namespace PlayerStates
{

    public enum StateIndex : int 
    {
        Health,
        Ability,
        Action
    }
    //_____________________________________________________________________________________

    public enum PlayerHealth : int
    {
        Low,
        Medium,
        High
    }

    public enum PlayerAbility : int
    {
        Ready,
        OnCooldown
    }

    public enum PlayerClass : int
    {
        Ranger,
        Melee
    }

    public enum PlayerAction : int
    {
        Nothing,
        Attack,
        UsingAbility // change later
    }
}


