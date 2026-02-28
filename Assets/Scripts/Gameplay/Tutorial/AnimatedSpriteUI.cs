using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Tutorial
{
    /// <summary>
    /// Cycles through sprite frames on a UI Image using unscaledDeltaTime,
    /// so animation runs even when Time.timeScale == 0.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class AnimatedSpriteUI : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float fps = 8f;

        public float Fps { get => fps; set => fps = value; }

        private Image image;
        private bool playing;
        private float elapsed;
        private int currentFrame;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        public void Play()
        {
            playing = true;
        }

        public void Stop()
        {
            playing = false;
        }

        public void ResetAnimation()
        {
            elapsed = 0f;
            currentFrame = 0;
            ApplyFrame();
        }

        private void Update()
        {
            if (!playing || frames == null || frames.Length == 0) return;

            elapsed += Time.unscaledDeltaTime;
            float frameDuration = fps > 0f ? 1f / fps : 1f;

            while (elapsed >= frameDuration)
            {
                elapsed -= frameDuration;
                currentFrame = (currentFrame + 1) % frames.Length;
            }

            ApplyFrame();
        }

        private void ApplyFrame()
        {
            if (image != null && frames != null && frames.Length > 0)
                image.sprite = frames[currentFrame];
        }
    }
}
