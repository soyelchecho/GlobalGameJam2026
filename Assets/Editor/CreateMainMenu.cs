using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Gameplay.UI;

namespace GameEditor
{
    public static class CreateMainMenu
    {
        [MenuItem("Tools/UI/Create Main Menu")]
        public static void Create()
        {
            // =============================================
            // ROOT CANVAS
            // =============================================
            GameObject canvasGO = new GameObject("MainMenuCanvas");
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
            // BACKGROUND
            // =============================================
            GameObject bgGO = CreateFullscreenImage(canvasGO.transform, "Background");
            Image bgImage = bgGO.GetComponent<Image>();
            bgImage.color = new Color(0.1f, 0.05f, 0.15f, 1f);

            // =============================================
            // MAIN PANEL (4 buttons)
            // =============================================
            GameObject mainPanel = CreatePanel(canvasGO.transform, "MainPanel");

            // Vertical layout for buttons
            VerticalLayoutGroup mainLayout = mainPanel.AddComponent<VerticalLayoutGroup>();
            mainLayout.spacing = 30f;
            mainLayout.childAlignment = TextAnchor.MiddleCenter;
            mainLayout.childControlWidth = false;
            mainLayout.childControlHeight = false;
            mainLayout.childForceExpandWidth = false;
            mainLayout.childForceExpandHeight = false;

            // Add content size fitter to center properly
            ContentSizeFitter mainFitter = mainPanel.AddComponent<ContentSizeFitter>();
            mainFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            mainFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Center the panel
            RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.5f, 0.5f);
            mainRect.anchorMax = new Vector2(0.5f, 0.5f);
            mainRect.pivot = new Vector2(0.5f, 0.5f);
            mainRect.anchoredPosition = Vector2.zero;

            // Create 4 SpriteButtons
            GameObject btnStart = CreateSpriteButton(mainPanel.transform, "Btn_Inicio", new Vector2(400, 100));
            GameObject btnOptions = CreateSpriteButton(mainPanel.transform, "Btn_Opciones", new Vector2(400, 100));
            GameObject btnCredits = CreateSpriteButton(mainPanel.transform, "Btn_Creditos", new Vector2(400, 100));
            GameObject btnExit = CreateSpriteButton(mainPanel.transform, "Btn_Salir", new Vector2(400, 100));

            // =============================================
            // OPTIONS PANEL
            // =============================================
            GameObject optionsPanel = CreatePanel(canvasGO.transform, "OptionsPanel");
            SetupFullscreenRect(optionsPanel);

            // Options background
            Image optionsBg = optionsPanel.GetComponent<Image>();
            optionsBg.color = new Color(0, 0, 0, 0.85f);

            // Options content
            GameObject optionsContent = new GameObject("OptionsContent");
            optionsContent.transform.SetParent(optionsPanel.transform, false);
            RectTransform optContentRect = optionsContent.AddComponent<RectTransform>();
            optContentRect.anchorMin = new Vector2(0.1f, 0.2f);
            optContentRect.anchorMax = new Vector2(0.9f, 0.8f);
            optContentRect.offsetMin = Vector2.zero;
            optContentRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup optLayout = optionsContent.AddComponent<VerticalLayoutGroup>();
            optLayout.spacing = 40f;
            optLayout.childAlignment = TextAnchor.MiddleCenter;
            optLayout.childControlWidth = true;
            optLayout.childControlHeight = false;
            optLayout.childForceExpandWidth = true;
            optLayout.childForceExpandHeight = false;
            optLayout.padding = new RectOffset(40, 40, 20, 20);

            // Title
            CreateLabel(optionsContent.transform, "TitleLabel", "OPCIONES", 48);

            // Sliders
            GameObject masterSlider = CreateSliderRow(optionsContent.transform, "MasterVolume", "Volumen General");
            GameObject musicSlider = CreateSliderRow(optionsContent.transform, "MusicVolume", "Musica");
            GameObject sfxSlider = CreateSliderRow(optionsContent.transform, "SFXVolume", "Efectos");

            // Back button
            GameObject btnOptionsBack = CreateSpriteButton(optionsContent.transform, "Btn_Volver", new Vector2(300, 80));

            optionsPanel.SetActive(false);

            // =============================================
            // CREDITS PANEL
            // =============================================
            GameObject creditsPanel = CreatePanel(canvasGO.transform, "CreditsPanel");
            SetupFullscreenRect(creditsPanel);

            Image creditsBg = creditsPanel.GetComponent<Image>();
            creditsBg.color = new Color(0, 0, 0, 0.85f);

            // Credits image (fullscreen, assign your sprite in inspector)
            GameObject creditsImageGO = new GameObject("CreditsImage");
            creditsImageGO.transform.SetParent(creditsPanel.transform, false);
            RectTransform credImgRect = creditsImageGO.AddComponent<RectTransform>();
            credImgRect.anchorMin = Vector2.zero;
            credImgRect.anchorMax = Vector2.one;
            credImgRect.offsetMin = Vector2.zero;
            credImgRect.offsetMax = Vector2.zero;
            Image creditsImg = creditsImageGO.AddComponent<Image>();
            creditsImg.preserveAspect = true;
            creditsImg.color = Color.white;

            // Back button (bottom center)
            GameObject btnCreditsBack = CreateSpriteButton(creditsPanel.transform, "Btn_Volver", new Vector2(300, 80));
            RectTransform backRect = btnCreditsBack.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.5f, 0.05f);
            backRect.anchorMax = new Vector2(0.5f, 0.05f);
            backRect.pivot = new Vector2(0.5f, 0.5f);
            backRect.anchoredPosition = Vector2.zero;

            creditsPanel.SetActive(false);

            // =============================================
            // WIRE UP MainMenu COMPONENT
            // =============================================
            MainMenu menu = canvasGO.AddComponent<MainMenu>();

            // Use SerializedObject to assign private serialized fields
            SerializedObject so = new SerializedObject(menu);
            so.FindProperty("backgroundImage").objectReferenceValue = bgImage;
            so.FindProperty("mainPanel").objectReferenceValue = mainPanel;
            so.FindProperty("optionsPanel").objectReferenceValue = optionsPanel;
            so.FindProperty("creditsPanel").objectReferenceValue = creditsPanel;
            so.FindProperty("creditsImage").objectReferenceValue = creditsImg;
            so.FindProperty("startButton").objectReferenceValue = btnStart.GetComponent<SpriteButton>();
            so.FindProperty("optionsButton").objectReferenceValue = btnOptions.GetComponent<SpriteButton>();
            so.FindProperty("creditsButton").objectReferenceValue = btnCredits.GetComponent<SpriteButton>();
            so.FindProperty("exitButton").objectReferenceValue = btnExit.GetComponent<SpriteButton>();
            so.FindProperty("optionsBackButton").objectReferenceValue = btnOptionsBack.GetComponent<SpriteButton>();
            so.FindProperty("creditsBackButton").objectReferenceValue = btnCreditsBack.GetComponent<SpriteButton>();
            so.FindProperty("masterVolumeSlider").objectReferenceValue = masterSlider.GetComponentInChildren<Slider>();
            so.FindProperty("musicVolumeSlider").objectReferenceValue = musicSlider.GetComponentInChildren<Slider>();
            so.FindProperty("sfxVolumeSlider").objectReferenceValue = sfxSlider.GetComponentInChildren<Slider>();
            so.ApplyModifiedProperties();

            // Register undo
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Main Menu");

            // Select
            Selection.activeGameObject = canvasGO;

            Debug.Log("[CreateMainMenu] Main Menu created! Assign your sprites to each SpriteButton and configure the scene name in MainMenu component.");
        }

        // =============================================
        // HELPERS
        // =============================================

        private static GameObject CreateFullscreenImage(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.AddComponent<Image>();
            return go;
        }

        private static GameObject CreatePanel(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            go.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            return go;
        }

        private static void SetupFullscreenRect(GameObject go)
        {
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static GameObject CreateSpriteButton(Transform parent, string name, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            img.raycastTarget = true;

            go.AddComponent<SpriteButton>();

            // Add layout element for proper sizing in layouts
            LayoutElement layout = go.AddComponent<LayoutElement>();
            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;

            return go;
        }

        private static GameObject CreateLabel(Transform parent, string name, string text, int fontSize)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600, fontSize + 20);

            Text label = go.AddComponent<Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;

            LayoutElement layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = fontSize + 30;

            return go;
        }

        private static GameObject CreateSliderRow(Transform parent, string name, string labelText)
        {
            // Row container
            GameObject row = new GameObject(name + "Row");
            row.transform.SetParent(parent, false);
            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0, 80);

            HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 20f;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;

            LayoutElement rowLayoutEl = row.AddComponent<LayoutElement>();
            rowLayoutEl.preferredHeight = 80;

            // Label
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(row.transform, false);
            Text label = labelGO.AddComponent<Text>();
            label.text = labelText;
            label.fontSize = 30;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = Color.white;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.raycastTarget = false;

            LayoutElement labelLayout = labelGO.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 300;
            labelLayout.flexibleWidth = 0;

            // Slider
            GameObject sliderGO = CreateDefaultSlider(row.transform, name + "Slider");

            LayoutElement sliderLayout = sliderGO.AddComponent<LayoutElement>();
            sliderLayout.preferredWidth = 400;
            sliderLayout.flexibleWidth = 1;

            return row;
        }

        private static GameObject CreateDefaultSlider(Transform parent, string name)
        {
            // Slider root
            GameObject sliderGO = new GameObject(name);
            sliderGO.transform.SetParent(parent, false);
            RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(400, 40);

            // Transparent image so slider receives raycasts
            Image sliderBgImg = sliderGO.AddComponent<Image>();
            sliderBgImg.color = new Color(0, 0, 0, 0);

            Slider slider = sliderGO.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.75f;
            slider.wholeNumbers = false;

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderGO.transform, false);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.25f);
            bgRect.anchorMax = new Vector2(1, 0.75f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            bgImg.raycastTarget = false;

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGO.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.offsetMin = new Vector2(5, 0);
            fillAreaRect.offsetMax = new Vector2(-5, 0);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.8f, 0.4f, 1f, 1f);
            fillImg.raycastTarget = false;

            // Handle Slide Area
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderGO.transform, false);
            RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10, 0);
            handleAreaRect.offsetMax = new Vector2(-10, 0);

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(30, 0);
            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;

            // Wire slider references
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImg;

            return sliderGO;
        }
    }
}
