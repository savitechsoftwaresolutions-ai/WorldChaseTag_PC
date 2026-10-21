using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldChaseTag.Core
{
    public class WCTMatchController : MonoBehaviour
    {
        public static WCTMatchController Instance { get; private set; }

        [Header("Match Settings")]
        [SerializeField] private GameMode gameMode = GameMode.FullTournamentMatch;
        [SerializeField] private AIDifficulty aiDifficulty = AIDifficulty.Normal;
        [SerializeField, Min(1f)] private float chaseDuration = 20.0f;
        [SerializeField, Min(1)] private int maxChases = 16;
        [SerializeField, Min(0f)] private float preChaseDuration = 3.0f;
        [SerializeField, Min(0f)] private float resultDisplayDuration = 6.0f;
        [SerializeField, Min(0f)] private float betweenChasesInterval = 2.5f; // Fast digital transition (configurable)

        [Header("Runtime State")]
        [SerializeField] private WCTMatchState currentState = WCTMatchState.LOBBY;
        [SerializeField] private int currentChaseNumber = 0;
        [SerializeField] private int teamAScore = 0; // Evasion points
        [SerializeField] private int teamBScore = 0;
        [SerializeField] private float chaseTimeRemaining = 20.0f;
        [SerializeField] private float subTimer = 0.0f;
        [SerializeField] private int activeEvaderTeam = 0; // 0 = Team A (Alpha), 1 = Team B (Omega)
        [SerializeField] private int activeChaserTeam = 1;
        [SerializeField] private ChaseOutcome lastOutcome = ChaseOutcome.None;

        [Header("Sudden Death Chase-Off")]
        [SerializeField] private bool isSuddenDeath = false;
        [SerializeField] private int suddenDeathChaseCount = 0;
        [SerializeField] private float suddenDeathTeamATime = 0f;
        [SerializeField] private float suddenDeathTeamBTime = 0f;

        // Active Athletes
        public WCTAthlete CurrentChaserAthlete { get; private set; }
        public WCTAthlete CurrentEvaderAthlete { get; private set; }

        // Rosters (Super Six)
        public List<WCTAthlete> TeamARoster { get; private set; } = new List<WCTAthlete>();
        public List<WCTAthlete> TeamBRoster { get; private set; } = new List<WCTAthlete>();
        public List<ChaseRecord> MatchHistory { get; private set; } = new List<ChaseRecord>();

        private int nextTeamAChaserIndex = 0;
        private int nextTeamBChaserIndex = 0;
        private bool hasResolvedCurrentChase = false;
        private int lastCountdownSec = -1;

        // Events
        public event Action<WCTMatchState> OnStateChanged;
        public event Action<float> OnTimerTick;
        public event Action<int, int> OnScoreChanged;
        public event Action<ChaseRecord> OnChaseResultCommitted;
        public event Action<string> OnBannerAnnouncement;
        public event Action<WCTAthlete, WCTAthlete> OnAthletesReady;

        public WCTMatchState State => currentState;
        public int ChaseNumber => currentChaseNumber;
        public int MaxChases => maxChases;
        public int TeamAScore => teamAScore;
        public int TeamBScore => teamBScore;
        public float TimeRemaining => chaseTimeRemaining;
        public float SubTimer => subTimer;
        public int ActiveEvaderTeam => activeEvaderTeam;
        public int ActiveChaserTeam => activeChaserTeam;
        public ChaseOutcome LastOutcome => lastOutcome;
        public AIDifficulty Difficulty => aiDifficulty;
        public GameMode Mode => gameMode;
        public bool IsSuddenDeath => isSuddenDeath;
        public bool IsChaseActive => currentState == WCTMatchState.CHASE_ACTIVE;

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

            InitializeRosters();
        }

        private void Start()
        {
            if (currentState == WCTMatchState.LOBBY)
            {
                // Auto-advance to match setup if launched directly
                StartMatch(gameMode, aiDifficulty);
            }
        }

        private void Update()
        {
            switch (currentState)
            {
                case WCTMatchState.PRE_CHASE:
                    UpdatePreChase();
                    break;

                case WCTMatchState.CHASE_ACTIVE:
                    UpdateChaseActive();
                    break;

                case WCTMatchState.CHASE_RESULT:
                    UpdateChaseResult();
                    break;

                case WCTMatchState.ROLE_ROTATION:
                    UpdateRoleRotation();
                    break;
            }
        }

        public void SetDifficulty(AIDifficulty difficulty)
        {
            aiDifficulty = difficulty;
        }

        public void SetGameMode(GameMode mode)
        {
            gameMode = mode;
            if (mode == GameMode.Practice)
            {
                maxChases = 1;
            }
            else if (mode == GameMode.QuickMatch)
            {
                maxChases = 4;
            }
            else
            {
                maxChases = 16;
            }
        }

        public void InitializeRosters()
        {
            TeamARoster.Clear();
            TeamBRoster.Clear();

            // Team Alpha (Blue) Super Six - Reference Image 4 Athlete Cards
            TeamARoster.Add(new WCTAthlete("RYAN", 0, 1.05f, 1.0f, 91, "Balanced & Fast", 80, 70, 70, 80, true, "A1", 60, "Quick Recovery", "Faster stamina recovery after slide or vault."));
            TeamARoster.Add(new WCTAthlete("KAI", 0, 1.02f, 0.95f, 89, "Precision Vaulter", 80, 60, 90, 90, false, "A2", 80, "Apex Leap", "Increased hurdle vault launch velocity."));
            TeamARoster.Add(new WCTAthlete("ZARA", 0, 1.04f, 1.00f, 92, "Apex Sprinter", 90, 70, 80, 80, false, "A3", 60, "Burst Acceleration", "Reaches maximum sprint velocity 30% faster."));
            TeamARoster.Add(new WCTAthlete("LEO", 0, 0.98f, 1.05f, 88, "Evasion Master", 70, 80, 90, 85, false, "A4", 70, "Agile Slide", "Extended slide distance and lower hit-box."));
            TeamARoster.Add(new WCTAthlete("SAMIR", 0, 1.03f, 0.98f, 90, "Flank Runner", 80, 80, 80, 80, false, "A5", 70, "Tight Cornering", "Conserves momentum through sharp 90-degree turns."));
            TeamARoster.Add(new WCTAthlete("ELI", 0, 1.01f, 1.05f, 87, "Anchor Defender", 70, 90, 70, 80, false, "A6", 80, "Iron Lung", "Significantly slower stamina drain when exhausted."));

            // Team Omega (Orange) Super Six - Reference Image 4 Athlete Cards
            TeamBRoster.Add(new WCTAthlete("DANTE", 1, 1.04f, 1.0f, 92, "Power & Aggression", 90, 80, 70, 85, true, "O1", 70, "Power Vault", "Maintains forward sprint momentum through all hurdles."));
            TeamBRoster.Add(new WCTAthlete("RAVI", 1, 1.02f, 1.05f, 90, "Dominant Pursuit", 80, 80, 80, 85, false, "O2", 70, "Relentless Chaser", "Reduces corner deceleration during continuous pursuit."));
            TeamBRoster.Add(new WCTAthlete("MAYA", 1, 1.00f, 0.95f, 89, "Acrobatic Chaser", 80, 70, 90, 90, false, "O3", 70, "Cat Balance", "Instant recovery upon landing from high platforms."));
            TeamBRoster.Add(new WCTAthlete("TYLER", 1, 1.06f, 0.92f, 91, "High Pressure Anchor", 80, 90, 70, 80, false, "O4", 80, "Extended Reach", "Increases effective tag cone angle and radius."));
            TeamBRoster.Add(new WCTAthlete("NICO", 1, 0.99f, 1.08f, 88, "Endurance Chaser", 80, 90, 80, 80, false, "O5", 70, "Second Wind", "Recovers 50% stamina instantly upon entering final 5s."));
            TeamBRoster.Add(new WCTAthlete("JAX", 1, 1.03f, 1.00f, 90, "Dynamic Vaulter", 90, 70, 80, 90, false, "O6", 70, "Spring Ledge", "Instantly pulls up onto high ledges without pause."));

            nextTeamAChaserIndex = 0;
            nextTeamBChaserIndex = 0;
        }

        public void StartMatch(GameMode mode, AIDifficulty difficulty)
        {
            if (TeamARoster == null || TeamARoster.Count == 0 || TeamBRoster == null || TeamBRoster.Count == 0)
            {
                InitializeRosters();
            }

            SetGameMode(mode);
            SetDifficulty(difficulty);

            currentChaseNumber = 0;
            teamAScore = 0;
            teamBScore = 0;
            suddenDeathChaseCount = 0;
            suddenDeathTeamATime = 0f;
            suddenDeathTeamBTime = 0f;
            isSuddenDeath = false;
            MatchHistory.Clear();

            foreach (var a in TeamARoster) a.ResetStats();
            foreach (var b in TeamBRoster) b.ResetStats();

            TransitionState(WCTMatchState.ROLE_SELECTION);
        }

        private void TransitionState(WCTMatchState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(currentState);

            switch (newState)
            {
                case WCTMatchState.ROLE_SELECTION:
                    ExecuteRoleSelection();
                    break;

                case WCTMatchState.ARENA_LOADING:
                    PrepareArenaAndPositions();
                    break;

                case WCTMatchState.PRE_CHASE:
                    subTimer = preChaseDuration;
                    lastCountdownSec = -1;
                    hasResolvedCurrentChase = false;
                    OnBannerAnnouncement?.Invoke("GET SET");
                    break;

                case WCTMatchState.CHASE_ACTIVE:
                    chaseTimeRemaining = chaseDuration;
                    hasResolvedCurrentChase = false;
                    OnBannerAnnouncement?.Invoke("GO!");
                    break;

                case WCTMatchState.CHASE_RESULT:
                    subTimer = resultDisplayDuration;
                    break;

                case WCTMatchState.ROLE_ROTATION:
                    subTimer = betweenChasesInterval;
                    break;

                case WCTMatchState.MATCH_RESULT:
                    EvaluateMatchConclusion();
                    break;

                case WCTMatchState.POST_MATCH:
                    break;
            }
        }

        private void ExecuteRoleSelection()
        {
            // Official Rule: Rock-Paper-Scissors decides first role
            // Random fair 50/50 starting choice
            bool teamAStartsEvader = UnityEngine.Random.value > 0.5f;
            activeEvaderTeam = teamAStartsEvader ? 0 : 1;
            activeChaserTeam = 1 - activeEvaderTeam;

            CurrentEvaderAthlete = activeEvaderTeam == 0 ? TeamARoster[0] : TeamBRoster[0];
            CurrentChaserAthlete = activeChaserTeam == 0 ? TeamARoster[0] : TeamBRoster[0];

            string evaderTeamName = activeEvaderTeam == 0 ? "TEAM ALPHA" : "TEAM OMEGA";
            OnBannerAnnouncement?.Invoke($"{evaderTeamName} EVADES FIRST");

            TransitionState(WCTMatchState.ARENA_LOADING);
        }

        private void PrepareArenaAndPositions()
        {
            currentChaseNumber++;
            chaseTimeRemaining = chaseDuration;
            hasResolvedCurrentChase = false;
            lastOutcome = ChaseOutcome.None;

            OnAthletesReady?.Invoke(CurrentChaserAthlete, CurrentEvaderAthlete);
            TransitionState(WCTMatchState.PRE_CHASE);
        }

        private void UpdatePreChase()
        {
            subTimer -= Time.deltaTime;
            int count = Mathf.CeilToInt(subTimer);

            if (count > 0 && count != lastCountdownSec)
            {
                lastCountdownSec = count;
                OnBannerAnnouncement?.Invoke(count.ToString());
                WorldChaseTag.Audio.WCTAudioSynthesizer.Instance?.PlayCountdownBeep(false);
            }

            if (subTimer <= 0f)
            {
                WorldChaseTag.Audio.WCTAudioSynthesizer.Instance?.PlayBuzzer();
                OnBannerAnnouncement?.Invoke("GO!");
                TransitionState(WCTMatchState.CHASE_ACTIVE);
            }
        }

        private void UpdateChaseActive()
        {
            if (hasResolvedCurrentChase) return;

            chaseTimeRemaining -= Time.deltaTime;
            OnTimerTick?.Invoke(Mathf.Max(0f, chaseTimeRemaining));

            if (chaseTimeRemaining <= 0f)
            {
                chaseTimeRemaining = 0f;
                // Official Rule: Evader survives the full 20.0s -> 1 evasion point awarded!
                ResolveChase(ChaseOutcome.EvaderSurvived);
            }
        }

        public void ResolveTag(float exactTimeRemaining)
        {
            if (!IsChaseActive || hasResolvedCurrentChase) return;
            chaseTimeRemaining = Mathf.Clamp(exactTimeRemaining, 0f, chaseDuration);
            ResolveChase(ChaseOutcome.Tagged);
        }

        public void ResolveBoundaryViolation(int violatingTeam)
        {
            if (!IsChaseActive || hasResolvedCurrentChase) return;

            // If Evader steps out, Chaser wins
            if (violatingTeam == activeEvaderTeam)
            {
                ResolveChase(ChaseOutcome.OutOfBounds);
            }
        }

        public void ResolveChase(ChaseOutcome outcome)
        {
            if (hasResolvedCurrentChase || !IsChaseActive) return;
            hasResolvedCurrentChase = true;
            lastOutcome = outcome;

            // Immediately sound the loud referee buzzer!
            WorldChaseTag.Audio.WCTAudioSynthesizer.Instance?.PlayBuzzer();

            float timeElapsed = Mathf.Clamp(chaseDuration - chaseTimeRemaining, 0f, chaseDuration);

            // Record record
            ChaseRecord record = new ChaseRecord
            {
                chaseNumber = currentChaseNumber,
                chaserName = CurrentChaserAthlete != null ? CurrentChaserAthlete.athleteName : "Chaser",
                evaderName = CurrentEvaderAthlete != null ? CurrentEvaderAthlete.athleteName : "Evader",
                chaserTeam = activeChaserTeam,
                evaderTeam = activeEvaderTeam,
                outcome = outcome,
                duration = timeElapsed
            };

            int winningTeam;

            if (outcome == ChaseOutcome.EvaderSurvived)
            {
                // Evader team scores 1 point
                winningTeam = activeEvaderTeam;
                if (activeEvaderTeam == 0) teamAScore++;
                else teamBScore++;

                CurrentEvaderAthlete?.RecordEvasion(20.0f);
                OnBannerAnnouncement?.Invoke("EVADED! +1 POINT");
                WorldChaseTag.Audio.WCTAudioSynthesizer.Instance?.PlayEvasionWin();
                record.summary = $"{record.evaderName} survived 20.0s!";
            }
            else if (outcome == ChaseOutcome.Tagged)
            {
                // Chaser team wins chase, NO evasion points
                winningTeam = activeChaserTeam;
                CurrentChaserAthlete?.RecordTag(timeElapsed);
                CurrentEvaderAthlete?.RecordTagged(timeElapsed);
                OnBannerAnnouncement?.Invoke($"TAGGED! ({timeElapsed:F2}s)");
                WorldChaseTag.Audio.WCTAudioSynthesizer.Instance?.PlayTagSound();
                record.summary = $"{record.chaserName} tagged {record.evaderName} in {timeElapsed:F2}s!";
            }
            else // OutOfBounds
            {
                winningTeam = activeChaserTeam;
                CurrentEvaderAthlete?.RecordTagged(timeElapsed);
                OnBannerAnnouncement?.Invoke("OUT OF BOUNDS!");
                WorldChaseTag.Audio.WCTAudioSynthesizer.Instance?.PlayBoundaryViolation();
                record.summary = $"{record.evaderName} stepped out of bounds at {timeElapsed:F2}s!";
            }

            // Sudden Death accounting
            if (isSuddenDeath)
            {
                if (activeEvaderTeam == 0) suddenDeathTeamATime += timeElapsed;
                else suddenDeathTeamBTime += timeElapsed;
                suddenDeathChaseCount++;
            }

            record.teamAScoreAfter = teamAScore;
            record.teamBScoreAfter = teamBScore;
            MatchHistory.Add(record);

            OnScoreChanged?.Invoke(teamAScore, teamBScore);
            OnChaseResultCommitted?.Invoke(record);

            // Official WCT Winner-Stays-On-as-Evader rule:
            // Winning team becomes / remains the Evader team
            activeEvaderTeam = winningTeam;
            activeChaserTeam = 1 - activeEvaderTeam;

            TransitionState(WCTMatchState.CHASE_RESULT);
        }

        private void UpdateChaseResult()
        {
            subTimer -= Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ProceedToNextChase();
                return;
            }

            if (subTimer <= 0f)
            {
                ProceedToNextChase();
            }
        }

        public void ProceedToNextChase()
        {
            if (currentState != WCTMatchState.CHASE_RESULT && currentState != WCTMatchState.ROLE_ROTATION) return;

            // Check if match should conclude or rotate
            if (isSuddenDeath)
            {
                if (suddenDeathChaseCount >= 4)
                {
                    TransitionState(WCTMatchState.MATCH_RESULT);
                    return;
                }
            }
            else if (currentChaseNumber >= maxChases)
            {
                TransitionState(WCTMatchState.MATCH_RESULT);
                return;
            }

            RotateAthletes();
            TransitionState(WCTMatchState.ARENA_LOADING);
        }

        private void UpdateRoleRotation()
        {
            subTimer -= Time.deltaTime;
            if (subTimer <= 0f)
            {
                RotateAthletes();
                TransitionState(WCTMatchState.ARENA_LOADING);
            }
        }

        private void RotateAthletes()
        {
            // Winning athlete remains as Evader if outcome was EvaderSurvived
            // If Chaser tagged or Evader went out of bounds, winning team chooses next Evader
            if (lastOutcome == ChaseOutcome.EvaderSurvived)
            {
                // Evader stays!
            }
            else
            {
                // Chaser who won becomes the next Evader
                CurrentEvaderAthlete = CurrentChaserAthlete;
            }

            // Losing team supplies next Chaser
            if (activeChaserTeam == 0)
            {
                nextTeamAChaserIndex = (nextTeamAChaserIndex + 1) % TeamARoster.Count;
                CurrentChaserAthlete = TeamARoster[nextTeamAChaserIndex];
            }
            else
            {
                nextTeamBChaserIndex = (nextTeamBChaserIndex + 1) % TeamBRoster.Count;
                CurrentChaserAthlete = TeamBRoster[nextTeamBChaserIndex];
            }
        }

        private void EvaluateMatchConclusion()
        {
            if (teamAScore == teamBScore && !isSuddenDeath && gameMode == GameMode.FullTournamentMatch)
            {
                // Enter Sudden Death Chase-Off (4 chases, comparing cumulative evasion times)
                isSuddenDeath = true;
                suddenDeathChaseCount = 0;
                suddenDeathTeamATime = 0f;
                suddenDeathTeamBTime = 0f;
                OnBannerAnnouncement?.Invoke("SUDDEN DEATH CHASE-OFF!");
                TransitionState(WCTMatchState.ROLE_ROTATION);
                return;
            }

            string resultText;
            if (isSuddenDeath)
            {
                if (suddenDeathTeamATime > suddenDeathTeamBTime)
                {
                    resultText = $"TEAM ALPHA WINS SUDDEN DEATH! ({suddenDeathTeamATime:F1}s vs {suddenDeathTeamBTime:F1}s)";
                }
                else if (suddenDeathTeamBTime > suddenDeathTeamATime)
                {
                    resultText = $"TEAM OMEGA WINS SUDDEN DEATH! ({suddenDeathTeamBTime:F1}s vs {suddenDeathTeamATime:F1}s)";
                }
                else
                {
                    resultText = "MATCH TIED!";
                }
            }
            else
            {
                if (teamAScore > teamBScore) resultText = $"TEAM ALPHA WINS {teamAScore} - {teamBScore}!";
                else if (teamBScore > teamAScore) resultText = $"TEAM OMEGA WINS {teamBScore} - {teamAScore}!";
                else resultText = $"MATCH DRAW {teamAScore} - {teamBScore}!";
            }

            OnBannerAnnouncement?.Invoke(resultText);
            TransitionState(WCTMatchState.POST_MATCH);
        }

        public void RestartMatch()
        {
            StartMatch(gameMode, aiDifficulty);
        }

        public void ForceStateForTesting(WCTMatchState state)
        {
            currentState = state;
            if (state == WCTMatchState.CHASE_ACTIVE)
            {
                chaseTimeRemaining = chaseDuration;
                hasResolvedCurrentChase = false;
            }
        }
    }
}
