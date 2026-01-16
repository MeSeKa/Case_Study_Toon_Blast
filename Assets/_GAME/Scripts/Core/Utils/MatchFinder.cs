using System.Collections.Generic;
using UnityEngine;

// MonoBehaviour deðil, statik bir yardýmcý sýnýf
public static class MatchFinder
{
    // BoardManager'daki Grid'i parametre olarak alýyoruz
    public static List<Tile> FindMatches(Tile startTile, Tile[,] grid, int rows, int columns)
    {
        List<Tile> matches = new List<Tile>();

        bool[,] visited = new bool[columns, rows];
        Queue<Tile> tilesToCheck = new Queue<Tile>();

        tilesToCheck.Enqueue(startTile);
        visited[startTile.x, startTile.y] = true;
        matches.Add(startTile);

        int targetColor = startTile.ItemType;

        while (tilesToCheck.Count > 0)
        {
            Tile current = tilesToCheck.Dequeue();

            // Komþularý bulmak için grid'i gönderiyoruz
            List<Tile> neighbors = GetNeighbors(current, grid, rows, columns);

            foreach (Tile neighbor in neighbors)
            {
                if (!visited[neighbor.x, neighbor.y] && neighbor.ItemType == targetColor)
                {
                    matches.Add(neighbor);
                    tilesToCheck.Enqueue(neighbor);
                    visited[neighbor.x, neighbor.y] = true;
                }
            }
        }

        return matches;
    }

    private static List<Tile> GetNeighbors(Tile tile, Tile[,] grid, int rows, int columns)
    {
        List<Tile> neighbors = new List<Tile>();
        Vector2Int[] directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        foreach (Vector2Int dir in directions)
        {
            int newX = tile.x + dir.x;
            int newY = tile.y + dir.y;

            if (newX >= 0 && newX < columns && newY >= 0 && newY < rows)
            {
                if (grid[newX, newY] != null)
                {
                    neighbors.Add(grid[newX, newY]);
                }
            }
        }
        return neighbors;
    }
}