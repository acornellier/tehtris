using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Utilities
{
    public static void SetPiece(Tilemap tilemap, Piece piece)
    {
        SetCells(tilemap, piece.Cells, piece.Data.tile, piece.Position);
    }

    public static void ClearPiece(Tilemap tilemap, Piece piece)
    {
        SetCells(tilemap, piece.Cells, null, piece.Position);
    }

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
