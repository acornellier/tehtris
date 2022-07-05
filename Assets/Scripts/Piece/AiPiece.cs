using UnityEngine;

public class AiPiece : Piece
{
    public override void Initialize(Board board, Vector2Int position, TetrominoData data)
    {
        base.Initialize(board, position, data);
    }

    protected override void MakeMove()
    {
        var boardState = new BoardState(Board);

        if (Random.value < 0.01)
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
        var val = Random.value;

        if (val < 0.01)
        {
            HardDrop();
        }
        else if (val < 0.33)
        {
            Move(Vector2Int.left);
        }
        else if (val < 0.66)
        {
            Move(Vector2Int.right);
        }
        else if (val < 1)
        {
            Move(Vector2Int.down);
        }
    }

    private void HandleRotation()
    {
        var val = Random.value;

        if (val < 0.1)
        {
            Rotate(-1);
        }
        else if (val < 0.2)
        {
            Rotate(1);
        }
    }
}
