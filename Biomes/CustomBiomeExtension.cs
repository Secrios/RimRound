using RimWorld.Planet;

namespace RimRound.Biomes;

public class CustomBiomeExtension : DefModExtension
{
    public TerrainDef? DefaultTerrain, DefaultRockTerrain;
    public bool UseCavesTerrain = true;
    public List<ThingDef>? NaturalRocks;
    public ThingDef? Geyser;
    public string RocksScatterType = DefaultRockScatterType;
    public static string DefaultRockScatterType = "Rocky";
    
    public static bool TryGet(BiomeDef? biome, out CustomBiomeExtension extension)
    {
        extension = biome?.GetModExtension<CustomBiomeExtension>()!;
        return extension is not null;
    }

    public record struct TerrainRequest(
        IntVec3 Cell,
        Map Map,
        float Elevation,
        float Fertility,
        RiverMaker? River = null,
        bool PreferSolid = false)
    {
        public BiomeDef Biome => Map.Biome;
    }

    public virtual TerrainDef? RockTerrainAt(TerrainRequest request) => 
        GenStep_RocksFromGrid.RockDefAt(request.Cell)?.building.naturalTerrain ?? DefaultRockTerrain;
    
    public virtual TerrainDef? TerrainAt(TerrainRequest request)
    {
        var result = DefaultTerrain;
        
        if (request.Elevation >= 0.61f || request.PreferSolid)
            return RockTerrainAt(request);

        var list = request.Biome.terrainPatchMakers;
        
        for (var i = 0; i < list.Count; i++)
        {
            var terrain = list[i].TerrainAt(request.Cell, request.Map, request.Fertility);
            
            if (terrain is not null)
                return terrain;
        }

        if (result is null)
            return null;

        if (RocksScatterType != DefaultRockScatterType)
            PatchRockTerrain(result);
        
        return result;
    }

    private void PatchRockTerrain(TerrainDef terrain)
    {
        if (terrain.scatterType != DefaultRockScatterType)
            return;
        
        terrain.scatterType = RocksScatterType;
        if(terrain.smoothedTerrain is { } smoothedTerrain)
            smoothedTerrain.scatterType = RocksScatterType;
    }

    public virtual IEnumerable<ThingDef>? NaturalRockTypesIn(World world, int tile)
    {
        if (NaturalRocks is null) return null;

        foreach (var rock in NaturalRocks)
        {
            var building = rock.building;
            var terrain = building.naturalTerrain;
            
        }
        
        return NaturalRocks;
    }

    public virtual ThingDef? GetGeyser(Map map, GenStepParams parms) => Geyser;
}