﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimRound.Patch
{
    [HarmonyPatch(typeof(HealthCardUtility))]
    [HarmonyPatch("DrawHediffRow")]
    internal class HealthCardUtility_DrawHediffRow_AddProgressBarToNextWeightStage
    {
		static MethodInfo drawHighlightIfMouseOverMI = typeof(Widgets)
			.GetMethod(nameof(Widgets.DrawHighlightIfMouseover), 
			BindingFlags.Public | BindingFlags.Static);

		static MethodInfo drawProgressBarToNextWeightStageMI = typeof(HealthCardUtility_DrawHediffRow_AddProgressBarToNextWeightStage)
			.GetMethod(nameof(HealthCardUtility_DrawHediffRow_AddProgressBarToNextWeightStage.DrawProgressBarToNextWeightStage), 
			BindingFlags.Public | BindingFlags.Static);

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			List<CodeInstruction> codeInstructions = new(instructions);

			List<CodeInstruction> newInstructions = new();
			List<CodeInstruction> newInstructions2 = new();
			List<CodeInstruction> newInstructions3 = new();

			if (drawHighlightIfMouseOverMI is null)
			{
				Log.Error("drawHighlightIfMouseOverMI was null!");
				return codeInstructions;
			}

            bool foundInsertionPoint = false;


            int rectDupIndex = -1;
			int hediffDUpIndex = -1;
			int callNewFuncIndex = -1;

			for (int i = 0; i < codeInstructions.Count; i++) 
			{
				if (codeInstructions[i].Calls(drawHighlightIfMouseOverMI)) 
				{
                    foundInsertionPoint = true;

                    rectDupIndex = i;
					hediffDUpIndex = i + 2;
					callNewFuncIndex = i + 4;
					break;
				}
			}

			newInstructions.Add(new CodeInstruction(OpCodes.Dup));

			newInstructions2.Add(new CodeInstruction(OpCodes.Dup));
			newInstructions3.Add(new CodeInstruction(OpCodes.Ldarg_1));
			newInstructions3.Add(new CodeInstruction(OpCodes.Call, drawProgressBarToNextWeightStageMI));

			if (rectDupIndex != -1)
			{
				codeInstructions.InsertRange(callNewFuncIndex, newInstructions3);
				codeInstructions.InsertRange(hediffDUpIndex, newInstructions2);
				codeInstructions.InsertRange(rectDupIndex, newInstructions);
			}

            if (!foundInsertionPoint)
                Log.Error($"Failed to find insertion point in {nameof(HealthCardUtility_DrawHediffRow_AddProgressBarToNextWeightStage)}.");

            return codeInstructions;
		}

		public static void DrawProgressBarToNextWeightStage(Rect rect, Hediff hediff, Pawn pawn) 
		{
			if (hediff.def.defName != "RimRound_Weight")
				return;

			Rect rectForBar = new(rect);
			rectForBar.height = 4f;
			rectForBar.y = rect.yMax - 4f;

			Widgets.FillableBar(rectForBar, Utilities.HediffUtility.ProgressToNextWeightStage(pawn), Utilities.Resources.weightProgressBarTex2);
		}
	}
}
