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

            GameObject gearLabel = new GameObject("Icon");
            gearLabel.transform.SetParent(gearGO.transform, false);
            RectTransform gearLabelRect = gearLabel.AddComponent<RectTransform>();
            gearLabelRect.anchorMin = Vector2.zero;
            gearLabelRect.anchorMax = Vector2.one;
            gearLabelRect.offsetMin = Vector2.zero;
            gearLabelRect.offsetMax = Vector2.zero;
            Text gearText = gearLabel.AddComponent<Text>();
            gearText.text = "\u2699";
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
            overlayImg.raycastTarget = true;

            // =============================================
            // PAUSE PANEL
            // Assign your background sprite directly to the PausePanel Image in the Inspector.
            // All child elements are freely positionable — drag them in the Scene view.
            // =============================================
            GameObject panelGO = new GameObject("PausePanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(700, 1100);
            panelRect.anchoredPosition = Vector2.zero;

            // Panel background Image — assign your sprite here
            Image panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.08f, 0.18f, 0.95f);

            // ---- Close button (X) — top-right of panel ----
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

            // ---- Title — drag freely in Scene view ----
            CreatePanelLabel(panelGO.transform, "TitleLabel", "PAUSA", 48, new Vector2(0, 430));

            // ---- Volume sliders — drag each group freely in Scene view ----
            GameObject masterGroup = CreateFreeSliderGroup(panelGO.transform, "MasterVolume", "Volumen General", 600, 260);
            GameObject musicGroup  = CreateFreeSliderGroup(panelGO.transform, "MusicVolume",  "Musica",          600, 110);
            GameObject sfxGroup    = CreateFreeSliderGroup(panelGO.transform, "SFXVolume",    "Efectos",         600, -40);

            // ---- Navigation buttons — drag freely in Scene view ----
            GameObject resumeBtn      = CreatePanelButton(panelGO.transform, "Btn_Reanudar",      "REANUDAR",           new Color(0.2f, 0.7f, 0.3f, 1f), new Vector2(0, -200));
            GameObject levelSelectBtn = CreatePanelButton(panelGO.transform, "Btn_NivelSelect",   "SELECCION DE NIVEL", new Color(0.3f, 0.5f, 0.8f, 1f), new Vector2(0, -310));
            GameObject mainMenuBtn    = CreatePanelButton(panelGO.transform, "Btn_MenuPrincipal", "MENU PRINCIPAL",     new Color(0.6f, 0.3f, 0.7f, 1f), new Vector2(0, -420));

            // Hide panel initially
            overlayGO.SetActive(false);
            panelGO.SetActive(false);

            // =============================================
            // WIRE UP PauseMenu COMPONENT
            // =============================================
            PauseMenu pauseMenu = canvasGO.AddComponent<PauseMenu>();

            SerializedObject so = new SerializedObject(pauseMenu);
            so.FindProperty("gearButton").objectReferenceValue  = gearGO;
            so.FindProperty("pausePanel").objectReferenceValue  = panelGO;
            so.FindProperty("overlay").objectReferenceValue     = overlayGO;
            so.FindProperty("gearBtn").objectReferenceValue     = gearBtn;
            so.FindProperty("closeBtn").objectReferenceValue    = closeBtn;
            so.FindProperty("resumeBtn").objectReferenceValue      = resumeBtn.GetComponent<Button>();
            so.FindProperty("levelSelectBtn").objectReferenceValue = levelSelectBtn.GetComponent<Button>();
            so.FindProperty("mainMenuBtn").objectReferenceValue    = mainMenuBtn.GetComponent<Button>();
            so.FindProperty("masterVolumeSlider").objectReferenceValue = masterGroup.GetComponentInChildren<Slider>();
            so.FindProperty("musicVolumeSlider").objectReferenceValue  = musicGroup.GetComponentInChildren<Slider>();
            so.FindProperty("sfxVolumeSlider").objectReferenceValue    = sfxGroup.GetComponentInChildren<Slider>();
            so.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Pause Menu");
            Selection.activeGameObject = canvasGO;

            Debug.Log("[CreatePauseMenu] Created! All panel elements are freely positionable — drag them in Scene view. Assign the panel background sprite directly to PausePanel > Image in the Inspector.");
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

        /// <summary>Text label freely positionable inside the panel.</summary>
        private static void CreatePanelLabel(Transform parent, string name, string text, int fontSize, Vector2 anchoredPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot     = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(600, fontSize + 20);
            rect.anchoredPosition = anchoredPos;
            Text label = go.AddComponent<Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontStyle = FontStyle.Bold;
            label.raycastTarget = false;
        }

        /// <summary>Button freely positionable inside the panel.</summary>
        private static GameObject CreatePanelButton(Transform parent, string name, string text, Color bgColor, Vector2 anchoredPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot     = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(500, 80);
            rect.anchoredPosition = anchoredPos;

            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;

            Button btn = go.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(bgColor.r + 0.15f, bgColor.g + 0.15f, bgColor.b + 0.15f, 1f);
            colors.pressedColor     = new Color(bgColor.r - 0.1f,  bgColor.g - 0.1f,  bgColor.b - 0.1f,  1f);
            btn.colors = colors;

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

            return go;
        }

        /// <summary>
        /// Self-contained slider group (label + slider) with NO layout component.
        /// Drag the whole group in Scene view to reposition it.
        /// </summary>
        private static GameObject CreateFreeSliderGroup(Transform parent, string name, string labelText, float width, float anchoredY)
        {
            // ---- Container (move this in Scene view) ----
            GameObject group = new GameObject(name + "Group");
            group.transform.SetParent(parent, false);
            RectTransform groupRect = group.AddComponent<RectTransform>();
            groupRect.anchorMin = new Vector2(0.5f, 0.5f);
            groupRect.anchorMax = new Vector2(0.5f, 0.5f);
            groupRect.pivot     = new Vector2(0.5f, 0.5f);
            groupRect.sizeDelta = new Vector2(width, 110);
            groupRect.anchoredPosition = new Vector2(0, anchoredY);

            // ---- Label (top half of group) ----
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(group.transform, false);
            RectTransform labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.55f);
            labelRect.anchorMax = new Vector2(1, 1f);
            labelRect.offsetMin = new Vector2(10, 0);
            labelRect.offsetMax = new Vector2(-10, 0);
            Text label = labelGO.AddComponent<Text>();
            label.text = labelText;
            label.fontSize = 28;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.raycastTarget = false;

            // ---- Slider root (bottom half of group) ----
            GameObject sliderGO = new GameObject(name + "Slider");
            sliderGO.transform.SetParent(group.transform, false);
            RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0, 0);
            sliderRect.anchorMax = new Vector2(1, 0.5f);
            sliderRect.offsetMin = new Vector2(10, 5);
            sliderRect.offsetMax = new Vector2(-10, -5);

            // Transparent image so the slider root receives raycasts
            Image sliderBgImg = sliderGO.AddComponent<Image>();
            sliderBgImg.color = new Color(0, 0, 0, 0);

            Slider slider = sliderGO.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.75f;
            slider.wholeNumbers = false;

            // Track background
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

            // Fill area
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

            // Handle
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
            handleRect.sizeDelta = new Vector2(40, 0);
            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImg;

            return group;
        }
    }
}
