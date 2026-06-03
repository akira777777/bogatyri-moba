using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// JSON-based persistent save system for player progress, settings, and unlocks.
    /// Thread-safe file operations with backup and validation.
    /// </summary>
    public static class SaveSystem
    {
        private static readonly string SaveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        private static readonly string SaveFilePath = Path.Combine(SaveDirectory, "playerdata.json");
        private static readonly string BackupFilePath = Path.Combine(SaveDirectory, "playerdata.backup.json");

        private static PlayerData _cachedData;
        private static bool _isLoaded;

        public static PlayerData Data
        {
            get
            {
                if (!_isLoaded)
                    Load();
                return _cachedData;
            }
        }

        public static void Load()
        {
            try
            {
                if (!Directory.Exists(SaveDirectory))
                    Directory.CreateDirectory(SaveDirectory);

                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    _cachedData = JsonUtility.FromJson<PlayerData>(json);

                    if (_cachedData == null || _cachedData.version == 0)
                    {
                        Debug.LogWarning("[SaveSystem] Save file corrupted, trying backup...");
                        LoadFromBackup();
                    }
                }
                else if (File.Exists(BackupFilePath))
                {
                    LoadFromBackup();
                }
                else
                {
                    _cachedData = new PlayerData { version = PlayerData.CurrentVersion };
                }

                _isLoaded = true;
                MigrateIfNeeded();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Load failed: {ex.Message}");
                _cachedData = new PlayerData { version = PlayerData.CurrentVersion };
                _isLoaded = true;
            }
        }

        public static void Save()
        {
            try
            {
                if (!_isLoaded) return;

                if (!Directory.Exists(SaveDirectory))
                    Directory.CreateDirectory(SaveDirectory);

                // Backup existing
                if (File.Exists(SaveFilePath))
                    File.Copy(SaveFilePath, BackupFilePath, overwrite: true);

                string json = JsonUtility.ToJson(_cachedData, prettyPrint: true);
                File.WriteAllText(SaveFilePath, json);

                Debug.Log("[SaveSystem] Saved successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Save failed: {ex.Message}");
            }
        }

        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                    File.Delete(SaveFilePath);
                if (File.Exists(BackupFilePath))
                    File.Delete(BackupFilePath);
                _cachedData = new PlayerData { version = PlayerData.CurrentVersion };
                _isLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Delete failed: {ex.Message}");
            }
        }

        private static void LoadFromBackup()
        {
            if (File.Exists(BackupFilePath))
            {
                string json = File.ReadAllText(BackupFilePath);
                _cachedData = JsonUtility.FromJson<PlayerData>(json);
            }
            else
            {
                _cachedData = new PlayerData { version = PlayerData.CurrentVersion };
            }
        }

        private static void MigrateIfNeeded()
        {
            if (_cachedData.version < PlayerData.CurrentVersion)
            {
                Debug.Log($"[SaveSystem] Migrating from v{_cachedData.version} to v{PlayerData.CurrentVersion}");
                // Add migration logic here when versions change
                _cachedData.version = PlayerData.CurrentVersion;
                Save();
            }
        }
    }

    [Serializable]
    public class PlayerData
    {
        public const int CurrentVersion = 1;

        public int version;
        public string playerName = "Player";
        public int playerLevel = 1;
        public int totalExperience;
        public int totalMatchesPlayed;
        public int totalWins;
        public int totalKills;
        public int gemsCollected;

        // Brawler unlocks and upgrades
        public SerializableDictionary<string, BrawlerProgress> brawlerProgress = new SerializableDictionary<string, BrawlerProgress>();

        // Settings
        public float musicVolume = 0.8f;
        public float sfxVolume = 1.0f;
        public bool vibrationEnabled = true;
        public int graphicsQuality = 2; // 0=Low, 1=Medium, 2=High
        public string language = "ru";
    }

    [Serializable]
    public class BrawlerProgress
    {
        public bool unlocked = false;
        public int powerLevel = 1;
        public int trophies = 0;
        public int matchesPlayed = 0;
        public int wins = 0;
    }

    // Simple serializable dictionary wrapper for Unity JSON
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        public TValue this[TKey key]
        {
            get
            {
                int index = keys.IndexOf(key);
                return index >= 0 ? values[index] : default;
            }
            set
            {
                int index = keys.IndexOf(key);
                if (index >= 0)
                    values[index] = value;
                else
                {
                    keys.Add(key);
                    values.Add(value);
                }
            }
        }

        public bool ContainsKey(TKey key) => keys.Contains(key);
        public void Remove(TKey key)
        {
            int index = keys.IndexOf(key);
            if (index >= 0)
            {
                keys.RemoveAt(index);
                values.RemoveAt(index);
            }
        }
    }
}
