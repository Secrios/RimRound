using HarmonyLib;
using RimRound.Biomes;
using RimRound.Comps;
using RimWorld.Planet;

namespace RimRound.Patch.Plants;

[Harmony]
public static class ChopTree_Designation_Patch
{
    public static bool HasExtension(Thing thing, out Replace_ChopTree_Designation? extension) =>
        (extension = thing?.def?.GetModExtension<Replace_ChopTree_Designation>()) is not null;
    
    [HarmonyPatch(typeof(Designator_PlantsHarvestWood), nameof(CanDesignateThing))]
    [HarmonyPrefix]
    public static bool CanDesignateThing(Thing t, ref AcceptanceReport __result)
    {
        if (!HasExtension(t, out var extension))
            return true;
            
        __result = extension!.CanDesignate(t);
        return false;
    }
    
    [HarmonyPatch(typeof(Designator_Plants), nameof(DesignateThing))]
    [HarmonyPrefix]
    public static bool DesignateThing(Designator_Plants __instance, Thing t)
    {
        if (__instance is not Designator_PlantsHarvestWood designator)
            return true;
        
        if (!HasExtension(t, out var extension))
            return true;
            
        extension!.Designate(designator, t);
        return false;
    }
}