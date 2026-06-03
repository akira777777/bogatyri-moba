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
        public static void GenerateAll()
        {
            string path = "Assets/ScriptableObjects/Brawlers/";
            string ultPath = "Assets/ScriptableObjects/Ultimates/";
            string resourcesPath = "Assets/Resources/Brawlers/";
            System.IO.Directory.CreateDirectory(path);
            System.IO.Directory.CreateDirectory(ultPath);
            System.IO.Directory.CreateDirectory(resourcesPath);

            var horseUlt = CreateUltimate<HorseUltimate>(ultPath, "HorseUltimate", "Конь-огонь");
            var roarUlt = CreateUltimate<RoarUltimate>(ultPath, "RoarUltimate", "Медвежий рёв");
            var snipeUlt = CreateUltimate<SnipeUltimate>(ultPath, "SnipeUltimate", "Соколиный взгляд");
            var wallUlt = CreateUltimate<WallUltimate>(ultPath, "WallUltimate", "Избушка");
            var fireUlt = CreateUltimate<FireBreathUltimate>(ultPath, "FireBreathUltimate", "Огненное дыхание");
            var smokeUlt = CreateUltimate<SmokeUltimate>(ultPath, "SmokeUltimate", "Дымовая завеса");
            var healUlt = CreateUltimate<HealUltimate>(ultPath, "HealUltimate", "Живая вода");
            var buffUlt = CreateUltimate<BuffUltimate>(ultPath, "BuffUltimate", "Золотой стяг");
            var stampedeUlt = CreateUltimate<StampedeUltimate>(ultPath, "StampedeUltimate", "Табун");

            ApplyUltimateDefaults(horseUlt, roarUlt, snipeUlt, wallUlt, fireUlt, smokeUlt, healUlt, buffUlt, stampedeUlt);

            var projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
            var horsePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HorseProjectile.prefab");
            var wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Wall.prefab");

            if (projectilePrefab != null)
            {
                snipeUlt.projectilePrefab = projectilePrefab;
                EditorUtility.SetDirty(snipeUlt);
            }

            if (horsePrefab != null)
            {
                horseUlt.horsePrefab = horsePrefab;
                stampedeUlt.horsePrefab = horsePrefab;
                EditorUtility.SetDirty(horseUlt);
                EditorUtility.SetDirty(stampedeUlt);
            }

            if (wallPrefab != null)
            {
                wallUlt.wallPrefab = wallPrefab;
                EditorUtility.SetDirty(wallUlt);
            }

            CreateBrawler(path, "Alesha", "Алёша Попович", BrawlerRole.DamageDealer, Rarity.Starting,
                3000, 4.5f, 6.5f, 1.2f, 800, 1000, 12, 7f,
                AttackType.SingleProjectile, SuperType.Damage, horseUlt, projectilePrefab);

            CreateBrawler(path, "Dobrynya", "Добрыня Никитич", BrawlerRole.Tank, Rarity.Starting,
                5000, 3.5f, 4f, 1.5f, 600, 800, 10, 5f,
                AttackType.Melee, SuperType.CrowdControl, roarUlt, projectilePrefab);

            CreateBrawler(path, "Ilya", "Илья Муромец", BrawlerRole.Sniper, Rarity.Starting,
                2600, 4.2f, 9f, 1.6f, 1100, 1400, 10, 10f,
                AttackType.SingleProjectile, SuperType.Damage, snipeUlt, projectilePrefab);

            CreateBrawler(path, "BabaYaga", "Баба Яга", BrawlerRole.Control, Rarity.Rare,
                2800, 4.3f, 7f, 1.4f, 750, 0, 15, 6f,
                AttackType.AoE, SuperType.CrowdControl, wallUlt, projectilePrefab);

            CreateBrawler(path, "ZmeyGorynych", "Змей Горыныч", BrawlerRole.DamageDealer, Rarity.Epic,
                3800, 4.0f, 5.5f, 1.3f, 500, 1200, 14, 6f,
                AttackType.Spread, SuperType.Damage, fireUlt, projectilePrefab);

            CreateBrawler(path, "Tugarin", "Тугарин Змей", BrawlerRole.Assassin, Rarity.Legendary,
                2400, 5.2f, 3.5f, 0.9f, 900, 1000, 16, 8f,
                AttackType.Melee, SuperType.Utility, smokeUlt, projectilePrefab);

            CreateBrawler(path, "Varvara", "Варвара-краса", BrawlerRole.Support, Rarity.Rare,
                2900, 4.4f, 6f, 1.1f, 600, 0, 18, 8f,
                AttackType.SingleProjectile, SuperType.Heal, healUlt, projectilePrefab);

            CreateBrawler(path, "Knyaz", "Князь Киевский", BrawlerRole.Support, Rarity.SuperRare,
                3200, 4.1f, 5f, 1.3f, 700, 0, 20, 7f,
                AttackType.SingleProjectile, SuperType.Buff, buffUlt, projectilePrefab);

            CreateBrawler(path, "Konyukh", "Конюх", BrawlerRole.Support, Rarity.Rare,
                3100, 4.2f, 5.5f, 1.2f, 650, 0, 16, 7f,
                AttackType.SingleProjectile, SuperType.Damage, stampedeUlt, projectilePrefab);

            CopyBrawlersToResources(path, resourcesPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("All brawler data, ultimates, and Resources/Brawlers copies generated!");
        }

        static void ApplyUltimateDefaults(params UltimateAbility[] ultimates)
        {
            foreach (var ability in ultimates)
            {
                if (ability == null) continue;

                switch (ability)
                {
                    case RoarUltimate roar:
                        roar.radius = 5f;
                        break;
                    case WallUltimate wall:
                        wall.distance = 4f;
                        wall.width = 2f;
                        wall.height = 0.5f;
                        break;
                    case FireBreathUltimate fire:
                        fire.range = 6f;
                        break;
                    case HealUltimate heal:
                        heal.radius = 5f;
                        break;
                    case BuffUltimate buff:
                        buff.radius = 5.5f;
                        break;
                }

                EditorUtility.SetDirty(ability);
            }
        }

        static void CopyBrawlersToResources(string sourcePath, string resourcesPath)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:BrawlerData", new[] { sourcePath }))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = System.IO.Path.GetFileName(assetPath);
                string destPath = resourcesPath + fileName;

                if (AssetDatabase.LoadAssetAtPath<BrawlerData>(destPath) != null)
                    AssetDatabase.DeleteAsset(destPath);

                AssetDatabase.CopyAsset(assetPath, destPath);
            }
        }

        static T CreateUltimate<T>(string path, string fileName, string displayName) where T : UltimateAbility
        {
            string fullPath = path + fileName + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            if (existing != null)
            {
                existing.abilityName = displayName;
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var asset = ScriptableObject.CreateInstance<T>();
            asset.abilityName = displayName;
            AssetDatabase.CreateAsset(asset, fullPath);
            return asset;
        }

        static void CreateBrawler(string path, string fileName, string displayName, BrawlerRole role, Rarity rarity,
            int baseHealth, float speed, float atkRange, float atkReload, int atkDmg,
            int superDmg, int superCharge, float superRange,
            AttackType attackType, SuperType superType, UltimateAbility ultimate, GameObject projectilePrefab)
        {
            string fullPath = path + fileName + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<BrawlerData>(fullPath);
            if (asset == null)
                asset = ScriptableObject.CreateInstance<BrawlerData>();

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
            asset.attackProjectilePrefab = projectilePrefab;
            asset.superProjectilePrefab = projectilePrefab;

            if (AssetDatabase.LoadAssetAtPath<BrawlerData>(fullPath) == null)
                AssetDatabase.CreateAsset(asset, fullPath);
            else
                EditorUtility.SetDirty(asset);
        }
    }
}
#endif
