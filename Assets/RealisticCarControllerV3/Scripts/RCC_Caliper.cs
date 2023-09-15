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
/// Rotates the brake caliper.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Misc/RCC Visual Brake Caliper")]
public class RCC_Caliper : MonoBehaviour {

    public RCC_WheelCollider wheelCollider;     //  Actual WheelCollider.
    private GameObject newPivot;        //  Creating new center pivot for correct position.
    private Quaternion defLocalRotation;        //  Default rotation.

    private void Start() {

        //	No need to go further if no wheelcollider found.
        if (!wheelCollider) {

            Debug.LogError("WheelCollider is not selected for this caliper named " + transform.name);
            enabled = false;
            return;

        }

        //	Creating new center pivot for correct position.
        newPivot = new GameObject("Pivot_" + transform.name);
        newPivot.transform.SetParent(wheelCollider.wheelCollider.transform, false);
        transform.SetParent(newPivot.transform, true);

        //	Assigning default rotation.
        defLocalRotation = newPivot.transform.localRotation;

    }

    private void LateUpdate() {

        //	No need to go further if no wheelcollider or no wheelmodel found.
        if (!wheelCollider.wheelModel || !wheelCollider.wheelCollider)
            return;

        // Left or right side?
        int side = 1;

        //  If left side...
        if (wheelCollider.transform.localPosition.x < 0)
            side = -1;

        //	Re-positioning camber pivot.
        newPivot.transform.position = wheelCollider.wheelPosition;

        //	Re-rotationing camber pivot.
        newPivot.transform.localRotation = defLocalRotation * Quaternion.Euler(wheelCollider.caster * side, wheelCollider.wheelCollider.steerAngle, wheelCollider.camber * side);

    }

}
