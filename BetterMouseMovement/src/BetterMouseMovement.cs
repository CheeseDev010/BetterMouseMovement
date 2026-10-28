using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BetterMouseMovement.src {
    public class BetterMouseMovement : ModBehaviour {
        public static BetterMouseMovement instance;
        public static IModConsole Console => instance.ModHelper.Console;

        public static bool modEnabled;

        public void Awake() {
            instance = this;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        public void Start() {
            ModHelper.Console.WriteLine($"Better Mouse Movement enabled!", MessageType.Success);
        }

        public void Update() {
            modEnabled = ModHelper.Config.GetSettingsValue<bool>("Enable Better Mouse Movement");
            if (ModHelper.Config.GetSettingsValue<bool>("Enable Max FPS (Disables VSync)")) {
                QualitySettings.vSyncCount = 0;
            }

            if (!modEnabled || Mouse.current == null) return;
            PlayerBody playerBody = (PlayerBody) Locator.GetPlayerBody();
            if (playerBody == null) return;
            PlayerCameraController cameraController = Locator.GetPlayerCameraController();

            bool flag = cameraController._shipController != null && cameraController._shipController.AllowFreeLook() && OWInput.IsPressed(InputLibrary.freeLook, 0f);
            bool flag2 = OWInput.IsInputMode(InputMode.Character | InputMode.ScopeZoom | InputMode.NomaiRemoteCam | InputMode.PatchingSuit);
            if (cameraController._isSnapping || cameraController._isLockedOn || (PlayerState.InZeroG() && PlayerState.IsWearingSuit()) || (!flag2 && !flag)) {
                return;
            }
            
            bool flag3 = Locator.GetAlarmSequenceController() != null && Locator.GetAlarmSequenceController().IsAlarmWakingPlayer();
            float scalar = 1f;
            scalar *= (cameraController._zoomed || flag3) ? PlayerCameraController.ZOOM_SCALAR : 1f;
            scalar *= cameraController._playerCamera.fieldOfView / cameraController._initFOV;
            if (Time.timeScale > 1f) {
                scalar /= Time.timeScale;
            }

            InputManager inputManager = (InputManager) OWInput.SharedInputManager;
            Vector2 delta = Mouse.current.delta.ReadUnprocessedValue();
            float mouseX = delta.x * scalar * inputManager._inputFadeFraction * PlayerData.GetLookSensitivity() * Time.fixedDeltaTime;
            float mouseY = delta.y * scalar * inputManager._inputFadeFraction * PlayerData.GetLookSensitivity() * Time.fixedDeltaTime;
            cameraController._degreesY += mouseY;
            if (flag) {
                cameraController._degreesX += mouseX;
            } else {
                playerBody.transform.rotation = Quaternion.AngleAxis(mouseX, playerBody.transform.up) * playerBody.transform.rotation;
            }
        }

        public override void Configure(IModConfig config) {
            if (config.GetSettingsValue<bool>("Enable Max FPS (Disables VSync)")) {
                Application.targetFrameRate = Mathf.Clamp(config.GetSettingsValue<int>("Max FPS (30 - 1000)"), 30, 1000);
            } else {
                Application.targetFrameRate = -1;
            }
        }
    }
}

