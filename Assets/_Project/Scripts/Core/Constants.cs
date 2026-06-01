namespace FrostShelter.Core
{
    public static class Constants
    {
        // Save
        public const int SAVE_DATA_VERSION = 1;
        public const int MAX_MANUAL_SLOTS = 3;
        public const int AUTO_SAVE_SLOT = 0;
        public const string SAVE_FILE_EXTENSION = ".sav";
        public const string SAVE_DIR = "Saves";

        // Time
        public const float TICK_INTERVAL = 1.0f;
        public const float PRODUCTION_TICK = 10.0f;
        public const float AUTO_SAVE_INTERVAL_SECONDS = 300f;
        public const float OFFLINE_MAX_HOURS = 72f;
        public const float SECONDS_PER_IN_GAME_HOUR = 60f;

        // Exploration
        public const int MAX_EXPLORATION_HEROES = 5;
        public const int FORMATION_FRONT_SLOTS = 3;
        public const int FORMATION_BACK_SLOTS = 3;
        public const int MAP_MIN_SIZE = 6;
        public const int MAP_MAX_SIZE = 8;

        // Survivor
        public const float SATISFACTION_ESCAPE_THRESHOLD = 30f;
        public const float SATISFACTION_DECAY_PER_HOUR = 0.5f;
        public const float MOOD_DECAY_PER_HOUR = 1.0f;
        public const int MAX_SURVIVORS = 200;

        // Temperature thresholds
        public const float TEMP_COMFORT_THRESHOLD = -20f;
        public const float TEMP_CRITICAL_THRESHOLD = -30f;
        public const float BASE_TEMPERATURE = -30f;
        public const float TEMP_PER_FURNACE_LEVEL = 3f;
        public const float BLIZZARD_TEMP_DROP = 10f;

        // Building
        public const int MAX_BUILDING_LEVEL = 30;

        // Hero
        public const int MAX_HERO_LEVEL = 50;
    }
}
