using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BetterMouseMovement.src {
    public class BetterMouseMovement : ModBehaviour {
        public static BetterMouseMovement instance;

        public static bool modEnabled = false;
        public float xRotation = 0f;

        public void Awake() {
            instance = this;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

        }

        public void Start() {
            ModHelper.Console.WriteLine($"Better Mouse Movement enabled!", MessageType.Success);
        }

        public void Update() {
            if (Keyboard.current[Key.Backquote].wasPressedThisFrame) {
                modEnabled = !modEnabled;
                ModHelper.Console.WriteLine($"modEnabled: {modEnabled}");
            }
            if (!modEnabled) return;
            if (Mouse.current == null) return;
            PlayerBody body = (PlayerBody) Locator.GetPlayerBody();
            if (body == null) return;
            Vector2 delta = Mouse.current.delta.ReadValue();
            float mouseX = delta.x * PlayerData.GetLookSensitivity() * Time.fixedDeltaTime;
            float mouseY = delta.y * PlayerData.GetLookSensitivity() * Time.fixedDeltaTime;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            Locator.GetPlayerCamera().transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            body.transform.rotation = Quaternion.AngleAxis(mouseX, body.transform.up) * body.transform.rotation;
        }
    }
}

