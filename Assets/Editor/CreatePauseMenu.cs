using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Gameplay.UI;

namespace GameEditor
{
    public static class CreatePauseMenu
    {
        [MenuItem("Tools/UI/Create Pause Menu")]
        public static void Create()
        {
            // =============================================
            // ROOT CANVAS (high sort order, above game UI)
            // =============================================
            GameObject canvasGO = new GameObject("PauseMenuCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
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
            // GEAR BUTTON (top-right corner, always visible)
            // =============================================
            GameObject gearGO = new GameObject("GearButton");
            gearGO.transform.SetParent(canvasGO.transform, false);
            RectTransform gearRect = gearGO.AddComponent<RectTransform>();
            gearRect.anchorMin = new Vector2(1f, 1f);
            gearRect.anchorMax = new Vector2(1f, 1f);
            gearRect.pivot = new Vector2(1f, 1f);
            gearRect.anchoredPosition = new Vector2(-30, -30);
            gearRect.sizeDelta = new Vector2(80, 80);

            Image gearImg = gearGO.AddComponent<Image>();
            gearImg.color = new Color(1f, 1f, 1f, 0.8f);
            gearImg.raycastTarget = true;
            Button gearBtn = gearGO.AddComponent<Button>();

            // Gear icon text (placeholder until sprite is assigned)
            GameObject gearLabel = new GameObject("Icon");
            gearLabel.transform.SetParent(gearGO.transform, false);
            RectTransform gearLabelRect = gearLabel.AddComponent<RectTransform>();
            gearLabelRect.anchorMin = Vector2.zero;
            gearLabelRect.anchorMax = Vector2.one;
            gearLabelRect.offsetMin = Vector2.zero;
            gearLabelRect.offsetMax = Vector2.zero;
            Text gearText = gearLabel.AddComponent<Text>();
            gearText.text = "\u2699"; // gear unicode
            gearText.fontSize = 50;
            gearText.alignment = TextAnchor.MiddleCenter;
            gearText.color = Color.black;
            gearText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // =============================================
            // OVERLAY (dark semi-transparent fullscreen)
            // =============================================
            GameObject overlayGO = new GameObject("Overlay");
            overlayGO.transform.SetParent(canvasGO.transform, false);
            SetupFullscreen(overlayGO);
            Image overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.7f);
            overlayImg.raycastTarget = true; // blocks input to game

            // =============================================
            // PAUSE PANEL (centered)
            // =============================================
            GameObject panelGO = new GameObject("PausePanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(700, 1100);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.08f, 0.18f, 0.95f);

            // =============================================
            // CLOSE BUTTON (X) - top right of panel
            // =============================================
            GameObject closeGO = new GameObject("CloseButton");
            closeGO.transform.SetParent(panelGO.transform, false);
            RectTransform closeRect = closeGO.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-15, -15);
            closeRect.sizeDelta = new Vector2(70, 70);

            Image closeImg = closeGO.AddComponent<Image>();
            closeImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            closeImg.raycastTarget = true;
            Button closeBtn = closeGO.AddComponent<Button>();

            // X text
            GameObject closeLabel = new GameObject("XLabel");
            closeLabel.transform.SetParent(closeGO.transform, false);
            RectTransform closeLabelRect = closeLabel.AddComponent<RectTransform>();
            closeLabelRect.anchorMin = Vector2.zero;
            closeLabelRect.anchorMax = Vector2.one;
            closeLabelRect.offsetMin = Vector2.zero;
            closeLabelRect.offsetMax = Vector2.zero;
            Text closeText = closeLabel.AddComponent<Text>();
            closeText.text = "X";
            closeText.fontSize = 40;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.color = Color.white;
            closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            closeText.fontStyle = FontStyle.Bold;

            // =============================================
            // PANEL CONTENT (vertical layout)
            // =============================================
            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(panelGO.transform, false);
            RectTransform contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.05f, 0.05f);
            contentRect.anchorMax = new Vector2(0.95f, 0.88f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup contentLayout = contentGO.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 35f;
            contentLayout.childAlignment = TextAnchor.MiddleCenter;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.padding = new RectOffset(30, 30, 10, 10);

            // Title
            CreateLabel(contentGO.transform, "TitleLabel", "PAUSA", 48);

            // Volume sliders
            GameObject masterSlider = CreateSliderRow(contentGO.transform, "MasterVolume", "Volumen General");
            GameObject musicSlider = CreateSliderRow(contentGO.transform, "MusicVolume", "Musica");
            GameObject sfxSlider = CreateSliderRow(contentGO.transform, "SFXVolume", "Efectos");

            // Spacer
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(contentGO.transform, false);
            spacer.AddComponent<RectTransform>();
            LayoutElement spacerLayout = spacer.AddComponent<LayoutElement>();
            spacerLayout.preferredHeight = 20;

            // Navigation buttons
            GameObject resumeBtn = CreateMenuButton(contentGO.transform, "Btn_Reanudar", "REANUDAR", new Color(0.2f, 0.7f, 0.3f, 1f));
            GameObject levelSelectBtn = CreateMenuButton(contentGO.transform, "Btn_NivelSelect", "SELECCION DE NIVEL", new Color(0.3f, 0.5f, 0.8f, 1f));
            GameObject mainMenuBtn = CreateMenuButton(contentGO.transform, "Btn_MenuPrincipal", "MENU PRINCIPAL", new Color(0.6f, 0.3f, 0.7f, 1f));

            // Hide panel initially
            overlayGO.SetActive(false);
            panelGO.SetActive(false);

            // =============================================
            // WIRE UP PauseMenu COMPONENT
            // =============================================
            PauseMenu pauseMenu = canvasGO.AddComponent<PauseMenu>();

            SerializedObject so = new SerializedObject(pauseMenu);
            so.FindProperty("gearButton").objectReferenceValue = gearGO;
            so.FindProperty("pausePanel").objectReferenceValue = panelGO;
            so.FindProperty("overlay").objectReferenceValue = overlayGO;
            so.FindProperty("gearBtn").objectReferenceValue = gearBtn;
            so.FindProperty("closeBtn").objectReferenceValue = closeBtn;
            so.FindProperty("resumeBtn").objectReferenceValue = resumeBtn.GetComponent<Button>();
            so.FindProperty("levelSelectBtn").objectReferenceValue = levelSelectBtn.GetComponent<Button>();
            so.FindProperty("mainMenuBtn").objectReferenceValue = mainMenuBtn.GetComponent<Button>();
            so.FindProperty("masterVolumeSlider").objectReferenceValue = masterSlider.GetComponentInChildren<Slider>();
            so.FindProperty("musicVolumeSlider").objectReferenceValue = musicSlider.GetComponentInChildren<Slider>();
            so.FindProperty("sfxVolumeSlider").objectReferenceValue = sfxSlider.GetComponentInChildren<Slider>();
            so.ApplyModifiedProperties();

            // Register undo
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Pause Menu");

            // Select
            Selection.activeGameObject = canvasGO;

            Debug.Log("[CreatePauseMenu] Pause Menu created! Assign gear sprite, configure scene names in PauseMenu component. Make it a prefab to reuse across levels.");
        }

        // =============================================
        // HELPERS
        // =============================================

        private static void SetupFullscreen(GameObject go)
        {
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
            label.fontStyle = FontStyle.Bold;
            label.raycastTarget = false;

            LayoutElement layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = fontSize + 30;

            return go;
        }

        private static GameObject CreateMenuButton(Transform parent, string name, string text, Color bgColor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(500, 80);

            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;

            Button btn = go.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(bgColor.r + 0.15f, bgColor.g + 0.15f, bgColor.b + 0.15f, 1f);
            colors.pressedColor = new Color(bgColor.r - 0.1f, bgColor.g - 0.1f, bgColor.b - 0.1f, 1f);
            btn.colors = colors;

            // Button label
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            RectTransform labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            Text label = labelGO.AddComponent<Text>();
            label.text = text;
            label.fontSize = 32;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontStyle = FontStyle.Bold;

            LayoutElement layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = 80;

            return go;
        }

        private static GameObject CreateSliderRow(Transform parent, string name, string labelText)
        {
            // Row container
            GameObject row = new GameObject(name + "Row");
            row.transform.SetParent(parent, false);
            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0, 70);

            VerticalLayoutGroup rowLayout = row.AddComponent<VerticalLayoutGroup>();
            rowLayout.spacing = 5f;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = false;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = false;

            LayoutElement rowLayoutEl = row.AddComponent<LayoutElement>();
            rowLayoutEl.preferredHeight = 90;

            // Label
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(row.transform, false);
            Text label = labelGO.AddComponent<Text>();
            label.text = labelText;
            label.fontSize = 26;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.raycastTarget = false;

            LayoutElement labelLayout = labelGO.AddComponent<LayoutElement>();
            labelLayout.preferredHeight = 30;

            // Slider
            GameObject sliderGO = CreateDefaultSlider(row.transform, name + "Slider");

            LayoutElement sliderLayout = sliderGO.AddComponent<LayoutElement>();
            sliderLayout.preferredHeight = 40;

            return row;
        }

        private static GameObject CreateDefaultSlider(Transform parent, string name)
        {
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

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImg;

            return sliderGO;
        }
    }
}
