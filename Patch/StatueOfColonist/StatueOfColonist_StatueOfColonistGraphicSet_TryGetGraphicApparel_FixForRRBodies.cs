﻿using HarmonyLib;
using RimRound.Utilities;
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
    
    public class StatueOfColonist_StatueOfColonistGraphicSet_TryGetGraphicApparel_FixForRRBodies
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codeInstructions = new(instructions);

            bool foundInsertionPoint1 = IfGraphicPathIsNullReturnNullApparelGraphicRecord(generator, codeInstructions);
            bool foundInsertionPoint2 = ReplaceVanillaGraphicDatabaseGetMethodWithMine(codeInstructions);

            if (!foundInsertionPoint1 || !foundInsertionPoint2)
                Log.Error($"Failed to find insertion point in {nameof(StatueOfColonist_StatueOfColonistGraphicSet_TryGetGraphicApparel_FixForRRBodies)}.");

            return codeInstructions;
        }

        /// <summary>
        /// This function inserts code instructions to divert missing textures for clothing to the block which typically returns a null GraphicApparelRecord in the original function.
        /// </summary>
        public static bool IfGraphicPathIsNullReturnNullApparelGraphicRecord(ILGenerator generator, List<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> newInstructions = new();
            Label label = generator.DefineLabel();
            bool foundInsertionPoint = false;

            int startJndex = -1;

            for (int jndex = 0; jndex < codeInstructions.Count; ++jndex) // Sets startJndex to branch if false OpCode
            {
                if (codeInstructions[jndex].Calls(genTextNullOrEmptyMI))
                {
                    startJndex = jndex + 1;
                    codeInstructions[startJndex + 1].labels.Add(label); // Add label to block that returns null graphic record
                    break;
                }
            }

            newInstructions.Add(new CodeInstruction(OpCodes.Brtrue_S, label)); // Keep base functionality where null text goes into block marked by [label]
            newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0)); // Apparel
            newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_2)); // Bodytype
            newInstructions.Add(CodeInstruction.Call(
                typeof(ApparelGraphicRecordGetter_TryGetGraphicApparel_UseTransparentImagesForBadTex),
                nameof(ApparelGraphicRecordGetter_TryGetGraphicApparel_UseTransparentImagesForBadTex.IsGraphicPathResultNullForApparel))); // Call additional clause

            if (startJndex != -1)
            {
                foundInsertionPoint = true;

                codeInstructions.InsertRange(startJndex, newInstructions);
            }

            return foundInsertionPoint;
        }

        private static bool ReplaceVanillaGraphicDatabaseGetMethodWithMine(List<CodeInstruction> codeInstructions)
        {
            bool foundInsertionPoint = false;

            for (int jndex = 1; jndex < codeInstructions.Count; jndex++)
            {
                int currentIndex = codeInstructions.Count - jndex;
                if (codeInstructions[currentIndex].opcode == OpCodes.Call) // Replace the final Call (which should be the GrahpicDatabase.Get)
                {
                    foundInsertionPoint = true;

                    codeInstructions[currentIndex].operand =
                        typeof(ApparelGraphicRecordGetter_TryGetGraphicApparel_UseTransparentImagesForBadTex).GetMethod(
                            nameof(GetApparelGraphic),
                            BindingFlags.Public | BindingFlags.Static);

                    codeInstructions.Insert(currentIndex, new CodeInstruction(OpCodes.Ldarg_2)); // Bodytype
                    codeInstructions.Insert(currentIndex, new CodeInstruction(OpCodes.Ldarg_0)); // Apparel

                    break;
                }
            }

            return foundInsertionPoint;
        }

        public static Graphic GetApparelGraphic(string graphicPath, Shader shader, Vector2 vector, Color color, Apparel apparel, BodyTypeDef bodyType)
        {
            if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover || PawnRenderer.RenderAsPack(apparel) || apparel.WornGraphicPath == BaseContent.PlaceholderImagePath || apparel.WornGraphicPath == BaseContent.PlaceholderGearImagePath || apparel.def.apparel.LastLayer.defName == "OnHead" || apparel.def.apparel.LastLayer.defName == "StrappedHead")
            {
                graphicPath = apparel.WornGraphicPath;
            }
            else
            {
                graphicPath = apparel.WornGraphicPath + "_" + BodyTypeUtility.ConvertBodyTypeDefDefnameAccordingToSettings(RacialBodyTypeInfoUtility.GetEquivalentBodyTypeDef(bodyType).defName);
            }

            if (!ApparelGraphicRecordGetter_TryGetGraphicApparel_UseTransparentImagesForBadTex.graphicPathResultIsNull.ContainsKey(graphicPath))
            {
                Log.Error($"Graphic path was not in null dict and should have been! Graphic path: {graphicPath}");
            }
            else if (GlobalSettings.preferDefaultOutfitOverNaked && ApparelGraphicRecordGetter_TryGetGraphicApparel_UseTransparentImagesForBadTex.graphicPathResultIsNull[graphicPath])
            {
                graphicPath = Values.defaultClothingSetGraphicPath + "_" + BodyTypeUtility.ConvertBodyTypeDefDefnameAccordingToSettings(RacialBodyTypeInfoUtility.GetEquivalentBodyTypeDef(bodyType).defName);
            }

            return GraphicDatabase.Get<Graphic_Multi>(graphicPath, shader, vector, color);
        }


        static MethodInfo genTextNullOrEmptyMI = typeof(GenText).GetMethod(
            nameof(GenText.NullOrEmpty),
            BindingFlags.Static | BindingFlags.Public);


        public static PatchCollection GetPatchCollection()
        {
            return new PatchCollection
            {
                transpiler = typeof(StatueOfColonist_StatueOfColonistGraphicSet_TryGetGraphicApparel_FixForRRBodies).GetMethod(
                    nameof(StatueOfColonist_StatueOfColonistGraphicSet_TryGetGraphicApparel_FixForRRBodies.Transpiler), ModCompatibilityUtility.majorFlags)
            };
        }
    }
}
