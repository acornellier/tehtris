using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardState
{
    private const float LockDelay = 0.5f;
    private const float MaxLockDelay = 5f;
    private const float StepDelay = 1f;

    public TileState[,] Tiles { get; private set; }
    public List<TetrominoData> Queue { get; private set; } = new();
    public TetrominoData HeldPiece { get; private set; }
    public Vector2Int PiecePosition { get; private set; }
    public Vector2Int[] PieceCells { get; private set; } = new Vector2Int[4];
    public TetrominoData PieceData { get; private set; }
    public bool GameOver { get; private set; }
    public bool HoldingLocked { get; private set; }
    private int PendingGarbage { get; set; }

    private int pieceRotation;

    public int PieceRotation
    {
        get => pieceRotation;
        private set => pieceRotation = (value % 4 + 4) % 4;
    }

    private float stepTime;
    private bool locking;
    private float lockTime; // delayed when moving
    private float maxLockTime; // never delayed

    public int Columns => Tiles.GetLength(0);
    public int Rows => Tiles.GetLength(1);

    private BoardState()
    {
    }

    public BoardState(Vector2Int boardSize, IEnumerable<TetrominoData> queue)
    {
        Tiles = new TileState[boardSize.x, boardSize.y];
        PushToQueue(queue);
        SpawnNextPiece();
    }

    public void PushToQueue(IEnumerable<TetrominoData> tetrominos)
    {
        Queue.AddRange(new List<TetrominoData>(tetrominos));
    }

    public BoardState DeepClone()
    {
        var newState = new BoardState
        {
            Tiles = (TileState[,])Tiles.Clone(),
            Queue = new List<TetrominoData>(Queue),
            HeldPiece = HeldPiece,
            PiecePosition = PiecePosition,
            PieceRotation = PieceRotation,
            PieceCells = (Vector2Int[])PieceCells.Clone(),
            PieceData = PieceData,
            GameOver = GameOver,
            HoldingLocked = HoldingLocked,
            PendingGarbage = PendingGarbage,
        };

        return newState;
    }

    public MoveResults MakeMove(Move move)
    {
        var moveResults = new MoveResults();
        if (move.hold)
        {
            Hold();
            moveResults.held = true;
        }
        else if (move.hardDrop)
        {
            moveResults.locked = true;
            moveResults.linesCleared = HardDrop();
        }
        else
        {
            if (move.direction != Move.Direction.None)
                MoveTranslation(Move.DirectionToTranslation(move.direction));

            if (move.rotation != Move.Rotation.None)
                Rotate(Move.RotationToAmount(move.rotation));

            HandleStep();
            HandleLocking(moveResults);
        }

        return moveResults;
    }

    private void HandleStep()
    {
        if (Time.time < stepTime)
            return;

        stepTime += StepDelay;
        var success = MoveTranslation(Vector2Int.down);

        if (success)
        {
            locking = false;
        }
        else if (!locking)
        {
            locking = true;
            lockTime = Time.time + LockDelay;
            maxLockTime = Time.time + MaxLockDelay;
        }
    }

    private void HandleLocking(MoveResults moveResults)
    {
        if (!locking || (Time.time <= lockTime && Time.time <= maxLockTime))
        {
            moveResults.locked = false;
            return;
        }

        var linesCleared = Lock();
        moveResults.linesCleared = linesCleared;
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return PieceCells.All(
            cell =>
            {
                var tilePosition = position + cell;
                return tilePosition.x >= 0 &&
                       tilePosition.x < Tiles.GetLength(0) &&
                       tilePosition.y >= 0 &&
                       tilePosition.y < Tiles.GetLength(1) &&
                       Tiles[tilePosition.x, tilePosition.y] == TileState.Empty;
            }
        );
    }

    public int HardDrop(bool skipSpawn = false)
    {
        while (MoveTranslation(Vector2Int.down))
        {
        }

        return Lock(skipSpawn);
    }

    public bool MoveTranslation(Vector2Int translation)
    {
        var newPosition = PiecePosition + translation;

        var valid = IsValidPosition(newPosition);
        if (!valid)
            return false;

        PiecePosition = newPosition;
        if (translation.x != 0)
            lockTime += LockDelay;

        return true;
    }

    public void Rotate(int rotationAmount)
    {
        var direction = (rotationAmount % 4 + 4) % 4;

        switch (direction)
        {
            case 0:
                return;
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
        for (var i = 0; i < PieceCells.Length; i++)
        {
            Vector2 cell = PieceCells[i];

            int x;
            int y;

            switch (PieceData.Tetromino)
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

            PieceCells[i] = new Vector2Int(x, y);
        }
    }

    private bool TestWallKicks(int newRotationIndex, int direction)
    {
        var wallKickIndex = GetWallKickIndex(newRotationIndex, direction);

        for (var i = 0; i < PieceData.WallKicks.GetLength(1); ++i)
        {
            if (MoveTranslation(PieceData.WallKicks[wallKickIndex, i]))
                return true;
        }

        return false;
    }

    private int GetWallKickIndex(int newRotationIndex, int direction)
    {
        var wallKickIndex = newRotationIndex * 2;

        if (direction < 0)
            wallKickIndex--;

        return (wallKickIndex + PieceData.WallKicks.GetLength(0)) % PieceData.WallKicks.GetLength(0);
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

    private int Lock(bool skipSpawn = false)
    {
        foreach (var cell in PieceCells)
        {
            var newPosition = PiecePosition + cell;
            Tiles[newPosition.x, newPosition.y] = PieceData.TileState;
        }

        var linesCleared = ClearLines();

        PushGarbage();

        if (!skipSpawn)
            SpawnNextPiece();

        return linesCleared;
    }

    private void SpawnNextPiece()
    {
        var nextTetromino = Queue[0];
        Queue.RemoveAt(0);
        SpawnPiece(nextTetromino);
    }

    private void SpawnPiece(TetrominoData data)
    {
        var spawnPosition = new Vector2Int(Columns / 2 - 1, Rows - 3);
        if (data.Tetromino == Tetromino.I)
            spawnPosition.y -= 1;

        PiecePosition = spawnPosition;
        PieceCells = (Vector2Int[])data.Cells.Clone();
        PieceData = data;
        PieceRotation = 0;
        stepTime = Time.time + StepDelay;
        locking = false;
        HoldingLocked = false;

        if (!IsValidPosition(spawnPosition))
            GameOver = true;
    }

    public void Hold()
    {
        if (HoldingLocked) return;

        var prevHeldPiece = HeldPiece;

        HeldPiece = PieceData;
        PieceData = HeldPiece;

        if (prevHeldPiece != null)
            SpawnPiece(prevHeldPiece);
        else
            SpawnNextPiece();

        HoldingLocked = true;
    }

    private int ClearLines()
    {
        var linesToClear = Enumerable
            .Range(0, Rows)
            .Where(
                y =>
                    Enumerable
                        .Range(0, Columns)
                        .All(x => Tiles[x, y] != TileState.Empty)
            )
            .ToList();

        if (!linesToClear.Any())
            return 0;

        var linesCleared = 0;
        for (var y = linesToClear[0]; y < Rows; ++y)
        {
            var clearing = linesToClear.Contains(y);
            for (var x = 0; x < Columns; ++x)
            {
                if (!clearing)
                    Tiles[x, y - linesCleared] = Tiles[x, y];

                Tiles[x, y] = TileState.Empty;
            }

            if (clearing)
                linesCleared += 1;
        }

        return linesCleared;
    }

    public void AddPendingGarbage(int linesToSend)
    {
        PendingGarbage += linesToSend;
    }

    private void PushGarbage()
    {
        var emptyColumn = GameManager.Instance.gen.Next(0, Columns);

        for (var y = Rows - 1 - PendingGarbage; y >= 0; --y)
        {
            for (var x = 0; x < Columns; ++x)
            {
                Tiles[x, y + PendingGarbage] = Tiles[x, y];
            }
        }

        for (var y = 0; y < PendingGarbage; ++y)
        {
            for (var x = 0; x < Columns; ++x)
            {
                Tiles[x, y] = x == emptyColumn ? TileState.Empty : TileState.Garbage;
            }
        }

        PendingGarbage = 0;
    }
}
