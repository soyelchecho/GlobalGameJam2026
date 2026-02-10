using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Gameplay.UI;

namespace GameEditor
{
    public static class CreateLevelSelectMenu
    {
        [MenuItem("Tools/UI/Create Level Select Menu")]
        public static void Create()
        {
            // =============================================
            // ROOT CANVAS
            // =============================================
            GameObject canvasGO = new GameObject("LevelSelectCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // EventSystem (if not present)
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            }

            // =============================================
            // BACKGROUND (fullscreen map image)
            // =============================================
            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            RectTransform bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.color = Color.white;
            bgImage.raycastTarget = false;

            // =============================================
            // LEVEL HOTSPOTS CONTAINER
            // =============================================
            GameObject hotspotsParent = new GameObject("LevelHotspots");
            hotspotsParent.transform.SetParent(canvasGO.transform, false);
            RectTransform hotspotsRect = hotspotsParent.AddComponent<RectTransform>();
            hotspotsRect.anchorMin = Vector2.zero;
            hotspotsRect.anchorMax = Vector2.one;
            hotspotsRect.offsetMin = Vector2.zero;
            hotspotsRect.offsetMax = Vector2.zero;

            // Create 3 hotspot buttons (one per volcano)
            // Positioned spread vertically - adjust positions in Scene view
            GameObject hotspot1 = CreateHotspot(hotspotsParent.transform, "Level1_Hotspot",
                new Vector2(0, 200), new Vector2(250, 250), "Nivel 1");
            GameObject hotspot2 = CreateHotspot(hotspotsParent.transform, "Level2_Hotspot",
                new Vector2(0, -100), new Vector2(250, 250), "Nivel 2");
            GameObject hotspot3 = CreateHotspot(hotspotsParent.transform, "Level3_Hotspot",
                new Vector2(0, -400), new Vector2(250, 250), "Nivel 3");

            // =============================================
            // BACK BUTTON (top-left)
            // =============================================
            GameObject backGO = new GameObject("BackButton");
            backGO.transform.SetParent(canvasGO.transform, false);
            RectTransform backRect = backGO.AddComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0f, 1f);
            backRect.anchorMax = new Vector2(0f, 1f);
            backRect.pivot = new Vector2(0f, 1f);
            backRect.anchoredPosition = new Vector2(30, -30);
            backRect.sizeDelta = new Vector2(100, 80);

            Image backImg = backGO.AddComponent<Image>();
            backImg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            backImg.raycastTarget = true;
            Button backBtn = backGO.AddComponent<Button>();

            ColorBlock backColors = backBtn.colors;
            backColors.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 0.9f);
            backColors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            backBtn.colors = backColors;

            // Back label
            GameObject backLabel = new GameObject("Label");
            backLabel.transform.SetParent(backGO.transform, false);
            RectTransform backLabelRect = backLabel.AddComponent<RectTransform>();
            backLabelRect.anchorMin = Vector2.zero;
            backLabelRect.anchorMax = Vector2.one;
            backLabelRect.offsetMin = Vector2.zero;
            backLabelRect.offsetMax = Vector2.zero;
            Text backText = backLabel.AddComponent<Text>();
            backText.text = "\u25C0"; // left arrow
            backText.fontSize = 44;
            backText.alignment = TextAnchor.MiddleCenter;
            backText.color = Color.white;
            backText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            backText.raycastTarget = false;

            // =============================================
            // WIRE UP LevelSelectMenu COMPONENT
            // =============================================
            LevelSelectMenu menu = canvasGO.AddComponent<LevelSelectMenu>();

            SerializedObject so = new SerializedObject(menu);
            so.FindProperty("backgroundImage").objectReferenceValue = bgImage;
            so.FindProperty("backButton").objectReferenceValue = backBtn;

            // Wire levels array
            SerializedProperty levelsProp = so.FindProperty("levels");
            levelsProp.arraySize = 3;

            Button[] hotspotButtons = new Button[]
            {
                hotspot1.GetComponent<Button>(),
                hotspot2.GetComponent<Button>(),
                hotspot3.GetComponent<Button>()
            };

            Text[] hotspotLabels = new Text[]
            {
                hotspot1.GetComponentInChildren<Text>(),
                hotspot2.GetComponentInChildren<Text>(),
                hotspot3.GetComponentInChildren<Text>()
            };

            for (int i = 0; i < 3; i++)
            {
                SerializedProperty level = levelsProp.GetArrayElementAtIndex(i);
                level.FindPropertyRelative("button").objectReferenceValue = hotspotButtons[i];
                level.FindPropertyRelative("sceneName").stringValue = "Level" + (i + 1);
                level.FindPropertyRelative("useSceneIndex").boolValue = false;
                level.FindPropertyRelative("sceneIndex").intValue = i + 1;
                level.FindPropertyRelative("locked").boolValue = false;
                level.FindPropertyRelative("levelLabel").objectReferenceValue = hotspotLabels[i];
            }

            so.ApplyModifiedProperties();

            // Register undo
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Level Select Menu");

            // Select
            Selection.activeGameObject = canvasGO;

            Debug.Log("[CreateLevelSelectMenu] Level Select Menu created!\n" +
                "1. Assign your volcano map sprite to the Background Image\n" +
                "2. Move the hotspot buttons over each volcano in the Scene view\n" +
                "3. Resize hotspots to cover the clickable area\n" +
                "4. Configure scene names in the LevelSelectMenu component");
        }

        // =============================================
        // HELPERS
        // =============================================

        private static GameObject CreateHotspot(Transform parent, string name,
            Vector2 position, Vector2 size, string labelText)
        {
            // Hotspot button (invisible, positioned over volcano)
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            // Invisible image for raycasting + hover feedback
            Image img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0f); // invisible
            img.raycastTarget = true;

            // Button component
            Button btn = go.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.25f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.4f);
            colors.selectedColor = new Color(1f, 1f, 1f, 0f);
            btn.colors = colors;
            btn.targetGraphic = img;

            // Hotspot visual feedback component
            go.AddComponent<LevelHotspotButton>();

            // Level label (below the hotspot)
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            RectTransform labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0f);
            labelRect.pivot = new Vector2(0.5f, 1f);
            labelRect.anchoredPosition = new Vector2(0, -10);
            labelRect.sizeDelta = new Vector2(0, 40);

            Text label = labelGO.AddComponent<Text>();
            label.text = labelText;
            label.fontSize = 30;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontStyle = FontStyle.Bold;
            label.raycastTarget = false;

            // Add outline for readability over map
            Outline outline = labelGO.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);

            return go;
        }
    }
}
