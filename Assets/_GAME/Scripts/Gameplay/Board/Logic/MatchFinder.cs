using System.Collections.Generic;
using UnityEngine;

public static class MatchFinder
{
    // Static buffers to prevent garbage collection (Zero-Allocation strategy)
    private static readonly Queue<Tile> _searchQueue = new Queue<Tile>(100);
    private static readonly HashSet<Tile> _visited = new HashSet<Tile>();

    // Neighbors: Right, Left, Up, Down
    private static readonly Vector2Int[] _directions =
    {
        Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down
    };

    // Performs a Breadth-First Search (BFS) to find connected tiles of the same color
    public static List<Tile> FindMatches(Tile startTile, BoardContext ctx)
    {
        List<Tile> matches = new List<Tile>();

        // Clear buffers before use
        _searchQueue.Clear();
        _visited.Clear();

        // Initialize search
        _searchQueue.Enqueue(startTile);
        _visited.Add(startTile);
        matches.Add(startTile);

        int targetColor = startTile.ItemType;

        // BFS Loop
        while (_searchQueue.Count > 0)
        {
            Tile current = _searchQueue.Dequeue();

            foreach (Vector2Int dir in _directions)
            {
                int newX = current.x + dir.x;
                int newY = current.y + dir.y;

                // Boundary Check
                if (newX >= 0 && newX < ctx.Columns && newY >= 0 && newY < ctx.Rows)
                {
                    Tile neighbor = ctx.Grid[newX, newY];

                    // Match Check: Must be valid, same color, and not visited
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