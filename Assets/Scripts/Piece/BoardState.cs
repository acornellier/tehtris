using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardState
{
    public bool[,] Tiles { get; private set; }
    private List<TetrominoData> queue;
    private TetrominoData heldPiece;
    private bool holdingLocked;

    public Vector2Int PiecePosition { get; private set; }
    private Vector2Int[] pieceCells = new Vector2Int[4];
    private TetrominoData pieceData;

    private int pieceRotation;

    public int PieceRotation
    {
        get => pieceRotation;
        private set => pieceRotation = value % 4;
    }

    public int Columns => Tiles.GetLength(0);
    public int Rows => Tiles.GetLength(1);

    private BoardState()
    {
    }

    public BoardState(Board board)
    {
        Tiles = new bool[board.boardSize.x, board.boardSize.y];
        queue = new List<TetrominoData>(board.tetrominoQueue.NextTetrominos);
        heldPiece = board.holder.HeldPiece;

        PiecePosition = new Vector2Int(
            board.ActivePiece.Position.x - board.Bounds.xMin,
            board.ActivePiece.Position.y - board.Bounds.yMin
        );
        pieceCells = (Vector2Int[])board.ActivePiece.Cells.Clone();
        pieceData = board.ActivePiece.Data;
        pieceRotation = board.ActivePiece.RotationIndex;

        for (var x = board.Bounds.xMin; x < board.Bounds.xMax; ++x)
        {
            for (var y = board.Bounds.yMin; y < board.Bounds.yMax; ++y)
            {
                var position = new Vector3Int(x, y, 0);
                Tiles[x - board.Bounds.xMin, y - board.Bounds.yMin] = board.Tilemap.HasTile(position);
            }
        }
    }

    public BoardState DeepClone()
    {
        var newState = new BoardState
        {
            Tiles = (bool[,])Tiles.Clone(),
            queue = new List<TetrominoData>(queue),
            heldPiece = heldPiece,
            PiecePosition = PiecePosition,
            pieceCells = (Vector2Int[])pieceCells.Clone(),
            pieceData = pieceData,
        };

        return newState;
    }

    private bool IsValidPosition(Vector2Int position)
    {
        return pieceCells.All(
            cell =>
            {
                var tilePosition = position + cell;
                return tilePosition.x >= 0 &&
                       tilePosition.x < Tiles.GetLength(0) &&
                       tilePosition.y >= 0 &&
                       tilePosition.y < Tiles.GetLength(1) &&
                       !Tiles[tilePosition.x, tilePosition.y];
            }
        );
    }

    public void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
        }

        Lock();
    }

    public bool Move(Vector2Int translation)
    {
        var newPosition = PiecePosition + translation;

        var valid = IsValidPosition(newPosition);
        if (!valid)
            return false;

        PiecePosition = newPosition;

        return true;
    }

    public void Rotate(int rotationAmount)
    {
        var direction = rotationAmount % 4;

        switch (direction)
        {
            case 2:
                Rotate(1);
                Rotate(1);
                return;
            case 3:
                direction = -1;
                break;
        }

        var originalRotationIndex = pieceRotation;
        PieceRotation += direction;

        if (PieceRotation == originalRotationIndex)
            return;

        ApplyRotationMatrix(direction);

        if (TestWallKicks(PieceRotation, direction))
            return;

        PieceRotation = originalRotationIndex;
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

    private bool TestWallKicks(int newRotationIndex, int direction)
    {
        var wallKickIndex = GetWallKickIndex(newRotationIndex, direction);

        for (var i = 0; i < pieceData.WallKicks.GetLength(1); ++i)
        {
            if (Move(pieceData.WallKicks[wallKickIndex, i]))
                return true;
        }

        return false;
    }

    private int GetWallKickIndex(int newRotationIndex, int direction)
    {
        var wallKickIndex = newRotationIndex * 2;

        if (direction < 0)
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

    private void Lock()
    {
        foreach (var cell in pieceCells)
        {
            var newPosition = PiecePosition + cell;
            Tiles[newPosition.x, newPosition.y] = true;
        }
    }

    private void SpawnNextPiece()
    {
        var nextTetromino = queue[0];
        queue.RemoveAt(0);
        SpawnPiece(nextTetromino);
    }

    private void SpawnPiece(TetrominoData data)
    {
        var spawnPosition = new Vector2Int(Columns / 2 - 1, Rows - 3);
        if (data.tetromino == Tetromino.I)
            spawnPosition.y -= 1;

        PiecePosition = spawnPosition;
        pieceCells = (Vector2Int[])data.Cells.Clone();
        pieceData = data;
        PieceRotation = 0;

        holdingLocked = false;
    }

    public void HoldPiece()
    {
        if (holdingLocked) return;

        var prevHeldPiece = heldPiece;

        heldPiece = pieceData;
        pieceData = heldPiece;

        if (prevHeldPiece != null)
            SpawnPiece(prevHeldPiece);
        else
            SpawnNextPiece();
    }

    public void ClearLines()
    {
        var linesToClear = Enumerable
            .Range(0, Rows)
            .Where(
                y =>
                    Enumerable
                        .Range(0, Columns)
                        .All(x => Tiles[x, y])
            )
            .ToList();

        if (!linesToClear.Any())
            return;

        var linesCleared = 0;
        for (var y = linesToClear[0]; y < Rows; ++y)
        {
            var clearing = linesToClear.Contains(y);
            for (var x = 0; x < Columns; ++x)
            {
                if (!clearing)
                    Tiles[x, y - linesCleared] = Tiles[x, y];

                Tiles[x, y] = false;
            }

            if (clearing)
                linesCleared += 1;
        }
    }
}
