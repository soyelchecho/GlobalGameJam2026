using UnityEngine;
using UnityEditor;
using Gameplay.Effects;

namespace GameEditor
{
    public static class CreateEarthquakeEffect
    {
        [MenuItem("Tools/Effects/Create Earthquake Effect")]
        public static void Create()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("[CreateEarthquakeEffect] No Camera tagged as MainCamera found in the scene. Tag your camera first.");
                return;
            }

            // ============================================================
            // DUST PARTICLE SYSTEM (world space, position managed by EarthquakeEffect.Update)
            // ============================================================
            GameObject dustGO = new GameObject("DustParticles");
            ParticleSystem ps = dustGO.AddComponent<ParticleSystem>();
            ConfigureDustParticles(ps);
            Undo.RegisterCreatedObjectUndo(dustGO, "Create DustParticles");

            // ============================================================
            // EARTHQUAKE MANAGER
            // ============================================================
            GameObject managerGO = new GameObject("EarthquakeManager");
            EarthquakeEffect effect = managerGO.AddComponent<EarthquakeEffect>();
            Undo.RegisterCreatedObjectUndo(managerGO, "Create EarthquakeManager");

            // Wire references
            SerializedObject so = new SerializedObject(effect);
            so.FindProperty("targetCamera").objectReferenceValue   = mainCam;
            so.FindProperty("dustParticles").objectReferenceValue  = ps;
            // Default top offset to match a typical 2D orthographic size of 6
            so.FindProperty("dustSpawnTopOffset").floatValue       = mainCam.orthographic ? mainCam.orthographicSize + 1f : 7f;
            so.ApplyModifiedProperties();

            Selection.activeGameObject = managerGO;

            Debug.Log("[CreateEarthquakeEffect] Done!\n" +
                      "• Adjust 'Dust Spawn Top Offset' on EarthquakeManager to match your camera's Orthographic Size.\n" +
                      "• Adjust 'Dust Spawn Half Width' to cover your screen width in world units.\n" +
                      "• Enable 'Auto Shake' for periodic earthquakes, or call TriggerShake() from code.\n" +
                      "• You can customize the DustParticles particle system colors/size freely.");
        }

        private static void ConfigureDustParticles(ParticleSystem ps)
        {
            // ---- Main ----
            var main = ps.main;
            main.loop          = false;
            main.playOnAwake   = false;
            main.startDelay    = 0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 4.5f);
            main.startSpeed    = new ParticleSystem.MinMaxCurve(0.2f, 1.5f);
            main.startSize     = new ParticleSystem.MinMaxCurve(0.06f, 0.22f);
            main.startColor    = new ParticleSystem.MinMaxGradient(
                new Color(0.90f, 0.82f, 0.68f, 0.95f),  // light sandy dust
                new Color(0.60f, 0.52f, 0.40f, 0.45f)   // darker, semi-transparent
            );
            main.startRotation   = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
            main.gravityModifier = new ParticleSystem.MinMaxCurve(0.35f, 0.80f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles    = 400;

            // ---- Emission: no continuous rate, only burst from code via Emit() ----
            var emission = ps.emission;
            emission.enabled      = true;
            emission.rateOverTime = 0;

            // ---- Shape: thin horizontal box at top of camera ----
            var shape = ps.shape;
            shape.enabled   = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale     = new Vector3(30f, 0.05f, 0.05f); // wide, thin horizontal line

            // ---- Velocity over lifetime: slight horizontal drift + extra downward push ----
            var vol = ps.velocityOverLifetime;
            vol.enabled = true;
            vol.space   = ParticleSystemSimulationSpace.World;
            vol.x       = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);   // left/right drift
            vol.y       = new ParticleSystem.MinMaxCurve(-0.3f, -1.5f);  // extra downward
            vol.z       = 0f;

            // ---- Color over lifetime: quick fade-in, slow fade-out ----
            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0f,   0.00f),
                    new GradientAlphaKey(1f,   0.08f),
                    new GradientAlphaKey(0.8f, 0.55f),
                    new GradientAlphaKey(0f,   1.00f)
                }
            );
            col.color = new ParticleSystem.MinMaxGradient(gradient);

            // ---- Size over lifetime: pop in, then shrink as they fall ----
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0.00f, 0.1f),
                new Keyframe(0.12f, 1.0f),
                new Keyframe(1.00f, 0.3f)
            ));

            // ---- Rotation over lifetime: slow tumble ----
            var rol = ps.rotationOverLifetime;
            rol.enabled = true;
            rol.z = new ParticleSystem.MinMaxCurve(
                -60f * Mathf.Deg2Rad,   // -60 deg/s
                 60f * Mathf.Deg2Rad    // +60 deg/s
            );

            // ---- Renderer: use Sprites/Default for correct 2D sort ----
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode   = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 30; // render on top of sprites

            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
                renderer.sharedMaterial = new Material(spriteShader);
        }
    }
}
