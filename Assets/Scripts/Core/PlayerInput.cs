using UnityEngine;

namespace BogatyriMoba.Core
{
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

        private void Awake()
        {
#if UNITY_ANDROID || UNITY_IOS
            inputSource = InputSource.Touch;
#else
            inputSource = InputSource.KeyboardMouse;
#endif
        }

        private void Update()
        {
            if (inputSource != InputSource.KeyboardMouse)
                return;

            UpdateKeyboardMouse();
        }

        private void UpdateKeyboardMouse()
        {
            Vector2 move = Vector2.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) move.y += 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) move.y -= 1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) move.x -= 1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) move.x += 1;

            if (move.magnitude > 1f) move.Normalize();
            MoveDirection = move.magnitude > joystickDeadzone ? move : Vector2.zero;

            if (Camera.main != null)
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = 0;
                AimDirection = (mouseWorld - transform.position).normalized;
            }

            AttackPressed = Input.GetMouseButtonDown(0);
            SuperPressed = Input.GetKeyDown(KeyCode.Space);
            GadgetPressed = Input.GetKeyDown(KeyCode.E);
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
