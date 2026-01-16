using UnityEngine;
using System.Collections.Generic;

// Bu sýnýfýn tek iþi: Mevcut tahtaya bakýp taþlarýn tiplerini (A, B, C durumlarýný) güncellemek.
public static class BoardVisualizer
{
    public static void UpdateAllIcons(Tile[,] grid, LevelData levelData)
    {
        int width = levelData.columns;
        int height = levelData.rows;
        bool[,] visited = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = grid[x, y];

                // Eðer taþ varsa ve daha önce bu grubu kontrol etmediysek
                if (tile != null && !visited[x, y])
                {
                    // Grubu bul
                    List<Tile> group = MatchFinder.FindMatches(tile, grid, levelData.rows, levelData.columns);

                    // Bu gruptaki herkesi "ziyaret edildi" iþaretle ki tekrar hesaplamayalým (Optimizasyon)
                    foreach (Tile t in group) visited[t.x, t.y] = true;

                    // Grubun büyüklüðüne göre görsel durumuna karar ver (Logic -> Visual Translation)
                    int visualState = 0; // Default
                    if (group.Count > levelData.conditionC) visualState = 3;
                    else if (group.Count > levelData.conditionB) visualState = 2;
                    else if (group.Count > levelData.conditionA) visualState = 1;

                    // Her taþa yeni durumunu bildir
                    foreach (Tile member in group)
                    {
                        member.SetVisualState(visualState);
                    }
                }
            }
        }
    }
}