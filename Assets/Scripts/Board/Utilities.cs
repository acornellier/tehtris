using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Utilities
{
    public static void SetCells(
        Tilemap tilemap,
        IEnumerable<Vector2Int> cells,
        Tile tile,
        Vector2Int position = new()
    )
    {
        foreach (var cell in cells)
        {
            tilemap.SetTile((Vector3Int)(position + cell), tile);
        }
    }
}
