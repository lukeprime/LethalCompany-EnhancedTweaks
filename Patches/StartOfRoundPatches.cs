using HarmonyLib;
using System;

namespace EnhancedTweaks.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatches
    {
        internal static bool _gameHasStarted = false;

        internal static void UpdateSeedUISize(StartOfRound __instance)
        {
            if (Plugin.showSeedNumber.Value)
            {
                if (HUDManagerPatches._prevDisplaySize == null
                    || HUDManagerPatches._prevDisplaySize != HUDManager.Instance.playerScreenTexture.canvas.renderingDisplaySize)
                {
                    // Resolution changed so recreate the SeedUI, simply resizing and repositioning won't
                    // work due to TextMeshProUGUI.autoSizeTextContainer working only on the first initialization
                    if (HUDManagerPatches._seedScreenCanvas != null
                        && HUDManagerPatches._seedUI != null)
                    {
                        HUDManagerPatches._seedScreenCanvas.transform.DetachChildren();
                        UnityEngine.Object.Destroy(HUDManagerPatches._seedUIText);
                        UnityEngine.Object.Destroy(HUDManagerPatches._seedUI);
                        HUDManagerPatches._seedUI = null;
                        HUDManagerPatches._seedUIText = null;
                    }

                    //Plugin.Log.LogInfo("Recreating SeedUI");
                    HUDManagerPatches.CreateSeedUI(HUDManager.Instance);
                }
            }
        }

        [HarmonyPatch("StartGame")]
        [HarmonyPostfix]
        static void UpdateSeedUI(StartOfRound __instance)
        {
            _gameHasStarted = true;
            if (Plugin.showSeedNumber.Value)
            {
                UpdateSeedUISize(__instance);

                if (HUDManagerPatches._seedUIText != null)
                {
                    if (Plugin.showSeedNumberOnCompanyMoon.Value
                        || !StartOfRound.Instance.currentLevel.PlanetName.Equals("71 Gordion", StringComparison.CurrentCultureIgnoreCase))
                    {
                        //Plugin.Log.LogInfo($"Moon: {StartOfRound.Instance.currentLevel.PlanetName}");
                        string newText = $"Seed: {__instance.randomMapSeed}";
                        //Plugin.Log.LogInfo(newText);
                        HUDManagerPatches._seedUIText.text = newText;
                        HUDManagerPatches._seedUIText.enabled = true;
                    }
                }
            }
        }

        [HarmonyPatch("EndOfGame")]
        [HarmonyPostfix]
        static void EndOfGame(StartOfRound __instance)
        {
            _gameHasStarted = false;
            if (Plugin.showSeedNumber.Value && HUDManagerPatches._seedUIText != null)
            {
                HUDManagerPatches._seedUIText.enabled = false;
            }
        }
    }
}
