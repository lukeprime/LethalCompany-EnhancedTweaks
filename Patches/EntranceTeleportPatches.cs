using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EnhancedTweaks.Patches
{
    [HarmonyPatch(typeof(EntranceTeleport))]
    internal class EntranceTeleportPatches
    {
        private static FieldInfo _exitPointField = typeof(EntranceTeleport).GetField("exitPoint", BindingFlags.Instance | BindingFlags.NonPublic);

        private static Dictionary<string, float> _defaultExitRotations = new Dictionary<string, float>()
        {
            { "41 Experimentation", 90.0f },
            { "220 Assurance", 90.0f },
            { "56 Vow", 0.0f },
            { "21 Offense", 90.0f },
            { "61 March", 90.0f },
            { "85 Rend", 90.0f },
            { "7 Dine", 90.0f },
            { "8 Titan", 0.0f },
        };

        private static Dictionary<string, ConfigEntry<float>> _exitRotationConfigs = new Dictionary<string, ConfigEntry<float>>();

        [HarmonyPatch("TeleportPlayer")]
        [HarmonyPostfix]
        private static void TeleportPlayer(EntranceTeleport __instance)
        {
            if (Plugin.fixRotationExitingFire.Value
                && __instance.entranceId != 0
                && !__instance.isEntranceToBuilding)
            {
                RotatePlayer(__instance, GameNetworkManager.Instance.localPlayerController, __instance.entranceId);
            }

        }

        [HarmonyPatch("TeleportPlayerClientRpc")]
        [HarmonyPostfix]
        private static void TeleportPlayerClientRpc(EntranceTeleport __instance, int playerObj)
        {
            if (Plugin.fixRotationExitingFire.Value
                && __instance.entranceId != 0
                && !__instance.isEntranceToBuilding)
            {
                var player = __instance.playersManager.allPlayerScripts[playerObj];
                if (!player.IsOwner)
                {
                    RotatePlayer(__instance, player, __instance.entranceId);
                }
            }
        }

        private static void RotatePlayer(EntranceTeleport instance, PlayerControllerB player, int entranceId)
        {
            string planetName = StartOfRound.Instance.currentLevel.PlanetName;
            string exitDoorConfigName = $"{planetName} door #{entranceId}";
            float defaultRotation = 0f;
            _defaultExitRotations.TryGetValue(planetName, out defaultRotation);

            ConfigEntry<float> rotationConfig;
            if (!_exitRotationConfigs.TryGetValue(exitDoorConfigName, out rotationConfig))
            {
                rotationConfig = Plugin.Instance.Config.Bind("Fixes - Fire Exit Player Rotations", exitDoorConfigName, defaultRotation);
                _exitRotationConfigs.Add(exitDoorConfigName, rotationConfig);
            }

            var targetAngles = ((Transform)_exitPointField.GetValue(instance)).eulerAngles;
            player.transform.rotation = Quaternion.Euler(targetAngles.x, targetAngles.y + rotationConfig.Value, targetAngles.z);
        }
    }
}
