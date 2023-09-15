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
/// Locks rotation of the shadow projector to avoid stretching.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Misc/RCC Shadow")]
public class RCC_ShadowRotConst : MonoBehaviour {

    private Transform root;     //  Root of the vehicle.

    private void Start() {

        root = GetComponentInParent<RCC_CarControllerV3>().transform;

    }

    private void Update() {

        transform.rotation = Quaternion.Euler(90f, root.eulerAngles.y, 0f);

    }

}
