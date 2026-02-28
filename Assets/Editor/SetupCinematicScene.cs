using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor;
using Gameplay.Cinematics;

namespace GameEditor
{
    public static class SetupCinematicScene
    {
        private const string RenderTexturePath = "Assets/_Project/Art/CinematicRenderTexture.renderTexture";

        [MenuItem("Tools/UI/Setup Level1 Cinematic Scene")]
        public static void Setup()
        {
            // =============================================
            // RENDER TEXTURE (asset persistente)
            // =============================================
            RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(RenderTexturePath);
            if (rt == null)
            {
                rt = new RenderTexture(1080, 1920, 0);
                rt.name = "CinematicRenderTexture";
                AssetDatabase.CreateAsset(rt, RenderTexturePath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[SetupCinematic] RenderTexture creada en {RenderTexturePath}");
            }

            // =============================================
            // CANVAS (fondo negro + video)
            // =============================================
            GameObject canvasGO = new GameObject("CinematicCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Fondo negro (por si el video tiene letterbox)
            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            Image bg = bgGO.AddComponent<Image>();
            bg.color = Color.black;
            bg.raycastTarget = false;
            RectTransform bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;

            // RawImage que muestra el video
            GameObject rawGO = new GameObject("VideoDisplay");
            rawGO.transform.SetParent(canvasGO.transform, false);
            RawImage rawImage = rawGO.AddComponent<RawImage>();
            rawImage.texture = rt;
            rawImage.raycastTarget = false;
            RectTransform rawRect = rawGO.GetComponent<RectTransform>();
            rawRect.anchorMin = Vector2.zero;
            rawRect.anchorMax = Vector2.one;
            rawRect.offsetMin = rawRect.offsetMax = Vector2.zero;

            // =============================================
            // VIDEO PLAYER
            // =============================================
            VideoPlayer vp = Object.FindObjectOfType<VideoPlayer>();
            if (vp == null)
            {
                GameObject vpGO = new GameObject("VideoPlayer");
                vp = vpGO.AddComponent<VideoPlayer>();
            }

            vp.renderMode = VideoRenderMode.RenderTexture;
            vp.targetTexture = rt;
            vp.playOnAwake = true;
            vp.isLooping = false;
            vp.audioOutputMode = VideoAudioOutputMode.AudioSource;

            // AudioSource para el audio del video
            AudioSource audioSource = vp.gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = vp.gameObject.AddComponent<AudioSource>();
            vp.SetTargetAudioSource(0, audioSource);

            // =============================================
            // CONTROLLER
            // =============================================
            Level1CinematicController ctrl = Object.FindObjectOfType<Level1CinematicController>();
            if (ctrl == null)
                ctrl = vp.gameObject.AddComponent<Level1CinematicController>();

            SerializedObject so = new SerializedObject(ctrl);
            so.FindProperty("videoPlayer").objectReferenceValue = vp;
            so.ApplyModifiedProperties();

            // =============================================
            // REGISTRO UNDO Y SELECCIÓN
            // =============================================
            Undo.RegisterCreatedObjectUndo(canvasGO, "Setup Cinematic Scene");
            Selection.activeGameObject = vp.gameObject;

            Debug.Log("[SetupCinematic] Escena configurada. Asigna el clip .mov al VideoPlayer en el Inspector.");
        }
    }
}
