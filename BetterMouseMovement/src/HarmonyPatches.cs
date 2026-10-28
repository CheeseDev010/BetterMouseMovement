using HarmonyLib;
using UnityEngine;

namespace BetterMouseMovement.src {
    [HarmonyPatch]
    public class HarmonyPatches {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.UpdateInput))]
        public static bool PlayerCameraController_UpdateInput() {
            return !BetterMouseMovement.modEnabled;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.UpdateTurning))]
        public static bool PlayerCameraController_UpdateTurning(PlayerCharacterController __instance) {
            if (!BetterMouseMovement.modEnabled) return true;
            if (__instance._isGrounded && __instance._groundBody != null)
            {
                Vector3 vector = (__instance._movingPlatform != null) ? __instance._movingPlatform.GetAngularVelocity() : __instance._groundBody.GetAngularVelocity();
                __instance._baseAngularVelocity = Vector3.Dot(vector, __instance.transform.up);
            }
            else
            {
                __instance._baseAngularVelocity *= 0.995f;
            }
            Quaternion carry = Quaternion.AngleAxis(__instance._baseAngularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime, __instance._transform.up);
            __instance._transform.rotation = carry * __instance._transform.rotation;
            return false;
        }
    }
}
