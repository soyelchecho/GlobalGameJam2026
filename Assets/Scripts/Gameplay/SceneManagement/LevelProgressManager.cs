using UnityEngine;

namespace Gameplay.SceneManagement
{
    /// <summary>
    /// Persistent level unlock state stored in PlayerPrefs.
    /// Level index 0 is always unlocked.
    /// Call UnlockLevel(nextIndex) from EndLevelTrigger when a level is completed.
    /// </summary>
    public static class LevelProgressManager
    {
        private const string KeyPrefix = "LevelUnlocked_";

        public static bool IsLevelUnlocked(int levelIndex)
        {
            if (levelIndex <= 0) return true;
            return PlayerPrefs.GetInt(KeyPrefix + levelIndex, 0) == 1;
        }

        public static void UnlockLevel(int levelIndex)
        {
            if (levelIndex <= 0) return;
            PlayerPrefs.SetInt(KeyPrefix + levelIndex, 1);
            PlayerPrefs.Save();
        }

        /// <summary>Wipe all progress (for debug / reset).</summary>
        public static void ResetProgress()
        {
            for (int i = 1; i < 20; i++)
                PlayerPrefs.DeleteKey(KeyPrefix + i);
            PlayerPrefs.Save();
        }
    }
}
