#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WorldChaseTag.Arena;
using WorldChaseTag.Core;
using WorldChaseTag.Movement;
using WorldChaseTag.AI;
using WorldChaseTag.Combat;
using WorldChaseTag.Audio;
using WorldChaseTag.UI;
using WorldChaseTag.Testing;
using WorldChaseTag.Visuals;

namespace WorldChaseTag.Editor
{
    public static class WCTSceneBuilder
    {
        [MenuItem("World Chase Tag/Build and Setup Main Arena Scene")]
        public static void BuildMainScene()
        {
            Debug.Log("[WCTSceneBuilder] Building Official World Chase Tag MainArena Scene with High-End Visuals...");

            // Create new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. High-End Stadium Lighting & Ambient Bounce
            // Key Light (Warm arena floodlight)
            GameObject keyLightObj = new GameObject("Key Directional Light");
            Light keyLight = keyLightObj.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = new Color(1.0f, 0.94f, 0.86f);
            keyLight.intensity = 1.35f;
            keyLight.shadows = LightShadows.Soft;
            keyLight.shadowStrength = 0.90f;
            keyLight.shadowBias = 0.04f;
            keyLightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Fill Light (Cool stadium reflection fill to eliminate pitch black shadows)
            GameObject fillLightObj = new GameObject("Fill Directional Light");
            Light fillLight = fillLightObj.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(0.32f, 0.44f, 0.62f);
            fillLight.intensity = 0.45f;
            fillLight.shadows = LightShadows.None;
            fillLightObj.transform.rotation = Quaternion.Euler(-45f, 150f, 0f);

            // Tri-light ambient model for rich athletic arena atmosphere
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.42f, 0.50f, 0.65f);
            RenderSettings.ambientEquatorColor = new Color(0.22f, 0.27f, 0.35f);
            RenderSettings.ambientGroundColor = new Color(0.10f, 0.12f, 0.16f);

            // Atmospheric Stadium Haze / Volumetric Fog
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.0075f;
            RenderSettings.fogColor = new Color(0.06f, 0.08f, 0.12f);

            // Real-Time High-End Reflection Probe for floor & metallic obstacle reflections
            GameObject probeObj = new GameObject("ArenaReflectionProbe");
            ReflectionProbe probe = probeObj.AddComponent<ReflectionProbe>();
            probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
            probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
            probe.timeSlicingMode = UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.NoTimeSlicing;
            probe.size = new Vector3(32f, 16f, 32f);
            probe.transform.position = new Vector3(0f, 3.5f, 0f);
            probe.boxProjection = true;
            probe.intensity = 1.25f;
            probe.clearFlags = UnityEngine.Rendering.ReflectionProbeClearFlags.SolidColor;
            probe.backgroundColor = new Color(0.06f, 0.08f, 0.12f);

            // 2. Create Arena with PBR Materials & Court Textures
            GameObject arenaObj = new GameObject("QuadArena");
            QuadArenaBuilder arenaBuilder = arenaObj.AddComponent<QuadArenaBuilder>();
            arenaBuilder.BuildArena();

            // 3. Create Player (FPP Parkour Athlete with Animated Hands)
            GameObject playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObj.name = "Player_Athlete";
            playerObj.tag = "Player";
            playerObj.transform.position = new Vector3(0f, 0.9f, -5.2f);
            Object.DestroyImmediate(playerObj.GetComponent<Collider>());

            CharacterController playerCC = playerObj.AddComponent<CharacterController>();
            playerCC.height = 1.8f;
            playerCC.radius = 0.35f;
            playerCC.center = new Vector3(0f, 0.9f, 0f);

            GameObject camHolder = new GameObject("CameraHolder");
            camHolder.transform.parent = playerObj.transform;
            camHolder.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            Camera cam = camHolder.AddComponent<Camera>();
            cam.fieldOfView = 75f;
            cam.nearClipPlane = 0.1f;
            cam.allowHDR = true;
            cam.allowMSAA = true;
            cam.useOcclusionCulling = true;
            cam.renderingPath = RenderingPath.DeferredShading;
            camHolder.AddComponent<AudioListener>();

            // Add First-Person Animated Parkour Hands & Arms
            camHolder.AddComponent<WCTFPPArms>();

            WCTParkourController parkourController = playerObj.AddComponent<WCTParkourController>();

            // 4. Create AI Opponent with High-Visibility 3D Athlete Visuals
            GameObject aiObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            aiObj.name = "AI_Opponent_Athlete";
            aiObj.tag = "Evader";
            aiObj.transform.position = new Vector3(0f, 0.9f, 3.2f);
            Object.DestroyImmediate(aiObj.GetComponent<Collider>());

            CharacterController aiCC = aiObj.AddComponent<CharacterController>();
            aiCC.height = 1.8f;
            aiCC.radius = 0.35f;
            aiCC.center = new Vector3(0f, 0.9f, 0f);

            WCTAgentMotor aiMotor = aiObj.AddComponent<WCTAgentMotor>();
            WCTChaserAI aiChaser = aiObj.AddComponent<WCTChaserAI>();
            WCTEvaderAI aiEvader = aiObj.AddComponent<WCTEvaderAI>();

            aiChaser.SetTarget(playerObj.transform);
            aiEvader.SetChaser(playerObj.transform);

            // Attach 3D Athlete Model with jersey and overhead role marker
            WCTAthleteVisuals aiVisuals = aiObj.AddComponent<WCTAthleteVisuals>();
            aiVisuals.SetRole(ChaseRole.Evader, "KAI CHEN");

            // 5. Game Managers & Coordinators
            GameObject managerObj = new GameObject("WCT_GameManager");
            WCTMatchController matchController = managerObj.AddComponent<WCTMatchController>();
            WCTTagSystem tagSystem = managerObj.AddComponent<WCTTagSystem>();
            WCTAudioSynthesizer audioSynth = managerObj.AddComponent<WCTAudioSynthesizer>();
            WCTHUDController hud = managerObj.AddComponent<WCTHUDController>();
            WCTMenuController menu = managerObj.AddComponent<WCTMenuController>();
            WCTSceneCoordinator coordinator = managerObj.AddComponent<WCTSceneCoordinator>();
            WCTAutomatedTests tests = managerObj.AddComponent<WCTAutomatedTests>();

            // Setup Tag System & HUD initial references
            tagSystem.SetActiveTargets(playerObj.transform, aiObj.transform);
            hud.SetOpponentTransform(aiObj.transform);

            // Ensure Scenes folder exists
            if (!AssetDatabase.IsValidFolder("Assets/WorldChaseTag/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets/WorldChaseTag", "Scenes");
            }

            string scenePath = "Assets/WorldChaseTag/Scenes/MainArena.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[WCTSceneBuilder] Successfully created and saved scene at: {scenePath}");

            // Add scene to Build Settings
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = scenes;
            AssetDatabase.SaveAssets();

            Debug.Log("[WCTSceneBuilder] Build Settings updated with MainArena scene.");
        }

        [MenuItem("World Chase Tag/Build Windows Standalone Player")]
        public static void BuildWindowsPlayer()
        {
            Debug.Log("[WCTSceneBuilder] Building Main Arena scene before build...");
            BuildMainScene();

            string buildDir = "Builds";
            if (!System.IO.Directory.Exists(buildDir))
            {
                System.IO.Directory.CreateDirectory(buildDir);
            }

            string buildPath = System.IO.Path.Combine(buildDir, "WorldChaseTag.exe");
            Debug.Log($"[WCTSceneBuilder] Starting StandaloneWindows64 build to: {buildPath}");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new string[] { "Assets/WorldChaseTag/Scenes/MainArena.unity" },
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            UnityEditor.Build.Reporting.BuildSummary summary = report.summary;

            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"================ BUILD SUCCEEDED ================\nTotal size: {summary.totalSize} bytes\nOutput: {buildPath}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            else
            {
                Debug.LogError($"================ BUILD FAILED ================\nResult: {summary.result}\nTotal errors: {summary.totalErrors}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }
    }
}
#endif
