#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using BogatyriMoba.Core;

namespace BogatyriMoba.EditorTools
{
    public class BrawlerDataFactory : EditorWindow
    {
        [MenuItem("Bogatyri/Generate Brawler Data")]
        static void GenerateAll()
        {
            string path = "Assets/ScriptableObjects/Brawlers/";
            System.IO.Directory.CreateDirectory(path);

            // 1. Алёша Попович — универсал (Starting)
            CreateBrawler(path, "Alesha", "Алёша Попович", BrawlerRole.DamageDealer, Rarity.Starting,
                baseHealth: 3000, speed: 4.5f, atkRange: 6.5f, atkReload: 1.2f, atkDmg: 800,
                superDmg: 1000, superCharge: 12, superRange: 7f,
                attackType: AttackType.SingleProjectile, superType: SuperType.Damage);

            // 2. Добрыня Никитич — танк (Starting)
            CreateBrawler(path, "Dobrynya", "Добрыня Никитич", BrawlerRole.Tank, Rarity.Starting,
                baseHealth: 5000, speed: 3.8f, atkRange: 4f, atkReload: 1.5f, atkDmg: 600,
                superDmg: 800, superCharge: 10, superRange: 5f,
                attackType: AttackType.Melee, superType: SuperType.CrowdControl);

            // 3. Илья Муромец — снайпер (Starting)
            CreateBrawler(path, "Ilya", "Илья Муромец", BrawlerRole.Sniper, Rarity.Starting,
                baseHealth: 2600, speed: 4.2f, atkRange: 9f, atkReload: 1.6f, atkDmg: 1100,
                superDmg: 1400, superCharge: 10, superRange: 10f,
                attackType: AttackType.SingleProjectile, superType: SuperType.Damage);

            // 4. Баба Яга — контроль (Rare)
            CreateBrawler(path, "BabaYaga", "Баба Яга", BrawlerRole.Control, Rarity.Rare,
                baseHealth: 2800, speed: 4.3f, atkRange: 7f, atkReload: 1.4f, atkDmg: 750,
                superDmg: 0, superCharge: 15, superRange: 6f,
                attackType: AttackType.AoE, superType: SuperType.CrowdControl);

            // 5. Змей Горыныч — AOE (Epic)
            CreateBrawler(path, "ZmeyGorynych", "Змей Горыныч", BrawlerRole.DamageDealer, Rarity.Epic,
                baseHealth: 3800, speed: 4.0f, atkRange: 5.5f, atkReload: 1.3f, atkDmg: 500,
                superDmg: 1200, superCharge: 14, superRange: 6f,
                attackType: AttackType.Spread, superType: SuperType.AoE);

            // 6. Тугарин Змей — ассасин (Legendary)
            CreateBrawler(path, "Tugarin", "Тугарин Змей", BrawlerRole.Assassin, Rarity.Legendary,
                baseHealth: 2400, speed: 5.2f, atkRange: 3.5f, atkReload: 0.8f, atkDmg: 900,
                superDmg: 1000, superCharge: 16, superRange: 8f,
                attackType: AttackType.Melee, superType: SuperType.Utility);

            // 7. Варвара-краса — хил (Rare)
            CreateBrawler(path, "Varvara", "Варвара-краса", BrawlerRole.Support, Rarity.Rare,
                baseHealth: 2900, speed: 4.4f, atkRange: 6f, atkReload: 1.1f, atkDmg: 600,
                superDmg: 0, superCharge: 18, superRange: 8f,
                attackType: AttackType.SingleProjectile, superType: SuperType.Heal);

            // 8. Князь Киевский — баффер (SuperRare)
            CreateBrawler(path, "Knyaz", "Князь Киевский", BrawlerRole.Support, Rarity.SuperRare,
                baseHealth: 3200, speed: 4.1f, atkRange: 5f, atkReload: 1.3f, atkDmg: 700,
                superDmg: 0, superCharge: 20, superRange: 7f,
                attackType: AttackType.SingleProjectile, superType: SuperType.Buff);

            AssetDatabase.SaveAssets();
            Debug.Log("All brawler data generated!");
        }

        static void CreateBrawler(string path, string fileName, string displayName, BrawlerRole role, Rarity rarity,
            int baseHealth, float speed, float atkRange, float atkReload, int atkDmg,
            int superDmg, int superCharge, float superRange,
            AttackType attackType, SuperType superType)
        {
            var asset = ScriptableObject.CreateInstance<BrawlerData>();
            asset.brawlerName = displayName;
            asset.role = role;
            asset.rarity = rarity;
            asset.baseHealth = baseHealth;
            asset.movementSpeed = speed;
            asset.attackRange = atkRange;
            asset.attackReloadTime = atkReload;
            asset.attackDamage = atkDmg;
            asset.superDamage = superDmg;
            asset.superChargePerHit = superCharge;
            asset.superRange = superRange;
            asset.attackType = attackType;
            asset.superType = superType;
            asset.projectileSpeed = 12f;

            string fullPath = path + fileName + ".asset";
            AssetDatabase.CreateAsset(asset, fullPath);
        }
    }
}
#endif
