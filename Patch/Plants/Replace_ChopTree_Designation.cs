namespace RimRound.Patch.Plants;

public class Replace_ChopTree_Designation : DefModExtension
{
    public DesignationDef? Def;
    
    public virtual AcceptanceReport CanDesignate(Thing thing) => AcceptanceReport.WasAccepted;

    public virtual void Designate(Designator_PlantsHarvestWood designator, Thing thing)
    {
        var map = designator.Map;
        var manager = map.designationManager;
        
        manager.RemoveAllDesignationsOn(thing);
        manager.AddDesignation(new (thing, Def ?? DesignationDefOf.CutPlant));    
    }
}