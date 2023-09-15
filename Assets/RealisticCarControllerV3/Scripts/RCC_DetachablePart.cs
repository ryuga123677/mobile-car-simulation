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
/// Detachable part of the vehicle.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Misc/RCC Detachable Part")]
public class RCC_DetachablePart : MonoBehaviour {

    private ConfigurableJoint joint;        //	Configurable Joint.
    private RCC_Joint jointProperties;      //	Joint properties class.
    private Rigidbody rigid;        //	Rigidbody.
    public Transform COM;       //	Center of mass.
    public Collider partCollider;       //  Collider.

    public enum DetachablePartType { Hood, Trunk, Door, Bumper_F, Bumper_R }
    public DetachablePartType partType = DetachablePartType.Hood;

    public bool lockAtStart = true;     //	Lock all motions of Configurable Joint at start.
    public float strength = 100f;       //	    Strength of the part. 
    internal float orgStrength = 100f;       //	    Original strength of the part. We will be using this original value while restoring the part.

    public bool isBreakable = true;     //	Can it break at certain damage?

    private bool broken = false;            //	Is this part broken currently?

    public int loosePoint = 35;     //  Part will be broken at this point.
    public int detachPoint = 0;     //  Part will be detached at this point.
    public float deactiveAfterSeconds = 5f; //	Part will be deactivated after the detachment.

    public Vector3 addTorqueAfterLoose = Vector3.zero;      //  	Adds angular velocity related to speed after the brake point reached.

    public bool dontAffectVehiclePhysics = true;        //	If it's enabled, mass and inertia tensor of the part won't affect to the vehicle physics.

    private void Start() {

        joint = gameObject.GetComponent<ConfigurableJoint>();     //	Getting Configurable Joint of the part.
        rigid = GetComponent<Rigidbody>();     //	Getting Rigidbody of the part.
        orgStrength = strength;     //	Getting original strength of the part. We will be using this original value while restoring the part.

        //  Getting collider.
        if (!partCollider)
            partCollider = GetComponentInChildren<Collider>();

        rigid.interpolation = RigidbodyInterpolation.Interpolate;
        joint.connectedMassScale = dontAffectVehiclePhysics ? 0 : 1;

        //	Setting center of mass if selected.
        if (COM)
            rigid.centerOfMass = transform.InverseTransformPoint(COM.transform.position);

        //	Disable the script if configurable joint not found.
        if (!joint) {

            Debug.LogWarning("Configurable Joint not found for " + gameObject.name + "!");
            enabled = false;
            return;

        }

        //	Getting original properties of the joint. We will be using the original data for restoring the part while repairing.
        GetJointProperties();

        //	Locks all motions of Configurable Joint at start.
        if (lockAtStart)
            StartCoroutine(LockJoint());

    }

    /// <summary>
    /// Getting original properties of the joint. We will be using the original data for restoring the part while repairing.
    /// </summary>
    private void GetJointProperties() {

        jointProperties = new RCC_Joint();
        jointProperties.GetProperties(joint);

    }

    /// <summary>
    /// Locks the parts.
    /// </summary>
    private IEnumerator LockJoint() {

        yield return new WaitForFixedUpdate();
        RCC_Joint.LockPart(joint);

    }

    private void Update() {

        // If part is broken, return.
        if (broken)
            return;

        //	If part is weak and loosen, apply angular velocity related to vehicle speed.
        if (addTorqueAfterLoose != Vector3.zero && strength <= loosePoint) {

            float speed = transform.InverseTransformDirection(rigid.velocity).z;        //	Local speed.
            rigid.AddRelativeTorque(new Vector3(addTorqueAfterLoose.x * speed, addTorqueAfterLoose.y * speed, addTorqueAfterLoose.z * speed));      //	Applying local torque.

        }

    }

    public void OnCollision(float impulse) {

        // If part is broken, return.
        if (broken)
            return;

        //	Decreasing strength of the part related to collision impulse.
        strength -= impulse * 5f;
        strength = Mathf.Clamp(strength, 0f, Mathf.Infinity);

        //	Check joint of the part based on strength.
        CheckJoint();

    }

    /// <summary>
    /// Checks joint of the part based on strength.
    /// </summary>
    private void CheckJoint() {

        // If part is broken, return.
        if (broken)
            return;

        // If strength is 0, unlock the parts and set their joint limits to none. Detach them from the vehicle. If strength is below detach point, only set joint limits to none.
        if (isBreakable && strength <= detachPoint) {

            if (joint) {

                broken = true;
                Destroy(joint);
                transform.SetParent(null);
                StartCoroutine(DisablePart(deactiveAfterSeconds));

            }

        } else if (strength <= loosePoint) {

            if (joint) {

                joint.angularXMotion = jointProperties.jointMotionAngularX;
                joint.angularYMotion = jointProperties.jointMotionAngularY;
                joint.angularZMotion = jointProperties.jointMotionAngularZ;

                joint.xMotion = jointProperties.jointMotionX;
                joint.yMotion = jointProperties.jointMotionY;
                joint.zMotion = jointProperties.jointMotionZ;

            }

        }

    }

    /// <summary>
    /// Repairs, and restores the part.
    /// </summary>
    public void OnRepair() {

        // Enabling gameobject first if it's disabled.
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        //	If joint is removed and part is detached, adding new configurable joint component. Configurable Joints cannot be toggled on or off. Therefore, we need to destroy and create configurable joints.
        if (joint == null) {

            // Setting properties of the configurable joint to original properties.
            joint = gameObject.AddComponent<ConfigurableJoint>();
            jointProperties.SetProperties(joint);

        } else {

            // Setting strength to original strength value. And make sure part is not broken anymore.
            strength = orgStrength;
            broken = false;

            //	Locking the part.
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

        }

    }

    /// <summary>
    /// Disables the part with delay.
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator DisablePart(float delay) {

        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);

    }

    private void Reset() {

        if (!COM) {

            COM = new GameObject("COM").transform;
            COM.SetParent(transform);
            COM.localPosition = Vector3.zero;
            COM.localRotation = Quaternion.identity;

        }

        ConfigurableJoint cJoint = GetComponent<ConfigurableJoint>();

        if (!cJoint)
            cJoint = gameObject.AddComponent<ConfigurableJoint>();

        cJoint.connectedBody = GetComponentInParent<RCC_CarControllerV3>().gameObject.GetComponent<Rigidbody>();

        if (!partCollider)
            partCollider = GetComponentInChildren<Collider>();

    }

}
