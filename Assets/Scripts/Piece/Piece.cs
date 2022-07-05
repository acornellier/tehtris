using System;
using UnityEngine;

public abstract class Piece : MonoBehaviour
{
    private const float LockDelay = 0.5f;
    private const float MaxLockDelay = 5f;
    private const float StepDelay = 1f;

    private bool locking;
    private float lockTime; // delayed when moving
    private float maxLockTime; // never delayed

    private float stepTime;
    protected Board Board { get; private set; }
    public int RotationIndex { get; private set; }
    public Vector2Int Position { get; private set; }
    public Vector2Int[] Cells { get; private set; }
    public TetrominoData Data { get; private set; }

    private void Update()
    {
        Utilities.ClearPiece(Board.Tilemap, this);

        MakeMove();
        HandleStep();
        HandleLocking();

        Utilities.SetPiece(Board.Tilemap, this);
    }

    protected abstract void MakeMove();

    public virtual void Initialize(Board board, Vector2Int position, TetrominoData data)
    {
        Board = board;
        Position = position;
        Data = data;

        stepTime = Time.time + StepDelay;
        locking = false;
        RotationIndex = 0;

        Cells = new Vector2Int[data.Cells.Length];
        Array.Copy(data.Cells, Cells, data.Cells.Length);
    }

    private void HandleStep()
    {
        if (Time.time < stepTime)
            return;

        stepTime += StepDelay;
        var success = Move(Vector2Int.down);

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

    private void HandleLocking()
    {
        if (locking && (Time.time > lockTime || Time.time > maxLockTime))
            Lock();
    }

    protected void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
        }

        Lock();
    }

    protected bool Move(Vector2Int translation)
    {
        var newPosition = Position + translation;

        var valid = Board.IsValidPosition(this, newPosition);
        if (!valid)
            return false;

        Position = newPosition;
        lockTime += LockDelay;

        return true;
    }

    protected void Rotate(int direction)
    {
        var originalRotationIndex = RotationIndex;
        RotationIndex = (RotationIndex + direction) % 4;

        ApplyRotationMatrix(direction);

        if (TestWallKicks(RotationIndex, direction))
            return;

        RotationIndex = originalRotationIndex;
        ApplyRotationMatrix(-direction);
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (var i = 0; i < Cells.Length; i++)
        {
            Vector2 cell = Cells[i];

            int x;
            int y;

            switch (Data.tetromino)
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

            Cells[i] = new Vector2Int(x, y);
        }
    }

    private bool TestWallKicks(int newRotationIndex, int rotationDirection)
    {
        var wallKickIndex = GetWallKickIndex(newRotationIndex, rotationDirection);

        for (var i = 0; i < Data.WallKicks.GetLength(1); ++i)
        {
            if (Move(Data.WallKicks[wallKickIndex, i]))
                return true;
        }

        return false;
    }

    private int GetWallKickIndex(int newRotationIndex, int rotationDirection)
    {
        var wallKickIndex = newRotationIndex * 2;

        if (rotationDirection < 0)
            wallKickIndex--;

        return (wallKickIndex + Data.WallKicks.GetLength(0)) % Data.WallKicks.GetLength(0);
    }

    private static float RotateX(Vector3 cell, int direction)
    {
        return cell.x * global::Data.RotationMatrix[0] * direction
               + cell.y * global::Data.RotationMatrix[1] * direction;
    }

    private static float RotateY(Vector3 cell, int direction)
    {
        return cell.x * global::Data.RotationMatrix[2] * direction
               + cell.y * global::Data.RotationMatrix[3] * direction;
    }

    private void Lock()
    {
        Utilities.SetPiece(Board.Tilemap, this);
        Board.ClearLines();
        Board.SpawnNextPiece();
    }
}
