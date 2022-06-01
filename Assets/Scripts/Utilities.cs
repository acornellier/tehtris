using UnityEngine;
using UnityEngine.Tilemaps;

public static class Utilities
{
    public static void SetPiece(Tilemap tilemap, Piece piece)
    {
        SetCells(tilemap, piece.cells, piece.data.tile, piece.position);
    }

    public static void ClearPiece(Tilemap tilemap, Piece piece)
    {
        SetCells(tilemap, piece.cells, null, piece.position);
    }

    public static void SetCells(
        Tilemap tilemap,
        Vector2Int[] cells,
        Tile tile,
        Vector2Int position = new Vector2Int()
    )
    {
        foreach (var cell in cells)
        {
            tilemap.SetTile((Vector3Int)(position + cell), tile);
        }
    }
}
