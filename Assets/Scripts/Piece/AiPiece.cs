using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

internal struct Goal
{
    public Vector2Int Position;
    public int Rotation;
}

public class AiPiece : Piece
{
    public float timeBetweenMoves = 0.1f;
    private float nextMoveTime;
    private Goal? currentGoal;

    public override void Initialize(Board board, Vector2Int position, TetrominoData data)
    {
        base.Initialize(board, position, data);
    }

    protected override void MakeMove()
    {
        if (Time.time < nextMoveTime)
            return;

        nextMoveTime = Time.time + timeBetweenMoves;

        if (!currentGoal.HasValue) FindNewGoal();

        if (currentGoal.HasValue)
            GetTo(currentGoal.Value);
    }

    private void FindNewGoal()
    {
        var boardState = new BoardState(Board);

        var bestGoals = new List<Goal>();
        var bestGoalScore = float.MinValue;

        for (var rotationIndex = 0; rotationIndex <= 3; ++rotationIndex)
        {
            for (var x = 0; x < boardState.tiles.GetLength(1); ++x)
            {
                var boardStateClone = boardState.DeepClone();
                boardStateClone.Rotate(rotationIndex - boardStateClone.PieceRotation);

                if (!boardStateClone.Move(new Vector2Int(x - boardStateClone.piecePosition.x, 0)))
                    continue;

                var newPosition = boardStateClone.piecePosition;
                boardStateClone.HardDrop();

                print($"EVALUATING {Data.tetromino} r{rotationIndex} {boardStateClone.piecePosition}");
                var score = EvaluateBoard(boardStateClone);
                print($"FINAL SCORE {score}");
                if (score <= bestGoalScore)
                    continue;

                if (score > bestGoalScore)
                {
                    bestGoals.Clear();
                    bestGoalScore = score;
                }

                bestGoals.Add(
                    new Goal()
                    {
                        Position = newPosition,
                        Rotation = rotationIndex,
                    }
                );
            }
        }

        if (bestGoals.Count == 0)
        {
            print("No valid moves! kinda sus tbh");
            return;
        }

        currentGoal = bestGoals[Random.Range(0, bestGoals.Count)];
    }

    private static float EvaluateBoard(BoardState boardState)
    {
        float score = 0;

        for (var y = 0; y < boardState.tiles.GetLength(1) - 1; ++y)
        {
            var tilesInRow = 0;

            for (var x = 0; x < boardState.tiles.GetLength(0); ++x)
            {
                if (boardState.tiles[x, y])
                    tilesInRow += 1;

                // check if there's a filled tile on top of an empty tile
                if (!boardState.tiles[x, y] && boardState.tiles[x, y + 1])
                    score -= 100;
            }

            var rowMultiplier = 1 + (float)(boardState.Rows - y) / boardState.Rows;
            score += tilesInRow * rowMultiplier;
            // print($"ROW {y} {tilesInRow} {rowMultiplier} {score}");
        }

        return score;
    }

    private void GetTo(Goal goal)
    {
        print($"GetTo {goal.Position} {goal.Rotation}");
        var convertedX = goal.Position.x + Board.Bounds.xMin;

        if (convertedX == Position.x && goal.Rotation == RotationIndex)
        {
            HardDrop();
            currentGoal = null;
            return;
        }

        if (convertedX < Position.x)
            Move(Vector2Int.left);
        else if (convertedX > Position.x)
            Move(Vector2Int.right);

        if (goal.Rotation - RotationIndex >= 3)
            Rotate(-1);
        else if (goal.Rotation != RotationIndex)
            Rotate(1);
    }
}
