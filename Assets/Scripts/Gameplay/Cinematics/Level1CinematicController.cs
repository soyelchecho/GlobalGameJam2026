using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using Gameplay.UI;

namespace Gameplay.Cinematics
{
    /// <summary>
    /// Controla la cinemática de introducción al Level 1.
    /// Coloca este script en la escena intermedia junto a un VideoPlayer.
    /// Al terminar (o al hacer tap para skip) carga la escena indicada.
    /// </summary>
    public class Level1CinematicController : MonoBehaviour
    {
        [Header("Video")]
        [SerializeField] private VideoPlayer videoPlayer;

        [Header("Navigation")]
        [Tooltip("Escena a cargar cuando termine la cinemática")]
        [SerializeField] private string nextSceneName = "Level1";

        [Header("Skip")]
        [Tooltip("Permitir saltar la cinemática con tap")]
        [SerializeField] private bool allowSkip = true;
        [Tooltip("Segundos antes de habilitar el skip (evita saltos accidentales al entrar)")]
        [SerializeField] private float skipDelay = 1f;

        private bool finished;
        private bool canSkip;

        private void Start()
        {
            if (videoPlayer == null)
            {
                Debug.LogWarning("[Level1CinematicController] VideoPlayer no asignado, cargando nivel directamente.");
                LoadNext();
                return;
            }

            videoPlayer.loopPointReached += OnVideoEnd;
            videoPlayer.Play();

            if (allowSkip)
                StartCoroutine(EnableSkipAfterDelay());
        }

        private void OnDestroy()
        {
            if (videoPlayer != null)
                videoPlayer.loopPointReached -= OnVideoEnd;
        }

        private IEnumerator EnableSkipAfterDelay()
        {
            yield return new WaitForSecondsRealtime(skipDelay);
            canSkip = true;
        }

        private void Update()
        {
            if (!canSkip || finished) return;

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                LoadNext();

#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
                LoadNext();
#endif
        }

        private void OnVideoEnd(VideoPlayer vp)
        {
            LoadNext();
        }

        private void LoadNext()
        {
            if (finished) return;
            finished = true;

            if (videoPlayer != null)
                videoPlayer.Stop();

            if (LoadingScreen.Instance != null)
                LoadingScreen.Instance.LoadScene(nextSceneName);
            else
                SceneManager.LoadScene(nextSceneName);
        }
    }
}
