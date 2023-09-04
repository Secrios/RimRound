namespace RimRound.Comps;

public class SpawnThingOnDestroy_CompProperties : CompProperties
{
    public SpawnThingOnDestroy_CompProperties()
    {
        compClass = typeof(SpawnThingOnDestroy_ThingComp);
    }
    
    public ThingDef Def;
    public IntRange Count = new(1, 1);
}