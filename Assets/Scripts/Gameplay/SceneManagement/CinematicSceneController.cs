using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Gameplay.SceneManagement
{
    /// <summary>
    /// Plays a fullscreen video clip and loads the main menu when it finishes.
    /// Tap/click skips to the end.
    /// </summary>
    public class CinematicSceneController : MonoBehaviour
    {
        [Header("Video")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RawImage displayImage;

        [Header("Navigation")]
        [SerializeField] private string sceneAfterCinematic = "MainMenu";

        private RenderTexture renderTexture;
        private bool isFinishing = false;

        private void Start()
        {
            SetupVideo();
        }

        private void SetupVideo()
        {
            if (videoPlayer == null || displayImage == null) return;

            renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
            videoPlayer.targetTexture = renderTexture;
            displayImage.texture = renderTexture;

            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Prepare();
            StartCoroutine(PlayWhenReady());
        }

        private IEnumerator PlayWhenReady()
        {
            while (!videoPlayer.isPrepared)
                yield return null;

            videoPlayer.Play();
        }

        private void Update()
        {
            if (isFinishing) return;

            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                Finish();
            }
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            Finish();
        }

        private void Finish()
        {
            if (isFinishing) return;
            isFinishing = true;

            videoPlayer.Stop();
            SceneManager.LoadScene(sceneAfterCinematic);
        }

        private void OnDestroy()
        {
            if (videoPlayer != null)
                videoPlayer.loopPointReached -= OnVideoFinished;

            if (renderTexture != null)
                renderTexture.Release();
        }
    }
}
