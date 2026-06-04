#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using System.IO;
using System.Linq;

namespace BogatyriMoba.Editor
{
    /// <summary>
    /// Editor utility to bootstrap Addressables and mark key assets.
    /// Run via menu: Bogatyri / Setup Addressables
    /// </summary>
    public static class AddressableSetupEditor
    {
        [MenuItem("Bogatyri/Setup Addressables")]
        public static void Setup()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                settings = AddressableAssetSettings.Create(
                    AddressableAssetSettingsDefaultObject.kDefaultConfigFolder,
                    AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName,
                    true,
                    true);
                Debug.Log("[AddressableSetup] Created AddressableAssetSettings.");
            }

            // Create or get groups
            var brawlerGroup = GetOrCreateGroup(settings, "Brawlers");
            var uiGroup = GetOrCreateGroup(settings, "UI");
            var gemGroup = GetOrCreateGroup(settings, "Gems");

            // Mark BrawlerData assets in Resources/Brawlers
            string brawlersPath = "Assets/Resources/Brawlers";
            if (Directory.Exists(brawlersPath))
            {
                var guids = AssetDatabase.FindAssets("t:BrawlerData", new[] { brawlersPath });
                foreach (var guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    MarkAsAddressable(settings, guid, assetPath, brawlerGroup);
                }
                Debug.Log($"[AddressableSetup] Marked {guids.Length} BrawlerData assets as Addressable.");
            }

            // Mark UITheme
            string uiThemePath = "Assets/Resources/UI/DefaultUITheme.asset";
            if (File.Exists(uiThemePath))
            {
                string guid = AssetDatabase.AssetPathToGUID(uiThemePath);
                MarkAsAddressable(settings, guid, uiThemePath, uiGroup);
                Debug.Log("[AddressableSetup] Marked UITheme as Addressable.");
            }

            // Mark prefabs if found in Resources or Prefabs folder
            string prefabsPath = "Assets/Prefabs";
            if (Directory.Exists(prefabsPath))
            {
                var prefabGuids = AssetDatabase.FindAssets("t:GameObject", new[] { prefabsPath });
                foreach (var guid in prefabGuids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    string fileName = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
                    if (fileName.Contains("brawler"))
                    {
                        MarkAsAddressable(settings, guid, assetPath, brawlerGroup);
                    }
                    else if (fileName.Contains("gem"))
                    {
                        MarkAsAddressable(settings, guid, assetPath, gemGroup);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[AddressableSetup] Addressables setup complete. Build the content via Window > Asset Management > Addressables > Build.");
        }

        private static AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings, string groupName)
        {
            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                group = settings.CreateGroup(groupName, false, false, true, null);
                group.AddSchema<BundledAssetGroupSchema>();
                group.AddSchema<ContentUpdateGroupSchema>();
            }
            return group;
        }

        private static void MarkAsAddressable(AddressableAssetSettings settings, string guid, string assetPath, AddressableAssetGroup group)
        {
            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                entry = settings.CreateOrMoveEntry(guid, group);
            }
            // Use file name (without extension) as the key so it matches Resources.Load paths
            string key = Path.GetFileNameWithoutExtension(assetPath);
            entry.address = key;
            entry.SetLabel("default", true, true);
        }
    }
}
#endif
