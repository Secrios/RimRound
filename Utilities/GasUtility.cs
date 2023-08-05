using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimRound.Utilities
{
    public static class GasUtility
    {
        public static Dictionary<RRGasType, HediffDef> gasToHediff = new()
        {
            { RRGasType.fatteningGas, Defs.HediffDefOf.RimRound_SuddenWeightGain },
        };
    }
}
