#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using BogatyriMoba.Core;
using BogatyriMoba.Core.Ultimates;
using BogatyriMoba.GameModes;
using BogatyriMoba.UI;
using UnityEngine.EventSystems;

namespace BogatyriMoba.EditorTools
{
    public class ProjectSetupWizard : EditorWindow
    {
        [UnityEditor.InitializeOnLoadMethod]
        static void TryAutoStartFromCLI()
        {
            // Skip in batch mode to prevent OOM crashes during headless compilation
            if (UnityEngine.Application.isBatchMode) return;

            Debug.Log("[AutoStart] TryAutoStartFromCLI invoked.");
            string scenePath = "Assets/Scenes/Gameplay.unity";
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.Log("[AutoStart] No Gameplay scene found - auto running full setup + play.");
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        Debug.Log("[AutoStart] Invoking FullSetupAndPlay from delayCall.");
                        FullSetupAndPlay();
                    }
                };
            }
            else
            {
                Debug.Log("[AutoStart] Gameplay scene already present.");
            }
        }

        [MenuItem("Bogatyri/Setup Project (Full Setup)")]
        static void ShowWindow()
        {
            GetWindow<ProjectSetupWizard>("Setup Bogatyri MOBA");
        }

        private void OnGUI()
        {
            GUILayout.Label("Богатыри MOBA — Мастер настройки", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("Этот wizard создаст всё необходимое для запуска:", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• Папки проекта\n• ScriptableObject'ы бойцов и ультимейтов\n• Префабы (Brawler, Gem, Projectile)\n• Сцены Gameplay и Heist\n• Теги и слои", EditorStyles.wordWrappedLabel);
            GUILayout.Space(20);

            if (GUILayout.Button("1. Создать папки", GUILayout.Height(30)))
                CreateFolders();

            if (GUILayout.Button("2. Сгенерировать бойцов и ультимейты", GUILayout.Height(30)))
                BrawlerDataFactory.GenerateAll();

            if (GUILayout.Button("3. Создать префабы", GUILayout.Height(30)))
                CreatePrefabs();

            if (GUILayout.Button("4. Создать сцену Gameplay", GUILayout.Height(30)))
                CreateGameplayScene();

            if (GUILayout.Button("5. Создать сцену Heist", GUILayout.Height(30)))
                CreateHeistScene();

            if (GUILayout.Button("6. Настроить теги и слои", GUILayout.Height(30)))
                SetupTagsAndLayers();

            GUILayout.Space(20);
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("ЗАПУСТИТЬ ПОЛНУЮ НАСТРОЙКУ И ИГРУ", GUILayout.Height(40)))
                FullSetupAndPlay();
            GUI.backgroundColor = Color.white;
        }

        private static void RunFullSetup()
        {
            CreateFolders();
            SetupTagsAndLayers(); // must be early: CreateGameplay/Heist + CreateWall set tags/layers that we register here
            CreatePrefabs();
            BrawlerDataFactory.GenerateAll();
            CreateGameplayScene();
            CreateHeistScene();

            EditorUtility.DisplayDialog("Готово!",
                "Проект полностью настроен.\n\nСейчас откроется сцена и запустится Play Mode.\nМатч Gem Grab начнётся автоматически (Алёша + боты).",
                "OK");
        }

        private static void CreateFolders()
        {
            string[] folders =
            {
                "Assets/Prefabs",
                "Assets/Prefabs/UI",
                "Assets/Scenes",
                "Assets/Resources",
                "Assets/Resources/UI",
                "Assets/Resources/Brawlers",
                "Assets/Materials",
                "Assets/Animations",
                "Assets/Sprites",
                "Assets/Audio",
                "Assets/ScriptableObjects/Brawlers",
                "Assets/ScriptableObjects/Ultimates"
            };

            foreach (var folder in folders)
            {
                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);
            }
            AssetDatabase.Refresh();
            Debug.Log("[Setup] Папки созданы.");
        }

        private static void CreatePrefabs()
        {
            GameObject brawlerGO = new GameObject("Brawler");

            var sr = brawlerGO.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            sr.color = Color.white;

            var rb = brawlerGO.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            var col = brawlerGO.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            brawlerGO.AddComponent<BrawlerController>();
            brawlerGO.AddComponent<PlayerInput>();

            GameObject visual = new GameObject("VisualContainer");
            visual.transform.SetParent(brawlerGO.transform);
            visual.transform.localPosition = Vector3.zero;

            GameObject spawnPoint = new GameObject("ProjectileSpawnPoint");
            spawnPoint.transform.SetParent(visual.transform);
            spawnPoint.transform.localPosition = new Vector3(0.6f, 0f, 0f);

            PrefabUtility.SaveAsPrefabAsset(brawlerGO, "Assets/Prefabs/Brawler.prefab");
            DestroyImmediate(brawlerGO);

            GameObject gemGO = new GameObject("Gem");
            var gemSr = gemGO.AddComponent<SpriteRenderer>();
            gemSr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            gemSr.color = Color.green;

            var gemCol = gemGO.AddComponent<CircleCollider2D>();
            gemCol.radius = 0.3f;
            gemCol.isTrigger = true;

            gemGO.AddComponent<Gem>();
            PrefabUtility.SaveAsPrefabAsset(gemGO, "Assets/Prefabs/Gem.prefab");
            DestroyImmediate(gemGO);

            GameObject projectileGO = new GameObject("Projectile");
            var projSr = projectileGO.AddComponent<SpriteRenderer>();
            projSr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            projSr.color = Color.yellow;
            projSr.transform.localScale = Vector3.one * 0.3f;

            var projRb = projectileGO.AddComponent<Rigidbody2D>();
            projRb.gravityScale = 0f;
            projRb.bodyType = RigidbodyType2D.Kinematic;

            var projCol = projectileGO.AddComponent<CircleCollider2D>();
            projCol.radius = 0.15f;
            projCol.isTrigger = true;

            projectileGO.AddComponent<Projectile>();
            PrefabUtility.SaveAsPrefabAsset(projectileGO, "Assets/Prefabs/Projectile.prefab");
            DestroyImmediate(projectileGO);

            GameObject horseGO = new GameObject("HorseProjectile");
            var horseSr = horseGO.AddComponent<SpriteRenderer>();
            horseSr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            horseSr.color = new Color(1f, 0.6f, 0.2f);
            horseSr.transform.localScale = new Vector3(1.2f, 0.6f, 1f);

            var horseRb = horseGO.AddComponent<Rigidbody2D>();
            horseRb.gravityScale = 0f;
            horseRb.freezeRotation = true;

            var horseCol = horseGO.AddComponent<BoxCollider2D>();
            horseCol.isTrigger = true;
            horseCol.size = new Vector2(1f, 0.5f);

            horseGO.AddComponent<HorseProjectile>();
            PrefabUtility.SaveAsPrefabAsset(horseGO, "Assets/Prefabs/HorseProjectile.prefab");
            DestroyImmediate(horseGO);

            GameObject wallGO = new GameObject("Wall");
            wallGO.layer = LayerMask.NameToLayer("Default");

            var wallSr = wallGO.AddComponent<SpriteRenderer>();
            wallSr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            wallSr.color = new Color(0.3f, 0.4f, 0.3f);
            wallSr.drawMode = SpriteDrawMode.Sliced;
            wallSr.size = new Vector2(2f, 0.5f);

            var wallCol = wallGO.AddComponent<BoxCollider2D>();
            wallCol.size = new Vector2(2f, 0.5f);

            wallGO.AddComponent<WallObject>();
            PrefabUtility.SaveAsPrefabAsset(wallGO, "Assets/Prefabs/Wall.prefab");
            DestroyImmediate(wallGO);

            GameObject safeGO = new GameObject("Safe");
            var safeSr = safeGO.AddComponent<SpriteRenderer>();
            safeSr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            safeSr.color = new Color(0.5f, 0.5f, 0.7f);
            safeSr.drawMode = SpriteDrawMode.Sliced;
            safeSr.size = new Vector2(2f, 2f);

            var safeCol = safeGO.AddComponent<BoxCollider2D>();
            safeCol.size = new Vector2(2f, 2f);
            safeCol.isTrigger = true;

            safeGO.AddComponent<Safe>();
            PrefabUtility.SaveAsPrefabAsset(safeGO, "Assets/Prefabs/Safe.prefab");
            DestroyImmediate(safeGO);

            AssetDatabase.Refresh();
            Debug.Log("[Setup] Префабы созданы: Brawler, Gem, Projectile, HorseProjectile, Wall, Safe.");
        }

        private static void CreateGameplayScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Gameplay";

            var cam = CreateMainCameraForScene(new Color(0.23f, 0.37f, 0.23f));
            var camFollow = cam.gameObject.AddComponent<CameraFollow>();

            GameObject gmGO = new GameObject("GameManager");
            var gm = gmGO.AddComponent<GameManager>();
            gm.cameraFollow = camFollow;
            gmGO.AddComponent<MatchBootstrap>();

            GameObject modeGO = new GameObject("GemGrabMode");
            var mode = modeGO.AddComponent<GemGrabMode>();
            gm.currentGameMode = mode;

            // New managers
            var matchManager = gmGO.AddComponent<MatchManager>();
            matchManager.currentGameMode = mode;

            var spawnManager = gmGO.AddComponent<SpawnManager>();
            var networkManager = gmGO.AddComponent<NetworkManager>();
            var poolManager = gmGO.AddComponent<PoolManager>();
            var perfManager = gmGO.AddComponent<PerformanceManager>();

            GameObject blueSpawns = new GameObject("TeamBlueSpawns");
            for (int i = 0; i < 3; i++)
            {
                GameObject sp = new GameObject($"BlueSpawn_{i}");
                sp.transform.SetParent(blueSpawns.transform);
                sp.transform.position = new Vector3(-12f + i * 2f, -4f + i * 4f, 0f);
            }

            GameObject redSpawns = new GameObject("TeamRedSpawns");
            for (int i = 0; i < 3; i++)
            {
                GameObject sp = new GameObject($"RedSpawn_{i}");
                sp.transform.SetParent(redSpawns.transform);
                sp.transform.position = new Vector3(12f - i * 2f, -4f + i * 4f, 0f);
            }

            spawnManager.team1SpawnPoints = new Transform[3];
            spawnManager.team2SpawnPoints = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                spawnManager.team1SpawnPoints[i] = blueSpawns.transform.GetChild(i);
                spawnManager.team2SpawnPoints[i] = redSpawns.transform.GetChild(i);
            }

            GameObject gemParent = new GameObject("Gems");
            spawnManager.gemSpawnParent = gemParent.transform;

            var mineSr = gemParent.AddComponent<SpriteRenderer>();
            mineSr.sortingOrder = -1;
            mineSr.transform.localScale = Vector3.one * 1.5f;
            string minePath = "Assets/Sprites/GemMine.png";
            if (System.IO.File.Exists(minePath))
            {
                var importer = AssetImporter.GetAtPath(minePath) as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                }
                mineSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(minePath);
            }

            GameObject obstacles = new GameObject("Obstacles");
            CreateWall(obstacles.transform, new Vector3(0f, 6f, 0f), new Vector3(4f, 0.5f, 1f), true); // rock
            CreateWall(obstacles.transform, new Vector3(0f, -6f, 0f), new Vector3(4f, 0.5f, 1f), true); // rock
            CreateWall(obstacles.transform, new Vector3(-5f, 0f, 0f), new Vector3(1f, 3f, 1f), false); // tree
            CreateWall(obstacles.transform, new Vector3(5f, 0f, 0f), new Vector3(1f, 3f, 1f), false); // tree

            AssignGameManagerPrefabs(gm);

            EnsureEventSystem();
            UiHudBuilder.BuildMatchUi(gm, mode);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Gameplay.unity");
            Debug.Log("[Setup] Сцена Gameplay создана.");
        }

        private static void CreateHeistScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Heist";

            var cam = CreateMainCameraForScene(new Color(0.2f, 0.25f, 0.35f));
            var camFollow = cam.gameObject.AddComponent<CameraFollow>();

            GameObject gmGO = new GameObject("GameManager");
            var gm = gmGO.AddComponent<GameManager>();
            gm.cameraFollow = camFollow;
            gmGO.AddComponent<MatchBootstrap>();

            GameObject modeGO = new GameObject("HeistMode");
            var mode = modeGO.AddComponent<HeistMode>();
            mode.modeType = GameModeType.Heist;
            gm.currentGameMode = mode;

            // New managers
            var matchManager = gmGO.AddComponent<MatchManager>();
            matchManager.currentGameMode = mode;

            var spawnManager = gmGO.AddComponent<SpawnManager>();
            var networkManager = gmGO.AddComponent<NetworkManager>();
            var poolManager = gmGO.AddComponent<PoolManager>();
            var perfManager = gmGO.AddComponent<PerformanceManager>();

            if (System.IO.File.Exists("Assets/Prefabs/Safe.prefab"))
                mode.safePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Safe.prefab");

            GameObject blueSpawns = new GameObject("TeamBlueSpawns");
            for (int i = 0; i < 3; i++)
            {
                GameObject sp = new GameObject($"BlueSpawn_{i}");
                sp.transform.SetParent(blueSpawns.transform);
                sp.transform.position = new Vector3(-10f + i * 2f, -3f + i * 3f, 0f);
            }

            GameObject redSpawns = new GameObject("TeamRedSpawns");
            for (int i = 0; i < 3; i++)
            {
                GameObject sp = new GameObject($"RedSpawn_{i}");
                sp.transform.SetParent(redSpawns.transform);
                sp.transform.position = new Vector3(10f - i * 2f, -3f + i * 3f, 0f);
            }

            spawnManager.team1SpawnPoints = new Transform[3];
            spawnManager.team2SpawnPoints = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                spawnManager.team1SpawnPoints[i] = blueSpawns.transform.GetChild(i);
                spawnManager.team2SpawnPoints[i] = redSpawns.transform.GetChild(i);
            }

            GameObject blueSafePos = new GameObject("BlueSafePosition");
            blueSafePos.transform.position = new Vector3(-14f, 0f, 0f);
            mode.blueSafePosition = blueSafePos.transform;

            GameObject redSafePos = new GameObject("RedSafePosition");
            redSafePos.transform.position = new Vector3(14f, 0f, 0f);
            mode.redSafePosition = redSafePos.transform;

            GameObject gemParent = new GameObject("Gems");
            spawnManager.gemSpawnParent = gemParent.transform;

            GameObject obstacles = new GameObject("Obstacles");
            CreateWall(obstacles.transform, new Vector3(0f, 5f, 0f), new Vector3(6f, 0.5f, 1f), true); // rock
            CreateWall(obstacles.transform, new Vector3(0f, -5f, 0f), new Vector3(6f, 0.5f, 1f), false); // tree

            AssignGameManagerPrefabs(gm);

            EnsureEventSystem();
            UiHudBuilder.BuildMatchUi(gm, mode);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Heist.unity");
            Debug.Log("[Setup] Сцена Heist создана.");
        }

        private static void AssignGameManagerPrefabs(GameManager gm)
        {
            var spawnManager = gm.GetComponent<SpawnManager>();
            if (spawnManager == null) spawnManager = gm.gameObject.AddComponent<SpawnManager>();

            if (System.IO.File.Exists("Assets/Prefabs/Brawler.prefab"))
                spawnManager.brawlerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Brawler.prefab");
            if (System.IO.File.Exists("Assets/Prefabs/Gem.prefab"))
                spawnManager.gemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gem.prefab");
        }

        private static void CreateWall(Transform parent, Vector3 position, Vector3 scale, bool isRock = true)
        {
            GameObject wall = new GameObject("Wall");
            wall.transform.SetParent(parent);
            wall.transform.position = position;
            wall.transform.localScale = scale;

            var sr = wall.AddComponent<SpriteRenderer>();
            string spriteName = isRock ? "RockObstacle" : "TreeObstacle";
            string spritePath = "Assets/Sprites/" + spriteName + ".png";

            if (System.IO.File.Exists(spritePath))
            {
                var importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                }
                sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            }
            else
            {
                sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
                sr.color = new Color(0.3f, 0.4f, 0.3f);
            }

            var col = wall.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);
            wall.layer = LayerMask.NameToLayer("Obstacles");
            wall.tag = "Obstacle";
        }

        /// <summary>
        /// Creates a proper Main Camera for the new empty scene (avoids NRE on Camera.main in EmptyScene).
        /// </summary>
        private static Camera CreateMainCameraForScene(Color backgroundColor)
        {
            // Remove any accidental previous main camera
            var oldCam = Camera.main;
            if (oldCam != null)
            {
                DestroyImmediate(oldCam.gameObject);
            }

            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";

            var cam = camGO.AddComponent<Camera>();
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.orthographic = true;
            cam.orthographicSize = 8f;
            cam.backgroundColor = backgroundColor;
            cam.clearFlags = CameraClearFlags.SolidColor;

            return cam;
        }

        /// <summary>
        /// Ensures an EventSystem exists in the scene so UI Buttons (GameOver, etc.) and raycasts work.
        /// </summary>
        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
            }
        }

        private static void SetupTagsAndLayers()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");
            SerializedProperty layers = tagManager.FindProperty("layers");

            // Ensure "Obstacle" tag exists (used by projectiles for early destroy)
            bool hasObstacleTag = false;
            for (int i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == "Obstacle")
                {
                    hasObstacleTag = true;
                    break;
                }
            }
            if (!hasObstacleTag)
            {
                tags.InsertArrayElementAtIndex(tags.arraySize);
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = "Obstacle";
                Debug.Log("[Setup] Тег Obstacle добавлен.");
            }

            bool hasObstacles = false;
            for (int i = 8; i < 32; i++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                if (layer.stringValue == "Obstacles")
                {
                    hasObstacles = true;
                    break;
                }
            }

            if (!hasObstacles)
            {
                for (int i = 8; i < 32; i++)
                {
                    SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(layer.stringValue))
                    {
                        layer.stringValue = "Obstacles";
                        break;
                    }
                }
                Debug.Log("[Setup] Слой Obstacles добавлен.");
            }

            tagManager.ApplyModifiedProperties();
        }

        [MenuItem("Bogatyri/Play Gameplay (Auto Setup + Play)")]
        public static void FullSetupAndPlay()
        {
            RunFullSetup();
            string scenePath = "Assets/Scenes/Gameplay.unity";
            if (System.IO.File.Exists(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath);
                EditorApplication.delayCall += () =>
                {
                    if (!EditorApplication.isPlaying)
                    {
                        EditorApplication.EnterPlaymode();
                        Debug.Log("[Setup] Entered play mode for Gameplay scene.");
                    }
                };
                Debug.Log("[Setup] Gameplay scene opened. Entering play mode...");
            }
            else
            {
                Debug.LogError("[Setup] Gameplay scene not found after setup.");
            }
        }
    }
}
#endif
