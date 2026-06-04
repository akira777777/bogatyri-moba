using UnityEngine;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Handles movement, facing direction, and speed modifiers.
    /// Decoupled from input and combat logic.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovementComponent : MonoBehaviour
    {
        [SerializeField] private Transform visualContainer;

        private Rigidbody2D rb;
        private StealthComponent stealth;
        private BuffComponent buff;
        private float baseSpeed;

        public float CurrentSpeed => baseSpeed * GetSpeedMultiplier();
        public Vector2 MoveDirection { get; set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stealth = GetComponent<StealthComponent>();
            buff = GetComponent<BuffComponent>();

            if (visualContainer == null)
            {
                var visual = transform.Find("VisualContainer");
                if (visual != null)
                    visualContainer = visual;
            }
        }

        public void Initialize(float speed)
        {
            baseSpeed = speed;
        }

        public void Tick()
        {
            if (rb == null) return;

            float speed = baseSpeed * GetSpeedMultiplier();
            rb.linearVelocity = MoveDirection * speed;

            if (MoveDirection.x != 0 && visualContainer != null)
            {
                Vector3 scale = visualContainer.transform.localScale;
                scale.x = MoveDirection.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                visualContainer.transform.localScale = scale;
            }
        }

        public Vector2 GetFacingDirection()
        {
            if (visualContainer != null)
                return visualContainer.transform.localScale.x < 0 ? Vector2.left : Vector2.right;
            return transform.localScale.x < 0 ? Vector2.left : Vector2.right;
        }

        private float GetSpeedMultiplier()
        {
            float mult = 1f;
            if (stealth != null && stealth.IsStealthed)
                mult *= stealth.SpeedMultiplier;
            if (buff != null && buff.IsActive)
                mult *= buff.SpeedMultiplier;
            return mult;
        }

        public float GetDamageMultiplier()
        {
            float mult = 1f;
            if (stealth != null && stealth.IsStealthed)
                mult *= stealth.CritMultiplier;
            if (buff != null && buff.IsActive)
                mult *= buff.DamageMultiplier;
            return mult;
        }

        public void Stop()
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            MoveDirection = Vector2.zero;
        }
    }
}
