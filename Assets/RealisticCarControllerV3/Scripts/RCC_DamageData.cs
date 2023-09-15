using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RCC_DamageData {


    public List<RCC_DetachablePart> Hood = new List<RCC_DetachablePart>();
    public List<RCC_DetachablePart> Trunk = new List<RCC_DetachablePart>();
    public List<RCC_DetachablePart> Door = new List<RCC_DetachablePart>();
    public List<RCC_DetachablePart> Bumper_F = new List<RCC_DetachablePart>();
    public List<RCC_DetachablePart> Bumper_R = new List<RCC_DetachablePart>();

    public float hoodDamage, trunkDamage, doorDamage, bumper_FDamage, bumper_RDamage = 0f;

    private bool initialized = false;

    public void GetParts(RCC_DetachablePart[] allParts) {

        List<RCC_DetachablePart> allDetach = new List<RCC_DetachablePart>();

        for (int i = 0; i < allParts.Length; i++) {

            allDetach.Add(allParts[i]);

            switch (allParts[i].partType) {

                case RCC_DetachablePart.DetachablePartType.Hood:
                    Hood.Add(allParts[i]);
                    break;

                case RCC_DetachablePart.DetachablePartType.Trunk:
                    Trunk.Add(allParts[i]);
                    break;

                case RCC_DetachablePart.DetachablePartType.Door:
                    Door.Add(allParts[i]);
                    break;

                case RCC_DetachablePart.DetachablePartType.Bumper_F:
                    Bumper_F.Add(allParts[i]);
                    break;

                case RCC_DetachablePart.DetachablePartType.Bumper_R:
                    Bumper_R.Add(allParts[i]);
                    break;

            }

        }

        initialized = true;

    }

    public void CalculateDamage() {

        if (!initialized)
            return;

        float damage_h = 0f;

        for (int i = 0; i < Hood.Count; i++)
            damage_h += Hood[i].orgStrength - Hood[i].strength;

        if (Hood.Count > 1)
            damage_h /= Hood.Count;

        hoodDamage = damage_h;

        float damage_t = 0f;

        for (int i = 0; i < Trunk.Count; i++)
            damage_t += Trunk[i].orgStrength - Trunk[i].strength;

        if (Trunk.Count > 1)
            damage_t /= Trunk.Count;

        trunkDamage = damage_t;

        float damage_d = 0f;

        for (int i = 0; i < Door.Count; i++)
            damage_d += Door[i].orgStrength - Door[i].strength;

        if (Door.Count > 1)
            damage_d /= Door.Count;

        doorDamage = damage_d;

        float damage_bf = 0f;

        for (int i = 0; i < Bumper_F.Count; i++)
            damage_bf += Bumper_F[i].orgStrength - Bumper_F[i].strength;

        if (Bumper_F.Count > 1)
            damage_bf /= Bumper_F.Count;

        bumper_FDamage = damage_bf;

        float damage_br = 0f;

        for (int i = 0; i < Bumper_R.Count; i++)
            damage_br += Bumper_R[i].orgStrength - Bumper_R[i].strength;

        if (Bumper_R.Count > 1)
            damage_br /= Bumper_R.Count;

        bumper_RDamage = damage_br;

    }

    public float GetPercentage(RCC_DetachablePart.DetachablePartType partType) {

        if (!initialized)
            return 0;

        float damagePer = 0f;

        switch (partType) {

            case RCC_DetachablePart.DetachablePartType.Hood:
                damagePer = hoodDamage;
                break;

            case RCC_DetachablePart.DetachablePartType.Trunk:
                damagePer = trunkDamage;
                break;

            case RCC_DetachablePart.DetachablePartType.Door:
                damagePer = doorDamage;
                break;

            case RCC_DetachablePart.DetachablePartType.Bumper_F:
                damagePer = bumper_FDamage;
                break;

            case RCC_DetachablePart.DetachablePartType.Bumper_R:
                damagePer = bumper_RDamage;
                break;

        }

        return damagePer;

    }

}
