using System;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private const float FirstMoveDelay = 0.15f;
    private const float HoldMoveDelay = 0.01f;
    private const float LockDelay = 0.5f;
    private const float MaxLockDelay = 5f;

    public float stepDelay = 1f;
    private Vector2Int lastMove;

    private bool locking;
    private float lockTime; // delayed when moving
    private float maxLockTime; // never delayed
    private float moveTime;

    private int rotationIndex;
    private int sameSequentialMoves;
    private float stepTime;
    private Board Board { get; set; }
    public Vector2Int Position { get; private set; }
    public Vector2Int[] Cells { get; private set; }
    public TetrominoData Data { get; private set; }

    private void Update()
    {
        Utilities.ClearPiece(Board.Tilemap, this);

        HandleMovement();
        HandleRotation();

        HandleStep();
        HandleLocking();

        Utilities.SetPiece(Board.Tilemap, this);
    }

    public void Initialize(Board board, Vector2Int position, TetrominoData data)
    {
        Board = board;
        Position = position;
        Data = data;

        stepTime = Time.time + stepDelay;
        locking = false;
        moveTime = 0f;
        sameSequentialMoves = 0;
        rotationIndex = 0;

        Cells = new Vector2Int[data.Cells.Length];
        Array.Copy(data.Cells, Cells, data.Cells.Length);
    }

    private void HandleMovement()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            lastMove = Vector2Int.zero;
            MoveWithDelay(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            lastMove = Vector2Int.zero;
            MoveWithDelay(Vector2Int.right);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveWithDelay(Vector2Int.left);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveWithDelay(Vector2Int.right);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            Move(Vector2Int.down);
        }
    }

    private void HandleRotation()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            Rotate(-1);
        else if (Input.GetKeyDown(KeyCode.UpArrow)) Rotate(1);
    }

    private void HandleStep()
    {
        if (Time.time < stepTime)
            return;

        stepTime += stepDelay;
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

    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
        }

        Lock();
    }

    private void MoveWithDelay(Vector2Int move)
    {
        if (Time.time < moveTime)
            return;

        if (!Move(move)) return;

        sameSequentialMoves = lastMove == move ? sameSequentialMoves + 1 : 0;
        var moveDelay = sameSequentialMoves == 0 ? FirstMoveDelay : HoldMoveDelay;
        moveTime = Time.time + moveDelay;
        lastMove = move;
    }

    private bool Move(Vector2Int translation)
    {
        var newPosition = Position + translation;

        var valid = Board.IsValidPosition(this, newPosition);
        if (!valid) return false;

        Position = newPosition;
        lockTime += LockDelay;

        return true;
    }

    private void Rotate(int direction)
    {
        var originalRotationIndex = rotationIndex;
        rotationIndex = (rotationIndex + direction) % 4;

        ApplyRotationMatrix(direction);

        if (TestWallKicks(rotationIndex, direction)) return;

        rotationIndex = originalRotationIndex;
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
            print(Data.WallKicks[wallKickIndex, i]);
            if (Move(Data.WallKicks[wallKickIndex, i])) return true;
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
        Board.SpawnRandomPiece();
    }
}
