using System;

namespace CustomChallenges
{
    public static class Properties
    {
        // General
        public const String ID = "id";
        public const String NAME = "name";
        public const String DESCRIPTION = "description";
        public const String AUTHOR = "author";
        public const String REQUIRED_MODS = "requiredMods";
        public const String BLACKLIST_ORBS = "blacklistOrbs";
        public const String WHITELIST_ORBS = "whitelistOrbs";
        public const String BLACKLIST_RELICS = "blacklistRelics";
        public const String WHITELIST_RELICS = "whitelistRelics";
        public const String BLACKLIST_SCENARIOS = "blacklistScenarios";
        public const String WHITELIST_SCENARIOS = "whitelistScenarios";
        public const String BLACKLIST_BATTLES = "blacklistBattles";
        public const String WHITELIST_BATTLES = "whitelistBattles";
        public const String BLACKLIST_ELITE_BATTLES = "blacklistEliteBattles";
        public const String WHITELIST_ELITE_BATTLES = "whitelistEliteBattles";
        public const String STARTING_ORBS = "startingOrbs";
        public const String STARTING_RELICS = "startingRelics";
        public const String SKIP_STARTING_RELIC = "skipStartingRelic";
        public const String FULL_HEAL_AT_END_OF_BATTLE = "fullHealAtEndOfBattle";
        public const String MAX_HEALTH = "maxHealth";
        public const String PERMANENT_DAMAGE = "permanentDamage";
        public const String IMMUNE_SCENARIO_DAMAGE = "immuneScenarioDamage";
        public const String ENEMY_HEALTH_MULTIPLIER = "enemyHealthMultiplier";
        public const String PREDICTION_BOUNCES = "predictionBounces";
        public const String REQUIRED_CHALLENGES = "requiredChallenges";
        public const String STARTING_REFRESHES = "startingRefreshes";
        public const String STARTING_CRITS = "startingCrits";
        public const String PREVENT_NEW_ORBS = "preventNewOrbs";
        public const String PREVENT_ORB_UPGRADES = "preventOrbUpgrades";
        public const String PREVENT_PEG_MINIGAME = "preventPegMinigame";
        public const String RIGGED_BOMB_SELF_DAMAGE = "riggedBombSelfDamage";
        public const String STARTING_ACT = "startingAct";
        public const String BATTLE_TO_ELITE_CONVERSION_CHANCE = "battleToEliteConversionChance";
        public const String ENRAGE_THRESHOLD = "enrageThreshold";
        public const String ENRAGE_AMOUNT = "enrageAmount";
        public const String ALLOW_CRUCIBALL = "allowCruciball";
        public const String PLAYER_DAMAGE_MULTIPLIER = "playerDamageMultiplier";
        public const String BOMB_DAMAGE_MULTIPLIER = "bombDamageMultiplier";
        public const String ORB_DESTROYS_PEG = "orbDestroysPeg";
        public const String FORCE_POST_BATTLE_PICK = "forcePostBattlePick";
        public const String FORCE_TREASURE_PICK = "forceTreasurePick";

        // Win Conditions
        public const String WIN_CONDITIONS = "winConditions";
        public const String REMAINING_PEGS = "remainingPegs";
        public const String BATTLE_TIME_LIMIT = "battleTimeLimit";
        public const String GLOBAL_TIME_LIMIT = "globalTimeLimit";

        // Localization
        public const String LOCALIZATION_NAME = "localizationName";
        public const String LOCALIZATION_DESCRIPTION = "localizationDescription";
        public const String LOCALIZATION_CRUCIBALL_DESCRIPTIONS = "localizationCruciballDescriptions";
        public const String USE_EXTERNAL_LOCALIZATION = "useExternalLocalization";
        public const String SOURCE = "source";
        public const String SOURCE_ID = "sourceId";

        // Cruciball
        public const String CRUCIBALL = "cruciball";
        public const String CRUCIBALL_DESCRIPTIONS = "cruciballDescriptions";
        public const String FORCE_CRUCIBALL_LEVEL = "forceCruciballLevel"; // Vanilla cruciball only. Allows custom and normal cruciball to coexist.
        public const String MAX_CRUCIBALL_LEVEL = "maxCruciballLevel";
        public const String STARTING_CRUCIBALL_LEVEL = "startingCruciballLevel";
        public const String OVERWRITE_CRUCIBALL_LEVELS = "overwriteCruciballLevels";
        public const String CASCADING_LEVELS = "cascadingLevels";
        public const String LEVELS = "levels";

        // Weekly Challenges
        public const String VERSION = "version";
    }
}
