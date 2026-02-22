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
            // MAIN PANEL (4 buttons — freely moveable)
            // =============================================
            GameObject mainPanel = CreatePanel(canvasGO.transform, "MainPanel");
            SetupFullscreenRect(mainPanel);

            // Buttons placed at free positions — move them in Scene view
            GameObject btnStart   = CreateFreeSpriteButton(mainPanel.transform, "Btn_Inicio",   new Vector2(400, 100), new Vector2(0,  300));
            GameObject btnOptions = CreateFreeSpriteButton(mainPanel.transform, "Btn_Opciones", new Vector2(400, 100), new Vector2(0,  150));
            GameObject btnCredits = CreateFreeSpriteButton(mainPanel.transform, "Btn_Creditos", new Vector2(400, 100), new Vector2(0,    0));
            GameObject btnExit    = CreateFreeSpriteButton(mainPanel.transform, "Btn_Salir",    new Vector2(400, 100), new Vector2(0, -150));

            // =============================================
            // OPTIONS PANEL
            // =============================================
            GameObject optionsPanel = CreatePanel(canvasGO.transform, "OptionsPanel");
            SetupFullscreenRect(optionsPanel);
            // Make the panel itself fully transparent so only the OptionsBg image shows
            optionsPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0);

            // Dedicated background image — assign your sprite here in the Inspector
            GameObject optionsBgGO = CreateFullscreenImage(optionsPanel.transform, "OptionsBg");
            Image optionsBgImg = optionsBgGO.GetComponent<Image>();
            optionsBgImg.color = new Color(0f, 0f, 0f, 0.9f); // default dark, replace with sprite

            // Title label — move freely in Scene view
            GameObject titleLabel = CreateFreeLabel(optionsPanel.transform, "TitleLabel", "OPCIONES", 52, new Vector2(0, 650));

            // Slider groups — move each freely in Scene view
            GameObject masterGroup = CreateFreeSliderGroup(optionsPanel.transform, "MasterVolume", "Volumen General", 800, 350);
            GameObject musicGroup  = CreateFreeSliderGroup(optionsPanel.transform, "MusicVolume",  "Musica",          800, 150);
            GameObject sfxGroup    = CreateFreeSliderGroup(optionsPanel.transform, "SFXVolume",    "Efectos",         800, -50);

            // Back button — move freely in Scene view
            GameObject btnOptionsBack = CreateFreeSpriteButton(optionsPanel.transform, "Btn_Volver", new Vector2(300, 90), new Vector2(0, -400));

            optionsPanel.SetActive(false);

            // =============================================
            // CREDITS PANEL
            // =============================================
            GameObject creditsPanel = CreatePanel(canvasGO.transform, "CreditsPanel");
            SetupFullscreenRect(creditsPanel);
            creditsPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.85f);

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
            GameObject btnCreditsBack = CreateFreeSpriteButton(creditsPanel.transform, "Btn_Volver", new Vector2(300, 80), new Vector2(0, -800));

            creditsPanel.SetActive(false);

            // =============================================
            // WIRE UP MainMenu COMPONENT
            // =============================================
            MainMenu menu = canvasGO.AddComponent<MainMenu>();

            SerializedObject so = new SerializedObject(menu);
            so.FindProperty("backgroundImage").objectReferenceValue  = bgImage;
            so.FindProperty("optionsBgImage").objectReferenceValue   = optionsBgImg;
            so.FindProperty("mainPanel").objectReferenceValue        = mainPanel;
            so.FindProperty("optionsPanel").objectReferenceValue     = optionsPanel;
            so.FindProperty("creditsPanel").objectReferenceValue     = creditsPanel;
            so.FindProperty("creditsImage").objectReferenceValue     = creditsImg;
            so.FindProperty("startButton").objectReferenceValue      = btnStart.GetComponent<SpriteButton>();
            so.FindProperty("optionsButton").objectReferenceValue    = btnOptions.GetComponent<SpriteButton>();
            so.FindProperty("creditsButton").objectReferenceValue    = btnCredits.GetComponent<SpriteButton>();
            so.FindProperty("exitButton").objectReferenceValue       = btnExit.GetComponent<SpriteButton>();
            so.FindProperty("optionsBackButton").objectReferenceValue = btnOptionsBack.GetComponent<SpriteButton>();
            so.FindProperty("creditsBackButton").objectReferenceValue = btnCreditsBack.GetComponent<SpriteButton>();
            so.FindProperty("masterVolumeSlider").objectReferenceValue = masterGroup.GetComponentInChildren<Slider>();
            so.FindProperty("musicVolumeSlider").objectReferenceValue  = musicGroup.GetComponentInChildren<Slider>();
            so.FindProperty("sfxVolumeSlider").objectReferenceValue    = sfxGroup.GetComponentInChildren<Slider>();
            so.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Main Menu");
            Selection.activeGameObject = canvasGO;

            Debug.Log("[CreateMainMenu] Created! Options panel elements are freely positionable — drag them in the Scene view. Assign sprites in the Inspector.");
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
            go.AddComponent<RectTransform>();
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

        /// <summary>Sprite button with a fixed size, freely moveable via anchoredPosition.</summary>
        private static GameObject CreateFreeSpriteButton(Transform parent, string name, Vector2 size, Vector2 anchoredPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot     = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;

            Image img = go.AddComponent<Image>();
            img.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            img.raycastTarget = true;

            go.AddComponent<SpriteButton>();
            return go;
        }

        /// <summary>Text label freely positionable.</summary>
        private static GameObject CreateFreeLabel(Transform parent, string name, string text, int fontSize, Vector2 anchoredPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot     = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(700, fontSize + 20);
            rect.anchoredPosition = anchoredPos;

            Text label = go.AddComponent<Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;

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
            label.fontSize = 30;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = Color.white;
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
