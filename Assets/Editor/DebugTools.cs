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
            // Prefijos conocidos — borra cualquier key que empiece por ellos
            string[] prefixes = { "TutorialHint_", "CinematicSeen_" };

            // PlayerPrefs no permite enumerar keys, así que borramos
            // todas las combinaciones posibles conocidas del proyecto
            string[] hintIds = { "Jump", "Mask" };
            foreach (var id in hintIds)
                PlayerPrefs.DeleteKey("TutorialHint_" + id);

            string[] cinematicIds = { "Level1Cinematic" };
            foreach (var id in cinematicIds)
                PlayerPrefs.DeleteKey("CinematicSeen_" + id);

            PlayerPrefs.Save();
            Debug.Log("[DebugTools] Tutorial hints y cinemáticas reseteadas.");
        }

        [MenuItem("Tools/Debug/Reset All Progress (PlayerPrefs.DeleteAll)")]
        public static void ResetAll()
        {
            if (EditorUtility.DisplayDialog(
                "Reset All Progress",
                "Esto borrará TODOS los PlayerPrefs (progreso, hints, cinemáticas, volumen, etc.).\n\n¿Continuar?",
                "Sí, borrar todo",
                "Cancelar"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("[DebugTools] Todos los PlayerPrefs borrados.");
            }
        }
    }
}
