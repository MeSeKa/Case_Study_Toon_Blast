using System.Collections.Generic;
using UnityEngine;

public static class MatchFinder
{
    // --- OPTÝMÝZASYON: Statik Bufferlar ---
    // Bu listeleri her seferinde yeniden oluþturmak yerine (new),
    // temizleyip (Clear) tekrar kullanacaðýz.
    // PDF'te max tahta boyutu 10x10 olduðu için kapasiteleri baþtan veriyoruz[cite: 12].

    private static readonly Queue<Tile> _searchQueue = new Queue<Tile>(100);
    private static readonly HashSet<Tile> _visited = new HashSet<Tile>();

    // Yönleri statik yapýyoruz, her seferinde new array oluþmasýn.
    private static readonly Vector2Int[] _directions =
    {
        Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down
    };

    public static List<Tile> FindMatches(Tile startTile, Tile[,] grid, int rows, int columns)
    {
        // 1. Her çaðrýda yeni liste oluþturmak yerine, dönülecek sonucu oluþturuyoruz.
        // Not: Sonuç listesini 'static' yapamayýz çünkü Coroutine'ler bunu kullanýrken
        // baþka bir iþlem listeyi temizleyebilir. Sadece "iþlem araçlarýný" static yapýyoruz.
        List<Tile> matches = new List<Tile>();

        // 2. Bufferlarý Temizle (Reset)
        _searchQueue.Clear();
        _visited.Clear();

        // 3. Baþlangýç Ayarlarý
        _searchQueue.Enqueue(startTile);
        _visited.Add(startTile);
        matches.Add(startTile);

        int targetColor = startTile.ItemType;

        while (_searchQueue.Count > 0)
        {
            Tile current = _searchQueue.Dequeue();

            // --- OPTÝMÝZASYON: Inlining ---
            // GetNeighbors metodunu çaðýrýp yeni liste oluþturmak yerine (GC Alloc),
            // döngüyü burada manuel yapýyoruz.
            foreach (Vector2Int dir in _directions)
            {
                int newX = current.x + dir.x;
                int newY = current.y + dir.y;

                // Sýnýr Kontrolü
                if (newX >= 0 && newX < columns && newY >= 0 && newY < rows)
                {
                    Tile neighbor = grid[newX, newY];

                    // Null deðilse, ayný renkse ve daha önce ziyaret edilmediyse
                    if (neighbor != null &&
                        neighbor.ItemType == targetColor &&
                        !_visited.Contains(neighbor))
                    {
                        matches.Add(neighbor);
                        _searchQueue.Enqueue(neighbor);
                        _visited.Add(neighbor);
                    }
                }
            }
        }

        return matches;
    }
}