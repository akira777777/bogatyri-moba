using UnityEngine;

namespace BogatyriMoba.Core.Ultimates
{
    [CreateAssetMenu(fileName = "SmokeUltimate", menuName = "Bogatyri/Ultimates/Smoke")]
    public class SmokeUltimate : UltimateAbility
    {
        [Header("Smoke Settings")]
        public float invisibilityDuration = 3f;
        public float speedMultiplier = 1.5f;
        public float critMultiplier = 2f;
        public GameObject smokeEffectPrefab;

        public override void Activate(BrawlerController owner)
        {
            var stealth = owner.gameObject.GetComponent<StealthComponent>();
            if (stealth == null)
                stealth = owner.gameObject.AddComponent<StealthComponent>();
            
            stealth.Activate(invisibilityDuration, speedMultiplier, critMultiplier);
            
            if (smokeEffectPrefab != null)
            {
                var effect = Instantiate(smokeEffectPrefab, owner.transform.position, Quaternion.identity);
                Destroy(effect, 1.2f);
            }
        }
    }
}
