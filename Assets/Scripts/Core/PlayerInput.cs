using UnityEngine;

namespace BogatyriMoba.Core
{
    public class PlayerInput : MonoBehaviour
    {
        [Header("Joystick Settings")]
        public float joystickDeadzone = 0.2f;
        
        // Outputs
        public Vector2 MoveDirection { get; private set; }
        public Vector2 AimDirection { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool SuperPressed { get; private set; }
        public bool GadgetPressed { get; private set; }

        private void Update()
        {
            // Keyboard movement
            Vector2 move = Vector2.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) move.y += 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) move.y -= 1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) move.x -= 1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) move.x += 1;

            // Normalize
            if (move.magnitude > 1f) move.Normalize();
            MoveDirection = move.magnitude > joystickDeadzone ? move : Vector2.zero;

            // Aim towards mouse for PC testing
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            AimDirection = (mouseWorld - transform.position).normalized;

            // Buttons
            AttackPressed = Input.GetMouseButtonDown(0);
            SuperPressed = Input.GetKeyDown(KeyCode.Space);
            GadgetPressed = Input.GetKeyDown(KeyCode.E);
        }

        // Public methods to be called by UI Joystick scripts on mobile
        public void SetMoveInput(Vector2 direction)
        {
            MoveDirection = direction.magnitude > joystickDeadzone ? direction : Vector2.zero;
        }

        public void SetAimInput(Vector2 direction)
        {
            AimDirection = direction;
        }

        public void OnAttackButtonDown()
        {
            AttackPressed = true;
        }

        public void OnSuperButtonDown()
        {
            SuperPressed = true;
        }

        public void OnGadgetButtonDown()
        {
            GadgetPressed = true;
        }

        public void ClearAttackFlag()
        {
            AttackPressed = false;
        }

        public void ClearSuperFlag()
        {
            SuperPressed = false;
        }
    }
}
