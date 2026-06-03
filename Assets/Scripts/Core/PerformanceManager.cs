using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace BogatyriMoba.Core
{
    /// <summary>
    /// Central performance and optimization manager.
    /// Handles quality settings, frame rate limiting, and dynamic LOD.
    /// </summary>
    public class PerformanceManager : MonoBehaviour
    {
        public static PerformanceManager Instance { get; private set; }

        [Header("Target Frame Rate")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private int mobileFrameRate = 30;

        [Header("Quality Tiers")]
        [SerializeField] private QualityTier lowTier;
        [SerializeField] private QualityTier mediumTier;
        [SerializeField] private QualityTier highTier;

        [Header("Dynamic Adjustment")]
        [SerializeField] private bool enableDynamicAdjustment = true;
        [SerializeField] private float adjustmentInterval = 5f;
        [SerializeField] private int fpsThresholdLow = 25;
        [SerializeField] private int fpsThresholdHigh = 55;

        private float _fpsTimer;
        private int _frameCount;
        private float _currentFps;
        private QualityTier _currentTier;
        private readonly Queue<float> _fpsHistory = new Queue<float>();
        private const int FpsHistorySize = 10;

        [System.Serializable]
        public struct QualityTier
        {
            public string name;
            public int targetFrameRate;
            public bool enableShadows;
            public bool enablePostProcessing;
            public int particleMultiplier;
            public float lodDistance;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ApplyInitialSettings();
        }

        private void ApplyInitialSettings()
        {
            bool isMobile = Application.isMobilePlatform;
            int target = isMobile ? mobileFrameRate : targetFrameRate;
            Application.targetFrameRate = target;

            // Start with medium tier, then adjust
            ApplyTier(mediumTier);
        }

        private void Update()
        {
            if (!enableDynamicAdjustment) return;

            _frameCount++;
            _fpsTimer += Time.unscaledDeltaTime;

            if (_fpsTimer >= 1f)
            {
                _currentFps = _frameCount / _fpsTimer;
                _frameCount = 0;
                _fpsTimer = 0f;

                _fpsHistory.Enqueue(_currentFps);
                if (_fpsHistory.Count > FpsHistorySize)
                    _fpsHistory.Dequeue();

                EvaluateQualityAdjustment();
            }
        }

        private void EvaluateQualityAdjustment()
        {
            if (_fpsHistory.Count < FpsHistorySize) return;

            float avgFps = 0f;
            foreach (var fps in _fpsHistory)
                avgFps += fps;
            avgFps /= _fpsHistory.Count;

            if (avgFps < fpsThresholdLow && _currentTier.name != lowTier.name)
            {
                Debug.Log($"[PerformanceManager] FPS low ({avgFps:F1}), downgrading to {lowTier.name}");
                ApplyTier(lowTier);
            }
            else if (avgFps > fpsThresholdHigh && _currentTier.name != highTier.name)
            {
                Debug.Log($"[PerformanceManager] FPS high ({avgFps:F1}), upgrading to {highTier.name}");
                ApplyTier(highTier);
            }
        }

        public void ApplyTier(QualityTier tier)
        {
            _currentTier = tier;
            Application.targetFrameRate = tier.targetFrameRate;

            // Shadows
            QualitySettings.shadows = tier.enableShadows ? ShadowQuality.HardOnly : ShadowQuality.Disable;

            // Post-processing (requires URP/HDRP setup)
            // Would toggle post-processing volumes here

            Debug.Log($"[PerformanceManager] Applied tier: {tier.name}");
        }

        public void SetQualityLevel(int level)
        {
            switch (level)
            {
                case 0: ApplyTier(lowTier); break;
                case 1: ApplyTier(mediumTier); break;
                case 2: ApplyTier(highTier); break;
            }
        }

        public float GetCurrentFps() => _currentFps;
    }
}
