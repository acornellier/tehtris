using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    public Tile tile;
    public Board board;
    private Vector2Int[] cells;
    private Vector2Int position;

    private Tilemap tilemap;

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
        Array.Copy(board.ActivePiece.Cells, cells, board.ActivePiece.Cells.Length);
    }

    private void Drop()
    {
        var newPosition = board.ActivePiece.Position;
        var bottom = -board.boardSize.y / 2 - 1;

        Utilities.ClearPiece(board.Tilemap, board.ActivePiece);

        for (var y = newPosition.y; y >= bottom; --y)
        {
            newPosition.y = y;
            if (!board.IsValidPosition(board.ActivePiece, newPosition))
            {
                break;
            }

            position = newPosition;
        }

        Utilities.SetPiece(board.Tilemap, board.ActivePiece);
    }

    private void Set()
    {
        Utilities.SetCells(tilemap, cells, tile, position);
    }
}
