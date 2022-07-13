using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

internal struct Goal
{
    public int xPosition;
    public int rotation;
}

public class AiController : Controller
{
    public float timeBetweenMoves = 0.2f;
    public float maxHeightMultiplier = -1;
    public float bumpinessMultiplier = -2;
    public float holesMultiplier = -20;
    public float tilesInLastColumnMultiplier = -10;
    public float clearLessThanFourMultiplier = -5;
    public float clearFourScore = 100;

    private float nextMoveTime;
    private Goal? currentGoal;
    private bool pendingHold;

    // debug stuff
    private int turn;
    public bool fastMode;
    public int slowDownTurn = -1;
    public int slowDownHeight = 20;
    public int debugTurn = -1;

    public override bool Muted => true;

    public override Move GetMove(BoardState state)
    {
        var move = new Move();

        if (Time.time < nextMoveTime)
            return move;

        nextMoveTime = Time.time + timeBetweenMoves;

        if (fastMode) nextMoveTime = Time.time;

        if (!currentGoal.HasValue)
            FindNewGoal(state.DeepClone());

        ConvertGoalToMove(state, move);

        return move;
    }

    private void FindNewGoal(BoardState boardState)
    {
#if UNITY_EDITOR
        ++turn;
        if (turn == slowDownTurn) fastMode = false;
#endif

        var bestScore = FindBestGoalsForState(boardState.DeepClone(), float.MinValue);

        var stateWithHeldPiece = boardState.DeepClone();
        stateWithHeldPiece.HoldPiece();
        var bestHoldScore = FindBestGoalsForState(stateWithHeldPiece, bestScore);

        if (bestHoldScore > bestScore)
            pendingHold = true;

#if UNITY_EDITOR
        if (!currentGoal.HasValue)
            return;

        var testState = boardState.DeepClone();
        if (pendingHold) testState.HoldPiece();

        var curHoleScore = EvaluateHoleScore(testState);
        GetStateToGoal(testState, currentGoal.Value);
        EvaluateBoard(testState, curHoleScore, true);
#endif
    }

    private float FindBestGoalsForState(BoardState boardState, float initialBestScore)
    {
        var bestGoalScore = initialBestScore;
        var curHoleScore = EvaluateHoleScore(boardState);

        for (var rotationIndex = 0; rotationIndex <= 3; ++rotationIndex)
        {
            for (var x = -1; x < boardState.Columns; ++x)
            {
                var boardStateClone = boardState.DeepClone();
                var goal = new Goal { xPosition = x, rotation = rotationIndex, };
                if (!GetStateToGoal(boardStateClone, goal))
                    continue;

                var score = EvaluateBoard(boardStateClone, curHoleScore);
                if (score <= bestGoalScore)
                    continue;

                bestGoalScore = score;
                currentGoal = goal;
            }
        }

        return bestGoalScore;
    }

    private static bool GetStateToGoal(BoardState state, Goal goal)
    {
        state.Rotate(goal.rotation - state.PieceRotation);

        return state.MoveTranslation(new Vector2Int(goal.xPosition - state.PiecePosition.x, 0));
    }

    private void ConvertGoalToMove(BoardState state, Move move)
    {
        if (!currentGoal.HasValue)
            return;

        if (pendingHold)
        {
            move.hold = true;
            pendingHold = false;
            return;
        }

        var goal = currentGoal.Value;

        if (goal.xPosition == state.PiecePosition.x && goal.rotation == state.PieceRotation)
        {
            move.hardDrop = true;
            currentGoal = null;
            return;
        }

        if (goal.xPosition < state.PiecePosition.x)
            move.direction = Move.Direction.Left;
        else if (goal.xPosition > state.PiecePosition.x)
            move.direction = Move.Direction.Right;

        if (goal.rotation - state.PieceRotation >= 3)
            move.rotation = Move.Rotation.Counterclockwise;
        else if (goal.rotation != state.PieceRotation)
            move.rotation = Move.Rotation.Clockwise;
    }

    private float EvaluateBoard(BoardState boardState, int curHoleScore, bool debugBest = false)
    {
        var clearedRows = boardState.HardDrop(true);

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
        if (holeScore < curHoleScore)
            score += float.MaxValue;

        if (clearedRows == 4)
            score += clearFourScore;
        else if (clearedRows != 0)
            score += (4 - clearedRows) * clearLessThanFourMultiplier;

        if (fastMode && maxHeight > slowDownHeight)
            fastMode = false;

#if UNITY_EDITOR
        if ((debugBest && clearedRows is > 0 and < 4) || (debugBest && holeScore > curHoleScore) ||
            turn == debugTurn)
            print("debugging11");
#endif

        return score;
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

            if ((x == 1 && height > prevHeight) || (x == boardState.Columns - 2 && height < prevHeight))
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
                return y + 1;
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
