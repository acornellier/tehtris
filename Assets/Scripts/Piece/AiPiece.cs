using UnityEngine;

public class AiPiece : Piece
{
    public float timeBetweenMoves = 0.1f;
    private float lastMoveTime;

    public override void Initialize(Board board, Vector2Int position, TetrominoData data)
    {
        base.Initialize(board, position, data);
    }

    protected override void MakeMove()
    {
        if (Time.time < lastMoveTime + timeBetweenMoves)
            return;

        lastMoveTime = Time.time;

        var boardState = new BoardState(Board);

        var idealPosition = boardState.piecePosition;
        var idealPositionScore = float.MinValue;

        for (var x = 0; x < boardState.tiles.GetLength(1); ++x)
        {
            var newPosition = boardState.piecePosition;
            newPosition.x = x;

            if (!boardState.IsValidPosition(newPosition))
                continue;

            var boardStateClone = boardState.DeepClone();
            boardStateClone.HardDrop();

            var score = boardStateClone.Evaluate();
            if (score <= idealPositionScore)
                continue;

            idealPosition = newPosition;
            idealPositionScore = score;
        }

        MoveTo(idealPosition);
    }

    private void MoveTo(Vector2Int newPosition)
    {
        var convertedX = newPosition.x + Board.Bounds.xMin;
        if (convertedX < Position.x)
            Move(Vector2Int.left);
        else if (convertedX > Position.x)
            Move(Vector2Int.right);
        else
            HardDrop();
    }

    private void HandleRotation()
    {
        var val = Random.value;

        if (val < 0.1)
            Rotate(-1);
        else if (val < 0.2)
            Rotate(1);
    }
}
