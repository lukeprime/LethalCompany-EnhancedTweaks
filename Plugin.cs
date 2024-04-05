using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace EnhancedTweaks
{
    enum HorizontalPositions
    {
        Left,
        Center,
        Right,
        CustomRelative,
        CustomAbsolute
    }

    enum VerticalPositions
    {
        Top,
        Center,
        Bottom,
        CustomRelative,
        CustomAbsolute
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Plugin Instance = null;

        private readonly Harmony _harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        // Quota
        internal static ConfigEntry<int> newQuotaRackupDuration;

        // Seed number
        internal static ConfigEntry<bool> showSeedNumber;
        internal static ConfigEntry<bool> showSeedNumberOnCompanyMoon;
        internal static ConfigEntry<float> seedNumberFontSize;
        internal static ConfigEntry<int> seedNumberColorRed;
        internal static ConfigEntry<int> seedNumberColorGreen;
        internal static ConfigEntry<int> seedNumberColorBlue;
        internal static ConfigEntry<int> seedNumberColorAlpha;
        internal static ConfigEntry<HorizontalPositions> seedNumberHorizontalPosition;
        internal static ConfigEntry<VerticalPositions> seedNumberVerticalPosition;
        internal static ConfigEntry<float> seedNumberCustomHorizontalPosition;
        internal static ConfigEntry<float> seedNumberCustomVerticalPosition;

        // Fixes
        internal static ConfigEntry<bool> fixRotationExitingFire;

        // GeneralImprovements helpers
        internal static ConfigEntry<bool> helpResizeLightningWarningSlots;

        private void Awake()
        {
            // Plugin startup logic
            Instance = this;
            Plugin.Log = base.Logger;

            Plugin.Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            // Quota
            newQuotaRackupDuration = Config.Bind("Quota", "New quota rack up duration", 5, "Length of time in seconds of how long to rack up new quota. Set to zero for instantly racking up.");

            // Seed number
            showSeedNumber = Config.Bind("Seed Number", "Show seed number", true, "If enabled, will show the random map seed number at a position of your choice.");
            showSeedNumberOnCompanyMoon = Config.Bind("Seed Number", "Show seed number on company moon", false, "If \"Show seed number\" is enabled, this option will show the random map seed number on the company moon in addition to the regular moons.");
            seedNumberFontSize = Config.Bind("Seed Number", "Seed number font size", 20.0f, "The point font size for the seed number relative to a 1080p resolution.");
            seedNumberColorRed = Config.Bind("Seed Number", "Red color component for seed number (0-255)", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 255)));
            seedNumberColorGreen = Config.Bind("Seed Number", "Green color component for seed number (0-255)", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 255)));
            seedNumberColorBlue = Config.Bind("Seed Number", "Blue color component for seed number (0-255)", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 255)));
            seedNumberColorAlpha = Config.Bind("Seed Number", "Alpha color component for seed number (0-255)", 192, new ConfigDescription("", new AcceptableValueRange<int>(0, 255)));
            seedNumberHorizontalPosition = Config.Bind("Seed Number", "Horizontal position of seed number", HorizontalPositions.Left, "CustomRelative and CustomAbsolute will use the custom values configured below rather than the presets. CustomRelative means 0.0 will be the center of the screen, 1.0 will be the right edge of the screen, and -1.0 will be left edge of the screen.  CustomAbsolute position will be the position in screen pixels with the origin at the center and positive going to the right.");
            seedNumberVerticalPosition = Config.Bind("Seed Number", "Vertical position of seed number", VerticalPositions.Bottom, "CustomRelative and CustomAbsolute will use the custom values configured below rather than the presets. CustomRelative means 0.0 will be the center of the screen, 1.0 will be the top edge of the screen, and -1.0 will be the bottom edge of the screen.  CustomAbsolute position will be the position in screen pixels with the origin at the center and positive going to the top.");
            seedNumberCustomHorizontalPosition = Config.Bind("Seed Number", "Custom horizontal position of seed number", 0.0f, "This option is only used if \"Horizontal position of seed number\" option is set to CustomRelative or CustomAbsolute. The center of the seed number text container will be used as the pivot point, so if placing on the edge, adjustments may be needed to place the text fully within the viewable area.");
            seedNumberCustomVerticalPosition = Config.Bind("Seed Number", "Custom vertical position of seed number", 0.0f, "This option is only used if \"Vertical position of seed number\" option is set to CustomRelative or CustomAbsolute. The center of the seed number text container will be used as the pivot point, so if placing on the edge, adjustments may be needed to place the text fully within the viewable area.");

            // GeneralImprovements helper
            helpResizeLightningWarningSlots = Config.Bind("Helpers", "Fix lightning warnings", true, "If enabled, this will attempt to help GeneralImprovements' lightning warnings to work with custom hotbar sizes.  Without this, GeneralImprovements' lightning warning feature may not work beyond the vanilla 4 item slots provided by some mods.  This option will have no effect if you don't have GeneralImprovements installed.");

            // Fixes
            fixRotationExitingFire = Config.Bind("Fixes", "Fix rotation exiting fire", true, "Fixes the rotation of the player when exiting through a fire exit.  In vanilla, some fire exits rotate the player 90 degrees to the left, this option attempts to fix this.  The rotation value can be tweaked in the \"Fixes - Fire Exit Player Rotations\" section.  This does not fix the rotation when entering the facility through a fire exit.  Please use one of the many existing mods to fix the entering rotation. Note: The \"Fixes - Fire Exit Player Rotations\" config list is dynamically generated as you discover each fire exit, so it may not even get created until you go through your first fire exit, and will continue to grow as you go through more.");

            _harmony.PatchAll();
        }
    }
}
