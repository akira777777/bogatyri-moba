using UnityEngine;

namespace BogatyriMoba.Core
{
    public class BuffComponent : MonoBehaviour
    {
        private float speedMult = 1f;
        private float damageMult = 1f;
        private float duration;
        private float timer;
        private bool isActive;

        public void Apply(float speedMult, float damageMult, float duration)
        {
            this.speedMult = speedMult;
            this.damageMult = damageMult;
            this.duration = duration;
            timer = 0f;
            isActive = true;
        }

        private void Update()
        {
            if (!isActive) return;
            timer += Time.deltaTime;
            if (timer >= duration)
            {
                isActive = false;
                speedMult = 1f;
                damageMult = 1f;
            }
        }

        public float SpeedMultiplier => isActive ? speedMult : 1f;
        public float DamageMultiplier => isActive ? damageMult : 1f;
        public bool IsActive => isActive;
    }
}
