using RimRound.Defs;
using RimRound.FeedingTube;
using RimWorld.Planet;
using RimWorld.BaseGen;
using ThingDefOf = RimRound.FeedingTube.Defs.ThingDefOf;

namespace RimRound.GenSteps;

public class GenStep_PrisonerWillingToJoin : GenStep_Scatterer
{
    public override int SeedPart => 69356099;

    public override bool CanScatterAt(IntVec3 cell, Map map)
    {
        if (!base.CanScatterAt(cell, map))
            return false;
        if (!cell.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy))
            return false;
        if (!map.reachability.CanReachMapEdge(cell,
                TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false, false, false)))
            return false;

        foreach (var c2 in CellRect.CenteredOn(cell, Size.x, Size.z))
            if (!c2.InBounds(map) || c2.GetEdifice(map) is not null)
                return false;

        return true;
    }

    public override void ScatterAt(IntVec3 cell, Map map, GenStepParams parameters, int count = 1)
    {
        Faction faction;
        if (map.ParentFaction is null || map.ParentFaction == Faction.OfPlayer)
            faction = Find.FactionManager.RandomEnemyFaction();
        else faction = map.ParentFaction;

        var cellRect = CellRect.CenteredOn(cell, Size.x, Size.z).ClipInsideMap(map);
        
        Pawn singlePawnToSpawn;
        if (parameters.sitePart?.things is { Any: true } things)
            singlePawnToSpawn = (Pawn)things.Take(things.First());
        else
        {
            var component = map.Parent.GetComponent<PrisonerWillingToJoinComp>();
            
            if (component?.pawn is { Any: true } pawn)
                singlePawnToSpawn = pawn.Take(pawn.innerList.First());
            else
                singlePawnToSpawn = PrisonerWillingToJoinQuestUtility.GeneratePrisoner(map.Tile, faction);
        }

        var resolveParams = new ResolveParams()
        {
            rect = cellRect,
            faction = faction,
        };
        
        BaseGen.globalSettings.map = map;
        BaseGen.symbolStack.Push(Rule.symbol, resolveParams);
        BaseGen.Generate();

        var feeder = map.listerBuildings.AllBuildingsNonColonistOfDef(ThingDefOf.RR_AutoFeeder).FirstOrDefault()
            as Building_AutoFeeder;
        
        resolveParams = new()
        {
            rect = cellRect,
            faction = faction,
            singlePawnToSpawn = singlePawnToSpawn,
            singlePawnSpawnCellExtraPredicate = ((IntVec3 x) => x.GetDoor(map) == null),
            postThingSpawn = x =>
            {
                if(x is not Pawn pawn) return;
                
                MapGenerator.rootsToUnfog.Add(pawn.Position);
                pawn.mindState.WillJoinColonyIfRescued = true;

                pawn.needs.rest.CurLevel = 0f;
                pawn.SetPositionDirect(pawn.GetRoom().ContainedBeds.First().Position);
                
                feeder.SetTarget(pawn);
                feeder.CurrentMode = AutoFeederMode.maxgain;
            }
        };
        
        BaseGen.globalSettings.map = map;
        BaseGen.symbolStack.Push(RuleDefOf.SinglePawn.symbol, resolveParams, null);
        BaseGen.Generate();
        
        MapGenerator.SetVar<CellRect>(MapGenerator.RectOfInterestName, cellRect);
    }

    private RuleDef Rule => RuleDefOf.RimRound_PrisonCell;

    private IntVec2? size;
    private IntVec2 Size => (size ??= Rule?.resolvers.FirstOrDefault()?.minRectSize) ?? default;
}