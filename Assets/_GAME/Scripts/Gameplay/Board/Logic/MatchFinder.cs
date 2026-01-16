using System.Collections.Generic;
using UnityEngine;

public static class MatchFinder
{
    private static readonly Queue<Tile> _searchQueue = new Queue<Tile>(100);
    private static readonly HashSet<Tile> _visited = new HashSet<Tile>();

    private static readonly Vector2Int[] _directions =
    {
        Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down
    };

    public static List<Tile> FindMatches(Tile startTile, BoardContext ctx)
    {
        List<Tile> matches = new List<Tile>();

        _searchQueue.Clear();
        _visited.Clear();

        _searchQueue.Enqueue(startTile);
        _visited.Add(startTile);
        matches.Add(startTile);

        int targetColor = startTile.ItemType;

        while (_searchQueue.Count > 0)
        {
            Tile current = _searchQueue.Dequeue();

            foreach (Vector2Int dir in _directions)
            {
                int newX = current.x + dir.x;
                int newY = current.y + dir.y;

                if (newX >= 0 && newX < ctx.Columns && newY >= 0 && newY < ctx.Rows)
                {
                    Tile neighbor = ctx.Grid[newX, newY];

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