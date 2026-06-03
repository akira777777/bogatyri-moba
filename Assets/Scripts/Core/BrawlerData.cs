using UnityEngine;

namespace BogatyriMoba.Core
{
    [CreateAssetMenu(fileName = "NewBrawler", menuName = "Bogatyri/Brawler Data")]
    public class BrawlerData : ScriptableObject
    {
        [Header("Identity")]
        public string brawlerName;
        public Sprite icon;
        public GameObject prefab;

        [Header("Base Stats")]
        public int baseHealth = 3200;
        public float movementSpeed = 4.5f;
        public float attackRange = 6f;
        public float attackReloadTime = 1.2f;
        public int attackDamage = 800;
        public int superDamage = 1200;
        public int superChargePerHit = 10; // percent
        public float superRange = 8f;

        [Header("Roles")]
        public BrawlerRole role;
        public Rarity rarity;

        [Header("Abilities")]
        public AttackType attackType;
        public SuperType superType;
        public UltimateAbility ultimateAbility;
        public GadgetData gadget;
        public StarPowerData starPower;

        [Header("Projectiles")]
        public GameObject attackProjectilePrefab;
        public GameObject superProjectilePrefab;
        public float projectileSpeed = 12f;

        public int GetHealthForPowerLevel(int powerLevel)
        {
            // Linear scaling: +5% per level
            float multiplier = 1f + (powerLevel - 1) * 0.05f;
            return Mathf.RoundToInt(baseHealth * multiplier);
        }

        public int GetDamageForPowerLevel(int powerLevel)
        {
            float multiplier = 1f + (powerLevel - 1) * 0.05f;
            return Mathf.RoundToInt(attackDamage * multiplier);
        }
    }

    public enum BrawlerRole { Tank, DamageDealer, Support, Assassin, Sniper, Control }
    public enum Rarity { Starting, Rare, SuperRare, Epic, Mythic, Legendary }
    public enum AttackType { SingleProjectile, Spread, AoE, Melee, Beam }
    public enum SuperType { Damage, Utility, CrowdControl, Buff, Heal }

    [System.Serializable]
    public class GadgetData
    {
        public string gadgetName;
        public int usesPerMatch = 3;
        public float cooldown = 5f;
    }

    [System.Serializable]
    public class StarPowerData
    {
        public string starPowerName;
        public string description;
    }
}
