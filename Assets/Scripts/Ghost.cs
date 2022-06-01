using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class Ghost : MonoBehaviour
{
    public Tile tile;
    public Board board;
    public Piece piece;

    private Tilemap tilemap;
    private Vector2Int position;
    private Vector2Int[] cells;

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        cells = new Vector2Int[4];
    }

    public void LateUpdate()
    {
        Clear();
        Copy();
        Drop();
        Set();
    }

    private void Clear()
    {
        Utilities.SetCells(tilemap, cells, null, position);
    }

    private void Copy()
    {
        Array.Copy(piece.cells, cells, piece.cells.Length);
    }

    private void Drop()
    {
        var newPosition = piece.position;
        var bottom = -board.boardSize.y / 2 - 1;

        Utilities.ClearPiece(board.tilemap, piece);

        for (int y = newPosition.y; y >= bottom; --y)
        {
            newPosition.y = y;
            if (!board.IsValidPosition(piece, newPosition))
                break;

            position = newPosition;
        }

        Utilities.SetPiece(board.tilemap, piece);
    }

    private void Set()
    {
        Utilities.SetCells(tilemap, cells, tile, position);
    }
}
