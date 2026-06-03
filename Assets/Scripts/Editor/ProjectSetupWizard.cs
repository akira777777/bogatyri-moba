#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using BogatyriMoba.Core;
using BogatyriMoba.Core.Ultimates;
using BogatyriMoba.GameModes;
using BogatyriMoba.UI;

namespace BogatyriMoba.EditorTools
{
    public class ProjectSetupWizard : EditorWindow
    {
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
            GUILayout.Label("• Папки проекта\n• ScriptableObject'ы бойцов и ультимейтов\n• Префабы (Brawler, Gem)\n• Сцену Gameplay\n• Теги и слои", EditorStyles.wordWrappedLabel);
            GUILayout.Space(20);

            if (GUILayout.Button("1. Создать папки", GUILayout.Height(30)))
            {
                CreateFolders();
            }

            if (GUILayout.Button("2. Сгенерировать бойцов и ультимейты", GUILayout.Height(30)))
            {
                BrawlerDataFactory.GenerateAll();
            }

            if (GUILayout.Button("3. Создать префабы", GUILayout.Height(30)))
            {
                CreatePrefabs();
            }

            if (GUILayout.Button("4. Создать сцену Gameplay", GUILayout.Height(30)))
            {
                CreateGameplayScene();
            }

            if (GUILayout.Button("5. Настроить теги и слои", GUILayout.Height(30)))
            {
                SetupTagsAndLayers();
            }

            GUILayout.Space(20);
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("ЗАПУСТИТЬ ПОЛНУЮ НАСТРОЙКУ", GUILayout.Height(40)))
            {
                RunFullSetup();
            }
            GUI.backgroundColor = Color.white;
        }

        private static void RunFullSetup()
        {
            CreateFolders();
            BrawlerDataFactory.GenerateAll();
            CreatePrefabs();
            CreateGameplayScene();
            SetupTagsAndLayers();
            
            EditorUtility.DisplayDialog("Готово!", 
                "Проект полностью настроен.\n\nСледующие шаги:\n1. Установите Unity 2022.3 LTS\n2. Откройте сцену Assets/Scenes/Gameplay.unity\n3. Нажмите Play!", 
                "OK");
        }

        private static void CreateFolders()
        {
            string[] folders = new[]
            {
                "Assets/Prefabs",
                "Assets/Scenes",
                "Assets/Resources",
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
                {
                    System.IO.Directory.CreateDirectory(folder);
                }
            }
            AssetDatabase.Refresh();
            Debug.Log("[Setup] Папки созданы.");
        }

        private static void CreatePrefabs()
        {
            // Create Brawler prefab
            GameObject brawlerGO = new GameObject("Brawler");
            brawlerGO.layer = LayerMask.NameToLayer("Default");
            
            var sr = brawlerGO.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"); // placeholder
            sr.color = Color.white;
            
            var rb = brawlerGO.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            
            var col = brawlerGO.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            
            brawlerGO.AddComponent<BrawlerController>();
            brawlerGO.AddComponent<PlayerInput>();
            
            // Projectile spawn point
            GameObject spawnPoint = new GameObject("ProjectileSpawnPoint");
            spawnPoint.transform.SetParent(brawlerGO.transform);
            spawnPoint.transform.localPosition = new Vector3(0.6f, 0f, 0f);

            // Save prefab
            string brawlerPath = "Assets/Prefabs/Brawler.prefab";
            PrefabUtility.SaveAsPrefabAsset(brawlerGO, brawlerPath);
            DestroyImmediate(brawlerGO);

            // Create Gem prefab
            GameObject gemGO = new GameObject("Gem");
            gemGO.layer = LayerMask.NameToLayer("Default");
            
            var gemSr = gemGO.AddComponent<SpriteRenderer>();
            gemSr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"); // placeholder circle
            gemSr.color = Color.green;
            
            var gemCol = gemGO.AddComponent<CircleCollider2D>();
            gemCol.radius = 0.3f;
            gemCol.isTrigger = true;
            
            gemGO.AddComponent<Gem>();

            string gemPath = "Assets/Prefabs/Gem.prefab";
            PrefabUtility.SaveAsPrefabAsset(gemGO, gemPath);
            DestroyImmediate(gemGO);

            // Create simple obstacle prefab
            GameObject wallGO = new GameObject("Wall");
            wallGO.layer = LayerMask.NameToLayer("Obstacles");
            
            var wallSr = wallGO.AddComponent<SpriteRenderer>();
            wallSr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            wallSr.color = new Color(0.3f, 0.4f, 0.3f);
            wallSr.drawMode = SpriteDrawMode.Sliced;
            wallSr.size = new Vector2(2f, 2f);
            
            var wallCol = wallGO.AddComponent<BoxCollider2D>();
            wallCol.size = new Vector2(2f, 2f);
            
            string wallPath = "Assets/Prefabs/Wall.prefab";
            PrefabUtility.SaveAsPrefabAsset(wallGO, wallPath);
            DestroyImmediate(wallGO);

            AssetDatabase.Refresh();
            Debug.Log("[Setup] Префабы созданы: Brawler, Gem, Wall.");
        }

        private static void CreateGameplayScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Gameplay";

            // Main Camera
            Camera.main.transform.position = new Vector3(0f, 0f, -10f);
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 8f;
            Camera.main.backgroundColor = new Color(0.23f, 0.37f, 0.23f); // dark green

            // Add CameraFollow
            var camFollow = Camera.main.gameObject.AddComponent<CameraFollow>();

            // GameManager
            GameObject gmGO = new GameObject("GameManager");
            var gm = gmGO.AddComponent<GameManager>();
            gm.cameraFollow = camFollow;

            // GemGrabMode
            GameObject modeGO = new GameObject("GemGrabMode");
            var mode = modeGO.AddComponent<GemGrabMode>();

            // Link them
            gm.currentGameMode = mode;

            // Spawn points - Team Blue (left side)
            GameObject blueSpawns = new GameObject("TeamBlueSpawns");
            for (int i = 0; i < 3; i++)
            {
                GameObject sp = new GameObject($"BlueSpawn_{i}");
                sp.transform.SetParent(blueSpawns.transform);
                sp.transform.position = new Vector3(-12f + i * 2f, -4f + i * 4f, 0f);
            }

            // Spawn points - Team Red (right side)
            GameObject redSpawns = new GameObject("TeamRedSpawns");
            for (int i = 0; i < 3; i++)
            {
                GameObject sp = new GameObject($"RedSpawn_{i}");
                sp.transform.SetParent(redSpawns.transform);
                sp.transform.position = new Vector3(12f - i * 2f, -4f + i * 4f, 0f);
            }

            // Assign spawn points to GameManager
            gm.team1SpawnPoints = new Transform[3];
            gm.team2SpawnPoints = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                gm.team1SpawnPoints[i] = blueSpawns.transform.GetChild(i);
                gm.team2SpawnPoints[i] = redSpawns.transform.GetChild(i);
            }

            // Gem spawn parent
            GameObject gemParent = new GameObject("Gems");
            gm.gemSpawnParent = gemParent.transform;

            // Create some walls/obstacles for cover
            GameObject obstacles = new GameObject("Obstacles");
            CreateWall(obstacles.transform, new Vector3(0f, 6f, 0f), new Vector3(4f, 0.5f, 1f));
            CreateWall(obstacles.transform, new Vector3(0f, -6f, 0f), new Vector3(4f, 0.5f, 1f));
            CreateWall(obstacles.transform, new Vector3(-5f, 0f, 0f), new Vector3(1f, 3f, 1f));
            CreateWall(obstacles.transform, new Vector3(5f, 0f, 0f), new Vector3(1f, 3f, 1f));

            // Assign prefabs (if they exist)
            string brawlerPrefabPath = "Assets/Prefabs/Brawler.prefab";
            string gemPrefabPath = "Assets/Prefabs/Gem.prefab";
            
            if (System.IO.File.Exists(brawlerPrefabPath))
                gm.brawlerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(brawlerPrefabPath);
            if (System.IO.File.Exists(gemPrefabPath))
                gm.gemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gemPrefabPath);

            // Canvas for UI
            GameObject canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // HUD
            GameObject hudGO = new GameObject("HUD");
            hudGO.transform.SetParent(canvasGO.transform);
            var hudRect = hudGO.AddComponent<RectTransform>();
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.offsetMin = Vector2.zero;
            hudRect.offsetMax = Vector2.zero;

            // Timer text
            GameObject timerGO = new GameObject("TimerText");
            timerGO.transform.SetParent(hudGO.transform);
            var timerRect = timerGO.AddComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.5f, 1f);
            timerRect.anchorMax = new Vector2(0.5f, 1f);
            timerRect.anchoredPosition = new Vector2(0f, -30f);
            timerRect.sizeDelta = new Vector2(200f, 50f);
            var timerText = timerGO.AddComponent<UnityEngine.UI.Text>();
            timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            timerText.fontSize = 32;
            timerText.alignment = TextAnchor.MiddleCenter;
            timerText.color = Color.yellow;
            timerText.text = "2:30";
            
            var timerUI = timerGO.AddComponent<MatchTimerUI>();
            timerUI.SetGameMode(mode);

            // Save scene
            string scenePath = "Assets/Scenes/Gameplay.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            
            Debug.Log("[Setup] Сцена Gameplay создана.");
        }

        private static void CreateWall(Transform parent, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Quad);
            wall.name = "Wall";
            wall.transform.SetParent(parent);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            
            var renderer = wall.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            renderer.sharedMaterial.color = new Color(0.3f, 0.4f, 0.3f);
            
            DestroyImmediate(wall.GetComponent<Collider>());
            var col = wall.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);
            wall.layer = LayerMask.NameToLayer("Obstacles");
        }

        private static void SetupTagsAndLayers()
        {
            // Add Obstacles layer if not exists
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

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
                tagManager.ApplyModifiedProperties();
                Debug.Log("[Setup] Слой Obstacles добавлен.");
            }
        }
    }
}
#endif
