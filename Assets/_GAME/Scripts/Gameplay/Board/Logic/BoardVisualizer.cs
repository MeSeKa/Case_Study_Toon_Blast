using UnityEngine;
using System.Collections.Generic;

public static class BoardVisualizer
{
    public static void UpdateAllIcons(BoardContext ctx, LevelData levelData)
    {
        bool[,] visited = new bool[ctx.Columns, ctx.Rows];

        for (int x = 0; x < ctx.Columns; x++)
        {
            for (int y = 0; y < ctx.Rows; y++)
            {
                Tile tile = ctx.Grid[x, y];

                if (tile != null && !visited[x, y])
                {
                    List<Tile> group = MatchFinder.FindMatches(tile, ctx);

                    foreach (Tile t in group) visited[t.x, t.y] = true;

                    int visualState = 0;
                    if (group.Count > levelData.conditionC) visualState = 3;
                    else if (group.Count > levelData.conditionB) visualState = 2;
                    else if (group.Count > levelData.conditionA) visualState = 1;

                    foreach (Tile member in group)
                    {
                        member.SetVisualState(visualState);
                    }
                }
            }
        }
    }
}