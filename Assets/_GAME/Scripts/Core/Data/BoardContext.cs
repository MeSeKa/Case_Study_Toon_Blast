using System.Collections.Generic;
using UnityEngine;

// Class deðil Struct (Hafýza dostu, Stack'te yaþar, GC oluþturmaz)
public struct BoardContext
{
    public Tile[,] Grid;
    public List<Tile> AllTiles; // Shuffle iþlemleri için gerekli
    public int Columns;
    public int Rows;

    public BoardContext(Tile[,] grid, List<Tile> allTiles, int columns, int rows)
    {
        Grid = grid;
        AllTiles = allTiles;
        Columns = columns;
        Rows = rows;
    }
}