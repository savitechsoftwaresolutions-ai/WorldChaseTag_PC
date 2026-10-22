#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
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
            playerObj.transform.position = new Vector3(0f, 1.75f, -4.15f);
            Object.DestroyImmediate(playerObj.GetComponent<Collider>());

            CharacterController playerCC = playerObj.AddComponent<CharacterController>();
            playerCC.height = 1.8f;
            playerCC.radius = 0.35f;
            playerCC.center = new Vector3(0f, 0.9f, 0f);
            playerCC.stepOffset = 0.5f;
            playerCC.slopeLimit = 60f;
            playerCC.skinWidth = 0.015f;
            playerCC.minMoveDistance = 0f;

            GameObject camHolder = new GameObject("CameraHolder");
            camHolder.transform.parent = playerObj.transform;
            camHolder.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            Camera cam = camHolder.AddComponent<Camera>();
            cam.fieldOfView = 75f;
            cam.nearClipPlane = 0.02f;
            cam.allowHDR = true;
            cam.allowMSAA = true;
            cam.useOcclusionCulling = true;
            cam.renderingPath = RenderingPath.UsePlayerSettings;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.10f, 0.14f);
            camHolder.AddComponent<AudioListener>();

            // Add First-Person Animated Parkour Hands & Arms
            camHolder.AddComponent<WCTFPPArms>();

            WCTParkourController parkourController = playerObj.AddComponent<WCTParkourController>();

            // 4. Create AI Opponent with High-Visibility 3D Athlete Visuals
            GameObject aiObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            aiObj.name = "AI_Opponent_Athlete";
            aiObj.transform.position = new Vector3(0f, 0.9f, 3.2f);
            Object.DestroyImmediate(aiObj.GetComponent<Collider>());

            CharacterController aiCC = aiObj.AddComponent<CharacterController>();
            aiCC.height = 1.8f;
            aiCC.radius = 0.35f;
            aiCC.center = new Vector3(0f, 0.9f, 0f);
            aiCC.stepOffset = 0.5f;
            aiCC.slopeLimit = 60f;
            aiCC.skinWidth = 0.03f;
            aiCC.minMoveDistance = 0f;

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

        [MenuItem("World Chase Tag/Import Snaps Sci-Fi Industrial Package")]
        public static void ImportSnapsPackage()
        {
            string packagePath = @"C:\Users\SURYAWANSHI\Downloads\Snaps Prototype Sci-Fi Industrial.unitypackage";
            if (!System.IO.File.Exists(packagePath))
            {
                Debug.LogError($"[WCTSceneBuilder] Package not found at: {packagePath}");
                return;
            }
            Debug.Log($"[WCTSceneBuilder] Importing Snaps Sci-Fi Industrial package from: {packagePath}");
            AssetDatabase.ImportPackage(packagePath, false);
            Debug.Log("[WCTSceneBuilder] Package imported successfully.");
        }

        [MenuItem("World Chase Tag/Upgrade Snaps Materials to URP")]
        public static void UpgradeSnapsMaterialsToURP()
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("[WCTSceneBuilder] Universal Render Pipeline/Lit shader not found!");
                return;
            }

            string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/AssetStoreOriginals" });
            int upgraded = 0;
            foreach (string guid in matGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null && mat.shader != urpLit)
                {
                    Color col = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
                    float metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0.5f;
                    float smoothness = mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness") : 0.5f;
                    Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;

                    mat.shader = urpLit;
                    mat.SetColor("_BaseColor", col);
                    mat.SetFloat("_Metallic", metallic);
                    mat.SetFloat("_Smoothness", smoothness);
                    if (mainTex != null) mat.SetTexture("_BaseMap", mainTex);

                    EditorUtility.SetDirty(mat);
                    upgraded++;
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[WCTSceneBuilder] Upgraded {upgraded} materials to Universal Render Pipeline Lit.");
        }

        [MenuItem("World Chase Tag/Build Windows Standalone Player")]
        public static void BuildWindowsPlayer()
        {
            Debug.Log("[WCTSceneBuilder] Switching Active Target to StandaloneWindows64...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.resizableWindow = true;

            Debug.Log("[WCTSceneBuilder] Building Main Arena scene before Windows PC build...");
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

        [MenuItem("World Chase Tag/Build Android APK")]
        public static void BuildAndroidAPK()
        {
            Debug.Log("[WCTSceneBuilder] Preparing official Android APK Build for World Chase Tag...");

            // 1. Ensure scene exists and is built
            BuildMainScene();

            // 2. Set Android SDK, NDK, and JDK roots in EditorPrefs
            string sdkRoot = @"C:\Program Files\Unity\Hub\Editor\6000.5.6f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK";
            string ndkRoot = @"C:\Program Files\Unity\Hub\Editor\6000.5.6f1\Editor\Data\PlaybackEngines\AndroidPlayer\NDK";
            string jdkRoot = @"C:\Program Files\Unity\Hub\Editor\6000.5.6f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK";

            if (System.IO.Directory.Exists(sdkRoot)) EditorPrefs.SetString("AndroidSdkRoot", sdkRoot);
            if (System.IO.Directory.Exists(ndkRoot)) EditorPrefs.SetString("AndroidNdkRoot", ndkRoot);
            if (System.IO.Directory.Exists(jdkRoot)) EditorPrefs.SetString("JdkPath", jdkRoot);

            // 3. Android Player Settings
            PlayerSettings.companyName = "WorldChaseTag";
            PlayerSettings.productName = "World Chase Tag";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.worldchasetag.championship");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;

            // Target APK (not App Bundle)
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            PlayerSettings.Android.optimizedFramePacing = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64;

            string buildDir = "Builds";
            if (!System.IO.Directory.Exists(buildDir))
            {
                System.IO.Directory.CreateDirectory(buildDir);
            }

            string apkPath = System.IO.Path.Combine(buildDir, "WorldChaseTag.apk");
            Debug.Log($"[WCTSceneBuilder] Starting Android APK build to: {apkPath}");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new string[] { "Assets/WorldChaseTag/Scenes/MainArena.unity" },
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            UnityEditor.Build.Reporting.BuildSummary summary = report.summary;

            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"================ ANDROID BUILD SUCCEEDED ================\nTotal size: {summary.totalSize} bytes\nOutput: {apkPath}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            else
            {
                Debug.LogError($"================ ANDROID BUILD FAILED ================\nResult: {summary.result}\nTotal errors: {summary.totalErrors}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }
    }
}
#endif
