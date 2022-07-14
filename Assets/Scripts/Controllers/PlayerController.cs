using UnityEngine;

public class PlayerController : Controller
{
    private const float FirstMoveDelay = 0.15f;
    private const float HoldMoveDelay = 0.01f;

    private int sameSequentialMoves;
    private float moveTime;
    private Move.Direction lastMove;

    public override void NotifyNewPiece()
    {
        sameSequentialMoves = 0;
        moveTime = 0f;
    }

    public override Move GetMove(BoardState state)
    {
        var move = new Move();

        if (Input.GetKeyDown(KeyCode.C))
        {
            move.hold = true;
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            move.hardDrop = true;
        }
        else
        {
            HandleMovement(move);
            HandleRotation(move);
        }

        return move;
    }

    private void HandleMovement(Move move)
    {
        var direction = Move.Direction.None;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            lastMove = Move.Direction.None;
            direction = Move.Direction.Left;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            lastMove = Move.Direction.None;
            direction = Move.Direction.Right;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            lastMove = Move.Direction.None;
            direction = Move.Direction.Down;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            direction = Move.Direction.Left;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            direction = Move.Direction.Right;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            direction = Move.Direction.Down;
        }

        if (direction != Move.Direction.None)
            MoveWithDelay(move, direction);

        if (move.direction != Move.Direction.None)
            print($"{move.direction}");
    }

    private void MoveWithDelay(Move move, Move.Direction direction)
    {
        if (Time.time < moveTime)
            return;

        move.direction = direction;
        sameSequentialMoves = lastMove == move.direction ? sameSequentialMoves + 1 : 0;
        var moveDelay = sameSequentialMoves == 0 ? FirstMoveDelay : HoldMoveDelay;
        moveTime = Time.time + moveDelay;
        lastMove = move.direction;
    }

    private static void HandleRotation(Move move)
    {
        if (Input.GetKeyDown(KeyCode.Z))
            move.rotation = Move.Rotation.Counterclockwise;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            move.rotation = Move.Rotation.Clockwise;
    }
}
