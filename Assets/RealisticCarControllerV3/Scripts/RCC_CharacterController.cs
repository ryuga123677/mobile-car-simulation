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
/// Animates Driver Sofie (Credits to 3DMaesen). Simply feeds floats and bools of Sofie's animator component.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Misc/RCC Animator Controller")]
public class RCC_CharacterController : MonoBehaviour {

    private RCC_CarControllerV3 carController;      //  Car controller.
    private Rigidbody carRigid;     //  Rigidbody.
    public Animator animator;       //  Animator.

    // String parameters of animator.
    public string driverSteeringParameter;
    public string driverShiftingGearParameter;
    public string driverDangerParameter;
    public string driverReversingParameter;

    // Inputs for feeding animator.
    public float steerInput = 0f;
    public float directionInput = 0f;
    public bool reversing = false;
    public float impactInput = 0f;
    public float gearInput = 0f;

    private void Start() {

        //  Getting components.
        if (!animator)
            animator = GetComponentInChildren<Animator>();

        carController = GetComponent<RCC_CarControllerV3>();
        carRigid = GetComponent<Rigidbody>();

    }

    private void Update() {

        //  Getting steer input.
        steerInput = Mathf.Lerp(steerInput, carController.steerInput, Time.deltaTime * 5f);
        directionInput = carRigid.transform.InverseTransformDirection(carRigid.velocity).z;
        impactInput -= Time.deltaTime * 5f;

        //  Clamping impact input.
        if (impactInput < 0)
            impactInput = 0f;
        if (impactInput > 1)
            impactInput = 1f;

        //  If vehicle is going backwards or not.
        if (directionInput <= -2f)
            reversing = true;
        else if (directionInput > -1f)
            reversing = false;

        //  If changing gear.
        if (carController.changingGear)
            gearInput = 1f;
        else
            gearInput -= Time.deltaTime * 5f;

        //  Clamping gear input.
        if (gearInput < 0)
            gearInput = 0f;
        if (gearInput > 1)
            gearInput = 1f;

        //  If reversing.
        if (!reversing)
            animator.SetBool(driverReversingParameter, false);
        else
            animator.SetBool(driverReversingParameter, true);

        //  If impact is high enough, animate collision animation by setting bool.
        if (impactInput > .5f)
            animator.SetBool(driverDangerParameter, true);
        else
            animator.SetBool(driverDangerParameter, false);

        //  If changing gear, animate change gear animation by setting bool.
        if (gearInput > .5f)
            animator.SetBool(driverShiftingGearParameter, true);
        else
            animator.SetBool(driverShiftingGearParameter, false);

        //  Setting steer input of the animator by setting float.
        animator.SetFloat(driverSteeringParameter, steerInput);

    }

    private void OnCollisionEnter(Collision col) {

        //  If collision is not high enough, return.
        if (col.relativeVelocity.magnitude < 2.5f)
            return;

        //  Setting impact to 1 on collisions.
        impactInput = 1f;

    }

}
