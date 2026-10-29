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

        public static bool mouseMovementEnabled;

        private const float SensitivityScale = 0.02f;
        private const float SmoothSpeed = 40f;

        private static bool smoothingEnabled;
        private static bool enableMaxFPS;
        private static int maxFPS;
        private static Vector2 accumulatedDelta;

        public void Awake() {
            instance = this;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        public void Start() {
            Console.WriteLine($"Better Mouse Movement enabled!", MessageType.Success);
        }

        public void Update() {
            mouseMovementEnabled = ModHelper.Config.GetSettingsValue<bool>("Enable Better Mouse Movement");
            if (enableMaxFPS) {
                QualitySettings.vSyncCount = 0;
            }

            if (!mouseMovementEnabled) return;
            
            if (!UpdateMouseMovement()) {
                accumulatedDelta = Vector2.zero;
            }
        }

        public bool UpdateMouseMovement() {
            PlayerBody playerBody = (PlayerBody) Locator.GetPlayerBody();
            PlayerCameraController cameraController = Locator.GetPlayerCameraController();
            if (Mouse.current == null || cameraController == null || playerBody == null) return false;

            bool isInFreeLook = cameraController._shipController != null && cameraController._shipController.AllowFreeLook() && OWInput.IsPressed(InputLibrary.freeLook, 0f);
            if (IsCameraLocked(cameraController, isInFreeLook)) {
                return false;
            }
            
            bool isAlarmWakingPlayer = Locator.GetAlarmSequenceController() != null && Locator.GetAlarmSequenceController().IsAlarmWakingPlayer();
            float scalar = 1f;
            scalar *= (cameraController._zoomed || isAlarmWakingPlayer) ? PlayerCameraController.ZOOM_SCALAR : 1f;
            scalar *= cameraController._playerCamera.fieldOfView / cameraController._initFOV;

            InputManager inputManager = (InputManager) OWInput.SharedInputManager;
            accumulatedDelta += Mouse.current.delta.ReadUnprocessedValue() * scalar * inputManager._inputFadeFraction * PlayerData.GetLookSensitivity() * SensitivityScale;
            float t = smoothingEnabled ? 1f - Mathf.Exp(-SmoothSpeed * Time.unscaledDeltaTime) : 1f;
            Vector2 frameDelta = accumulatedDelta * t;
            accumulatedDelta -= frameDelta;

            cameraController._degreesY += frameDelta.y;
            if (isInFreeLook) {
                cameraController._degreesX += frameDelta.x;
            } else {
                playerBody.transform.rotation = Quaternion.AngleAxis(frameDelta.x, playerBody.transform.up) * playerBody.transform.rotation;
            }
            cameraController.UpdateRotation();
            return true;
        }
        
        public bool IsCameraLocked(PlayerCameraController camera, bool isInFreeLook) {
            bool inLookInputMode = OWInput.IsInputMode(InputMode.Character | InputMode.ScopeZoom | InputMode.NomaiRemoteCam | InputMode.PatchingSuit);
            return camera._isSnapping || camera._isLockedOn || (PlayerState.InZeroG() && PlayerState.IsWearingSuit()) || (!inLookInputMode && !isInFreeLook);
        }

        public override void Configure(IModConfig config) {
            mouseMovementEnabled = config.GetSettingsValue<bool>("Enable Better Mouse Movement");
            smoothingEnabled = config.GetSettingsValue<bool>("Mouse Smoothing");
            enableMaxFPS = config.GetSettingsValue<bool>("Enable Max FPS (Disables VSync)");
            maxFPS = config.GetSettingsValue<int>("Max FPS (30 - 1000)");

            if (enableMaxFPS) {
                Application.targetFrameRate = Mathf.Clamp(maxFPS, 30, 1000);
            } else {
                Application.targetFrameRate = -1;
            }
        }
    }
}

