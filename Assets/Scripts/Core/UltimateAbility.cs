using UnityEngine;

namespace BogatyriMoba.Core
{
    public abstract class UltimateAbility : ScriptableObject
    {
        [Header("Ability Info")]
        public string abilityName;
        public string description;
        public Sprite icon;

        [Header("Cooldown & Charge")]
        public float cooldown = 0.5f;

        /// <summary>
        /// Called when the brawler activates their super.
        /// </summary>
        public abstract void Activate(BrawlerController owner);
    }
}
