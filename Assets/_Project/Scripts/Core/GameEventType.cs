namespace FrostShelter.Core
{
    public enum GameEventType
    {
        // Core
        GameInitialized,
        GameSaved,
        GameLoaded,
        GamePaused,
        GameResumed,

        // Time
        MinuteTick,
        HourTick,
        OfflineEarningsCalculated,

        // Resource
        ResourceChanged,
        ResourceCapacityChanged,
        ResourceReachedMax,

        // Temperature
        TemperatureChanged,
        BlizzardStarted,
        BlizzardEnded,

        // Building
        BuildingLevelUp,
        BuildingProductionCollected,
        BuildingConstructionStarted,
        BuildingConstructionCompleted,

        // Survivor
        SurvivorArrived,
        SurvivorDied,
        SurvivorEscaped,
        SurvivorSatisfactionChanged,

        // Hero
        HeroRecruited,
        HeroLevelUp,
        HeroStarUp,
        HeroSkillUnlocked,

        // Exploration
        ExpeditionStarted,
        ExpeditionStepTaken,
        ExpeditionNodeReached,
        ExpeditionBattleStarted,
        ExpeditionBattleEnded,
        ExpeditionEnded,
        ExpeditionFoodLow,
        ExpeditionWarmthLow,

        // Battle
        BattleUnitDied,
        BattlePhaseChanged,
        BattleCompleted,

        // TechTree
        TechNodeUnlocked,
        TechResearchStarted,
        TechResearchCompleted,

        // Event
        RandomEventTriggered,
        RandomEventResolved,

        // Story
        ChapterStarted,
        ChapterCompleted,
        BranchChoiceMade,
        EndingReached,
    }
}
