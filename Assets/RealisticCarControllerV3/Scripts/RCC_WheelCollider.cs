//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2022 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------


using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Based on Unity's WheelCollider. Modifies few curves, settings in order to get stable and realistic physics depends on selected behavior in RCC Settings.
/// </summary>
[RequireComponent(typeof(WheelCollider))]
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Main/RCC Wheel Collider")]
public class RCC_WheelCollider : RCC_Core {

    // WheelCollider.
    private WheelCollider _wheelCollider;
    public WheelCollider wheelCollider {
        get {
            if (_wheelCollider == null)
                _wheelCollider = GetComponent<WheelCollider>();
            return _wheelCollider;
        }
    }

    // Car controller.
    private RCC_CarControllerV3 _carController;
    public RCC_CarControllerV3 carController {
        get {
            if (_carController == null)
                _carController = GetComponentInParent<RCC_CarControllerV3>();
            return _carController;
        }
    }

    // Rigidbody of the vehicle.
    private Rigidbody _rigid;
    public Rigidbody rigid {
        get {
            if (_rigid == null)
                _rigid = carController.gameObject.GetComponent<Rigidbody>();
            return _rigid;
        }
    }

    private List<RCC_WheelCollider> allWheelColliders = new List<RCC_WheelCollider>();      // All wheelcolliders attached to this vehicle.
    public Transform wheelModel;        // Wheel model for animating and aligning.

    private WheelHit _wheelHit;
    public WheelHit wheelHit {

        get {

            wheelCollider.GetGroundHit(out _wheelHit);
            return _wheelHit;

        }

    }
    public bool isGrounded {

        get {

            return wheelCollider.GetGroundHit(out _wheelHit);

        }

    }

    public bool alignWheel = true;
    public bool drawSkid = true;

    // Locating correct position and rotation for the wheel.
    [HideInInspector] public Vector3 wheelPosition = Vector3.zero;
    [HideInInspector] public Quaternion wheelRotation = Quaternion.identity;

    [Space()]
    public bool canPower = false;       //	Can this wheel power?
    [Range(-1f, 1f)] public float powerMultiplier = 1f;
    public bool canSteer = false;       //	Can this wheel steer?
    [Range(-1f, 1f)] public float steeringMultiplier = 1f;
    public bool canBrake = false;       //	Can this wheel brake?
    [Range(0f, 1f)] public float brakingMultiplier = 1f;
    public bool canHandbrake = false;       //	Can this wheel handbrake?
    [Range(0f, 1f)] public float handbrakeMultiplier = 1f;

    [Space()]
    public float wheelWidth = .275f; //	Width of the wheel.
    public float wheelOffset = 0f;     // Offset by X axis.

    private float wheelRPM2Speed = 0f;     // Wheel RPM to Speed.

    [Space()]
    [Range(-5f, 5f)] public float camber = 0f;      // Camber angle.
    [Range(-5f, 5f)] public float caster = 0f;      // Caster angle.
    [Range(-5f, 5f)] public float toe = 0f;              // Toe angle.
    [Space()]

    //	Skidmarks
    private int lastSkidmark = -1;

    //	Slips
    [HideInInspector] public float wheelSlipAmountForward = 0f;       // Forward slip.
    [HideInInspector] public float wheelSlipAmountSideways = 0f;  // Sideways slip.
    [HideInInspector] public float totalSlip = 0f;                              // Total amount of forward and sideways slips.

    //	WheelFriction Curves and Stiffness.
    private WheelFrictionCurve forwardFrictionCurve;        //	Forward friction curve.
    private WheelFrictionCurve sidewaysFrictionCurve;   //	Sideways friction curve.

    //	Original WheelFriction Curves and Stiffness.
    private WheelFrictionCurve forwardFrictionCurve_Org;        //	Forward friction curve.
    private WheelFrictionCurve sidewaysFrictionCurve_Org;   //	Sideways friction curve.

    //	Audio
    private AudioSource audioSource;        // Audiosource for tire skid SFX.
    private AudioClip audioClip;                    // Audioclip for tire skid SFX.
    private float audioVolume = 1f;         //	Maximum volume for tire skid SFX.

    private int groundIndex {

        get {

            return GetGroundMaterialIndex();

        }

    }

    // List for all particle systems.
    [HideInInspector] public List<ParticleSystem> allWheelParticles = new List<ParticleSystem>();
    private ParticleSystem.EmissionModule emission;

    //	Tractions used for smooth drifting.
    [HideInInspector] public float tractionHelpedSidewaysStiffness = 1f;
    private readonly float minForwardStiffness = .75f;
    private readonly float maxForwardStiffness = 1f;
    private readonly float minSidewaysStiffness = .5f;
    private readonly float maxSidewaysStiffness = 1f;

    // Getting bump force.
    [HideInInspector] public float bumpForce, oldForce, RotationValue = 0f;

    private bool deflated = false;

    [Space()]
    public float deflateRadiusMultiplier = .8f;
    public float deflatedStiffnessMultiplier = .5f;

    private float defRadius = -1f;

    public AudioClip deflateAudio {

        get {

            return RCC_Settings.Instance.wheelDeflateClip;

        }

    }

    public AudioClip inflateAudio {

        get {

            return RCC_Settings.Instance.wheelInflateClip;

        }

    }

    private AudioSource flatSource;
    public AudioClip flatAudio {

        get {

            return RCC_Settings.Instance.wheelFlatClip;

        }

    }

    private ParticleSystem _wheelDeflateParticles;
    public ParticleSystem wheelDeflateParticles {

        get {

            return RCC_Settings.Instance.wheelDeflateParticles.GetComponent<ParticleSystem>();

        }

    }

    private void Start() {

        // Getting all WheelColliders attached to this vehicle (Except this).
        allWheelColliders = carController.GetComponentsInChildren<RCC_WheelCollider>(true).ToList();

        CheckBehavior();        //	Checks selected behavior in RCC Settings.

        // Increasing WheelCollider mass for avoiding unstable behavior.
        if (RCC_Settings.Instance.useFixedWheelColliders)
            wheelCollider.mass = rigid.mass / 15f;

        // Creating audiosource for skid SFX.
        audioSource = NewAudioSource(RCC_Settings.Instance.audioMixer, carController.gameObject, "Skid Sound AudioSource", 5f, 50f, 0f, audioClip, true, true, false);
        audioSource.transform.position = transform.position;

        // Creating all ground particles, and adding them to list.
        if (!RCC_Settings.Instance.dontUseAnyParticleEffects) {

            for (int i = 0; i < RCC_GroundMaterials.Instance.frictions.Length; i++) {

                GameObject ps = Instantiate(RCC_GroundMaterials.Instance.frictions[i].groundParticles, transform.position, transform.rotation);
                emission = ps.GetComponent<ParticleSystem>().emission;
                emission.enabled = false;
                ps.transform.SetParent(transform, false);
                ps.transform.localPosition = Vector3.zero;
                ps.transform.localRotation = Quaternion.identity;
                allWheelParticles.Add(ps.GetComponent<ParticleSystem>());

            }

        }

        //	Creating pivot position of the wheel at correct position and rotation.
        GameObject newPivot = new GameObject("Pivot_" + wheelModel.transform.name);
        newPivot.transform.position = RCC_GetBounds.GetBoundsCenter(wheelModel.transform);
        newPivot.transform.rotation = transform.rotation;
        newPivot.transform.SetParent(wheelModel.transform.parent, true);

        //	Settings offsets.
        if (newPivot.transform.localPosition.x > 0)
            wheelModel.transform.position += transform.right * .075f;
        else
            wheelModel.transform.position -= transform.right * .075f;

        //	Assigning temporary created wheel to actual wheel.
        wheelModel.SetParent(newPivot.transform, true);
        wheelModel = newPivot.transform;

        // Override wheels automatically if enabled.
        if (!carController.overrideAllWheels) {

            // Overriding canPower, canSteer, canBrake, canHandbrake.
            if (this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider) {

                canSteer = true;
                canBrake = true;
                brakingMultiplier = 1f;

            }

            if (this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider) {

                canHandbrake = true;
                canBrake = true;
                brakingMultiplier = .5f;

            }

        }

    }

    private void OnEnable() {

        // Listening an event when main behavior changed.
        RCC_SceneManager.OnBehaviorChanged += CheckBehavior;

        if (wheelModel && !wheelModel.gameObject.activeSelf)
            wheelModel.gameObject.SetActive(true);

        wheelSlipAmountForward = 0f;
        wheelSlipAmountSideways = 0f;
        totalSlip = 0f;

        if (audioSource) {

            audioSource.volume = 0f;
            audioSource.Stop();

        }

        wheelCollider.motorTorque = 0f;
        wheelCollider.brakeTorque = 0f;
        wheelCollider.steerAngle = 0f;

    }

    private void CheckBehavior() {

        forwardFrictionCurve = wheelCollider.forwardFriction;
        sidewaysFrictionCurve = wheelCollider.sidewaysFriction;

        //	Getting behavior if selected.
        RCC_Settings.BehaviorType behavior = RCC_Settings.Instance.selectedBehaviorType;

        //	If there is a selected behavior, override friction curves.
        if (behavior != null) {

            forwardFrictionCurve = SetFrictionCurves(forwardFrictionCurve, behavior.forwardExtremumSlip, behavior.forwardExtremumValue, behavior.forwardAsymptoteSlip, behavior.forwardAsymptoteValue);
            sidewaysFrictionCurve = SetFrictionCurves(sidewaysFrictionCurve, behavior.sidewaysExtremumSlip, behavior.sidewaysExtremumValue, behavior.sidewaysAsymptoteSlip, behavior.sidewaysAsymptoteValue);

        }

        // Assigning new frictons.
        wheelCollider.forwardFriction = forwardFrictionCurve;
        wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

        // Getting friction curves.
        forwardFrictionCurve_Org = wheelCollider.forwardFriction;
        sidewaysFrictionCurve_Org = wheelCollider.sidewaysFriction;

    }

    private void Update() {

        // Return if RCC is disabled.
        if (!carController.enabled)
            return;

        // Setting position and rotation of the wheel model.
        if (alignWheel)
            WheelAlign();

    }

    private void TotalSlip() {

        // Forward, sideways, and total slips.
        if (isGrounded && wheelHit.point != Vector3.zero) {

            wheelSlipAmountForward = wheelHit.forwardSlip;
            wheelSlipAmountSideways = wheelHit.sidewaysSlip;

        } else {

            wheelSlipAmountForward = 0f;
            wheelSlipAmountSideways = 0f;

        }

        totalSlip = Mathf.Lerp(totalSlip, ((Mathf.Abs(wheelSlipAmountSideways) + Mathf.Abs(wheelSlipAmountForward)) / 2f), Time.fixedDeltaTime * 10f);

    }

    private void FixedUpdate() {

        float circumFerence = 2.0f * 3.14f * wheelCollider.radius; // Finding circumFerence 2 Pi R.
        wheelRPM2Speed = (circumFerence * wheelCollider.rpm) * 60f; // Finding KMH.
        wheelRPM2Speed = Mathf.Clamp(wheelRPM2Speed / 1000f, 0f, Mathf.Infinity);

        // Setting power state of the wheels depending on drivetrain mode. Only overrides them if overrideWheels is enabled for the vehicle.
        if (!carController.overrideAllWheels) {

            switch (carController.wheelTypeChoise) {

                case RCC_CarControllerV3.WheelType.AWD:
                    canPower = true;
                    break;

                case RCC_CarControllerV3.WheelType.BIASED:
                    canPower = true;
                    break;

                case RCC_CarControllerV3.WheelType.FWD:

                    if (this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider)
                        canPower = true;
                    else
                        canPower = false;

                    break;

                case RCC_CarControllerV3.WheelType.RWD:

                    if (this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider)
                        canPower = true;
                    else
                        canPower = false;

                    break;

            }

        }

        Frictions();
        TotalSlip();
        SkidMarks();
        Particles();
        Audio();
        CheckDeflate();

        // Return if RCC is disabled.
        if (!carController.enabled)
            return;

        #region ESP.

        // ESP System. All wheels have individual brakes. In case of loosing control of the vehicle, corresponding wheel will brake for gaining the control again.
        if (carController.ESP && carController.brakeInput < .5f) {

            if (carController.handbrakeInput < .5f) {

                if (carController.underSteering) {

                    if (this == carController.FrontLeftWheelCollider)
                        ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(-carController.rearSlip, 0f, Mathf.Infinity));

                    if (this == carController.FrontRightWheelCollider)
                        ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(carController.rearSlip, 0f, Mathf.Infinity));

                }

                if (carController.overSteering) {

                    if (this == carController.RearLeftWheelCollider)
                        ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(-carController.frontSlip, 0f, Mathf.Infinity));

                    if (this == carController.RearRightWheelCollider)
                        ApplyBrakeTorque((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp(carController.frontSlip, 0f, Mathf.Infinity));

                }

            }

        }

        #endregion

    }

    // Aligning wheel model position and rotation.
    private void WheelAlign() {

        // Return if no wheel model selected.
        if (!wheelModel) {

            Debug.LogWarning(transform.name + " wheel of the " + carController.transform.name + " is missing wheel model. This wheel is disabled");
            return;

        }

        wheelCollider.GetWorldPose(out wheelPosition, out wheelRotation);

        //Increase the rotation value
        RotationValue += wheelCollider.rpm * (360f / 60f) * Time.deltaTime;

        //	Assigning position and rotation to the wheel model.
        wheelModel.transform.position = wheelPosition;
        wheelModel.transform.rotation = transform.rotation * Quaternion.Euler(RotationValue, wheelCollider.steerAngle, 0f);

        //	Adjusting offset by X axis.
        if (transform.localPosition.x < 0f)
            wheelModel.transform.position += transform.right * .075f - (transform.right * wheelOffset);
        else
            wheelModel.transform.position -= transform.right * .075f - (transform.right * wheelOffset); ;

        // Adjusting camber angle by Z axis.
        if (transform.localPosition.x < 0f)
            wheelModel.transform.RotateAround(wheelModel.transform.position, transform.forward, -camber);
        else
            wheelModel.transform.RotateAround(wheelModel.transform.position, transform.forward, camber);

        // Adjusting caster angle by X axis.
        if (transform.localPosition.x < 0f)
            wheelModel.transform.RotateAround(wheelModel.transform.position, transform.right, -caster);
        else
            wheelModel.transform.RotateAround(wheelModel.transform.position, transform.right, caster);

    }

    /// <summary>
    /// Skidmarks.
    /// </summary>
    private void SkidMarks() {

        if (!drawSkid)
            return;

        // If scene has skidmarks manager...
        if (!RCC_Settings.Instance.dontUseSkidmarks) {

            // If slips are bigger than target value...
            if (totalSlip > RCC_GroundMaterials.Instance.frictions[groundIndex].slip) {

                Vector3 skidPoint = wheelHit.point + 1f * (rigid.velocity) * Time.deltaTime;

                if (rigid.velocity.magnitude > 1f && isGrounded && wheelHit.normal != Vector3.zero && wheelHit.point != Vector3.zero && skidPoint != Vector3.zero && Mathf.Abs(skidPoint.x) > 1f && Mathf.Abs(skidPoint.z) > 1f)
                    lastSkidmark = RCC_SkidmarksManager.Instance.AddSkidMark(skidPoint, wheelHit.normal, totalSlip, wheelWidth, lastSkidmark, groundIndex);
                else
                    lastSkidmark = -1;

            } else {

                lastSkidmark = -1;

            }

        }

    }

    /// <summary>
    /// Sets forward and sideways frictions.
    /// </summary>
    private void Frictions() {

        // Handbrake input clamped 0f - 1f.
        float hbInput = carController.handbrakeInput;

        if (canHandbrake && hbInput > .75f)
            hbInput = .75f;
        else
            hbInput = 1f;

        // Setting wheel stiffness to ground physic material stiffness.
        forwardFrictionCurve.stiffness = RCC_GroundMaterials.Instance.frictions[groundIndex].forwardStiffness;
        sidewaysFrictionCurve.stiffness = (RCC_GroundMaterials.Instance.frictions[groundIndex].sidewaysStiffness * hbInput * tractionHelpedSidewaysStiffness);

        if (deflated) {

            forwardFrictionCurve.stiffness *= deflatedStiffnessMultiplier;
            sidewaysFrictionCurve.stiffness *= deflatedStiffnessMultiplier;

            if (!flatSource)
                flatSource = NewAudioSource(gameObject, flatAudio.name, 1f, 15f, .5f, flatAudio, true, false, false);

            flatSource.volume = Mathf.Clamp01(Mathf.Abs(wheelCollider.rpm * .001f));
            flatSource.volume *= isGrounded ? 1f : 0f;

            if (!flatSource.isPlaying)
                flatSource.Play();

        } else {

            if (flatSource && flatSource.isPlaying)
                flatSource.Stop();

        }

        if (_wheelDeflateParticles != null) {

            ParticleSystem.EmissionModule em = _wheelDeflateParticles.emission;

            if (deflated) {

                if (wheelCollider.rpm > 100f && isGrounded)
                    em.enabled = true;
                else
                    em.enabled = false;

            } else {

                em.enabled = false;

            }

        }

        // If drift mode is selected, apply specific frictions.
        if (RCC_Settings.Instance.selectedBehaviorType != null && RCC_Settings.Instance.selectedBehaviorType.applyExternalWheelFrictions)
            Drift();

        // Setting new friction curves to wheels.
        wheelCollider.forwardFriction = forwardFrictionCurve;
        wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

        // Also damp too.
        wheelCollider.wheelDampingRate = RCC_GroundMaterials.Instance.frictions[groundIndex].damp;

        // Set audioclip to ground physic material sound.
        audioClip = RCC_GroundMaterials.Instance.frictions[groundIndex].groundSound;
        audioVolume = RCC_GroundMaterials.Instance.frictions[groundIndex].volume;

    }

    /// <summary>
    /// Particles.
    /// </summary>
    private void Particles() {

        if (RCC_Settings.Instance.dontUseAnyParticleEffects)
            return;

        // If wheel slip is bigger than ground physic material slip, enable particles. Otherwise, disable particles.
        for (int i = 0; i < allWheelParticles.Count; i++) {

            if (totalSlip > RCC_GroundMaterials.Instance.frictions[groundIndex].slip) {

                if (i != groundIndex) {

                    ParticleSystem.EmissionModule em;

                    em = allWheelParticles[i].emission;
                    em.enabled = false;

                } else {

                    ParticleSystem.EmissionModule em;

                    em = allWheelParticles[i].emission;
                    em.enabled = true;

                }

            } else {

                ParticleSystem.EmissionModule em;

                em = allWheelParticles[i].emission;
                em.enabled = false;

            }

            if (isGrounded && wheelHit.point != Vector3.zero)
                allWheelParticles[i].transform.position = wheelHit.point + (.05f * transform.up);

        }

    }

    /// <summary>
    /// Drift.
    /// </summary>
    private void Drift() {

        Vector3 relativeVelocity = transform.InverseTransformDirection(rigid.velocity);
        float sqrVel = (relativeVelocity.x * relativeVelocity.x) / 100f;

        if (wheelHit.forwardSlip > 0)
            sqrVel += (Mathf.Abs(wheelHit.forwardSlip));

        if (carController.wheelTypeChoise == RCC_CarControllerV3.WheelType.RWD) {

            // Forward
            if (wheelCollider == carController.FrontLeftWheelCollider.wheelCollider || wheelCollider == carController.FrontRightWheelCollider.wheelCollider) {

                forwardFrictionCurve.extremumValue = Mathf.Clamp(forwardFrictionCurve_Org.extremumValue - sqrVel, minForwardStiffness / 2f, maxForwardStiffness);
                forwardFrictionCurve.asymptoteValue = Mathf.Clamp(forwardFrictionCurve_Org.asymptoteValue + (sqrVel / 2f), minForwardStiffness / 2f, maxForwardStiffness); ;

            } else {

                forwardFrictionCurve.extremumValue = Mathf.Clamp(forwardFrictionCurve_Org.extremumValue - sqrVel, minForwardStiffness, maxForwardStiffness);
                forwardFrictionCurve.asymptoteValue = Mathf.Clamp(forwardFrictionCurve_Org.asymptoteValue + (sqrVel / 2f), minForwardStiffness, maxForwardStiffness);

            }

            // Sideways
            if (wheelCollider == carController.FrontLeftWheelCollider.wheelCollider || wheelCollider == carController.FrontRightWheelCollider.wheelCollider) {

                sidewaysFrictionCurve.extremumValue = Mathf.Clamp(sidewaysFrictionCurve_Org.extremumValue - sqrVel, minSidewaysStiffness, maxSidewaysStiffness);
                sidewaysFrictionCurve.asymptoteValue = Mathf.Clamp(sidewaysFrictionCurve_Org.asymptoteValue - (sqrVel / 2f), minSidewaysStiffness, maxSidewaysStiffness);

            } else {

                sidewaysFrictionCurve.extremumValue = Mathf.Clamp(sidewaysFrictionCurve_Org.extremumValue - sqrVel, minSidewaysStiffness, maxSidewaysStiffness);
                sidewaysFrictionCurve.asymptoteValue = Mathf.Clamp(sidewaysFrictionCurve_Org.asymptoteValue - (sqrVel / 2f), minSidewaysStiffness, maxSidewaysStiffness);

            }

        } else {

            // Forward
            forwardFrictionCurve.extremumValue = Mathf.Clamp(forwardFrictionCurve_Org.extremumValue - sqrVel, minForwardStiffness / 2f, maxForwardStiffness);
            forwardFrictionCurve.asymptoteValue = Mathf.Clamp(forwardFrictionCurve_Org.asymptoteValue + (sqrVel / 2f), minForwardStiffness / 2f, maxForwardStiffness); ;

            // Sideways
            sidewaysFrictionCurve.extremumValue = Mathf.Clamp(sidewaysFrictionCurve_Org.extremumValue - sqrVel, minSidewaysStiffness, maxSidewaysStiffness);
            sidewaysFrictionCurve.asymptoteValue = Mathf.Clamp(sidewaysFrictionCurve_Org.asymptoteValue - (sqrVel / 2f), minSidewaysStiffness, maxSidewaysStiffness);

        }

    }

    /// <summary>
    /// Audio.
    /// </summary>
    private void Audio() {

        // If total slip is high enough...
        if (totalSlip > RCC_GroundMaterials.Instance.frictions[groundIndex].slip) {

            // Assigning corresponding audio clip.
            if (audioSource.clip != audioClip)
                audioSource.clip = audioClip;

            // Playing it.
            if (!audioSource.isPlaying)
                audioSource.Play();

            // If vehicle is moving, set volume and pitch. Otherwise set them to 0.
            if (rigid.velocity.magnitude > 1f) {

                audioSource.volume = Mathf.Lerp(0f, audioVolume, totalSlip);
                audioSource.pitch = Mathf.Lerp(1f, .8f, audioSource.volume);

            } else {

                audioSource.volume = 0f;

            }

        } else {

            audioSource.volume = 0f;

            // If volume is minimal and audio is still playing, stop.
            if (audioSource.volume <= .05f && audioSource.isPlaying)
                audioSource.Stop();

        }

        // Calculating bump force.
        bumpForce = wheelHit.force - oldForce;

        //	If bump force is high enough, play bump SFX.
        if ((bumpForce) >= 5000f) {

            // Creating and playing audiosource for bump SFX.
            AudioSource bumpSound = NewAudioSource(RCC_Settings.Instance.audioMixer, carController.gameObject, "Bump Sound AudioSource", 5f, 50f, (bumpForce - 5000f) / 3000f, RCC_Settings.Instance.bumpClip, false, true, true);
            bumpSound.pitch = Random.Range(.9f, 1.1f);

        }

        oldForce = wheelHit.force;

    }

    /// <summary>
    /// Returns true if one of the wheel is slipping.
    /// </summary>
    /// <returns><c>true</c>, if skidding was ised, <c>false</c> otherwise.</returns>
    private bool IsSkidding() {

        for (int i = 0; i < allWheelColliders.Count; i++) {

            if (allWheelColliders[i].totalSlip > RCC_GroundMaterials.Instance.frictions[groundIndex].slip)
                return true;

        }

        return false;

    }

    /// <summary>
    /// Applies the motor torque.
    /// </summary>
    /// <param name="torque">Torque.</param>
    public void ApplyMotorTorque(float torque) {

        //	If TCS is enabled, checks forward slip. If wheel is losing traction, don't apply torque.
        if (carController.TCS) {

            if (Mathf.Abs(wheelCollider.rpm) >= 1) {

                if (Mathf.Abs(wheelSlipAmountForward) > RCC_GroundMaterials.Instance.frictions[groundIndex].slip) {

                    carController.TCSAct = true;

                    torque -= Mathf.Clamp(torque * (Mathf.Abs(wheelSlipAmountForward)) * carController.TCSStrength, -Mathf.Infinity, Mathf.Infinity);

                    if (wheelCollider.rpm > 1) {

                        torque -= Mathf.Clamp(torque * (Mathf.Abs(wheelSlipAmountForward)) * carController.TCSStrength, 0f, Mathf.Infinity);
                        torque = Mathf.Clamp(torque, 0f, Mathf.Infinity);

                    } else {

                        torque += Mathf.Clamp(-torque * (Mathf.Abs(wheelSlipAmountForward)) * carController.TCSStrength, 0f, Mathf.Infinity);
                        torque = Mathf.Clamp(torque, -Mathf.Infinity, 0f);

                    }

                } else {

                    carController.TCSAct = false;

                }

            } else {

                carController.TCSAct = false;

            }

        }

        if (CheckOvertorque())
            torque = 0;

        if (Mathf.Abs(torque) > 1f)
            wheelCollider.motorTorque = torque;
        else
            wheelCollider.motorTorque = 0f;

    }

    /// <summary>
    /// Applies the steering.
    /// </summary>
    /// <param name="steerInput">Steer input.</param>
    /// <param name="angle">Angle.</param>
    public void ApplySteering(float steerInput, float angle) {

        //	Ackerman steering formula.
        if (steerInput > 0f) {

            if (transform.localPosition.x < 0)
                wheelCollider.steerAngle = (Mathf.Deg2Rad * angle * 2.55f) * (Mathf.Rad2Deg * Mathf.Atan(2.55f / (6 + (1.5f / 2))) * steerInput);
            else
                wheelCollider.steerAngle = (Mathf.Deg2Rad * angle * 2.55f) * (Mathf.Rad2Deg * Mathf.Atan(2.55f / (6 - (1.5f / 2))) * steerInput);

        } else if (steerInput < 0f) {

            if (transform.localPosition.x < 0)
                wheelCollider.steerAngle = (Mathf.Deg2Rad * angle * 2.55f) * (Mathf.Rad2Deg * Mathf.Atan(2.55f / (6 - (1.5f / 2))) * steerInput);
            else
                wheelCollider.steerAngle = (Mathf.Deg2Rad * angle * 2.55f) * (Mathf.Rad2Deg * Mathf.Atan(2.55f / (6 + (1.5f / 2))) * steerInput);

        } else {

            wheelCollider.steerAngle = 0f;

        }

        if (transform.localPosition.x < 0)
            wheelCollider.steerAngle += toe;
        else
            wheelCollider.steerAngle -= toe;

    }

    /// <summary>
    /// Applies the brake torque.
    /// </summary>
    /// <param name="torque">Torque.</param>
    public void ApplyBrakeTorque(float torque) {

        //	If ABS is enabled, checks forward slip. If wheel is losing traction, don't apply torque.
        if (carController.ABS && carController.handbrakeInput <= .1f) {

            if ((Mathf.Abs(wheelHit.forwardSlip) * Mathf.Clamp01(torque)) >= carController.ABSThreshold) {

                carController.ABSAct = true;
                torque = 0;

            } else {

                carController.ABSAct = false;

            }

        }

        if (Mathf.Abs(torque) > 1f)
            wheelCollider.brakeTorque = torque;
        else
            wheelCollider.brakeTorque = 0f;

    }

    /// <summary>
    /// Converts to splat map coordinate.
    /// </summary>
    /// <returns>The to splat map coordinate.</returns>
    /// <param name="playerPos">Player position.</param>
    private Vector3 ConvertToSplatMapCoordinate(Terrain terrain, Vector3 playerPos) {

        Vector3 vecRet = new Vector3();
        Vector3 terPosition = terrain.transform.position;
        vecRet.x = ((playerPos.x - terPosition.x) / terrain.terrainData.size.x) * terrain.terrainData.alphamapWidth;
        vecRet.z = ((playerPos.z - terPosition.z) / terrain.terrainData.size.z) * terrain.terrainData.alphamapHeight;
        return vecRet;

    }

    /// <summary>
    /// Gets the index of the ground material.
    /// </summary>
    /// <returns>The ground material index.</returns>
    private int GetGroundMaterialIndex() {

        // Contacted any physic material in Configurable Ground Materials yet?
        bool contacted = false;

        if (!isGrounded || wheelHit.point == Vector3.zero || wheelHit.collider == null)
            return 0;

        int ret = 0;

        for (int i = 0; i < RCC_GroundMaterials.Instance.frictions.Length; i++) {

            if (wheelHit.collider.sharedMaterial == RCC_GroundMaterials.Instance.frictions[i].groundMaterial) {

                contacted = true;
                ret = i;

            }

        }

        // If ground pyhsic material is not one of the ground material in Configurable Ground Materials, check if we are on terrain collider...
        if (!contacted) {

            if (!RCC_SceneManager.Instance.terrainsInitialized)
                return 0;

            for (int i = 0; i < RCC_GroundMaterials.Instance.terrainFrictions.Length; i++) {

                if (wheelHit.collider.sharedMaterial == RCC_GroundMaterials.Instance.terrainFrictions[i].groundMaterial) {

                    RCC_SceneManager.Terrains currentTerrain = null;

                    for (int l = 0; l < RCC_SceneManager.Instance.terrains.Length; l++) {

                        if (RCC_SceneManager.Instance.terrains[l].terrainCollider == RCC_GroundMaterials.Instance.terrainFrictions[i].groundMaterial) {

                            currentTerrain = RCC_SceneManager.Instance.terrains[l];
                            break;

                        }

                    }

                    if (currentTerrain != null) {

                        Vector3 playerPos = transform.position;
                        Vector3 TerrainCord = ConvertToSplatMapCoordinate(currentTerrain.terrain, playerPos);
                        float comp = 0f;

                        for (int k = 0; k < currentTerrain.mNumTextures; k++) {

                            if (comp < currentTerrain.mSplatmapData[(int)TerrainCord.z, (int)TerrainCord.x, k])
                                ret = k;

                        }

                        ret = RCC_GroundMaterials.Instance.terrainFrictions[i].splatmapIndexes[ret].index;

                    }

                }

            }

        }

        return ret;

    }

    private void CheckDeflate() {

        if (!isGrounded || wheelHit.point == Vector3.zero || wheelHit.collider == null)
            return;

        for (int i = 0; i < RCC_GroundMaterials.Instance.frictions.Length; i++) {

            if (wheelHit.collider.sharedMaterial == RCC_GroundMaterials.Instance.frictions[i].groundMaterial) {

                if (RCC_GroundMaterials.Instance.frictions[i].deflate)
                    Deflate();

            }

        }

    }

    /// <summary>
    /// Checks if overtorque applying.
    /// </summary>
    /// <returns><c>true</c>, if torque was overed, <c>false</c> otherwise.</returns>
    private bool CheckOvertorque() {

        if (carController.speed > carController.maxspeed || (carController.speed > carController.gears[carController.currentGear].maxSpeed && carController.engineRPM > (carController.maxEngineRPM)) || !carController.engineRunning)
            return true;

        if (carController.speed > carController.gears[carController.currentGear].maxSpeed && carController.engineRPM >= (carController.maxEngineRPM * .985f))
            return true;

        return false;

    }

    /// <summary>
    /// Sets a new friction to WheelCollider.
    /// </summary>
    /// <returns>The friction curves.</returns>
    /// <param name="curve">Curve.</param>
    /// <param name="extremumSlip">Extremum slip.</param>
    /// <param name="extremumValue">Extremum value.</param>
    /// <param name="asymptoteSlip">Asymptote slip.</param>
    /// <param name="asymptoteValue">Asymptote value.</param>
    public WheelFrictionCurve SetFrictionCurves(WheelFrictionCurve curve, float extremumSlip, float extremumValue, float asymptoteSlip, float asymptoteValue) {

        WheelFrictionCurve newCurve = curve;

        newCurve.extremumSlip = extremumSlip;
        newCurve.extremumValue = extremumValue;
        newCurve.asymptoteSlip = asymptoteSlip;
        newCurve.asymptoteValue = asymptoteValue;

        return newCurve;

    }

    public void Deflate() {

        if (deflated)
            return;

        deflated = true;

        if (defRadius == -1)
            defRadius = wheelCollider.radius;

        wheelCollider.radius = defRadius * deflateRadiusMultiplier;

        if (deflateAudio)
            NewAudioSource(gameObject, deflateAudio.name, 5f, 50f, 1f, deflateAudio, false, true, true);

        if (_wheelDeflateParticles == null && wheelDeflateParticles) {

            GameObject ps = Instantiate(wheelDeflateParticles.gameObject, transform.position, transform.rotation);
            _wheelDeflateParticles = ps.GetComponent<ParticleSystem>();
            _wheelDeflateParticles.transform.SetParent(transform, false);
            _wheelDeflateParticles.transform.localPosition = new Vector3(0f, -.2f, 0f);
            _wheelDeflateParticles.transform.localRotation = Quaternion.identity;

        }

        carController.rigid.AddForceAtPosition(transform.right * Random.Range(-1f, 1f) * 30f, transform.position, ForceMode.Acceleration);

    }

    public void Inflate() {

        if (!deflated)
            return;

        deflated = false;

        if (defRadius != -1)
            wheelCollider.radius = defRadius;

        if (inflateAudio)
            NewAudioSource(gameObject, inflateAudio.name, 5f, 50f, 1f, inflateAudio, false, true, true);

    }

    private void OnDisable() {

        RCC_SceneManager.OnBehaviorChanged -= CheckBehavior;

        if (wheelModel)
            wheelModel.gameObject.SetActive(false);

        wheelSlipAmountForward = 0f;
        wheelSlipAmountSideways = 0f;
        totalSlip = 0f;

        if (audioSource) {

            audioSource.volume = 0f;
            audioSource.Stop();

        }

        wheelCollider.motorTorque = 0f;
        wheelCollider.brakeTorque = 0f;
        wheelCollider.steerAngle = 0f;

    }

    /// <summary>
    /// Raises the draw gizmos event.
    /// </summary>
    private void OnDrawGizmos() {

#if UNITY_EDITOR
        if (Application.isPlaying) {

            wheelCollider.GetGroundHit(out WheelHit hit);

            // Drawing gizmos for wheel forces and slips.
            float extension = (-wheelCollider.transform.InverseTransformPoint(hit.point).y - (wheelCollider.radius * transform.lossyScale.y)) / wheelCollider.suspensionDistance;
            Debug.DrawLine(hit.point, hit.point + transform.up * (hit.force / rigid.mass), extension <= 0.0 ? Color.magenta : Color.white);
            Debug.DrawLine(hit.point, hit.point - transform.forward * hit.forwardSlip * 2f, Color.green);
            Debug.DrawLine(hit.point, hit.point - transform.right * hit.sidewaysSlip * 2f, Color.red);

        }
#endif

    }

}