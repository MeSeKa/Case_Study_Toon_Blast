using UnityEngine;
using System.Collections.Generic;

public static class DeadlockSolver
{
    // Checks if there are any possible moves on the board
    public static bool IsDeadlocked(BoardContext ctx)
    {
        for (int x = 0; x < ctx.Columns; x++)
        {
            for (int y = 0; y < ctx.Rows; y++)
            {
                Tile t = ctx.Grid[x, y];
                if (t != null)
                {
                    // Check Right
                    if (x < ctx.Columns - 1)
                    {
                        Tile r = ctx.Grid[x + 1, y];
                        if (r != null && r.ItemType == t.ItemType) return false;
                    }
                    // Check Up
                    if (y < ctx.Rows - 1)
                    {
                        Tile up = ctx.Grid[x, y + 1];
                        if (up != null && up.ItemType == t.ItemType) return false;
                    }
                }
            }
        }
        return true;
    }

    // Phase 1: Finds a candidate tile to change its color to match a neighbor
    public static bool TryFindInjectionCandidates(BoardContext ctx, int maxAttempts, out Tile source, out Tile target)
    {
        source = null;
        target = null;

        if (ctx.AllTiles == null || ctx.AllTiles.Count < 2) return false;

        int attempts = 0;
        while (attempts < maxAttempts)
        {
            source = ctx.AllTiles[Random.Range(0, ctx.AllTiles.Count)];
            List<Tile> validNeighbors = new List<Tile>();

            // Check 4 directions for valid neighbors
            if (source.x < ctx.Columns - 1)
            {
                Tile t = ctx.Grid[source.x + 1, source.y];
                if (t != null) validNeighbors.Add(t);
            }
            if (source.x > 0)
            {
                Tile t = ctx.Grid[source.x - 1, source.y];
                if (t != null) validNeighbors.Add(t);
            }
            if (source.y < ctx.Rows - 1)
            {
                Tile t = ctx.Grid[source.x, source.y + 1];
                if (t != null) validNeighbors.Add(t);
            }
            if (source.y > 0)
            {
                Tile t = ctx.Grid[source.x, source.y - 1];
                if (t != null) validNeighbors.Add(t);
            }

            if (validNeighbors.Count > 0)
            {
                target = validNeighbors[Random.Range(0, validNeighbors.Count)];
                return true;
            }
            attempts++;
        }

        // Fail-Safe: If random search fails, pick the first two available tiles
        if (ctx.AllTiles.Count >= 2)
        {
            source = ctx.AllTiles[0];
            target = ctx.AllTiles[1];
            return true;
        }

        return false;
    }

    // Phase 3: Finds two tiles to forcibly move adjacent to each other
    public static bool TryFindForceMatchCandidates(BoardContext ctx, int maxAttempts,
        out Tile tileA, out Tile tileB, out Tile slot1, out Tile slot2)
    {
        tileA = null; tileB = null; slot1 = null; slot2 = null;

        // Step A: Find a color that has at least 2 tiles
        int targetColor = -1;
        Dictionary<int, int> counts = new Dictionary<int, int>();
        foreach (var t in ctx.AllTiles)
        {
            if (!counts.ContainsKey(t.ItemType)) counts[t.ItemType] = 0;
            counts[t.ItemType]++;
            if (counts[t.ItemType] >= 2) { targetColor = t.ItemType; break; }
        }

        if (targetColor == -1) return false; // Impossible to solve

        // Step B: Select the tiles to move
        tileA = ctx.AllTiles.Find(t => t.ItemType == targetColor);
        tileB = ctx.AllTiles.FindLast(t => t.ItemType == targetColor);

        // Step C: Find target slots (a tile and its neighbor)
        int attempts = 0;
        while (attempts < maxAttempts)
        {
            int randX = Random.Range(0, ctx.Columns);
            int randY = Random.Range(0, ctx.Rows);
            slot1 = ctx.Grid[randX, randY];

            if (slot1 == null) { attempts++; continue; }

            List<Tile> validNeighbors = new List<Tile>();
            if (randX < ctx.Columns - 1) { Tile t = ctx.Grid[randX + 1, randY]; if (t != null) validNeighbors.Add(t); }
            if (randX > 0) { Tile t = ctx.Grid[randX - 1, randY]; if (t != null) validNeighbors.Add(t); }
            if (randY < ctx.Rows - 1) { Tile t = ctx.Grid[randX, randY + 1]; if (t != null) validNeighbors.Add(t); }
            if (randY > 0) { Tile t = ctx.Grid[randX, randY - 1]; if (t != null) validNeighbors.Add(t); }

            if (validNeighbors.Count > 0)
            {
                slot2 = validNeighbors[Random.Range(0, validNeighbors.Count)];
                return true;
            }
            attempts++;
        }

        // Fail-Safe: Default to bottom-left corner
        slot1 = ctx.Grid[0, 0];
        slot2 = ctx.Grid[0, 1];
        return true;
    }

    // Generic Fisher-Yates Shuffle
    public static void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int rnd = Random.Range(i, list.Count);
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }

    // Checks if the board has at least one pair of same-colored tiles
    public static bool IsSolvable(List<Tile> allTiles)
    {
        Dictionary<int, int> counts = new Dictionary<int, int>();
        foreach (var t in allTiles)
        {
            if (!counts.ContainsKey(t.ItemType)) counts[t.ItemType] = 0;
            counts[t.ItemType]++;

            if (counts[t.ItemType] >= 2) return true;
        }
        return false;
    }
}