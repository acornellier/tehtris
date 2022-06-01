using System;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public Vector2Int position { get; private set; }
    public Vector2Int[] cells { get; private set; }
    public TetrominoData data { get; private set; }

    private readonly float firstMoveDelay = 0.15f;
    private readonly float holdMoveDelay = 0.01f;
    private float moveTime;
    private Vector2Int lastMove;
    private int sameSequentialMoves;

    private int rotationIndex;

    public float stepDelay = 1f;
    private float stepTime;

    private bool locking;
    private readonly float lockDelay = 0.5f;
    private readonly float maxLockDelay = 5f;
    private float lockTime; // delayed when moving
    private float maxLockTime; // never delayed

    public void Initialize(Board board, Vector2Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;

        stepTime = Time.time + stepDelay;
        locking = false;
        moveTime = 0f;
        sameSequentialMoves = 0;
        rotationIndex = 0;

        cells = new Vector2Int[data.cells.Length];
        Array.Copy(data.cells, cells, data.cells.Length);
    }

    private void Update()
    {
        Utilities.ClearPiece(board.tilemap, this);

        HandleMovement();
        HandleRotation();
        HandleStep();
        HandleLocking();

        Utilities.SetPiece(board.tilemap, this);
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
        {
            Rotate(-1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Rotate(1);
        }
    }

    private void HandleStep()
    {
        if (Time.time < stepTime)
            return;

        stepTime += stepDelay;
        bool success = Move(Vector2Int.down);

        if (success)
        {
            locking = false;
        }
        else if (!success && !locking)
        {
            locking = true;
            lockTime = Time.time + lockDelay;
            maxLockTime = Time.time + maxLockDelay;
        }
    }

    private void HandleLocking()
    {
        if (locking && (Time.time > lockTime || Time.time > maxLockTime))
            Lock();
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down)) { }

        Lock();
    }

    private void MoveWithDelay(Vector2Int move)
    {
        if (Time.time < moveTime)
            return;

        if (Move(move))
        {
            sameSequentialMoves = lastMove == move ? sameSequentialMoves + 1 : 0;
            var moveDelay = sameSequentialMoves == 0 ? firstMoveDelay : holdMoveDelay;
            moveTime = Time.time + moveDelay;
            lastMove = move;
        }
    }

    private bool Move(Vector2Int translation)
    {
        var newPosition = position + translation;

        bool valid = board.IsValidPosition(this, newPosition);
        if (valid)
        {
            position = newPosition;
            lockTime += lockDelay;
        }

        return valid;
    }

    private void Rotate(int direction)
    {
        int originalRotationIndex = rotationIndex;
        rotationIndex = (rotationIndex + direction) % 4;

        ApplyRotationMatrix(direction);

        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotationIndex;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2 cell = cells[i];

            int x;
            int y;

            switch (data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt(RotateX(cell, direction));
                    y = Mathf.CeilToInt(RotateY(cell, direction));
                    break;

                default:
                    x = Mathf.RoundToInt(RotateX(cell, direction));
                    y = Mathf.RoundToInt(RotateY(cell, direction));
                    break;
            }

            cells[i] = new Vector2Int(x, y);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); ++i)
        {
            print(data.wallKicks[wallKickIndex, i]);
            if (Move(data.wallKicks[wallKickIndex, i]))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
            wallKickIndex--;

        return (wallKickIndex + data.wallKicks.GetLength(0)) % data.wallKicks.GetLength(0);
    }

    private float RotateX(Vector3 cell, int direction)
    {
        return (cell.x * Data.RotationMatrix[0] * direction)
            + (cell.y * Data.RotationMatrix[1] * direction);
    }

    private float RotateY(Vector3 cell, int direction)
    {
        return (cell.x * Data.RotationMatrix[2] * direction)
            + (cell.y * Data.RotationMatrix[3] * direction);
    }

    private void Lock()
    {
        Utilities.SetPiece(board.tilemap, this);
        board.ClearLines();
        board.SpawnRandomPiece();
    }
}
