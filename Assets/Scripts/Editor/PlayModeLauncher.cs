#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BogatyriMoba.Editor
{
    public static class PlayModeLauncher
    {
        [MenuItem("Bogatyri/Play Gameplay Scene")]
        public static void EnterPlayMode()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            // Save current scene if dirty
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                EditorSceneManager.SaveOpenScenes();
            }

            // Open Gameplay scene
            var scenePath = "Assets/Scenes/Gameplay.unity";
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
            Debug.Log("[PlayModeLauncher] Entering Play Mode with Gameplay scene.");
        }
    }
}
#endif
