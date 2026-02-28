using UnityEditor;
using UnityEngine;
using Gameplay.SceneManagement;

namespace GameEditor
{
    public static class DebugTools
    {
        [MenuItem("Tools/Debug/Reset Level Progress")]
        public static void ResetLevelProgress()
        {
            LevelProgressManager.ResetProgress();
            Debug.Log("[DebugTools] Level progress reset. All levels except Level 1 are now locked.");
        }

        [MenuItem("Tools/Debug/Reset Tutorial Hints")]
        public static void ResetTutorialHints()
        {
            string[] hintKeys = { "Jump", "Mask" };
            foreach (var key in hintKeys)
                PlayerPrefs.DeleteKey("TutorialHint_" + key);

            string[] cinematicKeys = { "Level1Cinematic" };
            foreach (var key in cinematicKeys)
                PlayerPrefs.DeleteKey("CinematicSeen_" + key);

            PlayerPrefs.Save();
            Debug.Log("[DebugTools] Tutorial hints and cinematics reset. All will show again.");
        }

        [MenuItem("Tools/Debug/Reset All Progress")]
        public static void ResetAll()
        {
            ResetLevelProgress();
            ResetTutorialHints();
            Debug.Log("[DebugTools] All progress reset.");
        }
    }
}
