﻿using System;
using System.Linq;
using UnityEngine;

internal struct Goal
{
    public Vector2Int Position;
    public int Rotation;
}

public class AiPiece : Piece
{
    public float timeBetweenMoves = 0.1f;
    public float maxHeightMultiplier = -1;
    public float bumpinessMultiplier = -1;
    public float holesMultiplier = -100;
    public float tilesInLastColumnMultiplier = -5;
    public float clearLessThanFourMultiplier = -10;
    public float clearFourScore = 100;

    private float nextMoveTime;
    private Goal? currentGoal;
    private bool pendingHold;

    private int turn;
    public int slowDownTurn = -1;
    public int debugTurn = -1;
    public bool fastMode = true;
    public int slowDownHeight = 20;

    protected override void MakeMove()
    {
        if (Time.time < nextMoveTime)
            return;

        nextMoveTime = Time.time + timeBetweenMoves;

        // TODO
        if (fastMode) nextMoveTime = Time.time;

        if (!currentGoal.HasValue) FindNewGoal();

        GetToGoal();
    }

    private void FindNewGoal()
    {
        ++turn;
        print($"turn {turn}");
        if (turn == slowDownTurn) fastMode = false;

        var boardState = new BoardState(Board);

        var bestScore = FindBestGoalsForState(boardState, float.MinValue);

        boardState.HoldPiece();
        var bestHoldScore = FindBestGoalsForState(boardState, bestScore);

        if (bestHoldScore > bestScore)
            pendingHold = true;

        if (!currentGoal.HasValue)
            print("No valid moves! kinda sus tbh");
    }

    private float FindBestGoalsForState(BoardState boardState, float initialBestScore)
    {
        var bestGoalScore = initialBestScore;
        var bestState = new BoardState(Board);
        var curHoleScore = EvaluateHoleScore(boardState);

        for (var rotationIndex = 0; rotationIndex <= 3; ++rotationIndex)
        {
            for (var x = 0; x < boardState.Columns; ++x)
            {
                var boardStateClone = boardState.DeepClone();
                boardStateClone.Rotate(rotationIndex - boardStateClone.PieceRotation);

                if (!boardStateClone.Move(new Vector2Int(x - boardStateClone.PiecePosition.x, 0)))
                    continue;

                var newPosition = boardStateClone.PiecePosition;
                var foo = boardStateClone.DeepClone();

                var score = EvaluateBoard(boardStateClone, curHoleScore);
                if (score <= bestGoalScore)
                    continue;

                bestGoalScore = score;
                bestState = foo;

                currentGoal = new Goal()
                {
                    Position = newPosition,
                    Rotation = rotationIndex,
                };
            }
        }

        // TODO
        if (bestGoalScore > initialBestScore)
            EvaluateBoard(bestState, curHoleScore, true);

        return bestGoalScore;
    }

    private void GetToGoal()
    {
        if (!currentGoal.HasValue)
            return;

        if (pendingHold)
        {
            Board.HoldPiece();
            pendingHold = false;
            return;
        }

        var goal = currentGoal.Value;
        var convertedX = goal.Position.x + Board.Bounds.xMin;

        if (convertedX == Position.x && goal.Rotation == RotationIndex)
        {
            HardDrop();
            currentGoal = null;
            return;
        }

        var moveSuccess = false;
        if (convertedX < Position.x)
            moveSuccess |= Move(Vector2Int.left);
        else if (convertedX > Position.x)
            moveSuccess |= Move(Vector2Int.right);

        var rotateSuccess = false;
        if (goal.Rotation - RotationIndex >= 3)
            rotateSuccess |= Rotate(-1);
        else if (goal.Rotation != RotationIndex)
            rotateSuccess |= Rotate(1);

        // TODO
        if (fastMode && (moveSuccess || rotateSuccess))
            GetToGoal();
    }

    private float EvaluateBoard(BoardState boardState, int curHoldScore, bool yes = false)
    {
        boardState.HardDrop();

        var clearedRows = CountRowsToBeCleared(boardState);
        boardState.ClearLines();

        var holeScore = EvaluateHoleScore(boardState);
        var maxHeight = EvaluateMaxHeight(boardState);
        var bumpiness = EvaluateBumpiness(boardState);
        var tilesInLastColumn = CountTilesInLastColumn(boardState);

        float score = 0;
        score += maxHeight * maxHeightMultiplier;
        score += bumpiness * bumpinessMultiplier;
        score += holeScore * holesMultiplier;
        score += tilesInLastColumn * tilesInLastColumnMultiplier;

        // reducing number of holes is ALWAYS top priority
        if (holeScore < curHoldScore)
            score += float.MaxValue;

        if (clearedRows == 4)
            score += clearFourScore;
        else
            score += clearedRows * clearLessThanFourMultiplier;

        // TODO
        if (yes && holeScore > curHoldScore)
            score += 0;

        if (maxHeight > slowDownHeight)
            fastMode = false;
        if (turn == debugTurn)
            print($"debug turn {turn}");

        return score;
    }

    private static int CountRowsToBeCleared(BoardState boardState)
    {
        return Enumerable
            .Range(0, boardState.Rows)
            .Count(
                y =>
                    Enumerable
                        .Range(0, boardState.Columns)
                        .All(x => boardState.Tiles[x, y])
            );
    }

    private static int EvaluateMaxHeight(BoardState boardState)
    {
        var maxHeight = 0;
        for (var x = 0; x < boardState.Columns - 1; ++x)
        {
            var height = GetColumnHeight(boardState, x);

            if (height > maxHeight)
                maxHeight = height;
        }

        return maxHeight;
    }

    private static int EvaluateBumpiness(BoardState boardState)
    {
        var bumpiness = 0;

        var prevHeight = GetColumnHeight(boardState, 0);
        var prevHeightDifference = 0;
        for (var x = 1; x < boardState.Columns - 1; ++x)
        {
            var height = GetColumnHeight(boardState, x);
            var heightDifference = Math.Abs(height - prevHeight);

            if (x == 1 || x == boardState.Columns - 2)
                heightDifference += 1;

            if (prevHeightDifference > 2 && heightDifference > 2)
                heightDifference += prevHeightDifference;

            bumpiness += heightDifference * (heightDifference + 1) / 2;

            prevHeight = height;
            prevHeightDifference = heightDifference;
        }

        return bumpiness;
    }

    private static int GetColumnHeight(BoardState boardState, int column)
    {
        for (var y = boardState.Rows - 1; y >= 0; --y)
        {
            if (boardState.Tiles[column, y])
                return y;
        }

        return 0;
    }

    private static int EvaluateHoleScore(BoardState boardState)
    {
        var holeScore = 0;

        for (var x = 0; x < boardState.Columns; ++x)
        {
            var columnHeight = GetColumnHeight(boardState, x);
            for (var y = 0; y < columnHeight; ++y)
            {
                if (!boardState.Tiles[x, y])
                    holeScore += columnHeight;
            }
        }

        return holeScore;
    }

    private static int CountTilesInLastColumn(BoardState boardState)
    {
        var tilesInLastColumn = 0;
        for (var y = 0; y < boardState.Rows; ++y)
        {
            if (boardState.Tiles[boardState.Columns - 1, y])
                tilesInLastColumn += 1;
        }

        return tilesInLastColumn;
    }
}
