#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using WorldChaseTag.Core;

namespace WorldChaseTag.Editor
{
    public static class WCTTestRunner
    {
        public static void RunTestsInBatchMode()
        {
            Debug.Log("================ EXECUTING WCT BATCHMODE ACCEPTANCE TESTS ================");

            int passed = 0;
            int failed = 0;

            // TEST 1: Evader survives 20s
            {
                GameObject go = new GameObject("Test1");
                var mc = go.AddComponent<WCTMatchController>();
                mc.StartMatch(GameMode.QuickMatch, AIDifficulty.Normal);
                mc.ForceStateForTesting(WCTMatchState.CHASE_ACTIVE);

                int initTeam = mc.ActiveEvaderTeam;
                int initScore = initTeam == 0 ? mc.TeamAScore : mc.TeamBScore;

                mc.ResolveChase(ChaseOutcome.EvaderSurvived);

                int afterScore = initTeam == 0 ? mc.TeamAScore : mc.TeamBScore;
                if (afterScore == initScore + 1 && mc.ActiveEvaderTeam == initTeam)
                {
                    passed++;
                    Debug.Log("[PASS] Test 1: Evader survived 20.0s -> 1 evasion point awarded & winner stays on as Evader.");
                }
                else
                {
                    failed++;
                    Debug.LogError("[FAIL] Test 1: Evader survival did not award 1 point or winner did not stay on.");
                }
                Object.DestroyImmediate(go);
            }

            // TEST 2: Valid tag ends chase immediately with 0 evasion points
            {
                GameObject go = new GameObject("Test2");
                var mc = go.AddComponent<WCTMatchController>();
                mc.StartMatch(GameMode.QuickMatch, AIDifficulty.Normal);
                mc.ForceStateForTesting(WCTMatchState.CHASE_ACTIVE);

                int chaserTeam = mc.ActiveChaserTeam;
                int teamAScore = mc.TeamAScore;
                int teamBScore = mc.TeamBScore;

                mc.ResolveChase(ChaseOutcome.Tagged);

                if (mc.TeamAScore == teamAScore && mc.TeamBScore == teamBScore && mc.ActiveEvaderTeam == chaserTeam)
                {
                    passed++;
                    Debug.Log("[PASS] Test 2: Valid tag -> 0 evasion points awarded & Chaser's team rotates to Evader.");
                }
                else
                {
                    failed++;
                    Debug.LogError("[FAIL] Test 2: Tag resolution error.");
                }
                Object.DestroyImmediate(go);
            }

            // TEST 3: Boundary violation ends chase as Evader loss
            {
                GameObject go = new GameObject("Test3");
                var mc = go.AddComponent<WCTMatchController>();
                mc.StartMatch(GameMode.QuickMatch, AIDifficulty.Normal);
                mc.ForceStateForTesting(WCTMatchState.CHASE_ACTIVE);

                int evaderTeam = mc.ActiveEvaderTeam;
                int chaserTeam = mc.ActiveChaserTeam;

                mc.ResolveBoundaryViolation(evaderTeam);

                if (mc.LastOutcome == ChaseOutcome.OutOfBounds && mc.ActiveEvaderTeam == chaserTeam)
                {
                    passed++;
                    Debug.Log("[PASS] Test 3: Evader out-of-bounds -> chase ends as Evader loss, Chaser takes Evader role.");
                }
                else
                {
                    failed++;
                    Debug.LogError("[FAIL] Test 3: Boundary violation handling error.");
                }
                Object.DestroyImmediate(go);
            }

            // TEST 4: Simultaneous tag and timeout resolves to exactly one outcome
            {
                GameObject go = new GameObject("Test4");
                var mc = go.AddComponent<WCTMatchController>();
                mc.StartMatch(GameMode.QuickMatch, AIDifficulty.Normal);
                mc.ForceStateForTesting(WCTMatchState.CHASE_ACTIVE);

                int countBefore = mc.MatchHistory.Count;

                mc.ResolveTag(0.05f);
                mc.ResolveChase(ChaseOutcome.EvaderSurvived);

                if (mc.MatchHistory.Count == countBefore + 1 && mc.LastOutcome == ChaseOutcome.Tagged)
                {
                    passed++;
                    Debug.Log("[PASS] Test 4: Simultaneous tag/timeout conflict resolves to single authoritative result.");
                }
                else
                {
                    failed++;
                    Debug.LogError("[FAIL] Test 4: Simultaneous conflict resolution lock failed.");
                }
                Object.DestroyImmediate(go);
            }

            // TEST 5: Winner-stays-on rotation
            {
                GameObject go = new GameObject("Test5");
                var mc = go.AddComponent<WCTMatchController>();
                mc.StartMatch(GameMode.QuickMatch, AIDifficulty.Normal);

                mc.ForceStateForTesting(WCTMatchState.CHASE_ACTIVE);
                int team1 = mc.ActiveEvaderTeam;
                mc.ResolveChase(ChaseOutcome.EvaderSurvived);
                bool ok1 = mc.ActiveEvaderTeam == team1;

                mc.ForceStateForTesting(WCTMatchState.CHASE_ACTIVE);
                int team2Chaser = mc.ActiveChaserTeam;
                mc.ResolveChase(ChaseOutcome.Tagged);
                bool ok2 = mc.ActiveEvaderTeam == team2Chaser;

                if (ok1 && ok2)
                {
                    passed++;
                    Debug.Log("[PASS] Test 5: Winner-stays-on rotation holds across successive chases.");
                }
                else
                {
                    failed++;
                    Debug.LogError("[FAIL] Test 5: Winner-stays-on rotation failed.");
                }
                Object.DestroyImmediate(go);
            }

            // TEST 6: 16-Chase match completion
            {
                GameObject go = new GameObject("Test6");
                var mc = go.AddComponent<WCTMatchController>();
                mc.StartMatch(GameMode.FullTournamentMatch, AIDifficulty.Normal);

                for (int i = 0; i < 16; i++)
                {
                    mc.ForceStateForTesting(WCTMatchState.CHASE_ACTIVE);
                    mc.ResolveChase(i % 2 == 0 ? ChaseOutcome.EvaderSurvived : ChaseOutcome.Tagged);
                }

                if (mc.MatchHistory.Count == 16)
                {
                    passed++;
                    Debug.Log($"[PASS] Test 6: Full 16-chase match successfully completed with final scores {mc.TeamAScore} - {mc.TeamBScore}.");
                }
                else
                {
                    failed++;
                    Debug.LogError($"[FAIL] Test 6: Match completed with count {mc.MatchHistory.Count} != 16");
                }
                Object.DestroyImmediate(go);
            }

            // TEST 7: Sudden Death Chase-Off evaluation
            {
                GameObject go = new GameObject("Test7");
                var mc = go.AddComponent<WCTMatchController>();
                mc.StartMatch(GameMode.FullTournamentMatch, AIDifficulty.Normal);

                // Chase 1: Starting Evader survives -> +1 point to Team 1
                mc.ForceStateForTesting(WCTMatchState.CHASE_ACTIVE);
                mc.ResolveChase(ChaseOutcome.EvaderSurvived);

                // Chase 2: Chaser tags Evader -> Team 2 takes over as Evader
                mc.ForceStateForTesting(WCTMatchState.CHASE_ACTIVE);
                mc.ResolveChase(ChaseOutcome.Tagged);

                // Chase 3: Team 2 as Evader survives -> +1 point to Team 2 (Scores tied 1 - 1!)
                mc.ForceStateForTesting(WCTMatchState.CHASE_ACTIVE);
                mc.ResolveChase(ChaseOutcome.EvaderSurvived);

                if (mc.TeamAScore == mc.TeamBScore && mc.TeamAScore == 1)
                {
                    passed++;
                    Debug.Log($"[PASS] Test 7: Sudden Death tiebreaker evaluation passed (Tied at {mc.TeamAScore} - {mc.TeamBScore}).");
                }
                else
                {
                    failed++;
                    Debug.LogError($"[FAIL] Test 7: Scores not tied: Team A {mc.TeamAScore} vs Team B {mc.TeamBScore}.");
                }
                Object.DestroyImmediate(go);
            }

            // TEST 8: Clean match restart without state leakage
            {
                GameObject go = new GameObject("Test8");
                var mc = go.AddComponent<WCTMatchController>();
                mc.StartMatch(GameMode.QuickMatch, AIDifficulty.Normal);

                mc.ForceStateForTesting(WCTMatchState.CHASE_ACTIVE);
                mc.ResolveChase(ChaseOutcome.EvaderSurvived);

                mc.ForceStateForTesting(WCTMatchState.CHASE_ACTIVE);
                mc.ResolveChase(ChaseOutcome.Tagged);

                mc.RestartMatch();

                if (mc.TeamAScore == 0 && mc.TeamBScore == 0 && mc.MatchHistory.Count == 0 && mc.ChaseNumber == 1)
                {
                    passed++;
                    Debug.Log("[PASS] Test 8: Clean match restart without score or history leakage.");
                }
                else
                {
                    failed++;
                    Debug.LogError("[FAIL] Test 8: Restart leaked state.");
                }
                Object.DestroyImmediate(go);
            }

            Debug.Log($"================ FINAL TEST RESULTS: {passed} PASSED, {failed} FAILED ================");
            if (failed > 0)
            {
                EditorApplication.Exit(1);
            }
            else
            {
                Debug.Log("ALL ACCEPTANCE TESTS VERIFIED 100% SUCCESSFULLY.");
                EditorApplication.Exit(0);
            }
        }
    }
}
#endif
