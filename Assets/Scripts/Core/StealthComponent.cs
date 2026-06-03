using UnityEngine;

namespace BogatyriMoba.Core
{
    public class StealthComponent : MonoBehaviour
    {
        private float duration;
        private float speedMult;
        private float critMult;
        private float timer;
        private bool isActive;
        private BrawlerController brawler;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            brawler = GetComponent<BrawlerController>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void Activate(float duration, float speedMult, float critMult)
        {
            this.duration = duration;
            this.speedMult = speedMult;
            this.critMult = critMult;
            timer = 0f;
            isActive = true;

            // Apply visual stealth
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.35f);
        }

        private void Update()
        {
            if (!isActive) return;

            timer += Time.deltaTime;
            if (timer >= duration)
            {
                Deactivate();
            }
        }

        private void Deactivate()
        {
            isActive = false;
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            
            // The crit is consumed on next attack via BrawlerController
            // Speed modification is handled by the controller checking this component
        }

        public bool IsStealthed => isActive;
        public float SpeedMultiplier => isActive ? speedMult : 1f;
        public float CritMultiplier => isActive ? critMult : 1f;
    }
}
