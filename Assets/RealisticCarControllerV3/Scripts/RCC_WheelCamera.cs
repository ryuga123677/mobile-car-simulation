//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2022 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using System.Collections;

/// <summary>
/// RCC Camera will be parented to this gameobject when current camera mode is Wheel Camera.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Camera/RCC Wheel Camera")]
public class RCC_WheelCamera : MonoBehaviour {

    /// <summary>
    /// Fix shaking bug related to rigidbody.
    /// </summary>
    public void FixShake() {

        StartCoroutine(FixShakeDelayed());

    }

    private IEnumerator FixShakeDelayed() {

        if (!GetComponent<Rigidbody>())
            yield break;

        yield return new WaitForFixedUpdate();
        GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
        yield return new WaitForFixedUpdate();
        GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;

    }

}
