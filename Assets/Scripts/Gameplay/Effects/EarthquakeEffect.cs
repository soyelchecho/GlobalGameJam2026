using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Effects
{
    /// <summary>
    /// Periodic earthquake effect: shakes the camera and emits a burst of falling dust particles.
    ///
    /// Setup via Tools > Effects > Create Earthquake Effect, or manually:
    ///   1. Add this component to any GameObject in the scene.
    ///   2. Assign the Target Camera (auto-finds Camera.main if empty).
    ///   3. Assign the Dust Particles ParticleSystem.
    ///   4. Set dustSpawnTopOffset to match your camera's orthographic size.
    ///
    /// Call TriggerShake() from any other script to trigger it manually.
    /// </summary>
    [DefaultExecutionOrder(100)] // LateUpdate runs after CameraFollowY / CameraScroller
    public class EarthquakeEffect : MonoBehaviour
    {
        [Header("Camera")]
        [Tooltip("Camera to shake. Auto-finds Camera.main if left empty.")]
        [SerializeField] private Camera targetCamera;

        [Header("Shake")]
        [Tooltip("How long each shake lasts in seconds.")]
        [SerializeField] private float shakeDuration = 0.8f;
        [Tooltip("Maximum displacement in world units at the start of the shake.")]
        [SerializeField] private float shakeIntensity = 0.15f;
        [Tooltip("Speed of the Perlin noise scroll — higher = more erratic.")]
        [SerializeField, Range(5f, 60f)] private float shakeSpeed = 25f;

        [Header("Auto Shake (Periodic)")]
        [Tooltip("Automatically trigger a shake every few seconds.")]
        [SerializeField] private bool autoShake = true;
        [SerializeField] private float minInterval = 8f;
        [SerializeField] private float maxInterval = 25f;

        [Header("Dust Particles")]
        [Tooltip("ParticleSystem configured for falling dust. Created automatically by the editor tool.")]
        [SerializeField] private ParticleSystem dustParticles;
        [Tooltip("Number of dust particles emitted per earthquake.")]
        [SerializeField] private int dustBurstCount = 60;
        [Tooltip("How many units above the camera center the dust spawns. Should match camera orthographic size or slightly above.")]
        [SerializeField] private float dustSpawnTopOffset = 6f;
        [Tooltip("Half-width (in world units) of the dust spawn area. Should cover the full screen width.")]
        [SerializeField] private float dustSpawnHalfWidth = 15f;

        [Header("Events")]
        public UnityEvent OnShakeStarted;
        public UnityEvent OnShakeEnded;

        private bool    isShaking;
        private Vector3 shakeOffset;
        private Vector3 lastShakeApplied;

        // -------------------------------------------------------

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        private void Start()
        {
            if (autoShake)
                StartCoroutine(AutoShakeRoutine());
        }

        private void Update()
        {
            // Keep the dust emitter anchored to the top edge of the camera view each frame
            if (dustParticles != null && targetCamera != null)
            {
                Vector3 camPos = targetCamera.transform.position;
                dustParticles.transform.position = new Vector3(camPos.x, camPos.y + dustSpawnTopOffset, 0f);

                // Keep emitter width in sync (update shape scale X)
                var shape = dustParticles.shape;
                shape.scale = new Vector3(dustSpawnHalfWidth * 2f, shape.scale.y, shape.scale.z);
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;

            // Undo last frame's offset so the camera follow scripts always read a clean position
            targetCamera.transform.position -= lastShakeApplied;

            // Apply current frame's offset (zero when not shaking → restores camera cleanly)
            lastShakeApplied = isShaking ? shakeOffset : Vector3.zero;
            targetCamera.transform.position += lastShakeApplied;
        }

        // -------------------------------------------------------

        private IEnumerator AutoShakeRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
                TriggerShake();
            }
        }

        /// <summary>Trigger a single earthquake shake + dust burst. Safe to call from any script.</summary>
        public void TriggerShake()
        {
            if (!isShaking)
                StartCoroutine(ShakeRoutine());

            if (dustParticles != null)
                dustParticles.Emit(dustBurstCount);
        }

        private IEnumerator ShakeRoutine()
        {
            isShaking = true;
            float timer  = 0f;
            float noiseX = Random.Range(0f, 100f);
            float noiseY = Random.Range(100f, 200f);
            OnShakeStarted?.Invoke();

            while (timer < shakeDuration)
            {
                timer += Time.deltaTime;
                float fade      = 1f - (timer / shakeDuration); // linear fade-out
                float intensity = shakeIntensity * fade;

                float x = (Mathf.PerlinNoise(noiseX + timer * shakeSpeed, 0f) - 0.5f) * 2f * intensity;
                float y = (Mathf.PerlinNoise(0f, noiseY + timer * shakeSpeed) - 0.5f) * 2f * intensity;

                shakeOffset = new Vector3(x, y, 0f);
                yield return null;
            }

            shakeOffset = Vector3.zero;
            isShaking   = false;
            OnShakeEnded?.Invoke();
        }
    }
}
