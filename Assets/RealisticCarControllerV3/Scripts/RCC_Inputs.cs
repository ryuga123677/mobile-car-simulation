//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2022 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RCC inputs.
/// </summary>
[System.Serializable]
public class RCC_Inputs {

    public float throttleInput = 0f;
    public float brakeInput = 0f;
    public float steerInput = 0f;
    public float clutchInput = 0f;
    public float handbrakeInput = 0f;
    public float boostInput = 0f;
    public int gearInput = 0;

    public float orbitX = 0f;
    public float orbitY = 0f;

    public Vector2 scroll;

}
