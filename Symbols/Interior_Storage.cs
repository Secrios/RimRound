using RimRound.Defs;
using RimRound.FeedingTube;
using RimRound.FeedingTube.Comps;
using RimWorld.BaseGen;
using ThingDefOf = RimRound.Defs.ThingDefOf;

namespace RimRound.Symbols;

public class SymbolResolver_Interior_Storage : SymbolResolver
{
    public override void Resolve(ResolveParams parameters) => ResolveInternal(parameters);

    public ResolveParams ResolveInternal(ResolveParams parameters)
    {
        var rect = parameters.rect;
        
        var point = rect.TopLeft;
        
        parameters.thingRot = Rot4.North;
        parameters.fillWithThingsPadding = 0;
        
        parameters.rect = CellRect.FromLimits(point, point with { x = point.x + 1, z = point.z - 1 });
        AddBatteries(parameters);
        
        point = rect.BottomLeft;
        
        parameters.rect = CellRect.FromLimits(point, point with { x = point.x + 1, z = point.z + 1 });
        AddVats(parameters);

        return parameters;
    }

    private void AddBatteries(ResolveParams parameters)
    {
        BaseGen.symbolStack.Push(RuleDefOf.ChargeBatteries.symbol, parameters);
        parameters.singleThingDef = RimWorld.ThingDefOf.Battery;
        BaseGen.symbolStack.Push(RuleDefOf.FillWithThings.symbol, parameters);
    }

    private void AddVats(ResolveParams parameters)
    {
        parameters.singleThingDef = RimRound.Defs.ThingDefOf.RR_TD_FoodStorageVat_Large;
        parameters.postThingSpawn += thing =>
        {
            var storage = thing.TryGetComp<FoodNetStorage_ThingComp>();
            if (storage is null)
                return;
            storage.Stored = storage.Capacity;
        };
        
        BaseGen.symbolStack.Push(RuleDefOf.FillWithThings.symbol, parameters);
    }
}