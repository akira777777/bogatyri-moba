using UnityEngine;
using UnityEngine.InputSystem;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Player input handler using the new Unity Input System.
    /// Supports Keyboard+Mouse and Gamepad with automatic device detection.
    /// Touch input is driven externally by MobileControlsUI.
    /// </summary>
    public class PlayerInput : MonoBehaviour
    {
        public enum InputSource
        {
            KeyboardMouse,
            Touch
        }

        [Header("Settings")]
        public float joystickDeadzone = 0.2f;
        [SerializeField] private InputSource inputSource = InputSource.KeyboardMouse;

        public Vector2 MoveDirection { get; private set; }
        public Vector2 AimDirection { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool SuperPressed { get; private set; }
        public bool GadgetPressed { get; private set; }

        // Input Actions
        private InputAction _moveAction;
        private InputAction _aimMouseAction;
        private InputAction _aimGamepadAction;
        private InputAction _attackAction;
        private InputAction _superAction;
        private InputAction _gadgetAction;

        private void Awake()
        {
#if UNITY_ANDROID || UNITY_IOS
            inputSource = InputSource.Touch;
#else
            inputSource = InputSource.KeyboardMouse;
#endif
            CreateActions();
        }

        private void OnEnable()
        {
            _moveAction?.Enable();
            _aimMouseAction?.Enable();
            _aimGamepadAction?.Enable();
            _attackAction?.Enable();
            _superAction?.Enable();
            _gadgetAction?.Enable();
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
            _aimMouseAction?.Disable();
            _aimGamepadAction?.Disable();
            _attackAction?.Disable();
            _superAction?.Disable();
            _gadgetAction?.Disable();
        }

        private void OnDestroy()
        {
            _moveAction?.Dispose();
            _aimMouseAction?.Dispose();
            _aimGamepadAction?.Dispose();
            _attackAction?.Dispose();
            _superAction?.Dispose();
            _gadgetAction?.Dispose();
        }

        private void CreateActions()
        {
            // Move — WASD / Arrow keys / Left stick
            _moveAction = new InputAction("Move", InputActionType.Value);
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            _moveAction.AddBinding("<Gamepad>/leftStick");

            // Aim — Mouse position (screen-space, converted to world in Update)
            _aimMouseAction = new InputAction("AimMouse", InputActionType.Value, "<Mouse>/position");

            // Aim — Gamepad right stick
            _aimGamepadAction = new InputAction("AimGamepad", InputActionType.Value, "<Gamepad>/rightStick");

            // Attack — Left mouse / Gamepad right trigger
            _attackAction = new InputAction("Attack", InputActionType.Button, "<Mouse>/leftButton");
            _attackAction.AddBinding("<Gamepad>/rightTrigger");

            // Super — Space / Gamepad West (X on Xbox, Square on PS)
            _superAction = new InputAction("Super", InputActionType.Button, "<Keyboard>/space");
            _superAction.AddBinding("<Gamepad>/buttonWest");

            // Gadget — E / Gamepad North (Y on Xbox, Triangle on PS)
            _gadgetAction = new InputAction("Gadget", InputActionType.Button, "<Keyboard>/e");
            _gadgetAction.AddBinding("<Gamepad>/buttonNorth");
        }

        private void Update()
        {
            if (inputSource != InputSource.KeyboardMouse)
                return;

            UpdateInput();
        }

        private void UpdateInput()
        {
            // Movement
            Vector2 move = _moveAction.ReadValue<Vector2>();
            if (move.magnitude > 1f) move.Normalize();
            MoveDirection = move.magnitude > joystickDeadzone ? move : Vector2.zero;

            // Aim — gamepad right stick takes priority if active, otherwise mouse
            Vector2 aimGamepad = _aimGamepadAction.ReadValue<Vector2>();
            if (aimGamepad.magnitude > joystickDeadzone)
            {
                AimDirection = aimGamepad.normalized;
            }
            else if (Camera.main != null && Mouse.current != null)
            {
                Vector2 mouseScreen = _aimMouseAction.ReadValue<Vector2>();
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
                    new Vector3(mouseScreen.x, mouseScreen.y, Camera.main.nearClipPlane));
                mouseWorld.z = 0;
                AimDirection = (mouseWorld - transform.position).normalized;
            }

            // Buttons
            AttackPressed = _attackAction.WasPressedThisFrame();
            SuperPressed = _superAction.WasPressedThisFrame();
            GadgetPressed = _gadgetAction.WasPressedThisFrame();
        }

        public void SetInputSource(InputSource source)
        {
            inputSource = source;
            if (source == InputSource.Touch)
            {
                MoveDirection = Vector2.zero;
                AimDirection = Vector2.right;
            }
        }

        public void SetMoveInput(Vector2 direction)
        {
            MoveDirection = direction.magnitude > joystickDeadzone ? direction : Vector2.zero;
        }

        public void SetAimInput(Vector2 direction)
        {
            if (direction.sqrMagnitude > joystickDeadzone * joystickDeadzone)
                AimDirection = direction.normalized;
        }

        public void OnAttackButtonDown() => AttackPressed = true;
        public void OnSuperButtonDown() => SuperPressed = true;
        public void OnGadgetButtonDown() => GadgetPressed = true;

        public void ClearAttackFlag() => AttackPressed = false;
        public void ClearSuperFlag() => SuperPressed = false;
        public void ClearGadgetFlag() => GadgetPressed = false;
    }
}
