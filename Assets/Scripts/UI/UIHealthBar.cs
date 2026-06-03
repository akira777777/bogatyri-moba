using UnityEngine;
using UnityEngine.UI;
using BogatyriMoba.Core;

namespace BogatyriMoba.UI
{
    public class UIHealthBar : MonoBehaviour
    {
        [SerializeField] private BrawlerController target;
        [SerializeField] private Image fillImage;
        [SerializeField] private Vector3 offset = new Vector3(0, 1.2f, 0);

        private void Start()
        {
            if (target != null)
            {
                target.OnHealthChanged += UpdateHealth;
                UpdateHealth(target.CurrentHealth, target.MaxHealth);
            }
        }

        private void Update()
        {
            if (target != null)
            {
                transform.position = target.transform.position + offset;
            }
        }

        private void UpdateHealth(int current, int max)
        {
            if (max > 0)
            {
                float pct = (float)current / max;
                fillImage.fillAmount = pct;
            }
        }

        public void SetTarget(BrawlerController newTarget)
        {
            if (target != null)
                target.OnHealthChanged -= UpdateHealth;

            target = newTarget;
            if (target != null)
            {
                target.OnHealthChanged += UpdateHealth;
                UpdateHealth(target.CurrentHealth, target.MaxHealth);
            }
        }
    }
}
