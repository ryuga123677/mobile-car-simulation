//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2022 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class RCC_SetScriptingSymbol {

    public static void SetEnabled(string defineName, bool enable) {

        bool updated = false;

        var defines = GetDefinesList(EditorUserBuildSettings.selectedBuildTargetGroup);

        if (enable) {

            if (!defines.Contains(defineName)) {

                defines.Add(defineName);
                updated = true;

            }

        } else {

            if (defines.Contains(defineName)) {

                while (defines.Contains(defineName))
                    defines.Remove(defineName);

                updated = true;

            }

        }

        if (updated) {

            string definesString = string.Join(";", defines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, definesString);

        }

    }

    public static List<string> GetDefinesList(BuildTargetGroup group) {

        return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));

    }

}
