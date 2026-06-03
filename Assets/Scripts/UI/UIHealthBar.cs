using UnityEngine;
using UnityEngine.UI;
using BogatyriMoba.Core;

namespace BogatyriMoba.UI
{
    public class UIHealthBar : MonoBehaviour
    {
        [SerializeField] private BrawlerController target;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.2f, 0f);

        public void Configure(Image fill, Image background)
        {
            fillImage = fill;
            backgroundImage = background;
        }

        public void SetTarget(BrawlerController newTarget, Color? teamTint = null)
        {
            if (target != null)
                target.OnHealthChanged -= UpdateHealth;

            target = newTarget;
            if (teamTint.HasValue && fillImage != null)
                fillImage.color = teamTint.Value;

            if (target != null)
            {
                target.OnHealthChanged += UpdateHealth;
                UpdateHealth(target.CurrentHealth, target.MaxHealth);
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            // When parented to the brawler (WorldHealthBarFactory), local offset is enough.
            if (transform.parent == target.transform)
                return;

            transform.position = target.transform.position + offset;
        }

        private void UpdateHealth(int current, int max)
        {
            if (fillImage == null || max <= 0) return;
            fillImage.fillAmount = Mathf.Clamp01((float)current / max);
        }

        private void OnDestroy()
        {
            if (target != null)
                target.OnHealthChanged -= UpdateHealth;
        }
    }
}
