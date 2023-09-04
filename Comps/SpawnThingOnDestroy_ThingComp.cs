namespace RimRound.Comps;

public class SpawnThingOnDestroy_ThingComp : ThingComp
{
    public SpawnThingOnDestroy_CompProperties Properties => props as SpawnThingOnDestroy_CompProperties;

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        SpawnItems(previousMap, Properties.Def, Properties.Count.RandomInRange);

        base.PostDestroy(mode, previousMap);
    }

    private void SpawnItems(Map map, ThingDef def, int count)
    {
        var position = parent.Position;
        
        var thing = ThingMaker.MakeThing(def);
        count = Mathf.Min(count, def.stackLimit);
        thing.stackCount = count;
        
        GenSpawn.Spawn(thing, position, map);
    }
}