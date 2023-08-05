using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace RimRound.AI
{
    public class JoyGiver_PlayWithBlob : JoyGiver_InteractBuilding
    {
        public override bool CanDoDuringGathering
        {
            get
            {
                return true;
            }
        }

        public override Job TryGivePlayJob(Pawn pawn, Thing t)
        {
            return JobMaker.MakeJob(this.def.jobDef, t);
        }


    }
}
