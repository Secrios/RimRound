using Verse.Sound;

namespace RimRound.Things;

public class ChocoSteamGeyser : Building_SteamGeyser
{
    protected new Sprayer steamSprayer;
    
    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        
        steamSprayer = new(this);
        steamSprayer.startSprayCallback = StartSpray;
        steamSprayer.endSprayCallback = EndSpray;
    }

    public virtual void StartSpray()
    {
        SnowUtility.AddSnowRadial(this.OccupiedRect().RandomCell, base.Map, 4f, -0.06f);
        spraySustainer = SoundDefOf.GeyserSpray.TrySpawnSustainer(new TargetInfo(Position, Map));
        spraySustainerStartTick = Find.TickManager.TicksGame;
    }

    public virtual void EndSpray()
    {
        ref var sustainer = ref spraySustainer;
        if (sustainer is null)
            return;

        sustainer.End();
        sustainer = null;
    }

    public override void Tick()
    {
        if (harvester is null)
            steamSprayer.Tick();

        if (spraySustainer is null || Find.TickManager.TicksGame < spraySustainerStartTick + 1000)
            return;
        
        Log.Message("Geyser spray sustainer still playing after 1000 ticks. Force-ending.");
        EndSpray();
    }
    
    public class Sprayer : IntermittentSteamSprayer
    {
        public Sprayer(Thing parent) : base(parent) { }
        
        public static void ThrowAirPuffUp(Vector3 position, Map map)
        {
            if (!position.ToIntVec3().ShouldSpawnMotesAt(map))
                return;
            
            var offset = new Vector3(Rand.Range(-0.02f, 0.02f), 0f, Rand.Range(-0.02f, 0.02f));
            var fleckDef = RimRound.Defs.FleckDefOf.RimRound_ChocoAirPuff;
            
            var dataStatic = FleckMaker.GetDataStatic(position + offset, map, fleckDef, 1.5f);
            dataStatic.rotationRate = (float)Rand.RangeInclusive(-240, 240);
            dataStatic.velocityAngle = (float)Rand.Range(-45, 45);
            dataStatic.velocitySpeed = Rand.Range(1.2f, 1.5f);
            
            map.flecks.CreateFleck(dataStatic);
        }

        public void Tick()
        {
            if (sprayTicksLeft < 0)
            {
                if (--ticksUntilSpray > 0)
                    return;
                startSprayCallback?.Invoke();

                sprayTicksLeft = Rand.RangeInclusive(200, 500);
                return;
            }
            
            sprayTicksLeft--;
        
            if (Rand.Value < 0.6f)
                ThrowAirPuffUp(parent.TrueCenter(), parent.Map);
        
            if (Find.TickManager.TicksGame % 20 == 0)
                GenTemperature.PushHeat(parent, 40f);
        
            if (sprayTicksLeft > 0)
                return;
        
            endSprayCallback?.Invoke();

            ticksUntilSpray = Rand.RangeInclusive(500, 2000);
        }
    }
}