using UnityEngine;
using System.Collections.Generic;

public static class DeadlockSolver
{
    // 1. GÖREV: Grid kilitli mi kontrolü
    public static bool IsDeadlocked(Tile[,] grid, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile t = grid[x, y];
                if (t != null)
                {
                    // Saða bak
                    if (x < width - 1)
                    {
                        Tile r = grid[x + 1, y];
                        if (r != null && r.ItemType == t.ItemType) return false;
                    }
                    // Yukarý bak
                    if (y < height - 1)
                    {
                        Tile up = grid[x, y + 1];
                        if (up != null && up.ItemType == t.ItemType) return false;
                    }
                }
            }
        }
        return true;
    }

    // 2. GÖREV: Injection (Renk Deðiþtirme) adaylarýný bulma
    // Ýþlem baþarýlýysa true döner ve out parametrelerini doldurur.
    public static bool TryFindInjectionCandidates(Tile[,] grid, int width, int height, List<Tile> allTiles, int maxAttempts, out Tile source, out Tile target)
    {
        source = null;
        target = null;

        int attempts = 0;
        while (attempts < maxAttempts)
        {
            source = allTiles[Random.Range(0, allTiles.Count)];
            List<Tile> validNeighbors = new List<Tile>();

            if (source.x < width - 1) { Tile t = grid[source.x + 1, source.y]; if (t != null) validNeighbors.Add(t); }
            if (source.x > 0) { Tile t = grid[source.x - 1, source.y]; if (t != null) validNeighbors.Add(t); }
            if (source.y < height - 1) { Tile t = grid[source.x, source.y + 1]; if (t != null) validNeighbors.Add(t); }
            if (source.y > 0) { Tile t = grid[source.x, source.y - 1]; if (t != null) validNeighbors.Add(t); }

            if (validNeighbors.Count > 0)
            {
                target = validNeighbors[Random.Range(0, validNeighbors.Count)];
                return true; // Bulduk!
            }
            attempts++;
        }

        // Fail-Safe: Bulamazsa ilk ikiliyi ver
        if (allTiles.Count >= 2)
        {
            source = allTiles[0];
            target = allTiles[1];
            return true;
        }

        return false;
    }

    // 3. GÖREV: Force Match (Zorla Taþýma) adaylarýný bulma
    public static bool TryFindForceMatchCandidates(Tile[,] grid, int width, int height, List<Tile> allTiles, int maxAttempts,
        out Tile tileA, out Tile tileB, out Tile slot1, out Tile slot2)
    {
        tileA = null; tileB = null; slot1 = null; slot2 = null;

        // A. Renk Analizi
        int targetColor = -1;
        Dictionary<int, int> counts = new Dictionary<int, int>();
        foreach (var t in allTiles)
        {
            if (!counts.ContainsKey(t.ItemType)) counts[t.ItemType] = 0;
            counts[t.ItemType]++;
            if (counts[t.ItemType] >= 2) { targetColor = t.ItemType; break; }
        }

        if (targetColor == -1) return false; // Çözüm imkansýz

        // B. Taþýnacak Taþlarý Seç
        tileA = allTiles.Find(t => t.ItemType == targetColor);
        tileB = allTiles.FindLast(t => t.ItemType == targetColor);

        // C. Hedef Slotlarý Seç
        int attempts = 0;
        while (attempts < maxAttempts)
        {
            int randX = Random.Range(0, width);
            int randY = Random.Range(0, height);
            slot1 = grid[randX, randY];

            if (slot1 == null) { attempts++; continue; }

            List<Tile> validNeighbors = new List<Tile>();
            if (randX < width - 1) { Tile t = grid[randX + 1, randY]; if (t != null) validNeighbors.Add(t); }
            if (randX > 0) { Tile t = grid[randX - 1, randY]; if (t != null) validNeighbors.Add(t); }
            if (randY < height - 1) { Tile t = grid[randX, randY + 1]; if (t != null) validNeighbors.Add(t); }
            if (randY > 0) { Tile t = grid[randX, randY - 1]; if (t != null) validNeighbors.Add(t); }

            if (validNeighbors.Count > 0)
            {
                slot2 = validNeighbors[Random.Range(0, validNeighbors.Count)];
                return true; // Bulduk!
            }
            attempts++;
        }

        // Fail-Safe
        slot1 = grid[0, 0];
        slot2 = grid[0, 1];
        return true;
    }

    // 4. GÖREV: Listeyi Karýþtýrma (Fisher-Yates Shuffle)
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
    public static bool IsSolvable(List<Tile> allTiles)
    {
        Dictionary<int, int> counts = new Dictionary<int, int>();
        foreach (var t in allTiles)
        {
            if (!counts.ContainsKey(t.ItemType)) counts[t.ItemType] = 0;
            counts[t.ItemType]++;

            // Eðer herhangi bir renkten 2 veya daha fazla varsa, karýþtýrarak çözülebilir.
            if (counts[t.ItemType] >= 2) return true;
        }
        return false;
    }
}