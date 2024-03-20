using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedTweaks.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatches
    {
        internal static GameObject _seedScreenCanvas;
        internal static GameObject _seedUI;
        internal static TextMeshProUGUI _seedUIText;
        internal static Vector2 _prevDisplaySize;

        public static void CreateSeedUI(HUDManager __instance)
        {
            if (Plugin.showSeedNumber.Value)
            {
                if (_seedScreenCanvas == null)
                {
                    //Plugin.Log.LogInfo("Creating _seedScreenCanvas");
                    _seedScreenCanvas = new GameObject("SeedScreenCanvas");
                    _seedScreenCanvas.transform.SetParent(__instance.playerScreenTexture.canvas.transform.parent, false);
                    Canvas myCanvas = _seedScreenCanvas.AddComponent<Canvas>();
                    myCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    _seedScreenCanvas.AddComponent<CanvasScaler>();
                    _seedScreenCanvas.AddComponent<GraphicRaycaster>();
                }

                if (_seedUI == null)
                {
                    _seedUI = new GameObject("MySeedUI");
                    _seedUI.transform.SetParent(_seedScreenCanvas.transform, false);
                }

                if (_seedUIText == null)
                {
                    // Scale fontSize to height or width, which ever is smaller than a 1080p resolution
                    _prevDisplaySize = HUDManager.Instance.playerScreenTexture.canvas.renderingDisplaySize;
                    //Plugin.Log.LogInfo($"renderingDisplaySize: {_prevDisplaySize}");

                    float seedUIScale = 1f;
                    float ar1080p = 1920f / 1080f;
                    float arScreen = _prevDisplaySize.x / _prevDisplaySize.y;

                    if (arScreen > ar1080p)
                    {
                        seedUIScale = _prevDisplaySize.y / 1080f;
                    }
                    else
                    {
                        seedUIScale = _prevDisplaySize.x / 1920f;
                    }

                    //Plugin.Log.LogInfo("Creating _seedUIText");
                    string seedString = "Seed: 99999999";
                    _seedUIText = _seedUI.AddComponent<TextMeshProUGUI>();
                    _seedUIText.enableWordWrapping = false;
                    _seedUIText.autoSizeTextContainer = true; // This only works when initializing, it will not autosize on subsequent text changes!
                    _seedUIText.font = __instance.weightCounter.font;
                    _seedUIText.faceColor = new Color(
                        Plugin.seedNumberColorRed.Value / 255f,
                        Plugin.seedNumberColorGreen.Value / 255f,
                        Plugin.seedNumberColorBlue.Value / 255f,
                        Plugin.seedNumberColorAlpha.Value / 255f);
                    _seedUIText.fontSize = Plugin.seedNumberFontSize.Value * (96f / 72f) * seedUIScale;  // Convert pt size to pixel then scale to rendering size
                    _seedUIText.alignment = TextAlignmentOptions.Center;
                    _seedUIText.text = seedString;

                    Vector2 newRect = _seedUIText.GetPreferredValues(seedString);

                    float newX = 0;
                    float newY = 0;

                    float halfDisplayWidth = _prevDisplaySize.x / 2f;
                    float halfDisplayHeight = _prevDisplaySize.y / 2f;

                    if (Plugin.seedNumberHorizontalPosition.Value == HorizontalPositions.Right)
                    {
                        newX = halfDisplayWidth;
                        newX -= newRect.x / 2f;
                    }
                    else if (Plugin.seedNumberHorizontalPosition.Value == HorizontalPositions.Left)
                    {
                        newX = -halfDisplayWidth;
                        newX += newRect.x / 2f;
                    }
                    else if (Plugin.seedNumberHorizontalPosition.Value == HorizontalPositions.CustomRelative)
                    {
                        newX = Plugin.seedNumberCustomHorizontalPosition.Value * halfDisplayWidth;
                    }
                    else if (Plugin.seedNumberHorizontalPosition.Value == HorizontalPositions.CustomAbsolute)
                    {
                        newX = Plugin.seedNumberCustomHorizontalPosition.Value;
                    }

                    if (Plugin.seedNumberVerticalPosition.Value == VerticalPositions.Top)
                    {
                        newY = halfDisplayHeight;
                        newY -= newRect.y / 2f;
                    }
                    else if (Plugin.seedNumberVerticalPosition.Value == VerticalPositions.Bottom)
                    {
                        newY = -halfDisplayHeight;
                        newY += newRect.y / 2f;
                    }
                    else if (Plugin.seedNumberVerticalPosition.Value == VerticalPositions.CustomRelative)
                    {
                        newY = Plugin.seedNumberCustomVerticalPosition.Value * halfDisplayHeight;
                    }
                    else if (Plugin.seedNumberVerticalPosition.Value == VerticalPositions.CustomAbsolute)
                    {
                        newY = Plugin.seedNumberCustomVerticalPosition.Value;
                    }

                    //Plugin.Log.LogInfo($"new pos: {newX}, {newY}");

                    _seedUIText.transform.localPosition = new Vector3(newX, newY, 0);

                    if (StartOfRoundPatches._gameHasStarted
                        && (Plugin.showSeedNumberOnCompanyMoon.Value
                            || !StartOfRound.Instance.currentLevel.PlanetName.Equals("71 Gordion", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        _seedUIText.text = $"Seed: {StartOfRound.Instance.randomMapSeed}";
                        _seedUIText.enabled = true;
                    }
                    else
                    {
                        _seedUIText.enabled = false;
                    }
                }
            }
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartPatch(HUDManager __instance)
        {
            if (Plugin.showSeedNumber.Value)
            {
                //Plugin.Log.LogInfo("Creating SeedUI");
                CreateSeedUI(__instance);
                _seedUIText.enabled = false;
            }
        }

        [HarmonyPatch("OpenMenu_performed")]
        [HarmonyPostfix]
        static void OpenMenu_performedPatch()
        {
            // It is overkill to update SeedUI size on every game update so do it here instead
            //Plugin.Log.LogInfo("OpenMenu_performed");
            StartOfRoundPatches.UpdateSeedUISize(StartOfRound.Instance);
        }

        [HarmonyPatch("PingScan_performed")]
        [HarmonyPostfix]
        static void PingScan_performedPatch()
        {
            // It is overkill to update SeedUI size on every game update so do it here instead
            //Plugin.Log.LogInfo("PingScan_performed");
            StartOfRoundPatches.UpdateSeedUISize(StartOfRound.Instance);
        }

        //static HUDManager hudManagerInstance;

        //static IEnumerator rackUpNewQuotaText()
        //{
        //    yield return new WaitForSeconds(3.5f);
        //    int quotaTextAmount = 0;
        //    float totalDeltaTime = 0;
        //    while (quotaTextAmount < TimeOfDay.Instance.profitQuota)
        //    {
        //        totalDeltaTime += Time.deltaTime;
        //        if (Plugin.newQuotaRackupDuration.Value > 0)
        //        {

        //            quotaTextAmount = (int)(totalDeltaTime / (float)Plugin.newQuotaRackupDuration.Value * TimeOfDay.Instance.profitQuota);
        //        }
        //        else
        //        {
        //            quotaTextAmount = TimeOfDay.Instance.profitQuota;
        //        }
        //        hudManagerInstance.newProfitQuotaText.text = "$" + (int)quotaTextAmount;
        //        yield return null;
        //    }
        //    hudManagerInstance.newProfitQuotaText.text = "$" + TimeOfDay.Instance.profitQuota;
        //    TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
        //    hudManagerInstance.UIAudio.PlayOneShot(hudManagerInstance.newProfitQuotaSFX);
        //    yield return new WaitForSeconds(1.25f);
        //    hudManagerInstance.displayingNewQuota = false;
        //    hudManagerInstance.reachedProfitQuotaAnimator.SetBool("display", value: false);
        //}

        // Alternative Prefix patch for rackUpNewQuotaText in case transpiler breaks one day
        //[HarmonyPatch("DisplayNewDeadline")]
        //[HarmonyPrefix]
        //static bool DisplayNewDeadlinePatch(HUDManager __instance, int overtimeBonus)
        //{
        //    __instance.reachedProfitQuotaAnimator.SetBool("display", value: true);
        //    __instance.newProfitQuotaText.text = "$0";
        //    __instance.UIAudio.PlayOneShot(__instance.reachedQuotaSFX);
        //    __instance.displayingNewQuota = true;
        //    if (overtimeBonus < 0)
        //    {
        //        __instance.reachedProfitQuotaBonusText.text = "";
        //    }
        //    else
        //    {
        //        __instance.reachedProfitQuotaBonusText.text = $"Overtime bonus: ${overtimeBonus}";
        //    }
        //    hudManagerInstance = __instance;
        //    __instance.StartCoroutine(rackUpNewQuotaText());

        //    return false;
        //}

        [HarmonyTranspiler]
        [HarmonyPatch("rackUpNewQuotaText", MethodType.Enumerator)]
        static IEnumerable<CodeInstruction> rackUpNewQuotaTextMoveNext(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo quotaTextAmountFieldInfo = null;

            foreach (CodeInstruction code in instructions)
            {
                if ((code.operand as FieldInfo)?.Name?.Contains("<quotaTextAmount>") == true
                    && (code.operand as FieldInfo)?.ReflectedType?.Name.Contains("<rackUpNewQuotaText>") == true)
                {
                    quotaTextAmountFieldInfo = (code.operand as FieldInfo);
                    break;
                }
            }

            if (quotaTextAmountFieldInfo == null)
            {
                Plugin.Log.LogWarning("Did vanilla code change?  Unable to find field quotaTextAmount in rackUpNewQuotaText enumerator");
                return instructions;
            }
            //else
            //{
            //    Plugin.Log.LogInfo("Found quotaTextAmount field in rackUpNewQuotaText enumerator");
            //}

            //foreach (CodeInstruction code in instructions)
            //{
            //    // For debugging only
            //    Plugin.Log.LogInfo($"{code.opcode}, {code.operand}, {(code.operand as FieldInfo)?.Name}, {(code.operand as FieldInfo)?.ReflectedType?.Name}, {(code.operand != null ? code.operand.GetType() : "null")}");
            //}

            // If newQuotaRackupDuration is 0, then just use Int32.MaxValue
            float quotaIncrementDelta = (float)Int32.MaxValue;
            const int incrementAmountPrecision = 100;

            if (Plugin.newQuotaRackupDuration.Value > 0)
            {
                // Too bad quotaTextAmount field is not a float, so put precision size (instead of 1) in numerator to
                // give us some fixed point precision (we'll divide by precision later to get back the integer part),
                // without this precision, small quotas rack up very quickly due to the fraction being lost (rounded up)
                quotaIncrementDelta = (float)incrementAmountPrecision / Plugin.newQuotaRackupDuration.Value;
            }

            IEnumerable<CodeInstruction> newInstructions = instructions;
            try
            {
                newInstructions = new CodeMatcher(instructions)
                    // Replace constant 250 quota increment amount with our own calculation using the
                    // duration, which vanilla logic will then use to multiply with Time.deltaTime
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldc_R4, 250f),
                        new CodeMatch(OpCodes.Mul))
                    .ThrowIfNotMatch("250f quota inrement amount not found")
                    .SetOperandAndAdvance(quotaIncrementDelta)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Mul))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TimeOfDay), "get_Instance")))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), "profitQuota")))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Conv_R4))
                    .Advance(1) // Next instruction in vanilla will multiply our adjusted increment ratio amount with profitQuota then add the result to quotaTextAmount

                    // Ensure we are not adding less than 1.0
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_R4, 1f))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Mathf), "Max", [typeof(float), typeof(float)])))
                    
                    // Remove (quotaTextAmount + 3) parameter calculation as we will be switching from Mathf.Clamp to Mathf.Min
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, quotaTextAmountFieldInfo),
                        new CodeMatch(OpCodes.Ldc_I4_3),
                        new CodeMatch(OpCodes.Add),
                        new CodeMatch(OpCodes.Conv_R4))
                    .ThrowIfNotMatch("(quotaTextAmount + 3) instructions not found")
                    .RemoveInstructions(5)

                    // Find the Mathf.Clamp call that we'll be replacing
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldc_I4_S, (SByte)10),
                        new CodeMatch(OpCodes.Add),
                        new CodeMatch(OpCodes.Conv_R4),
                        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Mathf), "Clamp", [typeof(float), typeof(float), typeof(float)]))
                        )
                    .ThrowIfNotMatch("Exact Mathf.Clamp instruction not found")
                    .Advance(3)

                    // Multiply profitQuota by precision to scale it up to our fixed point value for comparison
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_R4, (float)incrementAmountPrecision))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Mul))

                    // Replace Mathf.Clamp with Mathf.Max since we already ensured we are adding at least 1.0f
                    .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Mathf), "Min", [typeof(float), typeof(float)])))

                    // Divide quotaTextAmount by precision to get integer part of our fixed point value to put into text mesh
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldflda, quotaTextAmountFieldInfo),
                        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Int32), "ToString"))
                        )
                    .ThrowIfNotMatch("Exact quotaTextAmount.ToString() not found")
                    .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldfld, quotaTextAmountFieldInfo)) // replace Ldflda with Ldfld so we can divide by precision
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, incrementAmountPrecision))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Div))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_0)) // Looks like nothing is using local variable index 0 at this point so should be safe to repurpose it
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 0)) // Next instruction in vanilla will call ToString(), then Concat()

                    // Multiply profitQuota by precision to scale it up to our fixed point value for comparison
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldfld, quotaTextAmountFieldInfo),
                        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(TimeOfDay), "get_Instance")),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), "profitQuota")),
                        new CodeMatch(OpCodes.Blt)
                        )
                    .ThrowIfNotMatch("Exact quotaTextAmount < TimeOfDay.Instance.profitQuota compare not found")
                    .Advance(3)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, incrementAmountPrecision))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Mul))

                    // Done!
                    .InstructionEnumeration();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"Did vanilla code change?  Unable to patch rackUpNewQuotaText enumerator. Exception: {ex}");
            }

            return newInstructions;
        }
    }
}
