using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardState
{
    public bool[,] tiles;
    public List<TetrominoData> queue;
    public TetrominoData heldPiece;

    public Vector2Int piecePosition;
    public int pieceRotation;
    public Vector2Int[] pieceCells;
    public TetrominoData pieceData;

    private BoardState()
    {
    }

    public BoardState(Board board)
    {
        tiles = new bool[board.boardSize.x, board.boardSize.y];
        queue = new List<TetrominoData>(board.tetrominoQueue.NextTetrominos);
        heldPiece = board.holder.HeldPiece;

        piecePosition = new Vector2Int(
            board.ActivePiece.Position.x - board.Bounds.xMin,
            board.ActivePiece.Position.y - board.Bounds.yMin
        );
        pieceCells = board.ActivePiece.Cells;
        pieceData = board.ActivePiece.Data;
        pieceRotation = board.ActivePiece.RotationIndex;

        for (var x = board.Bounds.xMin; x < board.Bounds.xMax; ++x)
        {
            for (var y = board.Bounds.yMin; y < board.Bounds.yMax; ++y)
            {
                var position = new Vector3Int(x, y, 0);
                tiles[x - board.Bounds.xMin, y - board.Bounds.yMin] = board.Tilemap.HasTile(position);
            }
        }
    }

    public BoardState DeepClone()
    {
        return new BoardState
        {
            tiles = (bool[,])tiles.Clone(),
            queue = new List<TetrominoData>(queue),
            heldPiece = heldPiece,
            piecePosition = piecePosition,
            pieceCells = pieceCells,
            pieceData = pieceData,
        };
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return pieceCells.All(
            cell =>
            {
                var tilePosition = position + cell;
                return tilePosition.x >= 0 &&
                       tilePosition.x < tiles.GetLength(0) &&
                       tilePosition.y >= 0 &&
                       tilePosition.y < tiles.GetLength(1) &&
                       !tiles[tilePosition.x, tilePosition.y];
            }
        );
    }

    public void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
        }

        // Lock();
    }

    public bool Move(Vector2Int translation)
    {
        var newPosition = piecePosition + translation;

        var valid = IsValidPosition(newPosition);
        if (!valid)
            return false;

        piecePosition = newPosition;

        return true;
    }

    public void Rotate(int direction)
    {
        var originalRotationIndex = pieceRotation;
        pieceRotation = (pieceRotation + direction) % 4;

        ApplyRotationMatrix(direction);

        if (TestWallKicks(pieceRotation, direction))
            return;

        pieceRotation = originalRotationIndex;
        ApplyRotationMatrix(-direction);
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (var i = 0; i < pieceCells.Length; i++)
        {
            Vector2 cell = pieceCells[i];

            int x;
            int y;

            switch (pieceData.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt(RotateX(cell, direction));
                    y = Mathf.CeilToInt(RotateY(cell, direction));
                    break;

                case Tetromino.T:
                case Tetromino.J:
                case Tetromino.L:
                case Tetromino.S:
                case Tetromino.Z:
                default:
                    x = Mathf.RoundToInt(RotateX(cell, direction));
                    y = Mathf.RoundToInt(RotateY(cell, direction));
                    break;
            }

            pieceCells[i] = new Vector2Int(x, y);
        }
    }

    private bool TestWallKicks(int newRotationIndex, int rotationDirection)
    {
        var wallKickIndex = GetWallKickIndex(newRotationIndex, rotationDirection);

        for (var i = 0; i < pieceData.WallKicks.GetLength(1); ++i)
        {
            if (Move(pieceData.WallKicks[wallKickIndex, i]))
                return true;
        }

        return false;
    }

    private int GetWallKickIndex(int newRotationIndex, int rotationDirection)
    {
        var wallKickIndex = newRotationIndex * 2;

        if (rotationDirection < 0)
            wallKickIndex--;

        return (wallKickIndex + pieceData.WallKicks.GetLength(0)) % pieceData.WallKicks.GetLength(0);
    }

    private static float RotateX(Vector3 cell, int direction)
    {
        return cell.x * Data.RotationMatrix[0] * direction
               + cell.y * Data.RotationMatrix[1] * direction;
    }

    private static float RotateY(Vector3 cell, int direction)
    {
        return cell.x * Data.RotationMatrix[2] * direction
               + cell.y * Data.RotationMatrix[3] * direction;
    }

    public float Evaluate()
    {
        return tiles.GetLength(0) - piecePosition.x;
    }

    // private void Lock()
    // {
    //     Utilities.SetPiece(Board.Tilemap, this);
    //     Board.ClearLines();
    //     Board.SpawnNextPiece();
    // }
    //
    // public void SpawnNextPiece()
    // {
    //     var nextTetromino = queue.();
    //     SpawnPiece(nextTetromino);
    // }
    //
    // private void SpawnPiece(TetrominoData data)
    // {
    //     var spawnPosition = new Vector2Int(-1, Bounds.yMax - 2);
    //     if (data.tetromino == Tetromino.I)
    //     {
    //         spawnPosition.y -= 1;
    //     }
    //
    //     ActivePiece.Initialize(this, spawnPosition, data);
    //     holdingLocked = false;
    //     Utilities.SetPiece(Tilemap, ActivePiece);
    // }
    //
    // private void Clear(Piece piece)
    // {
    //     foreach (var cell in piece.Cells)
    //     {
    //         Tilemap.SetTile((Vector3Int)(cell + piece.Position), null);
    //     }
    // }
    //
    // public void ClearLines()
    // {
    //     var linesToClear = Enumerable
    //         .Range(board.Bounds.yMin, board.Bounds.size.y)
    //         .Where(
    //             y =>
    //                 Enumerable
    //                     .Range(Bounds.xMin, Bounds.size.x)
    //                     .All(x => Tilemap.HasTile(new Vector3Int(x, y, 0)))
    //         )
    //         .ToList();
    //
    //     if (!linesToClear.Any())
    //     {
    //         return;
    //     }
    //
    //     var linesCleared = 0;
    //     for (var y = linesToClear[0]; y < Bounds.yMax; ++y)
    //     {
    //         var clearing = linesToClear.Contains(y);
    //         for (var x = Bounds.xMin; x < Bounds.xMax; ++x)
    //         {
    //             var position = new Vector3Int(x, y, 0);
    //             if (!clearing)
    //             {
    //                 Tilemap.SetTile(
    //                     new Vector3Int(x, y - linesCleared, 0),
    //                     Tilemap.GetTile(position)
    //                 );
    //             }
    //
    //             Tilemap.SetTile(new Vector3Int(x, y, 0), null);
    //         }
    //
    //         if (clearing)
    //         {
    //             linesCleared += 1;
    //         }
    //     }
    // }
    //
    // public void HoldPiece()
    // {
    //     if (holdingLocked)
    //     {
    //         return;
    //     }
    //
    //     var prevHeldPiece = holder.HeldPiece;
    //
    //     holder.SetHeldPiece(ActivePiece.Data);
    //     Clear(ActivePiece);
    //
    //     if (prevHeldPiece != null)
    //     {
    //         SpawnPiece(prevHeldPiece);
    //     }
    //     else
    //     {
    //         SpawnNextPiece();
    //     }
    //
    //     holdingLocked = true;
    // }
}
