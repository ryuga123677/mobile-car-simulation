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
using UnityEngine.UI;

/// <summary>
/// Receiving inputs from active vehicle on your scene, and feeds visual dashboard needles (Not UI).
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Misc/RCC Visual Dashboard Objects")]
public class RCC_DashboardObjects : MonoBehaviour {

    private RCC_CarControllerV3 carController;

    [System.Serializable]
    public class RPMDial {

        public GameObject dial;
        public float multiplier = .05f;
        public RotateAround rotateAround = RotateAround.Z;
        private Quaternion dialOrgRotation = Quaternion.identity;
        public Text text;

        public void Init() {

            if (dial)
                dialOrgRotation = dial.transform.localRotation;

        }

        public void Update(float value) {

            Vector3 targetAxis = Vector3.forward;

            switch (rotateAround) {

                case RotateAround.X:

                    targetAxis = Vector3.right;
                    break;

                case RotateAround.Y:

                    targetAxis = Vector3.up;
                    break;

                case RotateAround.Z:

                    targetAxis = Vector3.forward;
                    break;

            }

            dial.transform.localRotation = dialOrgRotation * Quaternion.AngleAxis(-multiplier * value, targetAxis);

            if (text)
                text.text = value.ToString("F0");

        }

    }

    [System.Serializable]
    public class SpeedoMeterDial {

        public GameObject dial;
        public float multiplier = 1f;
        public RotateAround rotateAround = RotateAround.Z;
        private Quaternion dialOrgRotation = Quaternion.identity;
        public Text text;

        public void Init() {

            if (dial)
                dialOrgRotation = dial.transform.localRotation;

        }

        public void Update(float value) {

            Vector3 targetAxis = Vector3.forward;

            switch (rotateAround) {

                case RotateAround.X:

                    targetAxis = Vector3.right;
                    break;

                case RotateAround.Y:

                    targetAxis = Vector3.up;
                    break;

                case RotateAround.Z:

                    targetAxis = Vector3.forward;
                    break;

            }

            dial.transform.localRotation = dialOrgRotation * Quaternion.AngleAxis(-multiplier * value, targetAxis);

            if (text)
                text.text = value.ToString("F0");

        }

    }

    [System.Serializable]
    public class FuelDial {

        public GameObject dial;
        public float multiplier = .1f;
        public RotateAround rotateAround = RotateAround.Z;
        private Quaternion dialOrgRotation = Quaternion.identity;
        public Text text;

        public void Init() {

            if (dial)
                dialOrgRotation = dial.transform.localRotation;

        }

        public void Update(float value) {

            Vector3 targetAxis = Vector3.forward;

            switch (rotateAround) {

                case RotateAround.X:

                    targetAxis = Vector3.right;
                    break;

                case RotateAround.Y:

                    targetAxis = Vector3.up;
                    break;

                case RotateAround.Z:

                    targetAxis = Vector3.forward;
                    break;

            }

            dial.transform.localRotation = dialOrgRotation * Quaternion.AngleAxis(-multiplier * value, targetAxis);

            if (text)
                text.text = value.ToString("F0");

        }

    }

    [System.Serializable]
    public class HeatDial {

        public GameObject dial;
        public float multiplier = .1f;
        public RotateAround rotateAround = RotateAround.Z;
        private Quaternion dialOrgRotation = Quaternion.identity;
        public Text text;

        public void Init() {

            if (dial)
                dialOrgRotation = dial.transform.localRotation;

        }

        public void Update(float value) {

            Vector3 targetAxis = Vector3.forward;

            switch (rotateAround) {

                case RotateAround.X:

                    targetAxis = Vector3.right;
                    break;

                case RotateAround.Y:

                    targetAxis = Vector3.up;
                    break;

                case RotateAround.Z:

                    targetAxis = Vector3.forward;
                    break;

            }

            dial.transform.localRotation = dialOrgRotation * Quaternion.AngleAxis(-multiplier * value, targetAxis);

            if (text)
                text.text = value.ToString("F0");

        }

    }

    [System.Serializable]
    public class InteriorLight {

        public Light light;
        public float intensity = 1f;
        public LightRenderMode renderMode = LightRenderMode.Auto;

        public void Init() {

            light.renderMode = renderMode;

        }

        public void Update(bool state) {

            if (!light.enabled)
                light.enabled = true;

            light.intensity = state ? intensity : 0f;

        }

    }

    [Space()]
    public RPMDial rPMDial;
    [Space()]
    public SpeedoMeterDial speedDial;
    [Space()]
    public FuelDial fuelDial;
    [Space()]
    public HeatDial heatDial;
    [Space()]
    public InteriorLight[] interiorLights;

    public enum RotateAround { X, Y, Z }

    private void Awake() {

        //  Getting car controller.
        carController = GetComponentInParent<RCC_CarControllerV3>();

        //  Initializing dials.
        rPMDial.Init();
        speedDial.Init();
        fuelDial.Init();
        heatDial.Init();

        //  Initializing lights.
        for (int i = 0; i < interiorLights.Length; i++)
            interiorLights[i].Init();

    }

    private void Update() {

        //  If no vehicle found, return.
        if (!carController)
            return;

        Dials();
        Lights();

    }

    /// <summary>
    /// Updates dials rotation.
    /// </summary>
    private void Dials() {

        if (rPMDial.dial != null)
            rPMDial.Update(carController.engineRPM);

        if (speedDial.dial != null)
            speedDial.Update(carController.speed);

        if (fuelDial.dial != null)
            fuelDial.Update(carController.fuelTank);

        if (heatDial.dial != null)
            heatDial.Update(carController.engineHeat);

    }

    /// <summary>
    /// Updates lights of the dash.
    /// </summary>
    private void Lights() {

        for (int i = 0; i < interiorLights.Length; i++)
            interiorLights[i].Update(carController.lowBeamHeadLightsOn);

    }

}
