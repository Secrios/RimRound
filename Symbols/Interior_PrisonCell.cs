using RimRound.Defs;
using RimWorld.BaseGen;
using ThingDefOf = RimRound.Defs.ThingDefOf;

namespace RimRound.Symbols;

public class SymbolResolver_Interior_PrisonCell : SymbolResolver
{
    public override void Resolve(ResolveParams parameters)
    {
        var rect = parameters.rect;

        var point = rect.CenterCell;
        
        parameters.rect = CellRect.FromLimits(point, point with { x = point.x + 1 });
        
        AddBed(parameters);
        
        point.x--;
        parameters.rect = CellRect.SingleCell(point);

        AddFeeder(parameters);

        parameters.rect = rect;
        
        var storage = new SymbolResolver_Interior_Storage();
        parameters = storage.ResolveInternal(parameters);

        point.x--;

        var start = parameters.rect.TopRight;
        start.z++;
        
        AddPipes(parameters, start, point);

        parameters.rect = rect;
        
        AddOther(parameters);
    }

    private static void AddOther(ResolveParams parameters)
    {
        InteriorSymbolResolverUtility.PushBedroomHeatersCoolersAndLightSourcesSymbols(parameters, false);
    }

    private static void AddPipes(ResolveParams parameters, IntVec3 start, IntVec3 end)
    {
        parameters.rect = CellRect.FromLimits(start, end);
        parameters.singleThingDef = ThingDefOf.RR_TD_FeedingTubeConduit;
        
        BaseGen.symbolStack.Push(RuleDefOf.FillWithThings.symbol, parameters);
    }

    private void AddBed(ResolveParams parameters)
    {
        BaseGen.symbolStack.Push(RuleDefOf.PrisonerBed.symbol, parameters with { thingRot = Rot4.East });
    }

    private void AddFeeder(ResolveParams parameters)
    {
        AddThing(parameters, RimRound.FeedingTube.Defs.ThingDefOf.RR_AutoFeeder);
    }

    private void AddVats(ResolveParams parameters)
    {
        for (var i = 0; i < 2; i++)
            AddThing(parameters, RimRound.Defs.ThingDefOf.RR_TD_FoodStorageVat_Large);
    }

    private void AddThing(ResolveParams parameters, ThingDef def)
    {
        parameters.singleThingDef = def;
        BaseGen.symbolStack.Push(RuleDefOf.SingleThing.symbol, parameters);
    }
}