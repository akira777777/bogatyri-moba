#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using BogatyriMoba.Core;
using BogatyriMoba.Core.Ultimates;

namespace BogatyriMoba.EditorTools
{
    public class BrawlerDataFactory : EditorWindow
    {
        [MenuItem("Bogatyri/Generate All Brawler Data")]
        static void GenerateAll()
        {
            string path = "Assets/ScriptableObjects/Brawlers/";
            string ultPath = "Assets/ScriptableObjects/Ultimates/";
            System.IO.Directory.CreateDirectory(path);
            System.IO.Directory.CreateDirectory(ultPath);

            // Create Ultimate Abilities
            var horseUlt = CreateUltimate<HorseUltimate>(ultPath, "HorseUltimate", "Конь-огонь");
            var roarUlt = CreateUltimate<RoarUltimate>(ultPath, "RoarUltimate", "Медвежий рёв");
            var snipeUlt = CreateUltimate<SnipeUltimate>(ultPath, "SnipeUltimate", "Соколиный взгляд");
            var wallUlt = CreateUltimate<WallUltimate>(ultPath, "WallUltimate", "Избушка");
            var fireUlt = CreateUltimate<FireBreathUltimate>(ultPath, "FireBreathUltimate", "Огненное дыхание");
            var smokeUlt = CreateUltimate<SmokeUltimate>(ultPath, "SmokeUltimate", "Дымовая завеса");
            var healUlt = CreateUltimate<HealUltimate>(ultPath, "HealUltimate", "Живая вода");
            var buffUlt = CreateUltimate<BuffUltimate>(ultPath, "BuffUltimate", "Золотой стяг");
            var stampedeUlt = CreateUltimate<StampedeUltimate>(ultPath, "StampedeUltimate", "Табун");

            // Create Brawlers
            CreateBrawler(path, "Alesha", "Алёша Попович", BrawlerRole.DamageDealer, Rarity.Starting,
                3000, 4.5f, 6.5f, 1.2f, 800, 1000, 12, 7f,
                AttackType.SingleProjectile, SuperType.Damage, horseUlt);

            CreateBrawler(path, "Dobrynya", "Добрыня Никитич", BrawlerRole.Tank, Rarity.Starting,
                5000, 3.5f, 4f, 1.5f, 600, 800, 10, 5f,
                AttackType.Melee, SuperType.CrowdControl, roarUlt);

            CreateBrawler(path, "Ilya", "Илья Муромец", BrawlerRole.Sniper, Rarity.Starting,
                2600, 4.2f, 9f, 1.6f, 1100, 1400, 10, 10f,
                AttackType.SingleProjectile, SuperType.Damage, snipeUlt);

            CreateBrawler(path, "BabaYaga", "Баба Яга", BrawlerRole.Control, Rarity.Rare,
                2800, 4.3f, 7f, 1.4f, 750, 0, 15, 6f,
                AttackType.AoE, SuperType.CrowdControl, wallUlt);

            CreateBrawler(path, "ZmeyGorynych", "Змей Горыныч", BrawlerRole.DamageDealer, Rarity.Epic,
                3800, 4.0f, 5.5f, 1.3f, 500, 1200, 14, 6f,
                AttackType.Spread, SuperType.Damage, fireUlt);

            CreateBrawler(path, "Tugarin", "Тугарин Змей", BrawlerRole.Assassin, Rarity.Legendary,
                2400, 5.2f, 3.5f, 0.9f, 900, 1000, 16, 8f,
                AttackType.Melee, SuperType.Utility, smokeUlt);

            CreateBrawler(path, "Varvara", "Варвара-краса", BrawlerRole.Support, Rarity.Rare,
                2900, 4.4f, 6f, 1.1f, 600, 0, 18, 8f,
                AttackType.SingleProjectile, SuperType.Heal, healUlt);

            CreateBrawler(path, "Knyaz", "Князь Киевский", BrawlerRole.Support, Rarity.SuperRare,
                3200, 4.1f, 5f, 1.3f, 700, 0, 20, 7f,
                AttackType.SingleProjectile, SuperType.Buff, buffUlt);

            CreateBrawler(path, "Konyukh", "Конюх", BrawlerRole.Support, Rarity.Rare,
                3100, 4.2f, 5.5f, 1.2f, 650, 0, 16, 7f,
                AttackType.SingleProjectile, SuperType.Damage, stampedeUlt);

            AssetDatabase.SaveAssets();
            Debug.Log("All brawler data and ultimate abilities generated!");
        }

        static T CreateUltimate<T>(string path, string fileName, string displayName) where T : UltimateAbility
        {
            string fullPath = path + fileName + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            if (existing != null) return existing;

            var asset = ScriptableObject.CreateInstance<T>();
            asset.abilityName = displayName;
            AssetDatabase.CreateAsset(asset, fullPath);
            return asset;
        }

        static void CreateBrawler(string path, string fileName, string displayName, BrawlerRole role, Rarity rarity,
            int baseHealth, float speed, float atkRange, float atkReload, int atkDmg,
            int superDmg, int superCharge, float superRange,
            AttackType attackType, SuperType superType, UltimateAbility ultimate)
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
            asset.ultimateAbility = ultimate;
            asset.projectileSpeed = 12f;

            string fullPath = path + fileName + ".asset";
            AssetDatabase.CreateAsset(asset, fullPath);
        }
    }
}
#endif
