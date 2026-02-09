using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Gameplay.UI;

namespace GameEditor
{
    public static class CreateLoadingScreen
    {
        private const string SpritesPath = "Assets/_Project/Art/Sprites/UI/PANTALLA_CARGA_";

        [MenuItem("Tools/UI/Create Loading Screen")]
        public static void Create()
        {
            // =============================================
            // ROOT CANVAS (high sort order to render on top)
            // =============================================
            GameObject canvasGO = new GameObject("LoadingScreenCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // CanvasGroup for fading
            CanvasGroup canvasGroup = canvasGO.AddComponent<CanvasGroup>();

            // =============================================
            // LOADING ROOT (toggled on/off)
            // =============================================
            GameObject loadingRoot = new GameObject("LoadingRoot");
            loadingRoot.transform.SetParent(canvasGO.transform, false);
            RectTransform rootRect = loadingRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // =============================================
            // BACKGROUND (solid black behind animation)
            // =============================================
            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(loadingRoot.transform, false);
            RectTransform bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.color = Color.black;

            // =============================================
            // ANIMATION IMAGE (fullscreen)
            // =============================================
            GameObject animGO = new GameObject("AnimationImage");
            animGO.transform.SetParent(loadingRoot.transform, false);
            RectTransform animRect = animGO.AddComponent<RectTransform>();
            animRect.anchorMin = Vector2.zero;
            animRect.anchorMax = Vector2.one;
            animRect.offsetMin = Vector2.zero;
            animRect.offsetMax = Vector2.zero;
            Image animImage = animGO.AddComponent<Image>();
            animImage.preserveAspect = true;
            animImage.raycastTarget = false;

            // =============================================
            // PROGRESS BAR (optional, at bottom)
            // =============================================
            GameObject progressGO = new GameObject("ProgressBar");
            progressGO.transform.SetParent(loadingRoot.transform, false);
            RectTransform progressRect = progressGO.AddComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0.1f, 0.05f);
            progressRect.anchorMax = new Vector2(0.9f, 0.07f);
            progressRect.offsetMin = Vector2.zero;
            progressRect.offsetMax = Vector2.zero;

            Slider progressSlider = progressGO.AddComponent<Slider>();
            progressSlider.interactable = false;
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;

            // Progress background
            GameObject progBg = new GameObject("Background");
            progBg.transform.SetParent(progressGO.transform, false);
            RectTransform progBgRect = progBg.AddComponent<RectTransform>();
            progBgRect.anchorMin = Vector2.zero;
            progBgRect.anchorMax = Vector2.one;
            progBgRect.offsetMin = Vector2.zero;
            progBgRect.offsetMax = Vector2.zero;
            Image progBgImg = progBg.AddComponent<Image>();
            progBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Progress fill area
            GameObject progFillArea = new GameObject("Fill Area");
            progFillArea.transform.SetParent(progressGO.transform, false);
            RectTransform progFillAreaRect = progFillArea.AddComponent<RectTransform>();
            progFillAreaRect.anchorMin = Vector2.zero;
            progFillAreaRect.anchorMax = Vector2.one;
            progFillAreaRect.offsetMin = new Vector2(2, 2);
            progFillAreaRect.offsetMax = new Vector2(-2, -2);

            GameObject progFill = new GameObject("Fill");
            progFill.transform.SetParent(progFillArea.transform, false);
            RectTransform progFillRect = progFill.AddComponent<RectTransform>();
            progFillRect.anchorMin = Vector2.zero;
            progFillRect.anchorMax = Vector2.one;
            progFillRect.offsetMin = Vector2.zero;
            progFillRect.offsetMax = Vector2.zero;
            Image progFillImg = progFill.AddComponent<Image>();
            progFillImg.color = new Color(0.8f, 0.4f, 1f, 1f);

            progressSlider.fillRect = progFillRect;

            // =============================================
            // LOAD SPRITES AUTOMATICALLY
            // =============================================
            Sprite[] loadingSprites = new Sprite[12];
            bool allFound = true;

            for (int i = 0; i < 12; i++)
            {
                string spritePath = SpritesPath + (i + 1).ToString("D4") + ".png";
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

                if (sprite != null)
                {
                    loadingSprites[i] = sprite;
                }
                else
                {
                    Debug.LogWarning($"[CreateLoadingScreen] Sprite not found: {spritePath} - Make sure textures are imported as Sprite.");
                    allFound = false;
                }
            }

            if (allFound)
            {
                Debug.Log("[CreateLoadingScreen] All 12 loading sprites loaded successfully!");
                animImage.sprite = loadingSprites[0];
            }

            // =============================================
            // WIRE UP LoadingScreen COMPONENT
            // =============================================
            LoadingScreen loadingScreen = canvasGO.AddComponent<LoadingScreen>();

            SerializedObject so = new SerializedObject(loadingScreen);
            so.FindProperty("animationImage").objectReferenceValue = animImage;
            so.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            so.FindProperty("loadingRoot").objectReferenceValue = loadingRoot;
            so.FindProperty("progressBar").objectReferenceValue = progressSlider;

            // Assign sprites array
            SerializedProperty spritesProp = so.FindProperty("loadingSprites");
            spritesProp.arraySize = loadingSprites.Length;
            for (int i = 0; i < loadingSprites.Length; i++)
            {
                spritesProp.GetArrayElementAtIndex(i).objectReferenceValue = loadingSprites[i];
            }

            so.ApplyModifiedProperties();

            // Register undo
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Loading Screen");

            // Select
            Selection.activeGameObject = canvasGO;

            Debug.Log("[CreateLoadingScreen] Loading Screen created! It will persist across scenes (DontDestroyOnLoad). Call LoadingScreen.Instance.LoadScene(\"sceneName\") to use it.");
        }
    }
}
