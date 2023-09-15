﻿//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2022 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RCC_Camera))]
public class RCC_CameraEditor : Editor {

    RCC_Camera RCCCam;
    Color orgColor;

    public override void OnInspectorGUI() {

        RCCCam = (RCC_Camera)target;
        serializedObject.Update();
        orgColor = GUI.color;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Main Camera designed for RCC. It includes 6 different camera modes. It doesn't use many cameras for different modes like *other* assets. Just one single camera handles them.", MessageType.Info);
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraTarget"), new GUIContent("Camera Target", "Camera Target."), true);
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pivot"), new GUIContent("Pivot of the Camera", "Pivot of the Camera."), false);
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraMode"), new GUIContent("Current Camera Mode", "Current Camera Modes."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useAutoChangeCamera"), new GUIContent("Auto Change Camera Mode", "Auto Changes Camera Mode."), false);
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("TPS", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSDistance"), new GUIContent("TPS Distance", "TPS Distance."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSHeight"), new GUIContent("TPS Height", "TPS Height."), false);
        EditorGUILayout.BeginHorizontal();
        float org = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 125f;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSLockX"), new GUIContent("TPS Lock X Angle", "TPS Lock X Angle."), false, GUILayout.MaxWidth(150f));
        GUILayout.FlexibleSpace();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSLockY"), new GUIContent("TPS Lock Y Angle", "TPS Lock Y Angle."), false, GUILayout.MaxWidth(150f));
        GUILayout.FlexibleSpace();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSLockZ"), new GUIContent("TPS Lock Z Angle", "TPS Lock Z Angle."), false, GUILayout.MaxWidth(150f));
        EditorGUIUtility.labelWidth = org;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSFreeFall"), new GUIContent("TPS Free Fall", "TPS Free Fall."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSDynamic"), new GUIContent("TPS Dynamic", "TPS Dynamic."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSRotationDamping"), new GUIContent("TPS Rotation Damping", "TPS Rotation Damping."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSMinimumFOV"), new GUIContent("TPS Minimum FOV", "TPS Minimum FOV."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSMaximumFOV"), new GUIContent("TPS Maximum FOV", "TPS Maximum FOV."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSTiltMaximum"), new GUIContent("TPS Tilt Maximum", "TPS Tilt Maximum."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSTiltMultiplier"), new GUIContent("TPS Tilt Multiplier", "TPS Tilt Multiplier."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSYaw"), new GUIContent("TPS Yaw Angle", "TPS Pitch Angle."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSPitch"), new GUIContent("TPS Pitch Angle", "TPS Pitch Angle."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("zoomScrollMultiplier"), new GUIContent("Zoom Scroll Multiplier", "Zoom Scroll Multiplier."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumScroll"), new GUIContent("Zoom Scroll Minimum", "Zoom Scroll Minimum."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumScroll"), new GUIContent("Zoom Scroll Maximum", "Zoom Scroll Maximum."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSAutoFocus"), new GUIContent("Use Auto Focus", "Use Auto Focus."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSAutoReverse"), new GUIContent("Use Reverse", "Use Reverse."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useOrbitInTPSCameraMode"), new GUIContent("Use Orbit", "Use Orbit In TPS Camera Mode."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSCollision"), new GUIContent("TPS Collision", "TPS Collision."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSOffset"), new GUIContent("TPS Offset", "TPS Offset."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TPSStartRotation"), new GUIContent("TPS Start Rotation", "TPS Start Rotation."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useOcclusion"), new GUIContent("Use Occlusion", "Use Occlusion."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("occlusionLayerMask"), new GUIContent("Occlusion LayerMask", "Occlusion LayerMask."), false);

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("FPS", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useHoodCameraMode"), new GUIContent("Use Hood Camera Mode", "Shall we use Hood Camera Mode?"), false);

        if (RCCCam.useHoodCameraMode) {

            EditorGUILayout.HelpBox("Be sure your vehicle has ''Hood Camera''. Camera will be parented to this gameobject. You can create it from Tools --> BCG --> RCC --> Camera Systems --> Add Hood Camera.", MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hoodCameraFOV"), new GUIContent("Hood Camera FOV", "Hood Camera FOV."), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useOrbitInHoodCameraMode"), new GUIContent("Use Orbit", "Use Orbit In Hood Camera Mode."), false);

        }

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("Wheel", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useWheelCameraMode"), new GUIContent("Use Wheel Camera Mode", "Shall we use Wheel Camera Mode?"), false);

        if (RCCCam.useWheelCameraMode) {

            EditorGUILayout.HelpBox("Be sure your vehicle has ''Wheel Camera''. Camera will be parented to this gameobject. You can create it from Tools --> BCG --> RCC --> Camera Systems --> Add Wheel Camera.", MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wheelCameraFOV"), new GUIContent("Wheel Camera FOV", "Wheel Camera FOV."), false);

        }

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("Fixed", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useFixedCameraMode"), new GUIContent("Use Fixed Camera Mode", "Shall we use Fixed Camera Mode?"), false);

        if (RCCCam.useFixedCameraMode) {

            EditorGUILayout.HelpBox("Fixed Camera is overrided by ''Fixed Camera System'' on your scene.", MessageType.Info);

            EditorGUILayout.Space();

            if (!GameObject.FindObjectOfType<RCC_FixedCamera>()) {

                GUI.color = Color.green;

                if (GUILayout.Button("Create Fixed Camera System")) {

                    GameObject fixedCamera = new GameObject("RCC_FixedCamera");
                    fixedCamera.transform.position = Vector3.zero;
                    fixedCamera.transform.rotation = Quaternion.identity;
                    fixedCamera.AddComponent<RCC_FixedCamera>();

                }

            } else {

                GUI.color = orgColor;

                if (GUILayout.Button("Select Fixed Camera System"))
                    Selection.activeGameObject = GameObject.FindObjectOfType<RCC_FixedCamera>().gameObject;

            }

            GUI.color = orgColor;

        }

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("Cinematic", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useCinematicCameraMode"), new GUIContent("Use Cinematic Camera Mode", "Shall we use Cinematic Camera Mode?"), false);

        if (RCCCam.useCinematicCameraMode) {

            EditorGUILayout.HelpBox("Cinematic Camera is overrided by ''Cinematic Camera System'' on your scene.", MessageType.Info);

            EditorGUILayout.Space();

            if (!GameObject.FindObjectOfType<RCC_CinematicCamera>()) {

                GUI.color = Color.green;

                if (GUILayout.Button("Create Cinematic Camera System")) {

                    GameObject cinematicCamera = GameObject.Instantiate(RCC_Settings.Instance.cinematicCamera, Vector3.zero, Quaternion.identity) as GameObject;

                }

            } else {

                GUI.color = orgColor;

                if (GUILayout.Button("Select Cinematic Camera System"))
                    Selection.activeGameObject = GameObject.FindObjectOfType<RCC_CinematicCamera>().gameObject;

            }

            GUI.color = orgColor;

        }

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("Orbit", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("orbitXSpeed"), new GUIContent("Orbit X Speed", "Orbit X Speed."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("orbitYSpeed"), new GUIContent("Orbit Y Speed", "Orbit Y Speed."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("orbitSmooth"), new GUIContent("Orbit Smooth", "Orbit Smooth."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("minOrbitY"), new GUIContent("Min Orbit Y", "Min Orbit Y."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxOrbitY"), new GUIContent("Max Orbit Y", "Max Orbit Y."), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("orbitReset"), new GUIContent("Resets orbit rotation after 2 seconds."), false);

        GUI.color = orgColor;

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("Top-Down", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useTopCameraMode"), new GUIContent("Use Top Camera Mode", "Shall we use Top Camera Mode?"), false);

        if (RCCCam.useTopCameraMode) {

            EditorGUILayout.PropertyField(serializedObject.FindProperty("useOrthoForTopCamera"), new GUIContent("Use Ortho Mode", "Use Ortho Mode."), false);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("topCameraDistance"), new GUIContent("Top Camera Distance", "Top Camera Distance"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("topCameraAngle"), new GUIContent("Top Camera Angle", "Top Camera Angle"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumZDistanceOffset"), new GUIContent("Top Camera Maximum Z Distance", "Top Camera Maximum Z Distance"), false);

            if (RCCCam.useOrthoForTopCamera) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumOrtSize"), new GUIContent("Minimum Ortho Size", "Minimum Ortho Size related with vehicle speed."), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumOrtSize"), new GUIContent("Maximum Ortho Size", "Maximum Ortho Size related with vehicle speed."), false);
            } else {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumOrtSize"), new GUIContent("Minimum FOV", "Minimum FOV related with vehicle speed."), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumOrtSize"), new GUIContent("Maximum FOV", "Maximum FOV related with vehicle speed."), false);
            }

        }

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();

    }

}
