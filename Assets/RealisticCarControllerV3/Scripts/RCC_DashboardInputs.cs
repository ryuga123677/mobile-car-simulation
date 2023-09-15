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
using System.Collections.Generic;

/// <summary>
/// Receiving inputs from active vehicle on your scene, and feeds dashboard needles, texts, images.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/UI/RCC UI Dashboard Inputs")]
public class RCC_DashboardInputs : MonoBehaviour {

    //  Needles.
    public GameObject RPMNeedle;
    public GameObject KMHNeedle;
    public GameObject turboGauge;
    public GameObject turboNeedle;
    public GameObject NOSGauge;
    public GameObject NoSNeedle;
    public GameObject heatGauge;
    public GameObject heatNeedle;
    public GameObject fuelGauge;
    public GameObject fuelNeedle;

    //  Needle rotations.
    private float RPMNeedleRotation = 0f;
    private float KMHNeedleRotation = 0f;
    private float BoostNeedleRotation = 0f;
    private float NoSNeedleRotation = 0f;
    private float heatNeedleRotation = 0f;
    private float fuelNeedleRotation = 0f;

    //  Variables of the player vehicle.
    internal float RPM;
    internal float KMH;
    internal int direction = 1;
    internal float Gear;
    internal bool changingGear = false;
    internal bool NGear = false;
    internal bool ABS = false;
    internal bool ESP = false;
    internal bool Park = false;
    internal bool Headlights = false;
    internal RCC_CarControllerV3.IndicatorsOn indicators;

    private void Update() {

        //  If no any player vehicle, return.
        if (!RCC_SceneManager.Instance.activePlayerVehicle)
            return;

        //  If player vehicle is not controllable or controlled by AI, return.
        if (!RCC_SceneManager.Instance.activePlayerVehicle.canControl || RCC_SceneManager.Instance.activePlayerVehicle.externalController)
            return;

        //  If nos gauge is selected, enable or disable gauge related to vehicle. 
        if (NOSGauge) {

            if (RCC_SceneManager.Instance.activePlayerVehicle.useNOS) {

                if (!NOSGauge.activeSelf)
                    NOSGauge.SetActive(true);

            } else {

                if (NOSGauge.activeSelf)
                    NOSGauge.SetActive(false);

            }

        }

        //  If turbo gauge is selected, enable or disable turbo gauge related to vehicle.
        if (turboGauge) {

            if (RCC_SceneManager.Instance.activePlayerVehicle.useTurbo) {

                if (!turboGauge.activeSelf)
                    turboGauge.SetActive(true);

            } else {

                if (turboGauge.activeSelf)
                    turboGauge.SetActive(false);

            }

        }

        //  If heat gauge is selected, enable or disable heat gauge related to vehicle.
        if (heatGauge) {

            if (RCC_SceneManager.Instance.activePlayerVehicle.useEngineHeat) {

                if (!heatGauge.activeSelf)
                    heatGauge.SetActive(true);

            } else {

                if (heatGauge.activeSelf)
                    heatGauge.SetActive(false);

            }

        }

        //  If fuel  gauge is selected, enable or disable fuel gauge related to vehicle.
        if (fuelGauge) {

            if (RCC_SceneManager.Instance.activePlayerVehicle.useFuelConsumption) {

                if (!fuelGauge.activeSelf)
                    fuelGauge.SetActive(true);

            } else {

                if (fuelGauge.activeSelf)
                    fuelGauge.SetActive(false);

            }

        }

        // Getting varaibles from the player vehicle.
        RPM = RCC_SceneManager.Instance.activePlayerVehicle.engineRPM;
        KMH = RCC_SceneManager.Instance.activePlayerVehicle.speed;
        direction = RCC_SceneManager.Instance.activePlayerVehicle.direction;
        Gear = RCC_SceneManager.Instance.activePlayerVehicle.currentGear;
        changingGear = RCC_SceneManager.Instance.activePlayerVehicle.changingGear;
        NGear = RCC_SceneManager.Instance.activePlayerVehicle.NGear;
        ABS = RCC_SceneManager.Instance.activePlayerVehicle.ABSAct;
        ESP = RCC_SceneManager.Instance.activePlayerVehicle.ESPAct;
        Park = RCC_SceneManager.Instance.activePlayerVehicle.handbrakeInput > .1f ? true : false;
        Headlights = RCC_SceneManager.Instance.activePlayerVehicle.lowBeamHeadLightsOn || RCC_SceneManager.Instance.activePlayerVehicle.highBeamHeadLightsOn;
        indicators = RCC_SceneManager.Instance.activePlayerVehicle.indicatorsOn;

        //  If RPM needle is selected, assign rotation of the needle.
        if (RPMNeedle) {

            RPMNeedleRotation = (RCC_SceneManager.Instance.activePlayerVehicle.engineRPM / 50f);
            RPMNeedleRotation = Mathf.Clamp(RPMNeedleRotation, 0f, 180f);
            RPMNeedle.transform.eulerAngles = new Vector3(RPMNeedle.transform.eulerAngles.x, RPMNeedle.transform.eulerAngles.y, -RPMNeedleRotation);

        }

        //  If KMH needle is selected, assign rotation of the needle.
        if (KMHNeedle) {

            if (RCC_Settings.Instance.units == RCC_Settings.Units.KMH)
                KMHNeedleRotation = (RCC_SceneManager.Instance.activePlayerVehicle.speed);
            else
                KMHNeedleRotation = (RCC_SceneManager.Instance.activePlayerVehicle.speed * 0.62f);

            KMHNeedle.transform.eulerAngles = new Vector3(KMHNeedle.transform.eulerAngles.x, KMHNeedle.transform.eulerAngles.y, -KMHNeedleRotation);

        }

        //  If turbo needle is selected, assign rotation of the needle.
        if (turboNeedle) {

            BoostNeedleRotation = (RCC_SceneManager.Instance.activePlayerVehicle.turboBoost / 30f) * 270f;
            turboNeedle.transform.eulerAngles = new Vector3(turboNeedle.transform.eulerAngles.x, turboNeedle.transform.eulerAngles.y, -BoostNeedleRotation);

        }

        //  If nos needle is selected, assign rotation of the needle.
        if (NoSNeedle) {

            NoSNeedleRotation = (RCC_SceneManager.Instance.activePlayerVehicle.NoS / 100f) * 270f;
            NoSNeedle.transform.eulerAngles = new Vector3(NoSNeedle.transform.eulerAngles.x, NoSNeedle.transform.eulerAngles.y, -NoSNeedleRotation);

        }

        //  If heat needle is selected, assign rotation of the needle.
        if (heatNeedle) {

            heatNeedleRotation = (RCC_SceneManager.Instance.activePlayerVehicle.engineHeat / 110f) * 270f;
            heatNeedle.transform.eulerAngles = new Vector3(heatNeedle.transform.eulerAngles.x, heatNeedle.transform.eulerAngles.y, -heatNeedleRotation);

        }

        //  If fuel needle is selected, assign rotation of the needle.
        if (fuelNeedle) {

            fuelNeedleRotation = (RCC_SceneManager.Instance.activePlayerVehicle.fuelTank / RCC_SceneManager.Instance.activePlayerVehicle.fuelTankCapacity) * 270f;
            fuelNeedle.transform.eulerAngles = new Vector3(fuelNeedle.transform.eulerAngles.x, fuelNeedle.transform.eulerAngles.y, -fuelNeedleRotation);

        }

    }

}
