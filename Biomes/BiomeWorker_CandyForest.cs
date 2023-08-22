﻿using RimWorld.Planet;

namespace RimRound.Biomes;

public class BiomeWorker_CandyForest : BiomeWorker
{
    public override float GetScore(Tile tile, int tileID)
    {
        if (tile.WaterCovered)
        {
            return -100f;
        }
        if (tile.temperature < -7f)
        {
            return 0f;
        }
        if (tile.rainfall < 600)
        {
            return 0f;
        }
        return 12f + (tile.temperature - 7f) + (tile.rainfall - 600f) / 120f;
    }
}