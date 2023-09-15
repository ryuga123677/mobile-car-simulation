//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2022 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Receiving inputs from UI buttons, and feeds active vehicles on your scene.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/UI/Mobile/RCC UI Mobile Buttons")]
public class RCC_MobileButtons : RCC_Core {

    //  All buttons
    public RCC_UIController gasButton;
    public RCC_UIController gradualGasButton;
    public RCC_UIController brakeButton;
    public RCC_UIController leftButton;
    public RCC_UIController rightButton;
    public RCC_UIController handbrakeButton;
    public RCC_UIController NOSButton;
    public RCC_UIController NOSButtonSteeringWheel;
    public GameObject gearButton;

    // Steering wheel.
    public RCC_UISteeringWheelController steeringWheel;

    //  Joystick.
    public RCC_UIJoystick joystick;

    //  Mobile inputs.
    public static RCC_Inputs mobileInputs = new RCC_Inputs();

    //  Inputs.
    private float throttleInput = 0f;
    private float brakeInput = 0f;
    private float leftInput = 0f;
    private float rightInput = 0f;
    private float steeringWheelInput = 0f;
    private float handbrakeInput = 0f;
    private float boostInput = 1f;
    private float gyroInput = 0f;
    private float joystickInput = 0f;
    private bool canUseNos = false;

    private Vector3 orgBrakeButtonPos;

    private void Start() {

        //  If brake button is selected, take original position of the button.
        if (brakeButton)
            orgBrakeButtonPos = brakeButton.transform.position;

        //  Checking mobile buttons. Enabling or disabling them.
        CheckMobileButtons();

    }

    private void OnEnable() {

        RCC_SceneManager.OnVehicleChanged += CheckMobileButtons;

    }

    /// <summary>
    /// Checking mobile buttons. Enabling or disabling them.
    /// </summary>
    private void CheckMobileButtons() {

        //  If no any player vehicle, return.
        if (!RCC_SceneManager.Instance.activePlayerVehicle)
            return;

        // If mobile controllers are enabled, enable mobile buttons. Disable otherwise.
        if (RCC_Settings.Instance.mobileControllerEnabled) {

            EnableButtons();
            return;

        } else {

            DisableButtons();
            return;

        }

    }

    /// <summary>
    /// Disables all mobile buttons.
    /// </summary>
    private void DisableButtons() {

        if (gasButton)
            gasButton.gameObject.SetActive(false);
        if (gradualGasButton)
            gradualGasButton.gameObject.SetActive(false);
        if (leftButton)
            leftButton.gameObject.SetActive(false);
        if (rightButton)
            rightButton.gameObject.SetActive(false);
        if (brakeButton)
            brakeButton.gameObject.SetActive(false);
        if (steeringWheel)
            steeringWheel.gameObject.SetActive(false);
        if (handbrakeButton)
            handbrakeButton.gameObject.SetActive(false);
        if (NOSButton)
            NOSButton.gameObject.SetActive(false);
        if (NOSButtonSteeringWheel)
            NOSButtonSteeringWheel.gameObject.SetActive(false);
        if (gearButton)
            gearButton.gameObject.SetActive(false);
        if (joystick)
            joystick.gameObject.SetActive(false);

    }

    /// <summary>
    /// Enables all mobile buttons.
    /// </summary>
    private void EnableButtons() {

        if (gasButton)
            gasButton.gameObject.SetActive(true);
        //			if (gradualGasButton)
        //				gradualGasButton.gameObject.SetActive (true);
        if (leftButton)
            leftButton.gameObject.SetActive(true);
        if (rightButton)
            rightButton.gameObject.SetActive(true);
        if (brakeButton)
            brakeButton.gameObject.SetActive(true);
        if (steeringWheel)
            steeringWheel.gameObject.SetActive(true);
        if (handbrakeButton)
            handbrakeButton.gameObject.SetActive(true);

        if (canUseNos) {

            if (NOSButton)
                NOSButton.gameObject.SetActive(true);
            if (NOSButtonSteeringWheel)
                NOSButtonSteeringWheel.gameObject.SetActive(true);

        }

        if (joystick)
            joystick.gameObject.SetActive(true);

    }

    private void Update() {

        // If mobile controllers are not enabled, return.
        if (!RCC_Settings.Instance.mobileControllerEnabled)
            return;

        //  Mobile controller has four options. Buttons, gyro, steering wheel, and joystick.
        switch (RCC_Settings.Instance.mobileController) {

            case RCC_Settings.MobileController.TouchScreen:

                if (RCC_InputManager.gyroUsed) {

                    RCC_InputManager.gyroUsed = false;
                    InputSystem.DisableDevice(Accelerometer.current);

                }

                gyroInput = 0f;

                if (steeringWheel && steeringWheel.gameObject.activeInHierarchy)
                    steeringWheel.gameObject.SetActive(false);

                if (NOSButton && NOSButton.gameObject.activeInHierarchy != canUseNos)
                    NOSButton.gameObject.SetActive(canUseNos);

                if (joystick && joystick.gameObject.activeInHierarchy)
                    joystick.gameObject.SetActive(false);

                if (!leftButton.gameObject.activeInHierarchy) {

                    brakeButton.transform.position = orgBrakeButtonPos;
                    leftButton.gameObject.SetActive(true);

                }

                if (!rightButton.gameObject.activeInHierarchy)
                    rightButton.gameObject.SetActive(true);

                break;

            case RCC_Settings.MobileController.Gyro:

                if (!RCC_InputManager.gyroUsed) {

                    RCC_InputManager.gyroUsed = true;
                    InputSystem.EnableDevice(Accelerometer.current);

                }

                if (Accelerometer.current != null)
                    gyroInput = Mathf.Lerp(gyroInput, Accelerometer.current.acceleration.ReadValue().x * RCC_Settings.Instance.gyroSensitivity, Time.deltaTime * 5f);

                brakeButton.transform.position = leftButton.transform.position;

                if (steeringWheel && steeringWheel.gameObject.activeInHierarchy)
                    steeringWheel.gameObject.SetActive(false);

                if (NOSButton && NOSButton.gameObject.activeInHierarchy != canUseNos)
                    NOSButton.gameObject.SetActive(canUseNos);

                if (joystick && joystick.gameObject.activeInHierarchy)
                    joystick.gameObject.SetActive(false);

                if (leftButton.gameObject.activeInHierarchy)
                    leftButton.gameObject.SetActive(false);

                if (rightButton.gameObject.activeInHierarchy)
                    rightButton.gameObject.SetActive(false);

                break;

            case RCC_Settings.MobileController.SteeringWheel:

                if (RCC_InputManager.gyroUsed) {

                    RCC_InputManager.gyroUsed = false;
                    InputSystem.DisableDevice(Accelerometer.current);

                }

                gyroInput = 0f;

                if (steeringWheel && !steeringWheel.gameObject.activeInHierarchy) {
                    steeringWheel.gameObject.SetActive(true);
                    brakeButton.transform.position = orgBrakeButtonPos;
                }

                if (NOSButton && NOSButton.gameObject.activeInHierarchy)
                    NOSButton.gameObject.SetActive(false);

                if (NOSButtonSteeringWheel && NOSButtonSteeringWheel.gameObject.activeInHierarchy != canUseNos)
                    NOSButtonSteeringWheel.gameObject.SetActive(canUseNos);

                if (joystick && joystick.gameObject.activeInHierarchy)
                    joystick.gameObject.SetActive(false);

                if (leftButton.gameObject.activeInHierarchy)
                    leftButton.gameObject.SetActive(false);
                if (rightButton.gameObject.activeInHierarchy)
                    rightButton.gameObject.SetActive(false);

                break;

            case RCC_Settings.MobileController.Joystick:

                if (RCC_InputManager.gyroUsed) {

                    RCC_InputManager.gyroUsed = false;
                    InputSystem.DisableDevice(Accelerometer.current);

                }

                gyroInput = 0f;

                if (steeringWheel && steeringWheel.gameObject.activeInHierarchy)
                    steeringWheel.gameObject.SetActive(false);

                if (NOSButton && NOSButton.gameObject.activeInHierarchy != canUseNos)
                    NOSButton.gameObject.SetActive(canUseNos);

                if (joystick && !joystick.gameObject.activeInHierarchy) {
                    joystick.gameObject.SetActive(true);
                    brakeButton.transform.position = orgBrakeButtonPos;
                }

                if (leftButton.gameObject.activeInHierarchy)
                    leftButton.gameObject.SetActive(false);

                if (rightButton.gameObject.activeInHierarchy)
                    rightButton.gameObject.SetActive(false);

                break;

        }

        throttleInput = GetInput(gasButton) + GetInput(gradualGasButton);
        brakeInput = GetInput(brakeButton);
        leftInput = GetInput(leftButton);
        rightInput = GetInput(rightButton);
        handbrakeInput = GetInput(handbrakeButton);
        boostInput = Mathf.Clamp((GetInput(NOSButton) + GetInput(NOSButtonSteeringWheel)), 0f, 1f);

        if (steeringWheel && steeringWheel.gameObject.activeSelf)
            steeringWheelInput = steeringWheel.input;

        if (joystick && joystick.gameObject.activeSelf)
            joystickInput = joystick.inputHorizontal;

        SetMobileInputs();

    }

    /// <summary>
    /// Setting mobile inputs.
    /// </summary>
    private void SetMobileInputs() {

        if (!RCC_SceneManager.Instance.activePlayerVehicle)
            return;

        canUseNos = RCC_SceneManager.Instance.activePlayerVehicle.useNOS;

        mobileInputs.throttleInput = throttleInput;
        mobileInputs.brakeInput = brakeInput;
        mobileInputs.steerInput = -leftInput + rightInput + steeringWheelInput + gyroInput + joystickInput;
        mobileInputs.handbrakeInput = handbrakeInput;
        mobileInputs.boostInput = boostInput;

    }

    /// <summary>
    /// Gets input from button.
    /// </summary>
    /// <param name="button"></param>
    /// <returns></returns>
    private float GetInput(RCC_UIController button) {

        if (button == null)
            return 0f;

        if (!button.gameObject.activeSelf)
            return 0f;

        return (button.input);

    }

    private void OnDisable() {

        RCC_SceneManager.OnVehicleChanged -= CheckMobileButtons;

    }

}
