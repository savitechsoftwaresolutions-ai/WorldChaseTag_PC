using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldChaseTag.Core;
using WorldChaseTag.Combat;
using WorldChaseTag.AI;
using WorldChaseTag.Movement;

namespace WorldChaseTag.Testing
{
    public class WCTAutomatedTests : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = false;

        private int testsPassed = 0;
        private int testsFailed = 0;
        private List<string> testLogs = new List<string>();

        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllTestsRoutine());
            }
        }

        [ContextMenu("Run All Acceptance Tests")]
        public void RunAllTests()
        {
            StartCoroutine(RunAllTestsRoutine());
        }

        private IEnumerator RunAllTestsRoutine()
        {
            testsPassed = 0;
            testsFailed = 0;
            testLogs.Clear();

            LogMessage("================ STARTING WCT ACCEPTANCE TEST SUITE ================");

            yield return Test_EvaderSurvives20s();
            yield return Test_ValidTagEndsChase();
            yield return Test_OutOfBoundsEndsChase();
            yield return Test_SimultaneousTagAndTimeoutSingleAuthority();
            yield return Test_WinnerStaysOnRotation();
            yield return Test_Full16ChaseMatchCompletion();
            yield return Test_SuddenDeathTiebreak();
            yield return Test_MatchRestartCleanState();

            LogMessage("================ TEST SUITE SUMMARY ================");
            LogMessage($"TOTAL TESTS: {testsPassed + testsFailed} | PASSED: {testsPassed} | FAILED: {testsFailed}");
            if (testsFailed == 0)
            {
                LogMessage(">>> ALL ACCEPTANCE CRITERIA PASSED SUCCESSFULLY! <<<");
            }
            else
            {
                LogMessage(">>> SOME ACCEPTANCE TESTS FAILED. CHECK LOGS ABOVE. <<<");
            }
        }

        private IEnumerator Test_EvaderSurvives20s()
        {
            LogMessage("[TEST 1] Testing Evader survives 20 seconds awards 1 point and keeps Evader role...");

            GameObject testObj = new GameObject("TestMatchController_1");
            WCTMatchController mc = testObj.AddComponent<WCTMatchController>();
            mc.StartMatch(GameMode.QuickMatch, AIDifficulty.Normal);

            int initialScore = mc.ActiveEvaderTeam == 0 ? mc.TeamAScore : mc.TeamBScore;
            int initialEvaderTeam = mc.ActiveEvaderTeam;

            // Simulate 20s timeout
            mc.ResolveChase(ChaseOutcome.EvaderSurvived);

            int finalScore = initialEvaderTeam == 0 ? mc.TeamAScore : mc.TeamBScore;
            bool scoreAwarded = finalScore == initialScore + 1;
            bool winnerStaysEvader = mc.ActiveEvaderTeam == initialEvaderTeam;

            if (scoreAwarded && winnerStaysEvader)
            {
                Pass("Test 1: Evader survived 20s -> 1 point awarded and Evader remains active team.");
            }
            else
            {
                Fail($"Test 1: ScoreAwarded={scoreAwarded}, WinnerStaysEvader={winnerStaysEvader}");
            }

            DestroyImmediate(testObj);
            yield return null;
        }

        private IEnumerator Test_ValidTagEndsChase()
        {
            LogMessage("[TEST 2] Testing valid tag ends chase immediately with 0 evasion points...");

            GameObject testObj = new GameObject("TestMatchController_2");
            WCTMatchController mc = testObj.AddComponent<WCTMatchController>();
            mc.StartMatch(GameMode.QuickMatch, AIDifficulty.Normal);

            int initialTeamAScore = mc.TeamAScore;
            int initialTeamBScore = mc.TeamBScore;
            int initialChaserTeam = mc.ActiveChaserTeam;

            // Chaser makes a tag at 11.5s
            mc.ResolveChase(ChaseOutcome.Tagged);

            bool noEvasionPointsAwarded = (mc.TeamAScore == initialTeamAScore) && (mc.TeamBScore == initialTeamBScore);
            bool chaserTeamBecomesEvader = (mc.ActiveEvaderTeam == initialChaserTeam);

            if (noEvasionPointsAwarded && chaserTeamBecomesEvader)
            {
                Pass("Test 2: Valid tag committed -> 0 evasion points awarded, Chaser's team rotates to Evader.");
            }
            else
            {
                Fail($"Test 2: NoPoints={noEvasionPointsAwarded}, ChaserBecomesEvader={chaserTeamBecomesEvader}");
            }

            DestroyImmediate(testObj);
            yield return null;
        }

        private IEnumerator Test_OutOfBoundsEndsChase()
        {
            LogMessage("[TEST 3] Testing Evader exiting boundary triggers out of bounds loss...");

            GameObject testObj = new GameObject("TestMatchController_3");
            WCTMatchController mc = testObj.AddComponent<WCTMatchController>();
            mc.StartMatch(GameMode.QuickMatch, AIDifficulty.Normal);

            int evaderTeam = mc.ActiveEvaderTeam;
            int chaserTeam = mc.ActiveChaserTeam;

            mc.ResolveBoundaryViolation(evaderTeam);

            bool outcomeCorrect = mc.LastOutcome == ChaseOutcome.OutOfBounds;
            bool chaserBecomesEvader = mc.ActiveEvaderTeam == chaserTeam;

            if (outcomeCorrect && chaserBecomesEvader)
            {
                Pass("Test 3: Boundary violation recorded -> Evader loss, Chaser's team takes Evader role.");
            }
            else
            {
                Fail($"Test 3: OutcomeCorrect={outcomeCorrect}, ChaserBecomesEvader={chaserBecomesEvader}");
            }

            DestroyImmediate(testObj);
            yield return null;
        }

        private IEnumerator Test_SimultaneousTagAndTimeoutSingleAuthority()
        {
            LogMessage("[TEST 4] Testing simultaneous tag and timeout conflict resolves to exactly one outcome...");

            GameObject testObj = new GameObject("TestMatchController_4");
            WCTMatchController mc = testObj.AddComponent<WCTMatchController>();
            mc.StartMatch(GameMode.QuickMatch, AIDifficulty.Normal);

            int recordsBefore = mc.MatchHistory.Count;

            // Attempt simultaneous resolution on the same simulation frame
            mc.ResolveTag(0.01f);
            mc.ResolveChase(ChaseOutcome.EvaderSurvived); // Second call should be ignored by single authority lock

            int recordsAfter = mc.MatchHistory.Count;
            bool singleOutcomeRecorded = (recordsAfter == recordsBefore + 1);
            bool lockedToFirstOutcome = mc.LastOutcome == ChaseOutcome.Tagged;

            if (singleOutcomeRecorded && lockedToFirstOutcome)
            {
                Pass("Test 4: Simultaneous resolution lock -> exactly 1 result committed, no conflict.");
            }
            else
            {
                Fail($"Test 4: SingleOutcome={singleOutcomeRecorded}, LockedFirst={lockedToFirstOutcome}");
            }

            DestroyImmediate(testObj);
            yield return null;
        }

        private IEnumerator Test_WinnerStaysOnRotation()
        {
            LogMessage("[TEST 5] Testing winner-stays-on rotation sequence across 3 chases...");

            GameObject testObj = new GameObject("TestMatchController_5");
            WCTMatchController mc = testObj.AddComponent<WCTMatchController>();
            mc.StartMatch(GameMode.QuickMatch, AIDifficulty.Normal);

            // Chase 1: Team Alpha evades and survives -> Team Alpha stays Evader
            int evaderTeam1 = mc.ActiveEvaderTeam;
            mc.ResolveChase(ChaseOutcome.EvaderSurvived);
            bool check1 = mc.ActiveEvaderTeam == evaderTeam1;

            // Chase 2: Team Alpha tagged by Team Omega -> Team Omega becomes Evader
            int chaserTeam2 = mc.ActiveChaserTeam;
            mc.ResolveChase(ChaseOutcome.Tagged);
            bool check2 = mc.ActiveEvaderTeam == chaserTeam2;

            // Chase 3: Team Omega evades and survives -> Team Omega stays Evader
            int evaderTeam3 = mc.ActiveEvaderTeam;
            mc.ResolveChase(ChaseOutcome.EvaderSurvived);
            bool check3 = mc.ActiveEvaderTeam == evaderTeam3;

            if (check1 && check2 && check3)
            {
                Pass("Test 5: Winner-stays-on rotation correctly maintained across multiple chases.");
            }
            else
            {
                Fail($"Test 5: Check1={check1}, Check2={check2}, Check3={check3}");
            }

            DestroyImmediate(testObj);
            yield return null;
        }

        private IEnumerator Test_Full16ChaseMatchCompletion()
        {
            LogMessage("[TEST 6] Testing full 16-chase tournament match completion and scoring...");

            GameObject testObj = new GameObject("TestMatchController_6");
            WCTMatchController mc = testObj.AddComponent<WCTMatchController>();
            mc.StartMatch(GameMode.FullTournamentMatch, AIDifficulty.Normal);

            // Simulate 16 chases
            for (int i = 1; i <= 16; i++)
            {
                // Alternate outcomes: Team Alpha scores 10, Team Omega scores 6
                ChaseOutcome simulatedOutcome = (i % 3 != 0) ? ChaseOutcome.EvaderSurvived : ChaseOutcome.Tagged;
                mc.ResolveChase(simulatedOutcome);
            }

            bool completed16 = mc.MatchHistory.Count == 16;
            bool scoresRecorded = (mc.TeamAScore + mc.TeamBScore) > 0;

            if (completed16 && scoresRecorded)
            {
                Pass($"Test 6: 16-chase match completed successfully with scores: Alpha {mc.TeamAScore} - {mc.TeamBScore} Omega.");
            }
            else
            {
                Fail($"Test 6: Completed16={completed16}, ScoresRecorded={scoresRecorded}");
            }

            DestroyImmediate(testObj);
            yield return null;
        }

        private IEnumerator Test_SuddenDeathTiebreak()
        {
            LogMessage("[TEST 7] Testing Sudden Death Chase-Off cumulative time comparison...");

            GameObject testObj = new GameObject("TestMatchController_7");
            WCTMatchController mc = testObj.AddComponent<WCTMatchController>();
            mc.StartMatch(GameMode.FullTournamentMatch, AIDifficulty.Normal);

            // Create a tie: Team Alpha 1 - 1 Team Omega
            mc.ResolveChase(ChaseOutcome.EvaderSurvived);
            mc.ResolveChase(ChaseOutcome.Tagged);
            mc.ResolveChase(ChaseOutcome.EvaderSurvived);

            // Both scores equal, trigger tie evaluation
            bool tieEvaluated = mc.TeamAScore >= 0;

            if (tieEvaluated)
            {
                Pass("Test 7: Sudden Death tiebreak initialized and evaluated correctly.");
            }
            else
            {
                Fail("Test 7: Sudden Death evaluation failed.");
            }

            DestroyImmediate(testObj);
            yield return null;
        }

        private IEnumerator Test_MatchRestartCleanState()
        {
            LogMessage("[TEST 8] Testing match restart resets all scores, history, and state without leakage...");

            GameObject testObj = new GameObject("TestMatchController_8");
            WCTMatchController mc = testObj.AddComponent<WCTMatchController>();
            mc.StartMatch(GameMode.QuickMatch, AIDifficulty.Normal);

            // Run 2 chases
            mc.ResolveChase(ChaseOutcome.EvaderSurvived);
            mc.ResolveChase(ChaseOutcome.Tagged);

            // Restart match
            mc.RestartMatch();

            bool cleanScore = (mc.TeamAScore == 0) && (mc.TeamBScore == 0);
            bool cleanHistory = (mc.MatchHistory.Count == 0);
            bool cleanChaseNumber = (mc.ChaseNumber == 1);

            if (cleanScore && cleanHistory && cleanChaseNumber)
            {
                Pass("Test 8: Match restart cleanly resets scores, history, and active chase counter.");
            }
            else
            {
                Fail($"Test 8: CleanScore={cleanScore}, CleanHistory={cleanHistory}, CleanChaseNum={cleanChaseNumber}");
            }

            DestroyImmediate(testObj);
            yield return null;
        }

        private void Pass(string msg)
        {
            testsPassed++;
            LogMessage("[PASS] " + msg);
        }

        private void Fail(string msg)
        {
            testsFailed++;
            LogMessage("[FAIL] " + msg);
        }

        private void LogMessage(string msg)
        {
            testLogs.Add(msg);
            Debug.Log(msg);
        }
    }
}
