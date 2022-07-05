using UnityEngine;

public class PlayerPiece : Piece
{
    private const float FirstMoveDelay = 0.15f;
    private const float HoldMoveDelay = 0.01f;

    private int sameSequentialMoves;
    private float moveTime;
    private Vector2Int lastMove;

    public override void Initialize(Board board, Vector2Int position, TetrominoData data)
    {
        base.Initialize(board, position, data);
        sameSequentialMoves = 0;
        moveTime = 0f;
    }

    protected override void MakeMove()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Board.HoldPiece();
        }
        else
        {
            HandleMovement();
            HandleRotation();
        }
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

    private void MoveWithDelay(Vector2Int move)
    {
        if (Time.time < moveTime)
            return;

        if (!Move(move))
            return;

        sameSequentialMoves = lastMove == move ? sameSequentialMoves + 1 : 0;
        var moveDelay = sameSequentialMoves == 0 ? FirstMoveDelay : HoldMoveDelay;
        moveTime = Time.time + moveDelay;
        lastMove = move;
    }

    private void HandleRotation()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            Rotate(-1);
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            Rotate(1);
    }
}
