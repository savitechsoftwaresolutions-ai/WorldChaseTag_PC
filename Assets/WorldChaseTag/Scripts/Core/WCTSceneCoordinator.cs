using UnityEngine;
using WorldChaseTag.Movement;
using WorldChaseTag.AI;
using WorldChaseTag.Combat;
using WorldChaseTag.Arena;
using WorldChaseTag.Audio;
using WorldChaseTag.UI;
using WorldChaseTag.Visuals;

namespace WorldChaseTag.Core
{
    public class WCTSceneCoordinator : MonoBehaviour
    {
        public static WCTSceneCoordinator Instance { get; private set; }

        [Header("Arena & World")]
        [SerializeField] private QuadArenaBuilder arenaBuilder;

        [Header("Actors")]
        [SerializeField] private WCTParkourController playerActor;
        [SerializeField] private GameObject aiActorObject;

        private WCTChaserAI aiChaser;
        private WCTEvaderAI aiEvader;
        private WCTAgentMotor aiMotor;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            SetupCoreSystems();
        }

        private void SetupCoreSystems()
        {
            // Enforce High-End PC Ultra Graphics at runtime
            QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1, true);
            QualitySettings.vSyncCount = 1;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            QualitySettings.antiAliasing = 8;
            QualitySettings.pixelLightCount = 16;

            // Ensure MatchController
            if (FindFirstObjectByType<WCTMatchController>() == null)
            {
                gameObject.AddComponent<WCTMatchController>();
            }

            // Ensure TagSystem
            if (FindFirstObjectByType<WCTTagSystem>() == null)
            {
                gameObject.AddComponent<WCTTagSystem>();
            }

            // Ensure AudioSynthesizer
            if (FindFirstObjectByType<WCTAudioSynthesizer>() == null)
            {
                gameObject.AddComponent<WCTAudioSynthesizer>();
            }

            // Ensure HUDController
            if (FindFirstObjectByType<WCTHUDController>() == null)
            {
                gameObject.AddComponent<WCTHUDController>();
            }

            // Ensure MenuController
            if (FindFirstObjectByType<WCTMenuController>() == null)
            {
                gameObject.AddComponent<WCTMenuController>();
            }
        }

        private void Start()
        {
            if (arenaBuilder == null)
            {
                arenaBuilder = FindFirstObjectByType<QuadArenaBuilder>();
                if (arenaBuilder == null)
                {
                    GameObject arenaObj = new GameObject("QuadArena");
                    arenaBuilder = arenaObj.AddComponent<QuadArenaBuilder>();
                    arenaBuilder.BuildArena();
                }
            }

            EnsureActorsExist();

            // Subscribe to Match Controller events
            if (WCTMatchController.Instance != null)
            {
                WCTMatchController.Instance.OnAthletesReady += SetupChaseActors;
                WCTMatchController.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (WCTMatchController.Instance != null)
            {
                WCTMatchController.Instance.OnAthletesReady -= SetupChaseActors;
                WCTMatchController.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void EnsureActorsExist()
        {
            // Find or create Player
            if (playerActor == null)
            {
                playerActor = FindFirstObjectByType<WCTParkourController>();
            }

            if (playerActor == null)
            {
                playerActor = CreatePlayerGameObject(new Vector3(0f, 0.5f, -5.2f));
            }

            // Ensure First-Person Arms on Player Camera
            if (playerActor != null && playerActor.GetComponentInChildren<WCTFPPArms>() == null)
            {
                Camera cam = playerActor.GetComponentInChildren<Camera>();
                if (cam != null && cam.GetComponent<WCTFPPArms>() == null)
                {
                    cam.gameObject.AddComponent<WCTFPPArms>();
                }
            }

            // Find or create AI Actor
            if (aiActorObject == null)
            {
                WCTAgentMotor foundMotor = FindFirstObjectByType<WCTAgentMotor>();
                if (foundMotor != null)
                {
                    aiActorObject = foundMotor.gameObject;
                }
            }

            if (aiActorObject == null)
            {
                aiActorObject = CreateAIActorGameObject(new Vector3(0f, 0.5f, 3.2f));
            }

            // Ensure High-Visibility 3D Athlete Visuals on AI Opponent
            if (aiActorObject != null && aiActorObject.GetComponent<WCTAthleteVisuals>() == null)
            {
                aiActorObject.AddComponent<WCTAthleteVisuals>();
            }

            aiChaser = aiActorObject.GetComponent<WCTChaserAI>();
            aiEvader = aiActorObject.GetComponent<WCTEvaderAI>();
            aiMotor = aiActorObject.GetComponent<WCTAgentMotor>();

            // Wire AI targets
            if (aiChaser != null) aiChaser.SetTarget(playerActor.transform);
            if (aiEvader != null) aiEvader.SetChaser(playerActor.transform);

            // Link opponent to HUD Radar
            if (WCTHUDController.Instance != null && aiActorObject != null)
            {
                WCTHUDController.Instance.SetOpponentTransform(aiActorObject.transform);
            }
        }

        private WCTParkourController CreatePlayerGameObject(Vector3 position)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "WCT_Player";
            player.tag = "Player";
            player.transform.position = position;

            // Remove default capsule collider, CharacterController acts as collider
            Collider col = player.GetComponent<Collider>();
            if (col != null) Destroy(col);

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            // Camera Holder
            GameObject camHolder = new GameObject("CameraHolder");
            camHolder.transform.parent = player.transform;
            camHolder.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            Camera cam = camHolder.AddComponent<Camera>();
            camHolder.AddComponent<AudioListener>();
            camHolder.AddComponent<WCTFPPArms>();

            WCTParkourController pc = player.AddComponent<WCTParkourController>();
            return pc;
        }

        private GameObject CreateAIActorGameObject(Vector3 position)
        {
            GameObject ai = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            ai.name = "WCT_AIOpponent";
            ai.transform.position = position;

            Collider col = ai.GetComponent<Collider>();
            if (col != null) Destroy(col);

            CharacterController cc = ai.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            ai.AddComponent<WCTAgentMotor>();
            ai.AddComponent<WCTChaserAI>();
            ai.AddComponent<WCTEvaderAI>();
            ai.AddComponent<WCTAthleteVisuals>();

            return ai;
        }

        private void SetupChaseActors(WCTAthlete chaser, WCTAthlete evader)
        {
            if (arenaBuilder == null || playerActor == null || aiActorObject == null) return;

            Vector3 chaserPos = arenaBuilder.ChaserPlateSpawn != null ? arenaBuilder.ChaserPlateSpawn.position : new Vector3(0f, 0.85f, -4.15f);
            Vector3 evaderPos = arenaBuilder.EvaderSpawn != null ? arenaBuilder.EvaderSpawn.position : new Vector3(0f, 0.1f, 3.2f);

            var pCC = playerActor.GetComponent<CharacterController>();
            if (pCC != null)
            {
                pCC.stepOffset = 0.5f;
                pCC.slopeLimit = 60f;
                pCC.skinWidth = 0.03f;
            }

            var aCC = aiActorObject.GetComponent<CharacterController>();
            if (aCC != null)
            {
                aCC.stepOffset = 0.5f;
                aCC.slopeLimit = 60f;
                aCC.skinWidth = 0.03f;
            }

            bool isPlayerEvader = (WCTMatchController.Instance.ActiveEvaderTeam == 0);

            if (isPlayerEvader)
            {
                // Player is Evader
                playerActor.CurrentRole = ChaseRole.Evader;
                playerActor.TeleportTo(evaderPos, Quaternion.Euler(0f, 180f, 0f));

                aiChaser.IsActiveChaser = true;
                aiEvader.IsActiveEvader = false;
                aiChaser.SetTarget(playerActor.transform);
                aiMotor.TeleportTo(chaserPos, Quaternion.identity);

                WCTTagSystem.Instance?.SetActiveTargets(aiActorObject.transform, playerActor.transform);
            }
            else
            {
                // Player is Chaser
                playerActor.CurrentRole = ChaseRole.Chaser;
                playerActor.TeleportTo(chaserPos, Quaternion.identity);

                aiChaser.IsActiveChaser = false;
                aiEvader.IsActiveEvader = true;
                aiEvader.SetChaser(playerActor.transform);
                aiMotor.TeleportTo(evaderPos, Quaternion.Euler(0f, 180f, 0f));

                WCTTagSystem.Instance?.SetActiveTargets(playerActor.transform, aiActorObject.transform);
            }

            // Update AI Opponent Visuals (Jersey Color, Role Badge, Name)
            var aiVisuals = aiActorObject.GetComponent<WCTAthleteVisuals>();
            if (aiVisuals != null)
            {
                ChaseRole oppRole = isPlayerEvader ? ChaseRole.Chaser : ChaseRole.Evader;
                string oppName = (isPlayerEvader ? chaser : evader)?.Name ?? "OPPONENT";
                aiVisuals.SetRole(oppRole, oppName);
            }

            // Link Opponent to HUD Radar / Marker
            if (WCTHUDController.Instance != null)
            {
                WCTHUDController.Instance.SetOpponentTransform(aiActorObject.transform);
            }
        }

        private void HandleStateChanged(WCTMatchState state)
        {
            if (playerActor == null) return;

            if (state == WCTMatchState.PRE_CHASE)
            {
                // Freeze controls during 3-2-1 countdown
                playerActor.ControlsEnabled = false;
            }
            else if (state == WCTMatchState.CHASE_ACTIVE)
            {
                // Unlock controls on buzzer GO!
                playerActor.ControlsEnabled = true;
            }
            else if (state == WCTMatchState.CHASE_RESULT || state == WCTMatchState.MATCH_RESULT)
            {
                playerActor.ControlsEnabled = false;
            }
        }
    }
}
