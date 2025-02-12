﻿using RimRound.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRound.Patch
{
    public class CompMilkableHuman_ResourceAmount_AdjustForPawnBodyWeight
    {
        public static void Postfix(ref float __result, ThingComp __instance) 
        {

            float weightSeverity = Utilities.HediffUtility.KilosToSeverityWithBaseWeight(__instance.parent.AsPawn().Weight());

            if (weightSeverity > milkMultiplier.Last().First)
            {
                __result *= 1 + ((milkMultiplier.Last().Second - 1) * GlobalSettings.milkMultiplierForWeight.threshold);
                return;
            } 

            for (int i = 1; i < milkMultiplier.Count - 1; ++i) 
            {
                if (weightSeverity < milkMultiplier[i].First)
                {
                    __result *= 1 + ((milkMultiplier[i - 1].Second - 1) * GlobalSettings.milkMultiplierForWeight.threshold);                  
                    return;
                }    
            }

            return;
        }

        public static PatchCollection GetPatchCollection()
        {
            return new PatchCollection 
            {
                postfix = typeof(CompMilkableHuman_ResourceAmount_AdjustForPawnBodyWeight).GetMethod(
                    nameof(CompMilkableHuman_ResourceAmount_AdjustForPawnBodyWeight.Postfix), ModCompatibilityUtility.majorFlags)
            };
        }

        static List<Pair<float, float>> milkMultiplier = new()
        {
            new Pair<float, float>( 0.000f, 0.90f  ),
            new Pair<float, float>( 0.005f, 0.95f  ),
            new Pair<float, float>( 0.015f, 1.00f  ),
            new Pair<float, float>( 0.035f, 1.05f  ),
            new Pair<float, float>( 0.050f, 1.05f  ),
            new Pair<float, float>( 0.065f, 1.10f  ),
            new Pair<float, float>( 0.090f, 1.15f  ),
            new Pair<float, float>( 0.120f, 1.20f  ),
            new Pair<float, float>( 0.155f, 1.25f  ),
            new Pair<float, float>( 0.200f, 1.35f  ),
            new Pair<float, float>( 0.230f, 1.40f  ),
            new Pair<float, float>( 0.280f, 1.45f  ),
            new Pair<float, float>( 0.350f, 1.60f  ),
            new Pair<float, float>( 0.430f, 1.75f  ),
            new Pair<float, float>( 0.535f, 1.90f  ),
            new Pair<float, float>( 0.660f, 2.15f  ),
            new Pair<float, float>( 0.800f, 2.40f  ),
            new Pair<float, float>( 0.965f, 2.70f  ),
            new Pair<float, float>( 1.160f, 3.05f  ),
            new Pair<float, float>( 1.410f, 3.50f  ),

            new Pair<float, float>( 1.860f, 4.00f  ),
            new Pair<float, float>( 2.460f, 4.50f  ),
            new Pair<float, float>( 2.960f, 5.00f  ),
            new Pair<float, float>( 3.960f, 5.50f  ),
            new Pair<float, float>( 4.960f, 6.00f  ),
            new Pair<float, float>( 6.460f, 6.75f  ),
            new Pair<float, float>( 7.960f, 7.50f  ),
            new Pair<float, float>( 9.960f, 8.50f  ),
            new Pair<float, float>( 14.46f, 10.0f  ),
        };
    }
}
