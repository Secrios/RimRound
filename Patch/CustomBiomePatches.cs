using HarmonyLib;
using RimRound.Biomes;
using RimWorld.Planet;

namespace RimRound.Patch;

[Harmony]
public static class CustomBiomePatches
{
    [HarmonyPatch(typeof(World), nameof(NaturalRockTypesIn))]
    [HarmonyPrefix]
    public static bool NaturalRockTypesIn(World __instance, int tile, ref IEnumerable<ThingDef> __result)
    {
        var world = __instance;
        
        if (!CustomBiomeExtension.TryGet(world.grid[tile]?.biome, out var extension))
            return true;

        return (__result = extension.NaturalRockTypesIn(world, tile)) is null;
    }
    
    [HarmonyPatch(typeof(GenStep_Terrain), nameof(TerrainFrom))]
    [HarmonyPrefix]
    public static bool TerrainFrom(
        ref TerrainDef __result, 
        IntVec3 c, 
        Map map, 
        float elevation, 
        float fertility, 
        RiverMaker river, 
        bool preferSolid)
    {
        var biome = map?.Biome;
        
        if (!CustomBiomeExtension.TryGet(biome, out var extension))
            return true;

        CustomBiomeExtension.TerrainRequest request = new(c, map, elevation, fertility, river, preferSolid);
        
        return (__result = extension.TerrainAt(request)) is null;
    }

    private static extern void Generate();

    // caves using predefined terrain, we don't wanna use it
    [HarmonyPatch(typeof(GenStep_CavesTerrain), nameof(Generate))]
    [HarmonyPrefix]
    public static bool CavesTerrain_Generate(Map map, GenStepParams parms)
    {
        if (!CustomBiomeExtension.TryGet(map?.Biome, out var extension))
            return true;

        return extension.UseCavesTerrain;
    }

    [HarmonyPatch(typeof(GenStep_ScatterThings), nameof(Generate))]
    [HarmonyPrefix]
    public static bool ScatterThings_Generate(GenStep_ScatterThings __instance, Map map, GenStepParams parms)
    {
        if (!CustomBiomeExtension.TryGet(map?.Biome, out var extension))
            return true;

        ref var def = ref __instance.thingDef;

        if (def == ThingDefOf.SteamGeyser)
            def = extension.GetGeyser(map, parms) ?? def;
        
        return true;
    }
    
    // original requires proper definition
    [HarmonyPatch(typeof(PlaceWorker_OnSteamGeyser), nameof(AllowsPlacing))]
    [HarmonyPrefix]
    public static bool AllowsPlacing(
        ref AcceptanceReport __result,
        BuildableDef checkingDef, 
        IntVec3 loc, 
        Rot4 rot, 
        Map map, 
        Thing thingToIgnore = null, 
        Thing thing = null)
    {
        var geyser = map.thingGrid.ThingAt<Building_SteamGeyser>(loc);
        
        if (geyser is null || geyser.Position != loc)
            __result = "MustPlaceOnSteamGeyser".Translate();
        else __result = true;
        
        return false;
    }
    
    [HarmonyPatch(typeof(PlaceWorker_OnSteamGeyser), nameof(ForceAllowPlaceOver))]
    [HarmonyPrefix]
    public static bool ForceAllowPlaceOver(BuildableDef otherDef, ref bool __result)
    {
        if(otherDef is ThingDef def)
            __result = typeof(Building_SteamGeyser).IsAssignableFrom(def.thingClass);
        
        return false;
    }
}