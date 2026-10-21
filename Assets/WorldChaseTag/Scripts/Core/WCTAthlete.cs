using System;
using UnityEngine;

namespace WorldChaseTag.Core
{
    [Serializable]
    public class WCTAthlete
    {
        public string athleteName;
        public string Name => athleteName;
        public int teamId; // 0 = Team Alpha (Blue), 1 = Team Omega (Orange)
        public float speedMultiplier = 1.0f;
        public float staminaMultiplier = 1.0f;

        public int chasesPlayed = 0;
        public int evasionsWon = 0;
        public int tagsMade = 0;
        public float fastestTagTime = float.MaxValue;
        public float totalEvasionTime = 0f; // Cumulative time survived in seconds

        public int overallRating = 88;
        public string roleArchetype = "Speed Chaser";
        public int speedStat = 90;
        public int staminaStat = 88;
        public int agilityStat = 89;
        public int vaultStat = 87;
        public int controlStat = 7;
        public bool isCaptain = false;
        public string codeId = "A1";
        public string specialAbility = "Quick Recovery";
        public string abilityDescription = "Faster stamina recovery after slide or vault.";

        public WCTAthlete(string name, int team, float speed = 1.0f, float stamina = 1.0f, int ovr = 88, string archetype = "Speed Chaser", int spd = 90, int sta = 88, int agi = 89, int vlt = 87, bool captain = false, string code = "A1", int ctrl = 7, string ability = "Quick Recovery", string abilityDesc = "Faster stamina recovery after slide or vault.")
        {
            athleteName = name;
            teamId = team;
            speedMultiplier = speed;
            staminaMultiplier = stamina;
            overallRating = ovr;
            roleArchetype = archetype;
            speedStat = spd;
            staminaStat = sta;
            agilityStat = agi;
            vaultStat = vlt;
            controlStat = ctrl;
            isCaptain = captain;
            codeId = code;
            specialAbility = ability;
            abilityDescription = abilityDesc;
        }

        public void RecordEvasion(float timeSurvived)
        {
            chasesPlayed++;
            evasionsWon++;
            totalEvasionTime += timeSurvived;
        }

        public void RecordTag(float tagTime)
        {
            chasesPlayed++;
            tagsMade++;
            if (tagTime < fastestTagTime)
            {
                fastestTagTime = tagTime;
            }
        }

        public void RecordTagged(float timeSurvived)
        {
            chasesPlayed++;
            totalEvasionTime += timeSurvived;
        }

        public void ResetStats()
        {
            chasesPlayed = 0;
            evasionsWon = 0;
            tagsMade = 0;
            fastestTagTime = float.MaxValue;
            totalEvasionTime = 0f;
        }
    }

    [Serializable]
    public class ChaseRecord
    {
        public int chaseNumber;
        public string chaserName;
        public string evaderName;
        public int chaserTeam;
        public int evaderTeam;
        public ChaseOutcome outcome;
        public float duration; // Seconds elapsed before end
        public int teamAScoreAfter;
        public int teamBScoreAfter;
        public string summary;
    }
}
