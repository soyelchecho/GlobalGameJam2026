using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Gameplay.Tutorial;

namespace GameEditor
{
    public static class CreateTutorialHintManager
    {
        [MenuItem("Tools/UI/Create Tutorial Hint Manager")]
        public static void Create()
        {
            // =============================================
            // ROOT CANVAS
            // =============================================
            GameObject canvasGO = new GameObject("TutorialHintManager");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // =============================================
            // HINT PANEL (empieza inactivo)
            // pivot inferior-centro → queda encima del jugador
            // =============================================
            GameObject panelGO = new GameObject("HintPanel");
            panelGO.transform.SetParent(canvasGO.transform, false);

            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.65f);

            RectTransform hintPanel = panelGO.GetComponent<RectTransform>();
            hintPanel.pivot = new Vector2(0.5f, 0.5f);
            hintPanel.anchorMin = Vector2.zero;
            hintPanel.anchorMax = Vector2.zero;
            hintPanel.sizeDelta = new Vector2(500f, 380f);

            // =============================================
            // ANIM IMAGE + ANIMATOR (parte superior del panel)
            // Animator en UnscaledTime para funcionar con timeScale = 0
            // =============================================
            GameObject animGO = new GameObject("AnimImage");
            animGO.transform.SetParent(panelGO.transform, false);

            animGO.AddComponent<Image>();
            Animator hintAnimator = animGO.AddComponent<Animator>();
            hintAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

            RectTransform animRect = animGO.GetComponent<RectTransform>();
            animRect.anchorMin = new Vector2(0.5f, 1f);
            animRect.anchorMax = new Vector2(0.5f, 1f);
            animRect.pivot = new Vector2(0.5f, 1f);
            animRect.anchoredPosition = new Vector2(0f, -20f);
            animRect.sizeDelta = new Vector2(240f, 240f);

            // =============================================
            // HINT TEXT (parte inferior del panel)
            // =============================================
            GameObject textGO = new GameObject("HintText");
            textGO.transform.SetParent(panelGO.transform, false);

            Text hintText = textGO.AddComponent<Text>();
            hintText.fontSize = 48;
            hintText.color = Color.white;
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.resizeTextForBestFit = false;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 0f);
            textRect.pivot = new Vector2(0.5f, 0f);
            textRect.anchoredPosition = new Vector2(0f, 20f);
            textRect.sizeDelta = new Vector2(-60f, 120f);

            panelGO.SetActive(false);

            // =============================================
            // WIRE UP TutorialHintManager COMPONENT
            // =============================================
            TutorialHintManager manager = canvasGO.AddComponent<TutorialHintManager>();

            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("hintPanel").objectReferenceValue = hintPanel;
            so.FindProperty("hintAnimator").objectReferenceValue = hintAnimator;
            so.FindProperty("hintText").objectReferenceValue = hintText;
            so.ApplyModifiedProperties();

            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Tutorial Hint Manager");
            Selection.activeGameObject = canvasGO;

            Debug.Log("[CreateTutorialHintManager] Creado! En TutorialHintTrigger asigna el AnimatorController y el nombre del estado a reproducir.");
        }
    }
}
