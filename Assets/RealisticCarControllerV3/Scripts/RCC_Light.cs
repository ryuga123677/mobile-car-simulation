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
/// General lighting system for vehicles. It has all kind of lights such as Headlight, Brake Light, Indicator Light, Reverse Light, Park Light, etc...
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Light/RCC Light")]
public class RCC_Light : RCC_Core {

    internal RCC_CarControllerV3 carController;     //  Car controller.

    private Light _light;       //  Actual light component.
    private Projector projector;        //  Projector if used.
    private LensFlare lensFlare;        //  Lensflare if used.
    private TrailRenderer trail;        //  Trailrenderer in used.

    public float defaultIntensity = 0f;     //  Default intensity of the light.
    public float flareBrightness = 1.5f;        //  Max flare brigthness of the light.
    private float finalFlareBrightness;     //  Calculated final flare brightness of the light.

    public LightType lightType = LightType.HeadLight;       //  Light type.
    public enum LightType { HeadLight, BrakeLight, ReverseLight, Indicator, ParkLight, HighBeamHeadLight, External };
    public float inertia = 1f;      //  Light inertia. 
    public LightRenderMode renderMode = LightRenderMode.Auto;
    public bool overrideRenderMode = false;
    public Flare flare;     //  Lensflare if used.

    public int refreshRate = 30;        //  Refresh rate.
    private float refreshTimer = 0f;        //  Refresh rate interval timer.

    private bool parkLightFound = false;        //  If park light found, this means don't illuminate brake lights for tail lights.
    private bool highBeamLightFound = false;        //  If high beam light found, this means don't illuminate normal headlights for high beam headlights.

    public RCC_Emission[] emission;     //  Emission for illuminating the texture.
    public bool useEmissionTexture = false;     //  Use the emission texture.

    public float strength = 100f;       //  	Strength of the light. 
    private float orgStrength = 100f;       //	Original strength of the light. We will be using this original value while restoring the light.

    public bool isBreakable = true;     //	Can it break at certain damage?
    public int breakPoint = 35;     //	    Light will be broken at this point.
    private bool broken = false;        //	Is this light broken currently?

    // For Indicators.
    private RCC_CarControllerV3.IndicatorsOn indicatorsOn;
    private AudioSource indicatorSound;
    public AudioClip indicatorClip { get { return RCC_Settings.Instance.indicatorClip; } }

    private void Awake() {

        // Getting main car controller.
        carController = GetComponentInParent<RCC_CarControllerV3>();

        //	Getting original strength of the light. We will be using this original value while restoring the light.
        orgStrength = strength;

        //  Initializing the light is it's attached to the vehicle. Do not init the light if it's not attached to the vehicle (Used for trailers. Trailers have not main car controller script. Assigning car controller of the light when trailer is attached/detached).
        if (carController)
            Initialize();

    }

    /// <summary>
    /// Initializes the light.
    /// </summary>
    public void Initialize() {

        //  Getting actual light component, make sure it's enabled. And then getting lensflare and trailrenderer if attached.
        _light = GetComponent<Light>();
        _light.enabled = true;
        lensFlare = GetComponent<LensFlare>();
        trail = GetComponentInChildren<TrailRenderer>();

        //  If lensflare found, set brightness to 0, color to white, and set flare texture. This is only for initialization process.
        if (lensFlare) {

            lensFlare.brightness = 0f;
            lensFlare.color = Color.white;
            lensFlare.fadeSpeed = 20f;

            if (_light.flare != null)
                _light.flare = null;

            lensFlare.flare = flare;

        }

        //  If use projector option in RCC Settings is set to true, create projector and initialize it.
        if (RCC_Settings.Instance.useLightProjectorForLightingEffect) {

            //  Getting projector component.
            projector = GetComponent<Projector>();

            //  If light doesn't have a projector, create a new one.
            if (projector == null) {

                projector = (Instantiate(RCC_Settings.Instance.projector, transform.position, transform.rotation)).GetComponent<Projector>();
                projector.transform.SetParent(transform, true);

            }

            //  Projector will ignore layers selected in RCC Settings.
            projector.ignoreLayers = RCC_Settings.Instance.projectorIgnoreLayer;

            //  Assigning instance material of the projector.
            Material newMaterial = new Material(projector.material);
            projector.material = newMaterial;

        }

        if (!overrideRenderMode) {

            switch (lightType) {

                case LightType.HeadLight:

                    //  If light option in RCC Settings is set to "Use Vertex", set render mode of the light to "ForceVertex". Otherwise, force to "ForcePixel".
                    if (RCC_Settings.Instance.useHeadLightsAsVertexLights)
                        renderMode = LightRenderMode.ForceVertex;
                    else
                        renderMode = LightRenderMode.ForcePixel;

                    break;

                case LightType.BrakeLight:

                    //  If light option in RCC Settings is set to "Use Vertex", set render mode of the light to "ForceVertex". Otherwise, force to "ForcePixel".
                    if (RCC_Settings.Instance.useBrakeLightsAsVertexLights)
                        renderMode = LightRenderMode.ForceVertex;
                    else
                        renderMode = LightRenderMode.ForcePixel;

                    break;

                case LightType.ReverseLight:

                    //  If light option in RCC Settings is set to "Use Vertex", set render mode of the light to "ForceVertex". Otherwise, force to "ForcePixel".
                    if (RCC_Settings.Instance.useReverseLightsAsVertexLights)
                        renderMode = LightRenderMode.ForceVertex;
                    else
                        renderMode = LightRenderMode.ForcePixel;

                    break;

                case LightType.Indicator:

                    //  If light option in RCC Settings is set to "Use Vertex", set render mode of the light to "ForceVertex". Otherwise, force to "ForcePixel".
                    if (RCC_Settings.Instance.useIndicatorLightsAsVertexLights)
                        renderMode = LightRenderMode.ForceVertex;
                    else
                        renderMode = LightRenderMode.ForcePixel;

                    break;

                case LightType.ParkLight:

                    //  If light option in RCC Settings is set to "Use Vertex", set render mode of the light to "ForceVertex". Otherwise, force to "ForcePixel".
                    if (RCC_Settings.Instance.useOtherLightsAsVertexLights)
                        renderMode = LightRenderMode.ForceVertex;
                    else
                        renderMode = LightRenderMode.ForcePixel;

                    break;

                case LightType.External:

                    //  If light option in RCC Settings is set to "Use Vertex", set render mode of the light to "ForceVertex". Otherwise, force to "ForcePixel".
                    if (RCC_Settings.Instance.useOtherLightsAsVertexLights)
                        renderMode = LightRenderMode.ForceVertex;
                    else
                        renderMode = LightRenderMode.ForcePixel;

                    break;

            }

        }

        _light.renderMode = renderMode;

        //  If light type is indicator, create audiosource for indicator.
        if (lightType == LightType.Indicator) {

            if (!carController.transform.Find("All Audio Sources/Indicator Sound AudioSource"))
                indicatorSound = NewAudioSource(RCC_Settings.Instance.audioMixer, carController.gameObject, "Indicator Sound AudioSource", 1f, 3f, 1, indicatorClip, false, false, false);
            else
                indicatorSound = carController.transform.Find("All Audio Sources/Indicator Sound AudioSource").GetComponent<AudioSource>();

        }

        //  Getting all lights attached to this vehicle.
        RCC_Light[] allLights = carController.GetComponentsInChildren<RCC_Light>();

        //  Checking if vehicle has park light or highbeam headlight. 
        //  If park light found, this means don't illuminate brake lights for tail lights.
        //  If high beam light found, this means don't illuminate normal headlights for high beam headlights.
        for (int i = 0; i < allLights.Length; i++) {

            if (allLights[i].lightType == LightType.ParkLight)
                parkLightFound = true;

            if (allLights[i].lightType == LightType.HighBeamHeadLight)
                highBeamLightFound = true;

        }

        //  Checking rotation of the light. If it's facing to wrong direction, fix it.
        CheckRotation();

    }

    private void OnEnable() {

        //  Getting light component if not initialized yet. Useful for lights attached to truck trailers.
        if (!_light)
            _light = GetComponent<Light>();

        //  If default intensity of the light is set to 0, override it.
        if (defaultIntensity == 0)
            defaultIntensity = _light.intensity;

        //  Make sure intensity of the light is set to 0 on enable.
        _light.intensity = 0f;

    }

    private void Update() {

        //  If no car controller found, return.
        if (!carController)
            return;

        //  If use projector option in RCC Settings is set to true, use projectors.
        if (projector)
            Projectors();

        //  If lensflare found, use them.
        if (lensFlare)
            LensFlare();

        //  If trail renderer found, use them.
        if (trail)
            TrailRenderer();

        //  If use emission texture is enabled, use them.
        if (useEmissionTexture) {

            foreach (RCC_Emission item in emission)
                item.Emission(_light);

        }

        //  If light is broken due to damage, set intensity of the light to 0.
        if (broken) {

            Lighting(0f);
            return;

        }

        //  Light types. Illuminating lights with given values.
        switch (lightType) {

            case LightType.HeadLight:
                if (highBeamLightFound) {

                    Lighting(carController.lowBeamHeadLightsOn ? defaultIntensity : 0f, 50f, 90f);

                } else {

                    Lighting(carController.lowBeamHeadLightsOn ? defaultIntensity : 0f, 50f, 90f);

                    if (!carController.lowBeamHeadLightsOn && !carController.highBeamHeadLightsOn)
                        Lighting(0f);
                    if (carController.lowBeamHeadLightsOn && !carController.highBeamHeadLightsOn) {
                        Lighting(defaultIntensity, 50f, 90f);
                        transform.localEulerAngles = new Vector3(10f, 0f, 0f);
                    } else if (carController.highBeamHeadLightsOn) {
                        Lighting(defaultIntensity, 100f, 45f);
                        transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                    }

                }
                break;

            case LightType.BrakeLight:

                if (parkLightFound)
                    Lighting(carController.brakeInput >= .1f ? defaultIntensity : 0f);
                else
                    Lighting(carController.brakeInput >= .1f ? defaultIntensity : !carController.lowBeamHeadLightsOn ? 0f : .25f);
                break;

            case LightType.ReverseLight:
                Lighting(carController.direction == -1 ? defaultIntensity : 0f);
                break;

            case LightType.ParkLight:
                Lighting((!carController.lowBeamHeadLightsOn ? 0f : defaultIntensity));
                break;

            case LightType.Indicator:
                indicatorsOn = carController.indicatorsOn;
                Indicators();
                break;

            case LightType.HighBeamHeadLight:
                Lighting(carController.highBeamHeadLightsOn ? defaultIntensity : 0f, 200f, 45f);
                break;

        }

    }

    /// <summary>
    /// Illuminates the light with given input (intensity).
    /// </summary>
    /// <param name="input"></param>
    private void Lighting(float input) {

        if (input >= .05f)
            _light.intensity = Mathf.Lerp(_light.intensity, input, Time.deltaTime * inertia * 20f);
        else
            _light.intensity = 0f;

    }

    /// <summary>
    /// Illuminates the light with given input (intensity), range, and spot angle..
    /// </summary>
    /// <param name="input"></param>
    /// <param name="range"></param>
    /// <param name="spotAngle"></param>
    private void Lighting(float input, float range, float spotAngle) {

        if (input >= .05f)
            _light.intensity = Mathf.Lerp(_light.intensity, input, Time.deltaTime * inertia * 20f);
        else
            _light.intensity = 0f;

        _light.range = range;
        _light.spotAngle = spotAngle;

    }

    /// <summary>
    /// Operating indicators with timer.
    /// </summary>
    private void Indicators() {

        //  Is this indicator at left or right side?
        Vector3 relativePos = carController.transform.InverseTransformPoint(transform.position);

        //  If indicator is at left side, and indicator is set to left side as well, illuminate the light with timer.
        //  If indicator is at right side, and indicator is set to right side as well, illuminate the light with timer.
        //  Play created audio source while illuminating the light.
        switch (indicatorsOn) {

            case RCC_CarControllerV3.IndicatorsOn.Left:

                if (relativePos.x > 0f) {

                    Lighting(0);
                    break;

                }

                if (carController.indicatorTimer >= .5f) {

                    Lighting(0);

                    if (indicatorSound.isPlaying)
                        indicatorSound.Stop();

                } else {

                    Lighting(defaultIntensity);

                    if (!indicatorSound.isPlaying && carController.indicatorTimer <= .05f)
                        indicatorSound.Play();

                }

                if (carController.indicatorTimer >= 1f)
                    carController.indicatorTimer = 0f;

                break;

            case RCC_CarControllerV3.IndicatorsOn.Right:

                if (relativePos.x < 0f) {

                    Lighting(0);
                    break;

                }

                if (carController.indicatorTimer >= .5f) {

                    Lighting(0);

                    if (indicatorSound.isPlaying)
                        indicatorSound.Stop();

                } else {

                    Lighting(defaultIntensity);

                    if (!indicatorSound.isPlaying && carController.indicatorTimer <= .05f)
                        indicatorSound.Play();

                }

                if (carController.indicatorTimer >= 1f)
                    carController.indicatorTimer = 0f;

                break;

            case RCC_CarControllerV3.IndicatorsOn.All:

                if (carController.indicatorTimer >= .5f) {

                    Lighting(0);

                    if (indicatorSound.isPlaying)
                        indicatorSound.Stop();

                } else {

                    Lighting(defaultIntensity);

                    if (!indicatorSound.isPlaying && carController.indicatorTimer <= .05f)
                        indicatorSound.Play();

                }

                if (carController.indicatorTimer >= 1f)
                    carController.indicatorTimer = 0f;

                break;

            case RCC_CarControllerV3.IndicatorsOn.Off:

                Lighting(0);
                carController.indicatorTimer = 0f;
                break;

        }

    }

    /// <summary>
    /// Operating projectors.
    /// </summary>
    private void Projectors() {

        //  If light is not enabled, set projector to false and return. Otherwise enable.
        if (!_light.enabled) {

            projector.enabled = false;
            return;

        } else {

            projector.enabled = true;

        }

        //  Setting color of the material for projector.
        projector.material.color = _light.color * (_light.intensity / 5f);

        //  Setting range and field of view of the projector.
        projector.farClipPlane = Mathf.Lerp(10f, 40f, (_light.range - 50) / 150f);
        projector.fieldOfView = Mathf.Lerp(40f, 30f, (_light.range - 50) / 150f);

    }

    /// <summary>
    /// Operating lensflares related to camera angle.
    /// </summary>
    private void LensFlare() {

        //  Lensflares are not affected by collider of the vehicle. They will ignore it. Below code will calculate the angle of the light-camera, and set intensity of the lensflare.

        //  Working with refresh rate.
        if (refreshTimer > (1f / refreshRate)) {

            refreshTimer = 0f;

            if (!Camera.main)
                return;

            float distanceTocam = Vector3.Distance(transform.position, Camera.main.transform.position);
            float angle = 1f;

            if (lightType != LightType.External)
                angle = Vector3.Angle(transform.forward, Camera.main.transform.position - transform.position);

            if (angle != 0)
                finalFlareBrightness = flareBrightness * (4f / distanceTocam) * ((300f - (3f * angle)) / 300f) / 3f;

            lensFlare.brightness = finalFlareBrightness * _light.intensity;
            lensFlare.color = _light.color;

        }

        refreshTimer += Time.deltaTime;

    }

    /// <summary>
    /// Operating trailrenderers.
    /// </summary>
    private void TrailRenderer() {

        //  If intensity of the light is high enough, enable emission of the trail renderer. And set color.
        trail.emitting = _light.intensity > .1f ? true : false;
        trail.startColor = _light.color;

    }

    /// <summary>
    /// Checks rotation of the light if it's facing to wrong direction.
    /// </summary>
    private void CheckRotation() {

        Vector3 relativePos = carController.transform.InverseTransformPoint(transform.position);

        //  If light is at front side...
        if (relativePos.z > 0f) {

            //  ... and Y rotation of the light is over 90 degrees, reset it to 0.
            if (Mathf.Abs(transform.localRotation.y) > .5f)
                transform.localRotation = Quaternion.identity;

        } else {

            //  If light is at rear side...
            //  ... and Y rotation of the light is over 90 degrees, reset it to 0.
            if (Mathf.Abs(transform.localRotation.y) < .5f)
                transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        }

    }

    /// <summary>
    /// Listening vehicle collisions for braking the light.
    /// </summary>
    /// <param name="impulse"></param>
    public void OnCollision(float impulse) {

        // If light is broken, return.
        if (broken)
            return;

        //	Decreasing strength of the light related to collision impulse.
        strength -= impulse * 10f;
        strength = Mathf.Clamp(strength, 0f, Mathf.Infinity);

        //	Check joint of the part based on strength.
        if (strength <= breakPoint)
            broken = true;

    }

    /// <summary>
    /// Repairs, and restores the light.
    /// </summary>
    public void OnRepair() {

        strength = orgStrength;
        broken = false;

    }

    private void Reset() {

        CheckRotation();

        foreach (RCC_Emission item in emission)
            item.multiplier = 1f;

    }

}
