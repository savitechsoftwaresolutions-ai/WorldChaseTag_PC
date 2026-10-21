namespace WorldChaseTag.Core
{
    public enum WCTMatchState
    {
        LOBBY,
        ARENA_LOADING,
        ROLE_SELECTION,
        PRE_CHASE,
        CHASE_ACTIVE,
        CHASE_RESULT,
        ROLE_ROTATION,
        MATCH_RESULT,
        POST_MATCH
    }

    public enum ChaseOutcome
    {
        None,
        EvaderSurvived,
        Tagged,
        OutOfBounds,
        FalseStart
    }

    public enum ChaseRole
    {
        Chaser,
        Evader,
        Spectator
    }

    public enum AIDifficulty
    {
        Easy,
        Normal,
        Hard,
        Expert
    }

    public enum GameMode
    {
        Practice,
        QuickMatch,
        FullTournamentMatch
    }
}
