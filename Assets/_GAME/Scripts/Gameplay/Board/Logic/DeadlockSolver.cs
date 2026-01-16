using UnityEngine;
using System.Collections.Generic;

public static class DeadlockSolver
{
    // 1. GÖREV: Grid kilitli mi kontrolü
    // ARTIK: Tek parametre alýyor (BoardContext)
    public static bool IsDeadlocked(BoardContext ctx)
    {
        for (int x = 0; x < ctx.Columns; x++)
        {
            for (int y = 0; y < ctx.Rows; y++)
            {
                Tile t = ctx.Grid[x, y];
                if (t != null)
                {
                    // Saða bak
                    if (x < ctx.Columns - 1)
                    {
                        Tile r = ctx.Grid[x + 1, y];
                        if (r != null && r.ItemType == t.ItemType) return false;
                    }
                    // Yukarý bak
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

    // 2. GÖREV: Injection (Renk Deðiþtirme) adaylarýný bulma
    // ARTIK: Parametre çöplüðü yok, sadece Context ve Config deðeri
    public static bool TryFindInjectionCandidates(BoardContext ctx, int maxAttempts, out Tile source, out Tile target)
    {
        source = null;
        target = null;

        // Liste boþsa iþlem yapma
        if (ctx.AllTiles == null || ctx.AllTiles.Count < 2) return false;

        int attempts = 0;
        while (attempts < maxAttempts)
        {
            source = ctx.AllTiles[Random.Range(0, ctx.AllTiles.Count)];
            List<Tile> validNeighbors = new List<Tile>();

            // 4 Yöne Bak (Context verilerini kullanarak)
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
                return true; // Bulduk!
            }
            attempts++;
        }

        // Fail-Safe: Bulamazsa listenin baþýndaki ilk ikiliyi ver
        if (ctx.AllTiles.Count >= 2)
        {
            source = ctx.AllTiles[0];
            target = ctx.AllTiles[1];
            return true;
        }

        return false;
    }

    // 3. GÖREV: Force Match (Zorla Taþýma) adaylarýný bulma
    public static bool TryFindForceMatchCandidates(BoardContext ctx, int maxAttempts,
        out Tile tileA, out Tile tileB, out Tile slot1, out Tile slot2)
    {
        tileA = null; tileB = null; slot1 = null; slot2 = null;

        // A. Renk Analizi (Hangi renkten en az 2 tane var?)
        int targetColor = -1;
        Dictionary<int, int> counts = new Dictionary<int, int>();
        foreach (var t in ctx.AllTiles)
        {
            if (!counts.ContainsKey(t.ItemType)) counts[t.ItemType] = 0;
            counts[t.ItemType]++;
            if (counts[t.ItemType] >= 2) { targetColor = t.ItemType; break; }
        }

        if (targetColor == -1) return false; // Çözüm imkansýz (Yeterli renk yok)

        // B. Taþýnacak Taþlarý Seç
        tileA = ctx.AllTiles.Find(t => t.ItemType == targetColor);
        tileB = ctx.AllTiles.FindLast(t => t.ItemType == targetColor);

        // C. Hedef Slotlarý Seç
        int attempts = 0;
        while (attempts < maxAttempts)
        {
            int randX = Random.Range(0, ctx.Columns);
            int randY = Random.Range(0, ctx.Rows);
            slot1 = ctx.Grid[randX, randY];

            // Boþ slota denk geldiyse tekrar dene
            if (slot1 == null) { attempts++; continue; }

            // Slot1'in komþularýný bul
            List<Tile> validNeighbors = new List<Tile>();
            if (randX < ctx.Columns - 1) { Tile t = ctx.Grid[randX + 1, randY]; if (t != null) validNeighbors.Add(t); }
            if (randX > 0) { Tile t = ctx.Grid[randX - 1, randY]; if (t != null) validNeighbors.Add(t); }
            if (randY < ctx.Rows - 1) { Tile t = ctx.Grid[randX, randY + 1]; if (t != null) validNeighbors.Add(t); }
            if (randY > 0) { Tile t = ctx.Grid[randX, randY - 1]; if (t != null) validNeighbors.Add(t); }

            if (validNeighbors.Count > 0)
            {
                slot2 = validNeighbors[Random.Range(0, validNeighbors.Count)];
                return true; // Bulduk!
            }
            attempts++;
        }

        // Fail-Safe: Eðer rastgele yer bulunamazsa sol alt köþeyi kullan
        slot1 = ctx.Grid[0, 0];
        slot2 = ctx.Grid[0, 1];
        return true;
    }

    // 4. GÖREV: Listeyi Karýþtýrma (Generic Fisher-Yates Shuffle)
    // Bu metot context'ten baðýmsýz çalýþabilir, o yüzden generic kalmasý daha iyi.
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

    // 5. GÖREV: Çözülebilirlik Kontrolü
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