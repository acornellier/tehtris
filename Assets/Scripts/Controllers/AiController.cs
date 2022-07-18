using System;
using UnityEngine;

internal class Goal
{
    public bool hold;
    public int xPosition;
    public int rotation;
}

public class AiController : Controller
{
    public int maxDepth = 1;
    public float timeBetweenMoves = 0.2f;
    public float maxHeightMultiplier = -1;
    public float bumpinessMultiplier = -2;
    public float holesMultiplier = -20;
    public float lastColumnHeightMultiplier = -10;

    public float clearLessThanFourMultiplier = -5;
    public float clearFourScore = 100;

    private float nextMoveTime;
    private Goal currentGoal;

    // debug stuff
    private int turn;
    public bool fastMode;
    public int slowDownTurn = -1;
    public int slowDownHeight = 20;
    public int debugTurn = -1;

    public override bool Muted => true;

    private void Start()
    {
        timeBetweenMoves = GameManager.Instance.AiTimeBetweenMoves;
        GameManager.OnAiTimeBetweenMovesChange += OnAiTimeBetweenMovesChange;
    }

    private void OnDestroy()
    {
        GameManager.OnAiTimeBetweenMovesChange -= OnAiTimeBetweenMovesChange;
    }

    private void OnAiTimeBetweenMovesChange(float value)
    {
        timeBetweenMoves = value;
    }

    public override Move GetMove(BoardState state)
    {
        var move = new Move();

        if (Time.time < nextMoveTime)
            return move;

        nextMoveTime = Time.time + timeBetweenMoves;

        if (fastMode) nextMoveTime = Time.time;

        if (currentGoal == null)
            FindNewGoal(state.DeepClone());

        ConvertCurrentGoalToMove(state, move);

        return move;
    }

    private void FindNewGoal(BoardState boardState)
    {
#if UNITY_EDITOR
        ++turn;
        if (turn == slowDownTurn) fastMode = false;
        print($"turn {turn}");
#endif

        var startingHoleScore = EvaluateHoleScore(boardState);
        currentGoal = FindBestGoalForState(boardState.DeepClone(), startingHoleScore, 0).Item1;

#if UNITY_EDITOR
        if (currentGoal.hold)
            boardState.Hold();

        boardState.Rotate(currentGoal.rotation);
        var translation = new Vector2Int(currentGoal.xPosition - boardState.PiecePosition.x, 0);
        if (!boardState.MoveTranslation(translation))
            return;

        boardState.HardDrop(true);
        EvaluateBoard(boardState, startingHoleScore, true);
#endif
    }

    private Tuple<Goal, float> FindBestGoalForState(BoardState boardState, float startingHoleScore, int depth)
    {
        var isMaxDepth = depth >= maxDepth;
        var best = new Tuple<Goal, float>(new Goal(), float.MinValue);
        var bestHasWorseHoles = false;

        Goal goal;
        for (var rotation = 0; rotation <= boardState.PieceData.MaxRotation; ++rotation)
        {
            for (var x = -1; x < boardState.Columns; ++x)
            {
                goal = new Goal { xPosition = x, rotation = rotation, };

                var boardStateClone = boardState.DeepClone();
                boardStateClone.Rotate(goal.rotation - boardStateClone.PieceRotation);
                var translation = new Vector2Int(goal.xPosition - boardStateClone.PiecePosition.x, 0);
                if (!boardStateClone.MoveTranslation(translation))
                    continue;

                var clearedRows = boardStateClone.HardDrop(isMaxDepth);
                var holeScore = EvaluateHoleScore(boardStateClone);
                var score = EvaluateClearedRows(clearedRows);

                if (isMaxDepth)
                {
                    score += EvaluateBoard(boardStateClone, startingHoleScore);
                }
                else
                {
                    var tempScore = EvaluateBoard(boardStateClone, startingHoleScore);
                    if (tempScore < best.Item2 - 50 && !bestHasWorseHoles)
                        score = float.MinValue;
                    else
                        score += FindBestGoalForState(boardStateClone, startingHoleScore, depth + 1).Item2;
                }

                if (turn == debugTurn && depth == 0)
                    print($"x {x} r {rotation} s {score}");

                if (score <= best.Item2)
                    continue;

                best = new Tuple<Goal, float>(goal, score);
                bestHasWorseHoles = holeScore > startingHoleScore;
            }
        }

        if (boardState.HoldingLocked)
            return best;

        var clone = boardState.DeepClone();
        clone.Hold();
        var holdBest = FindBestGoalForState(clone, startingHoleScore, depth);

        if (holdBest.Item2 <= best.Item2)
            return best;

        goal = holdBest.Item1;
        goal.hold = true;
        best = new Tuple<Goal, float>(goal, holdBest.Item2);
        return best;
    }

    private void ConvertCurrentGoalToMove(BoardState state, Move move)
    {
        if (currentGoal == null)
            return;

        if (currentGoal.hold)
        {
            move.hold = true;
            currentGoal.hold = false;
            return;
        }

        if (currentGoal.xPosition == state.PiecePosition.x && currentGoal.rotation == state.PieceRotation)
        {
            move.hardDrop = true;
            currentGoal = null;
            return;
        }

        if (currentGoal.xPosition < state.PiecePosition.x)
            move.direction = Move.Direction.Left;
        else if (currentGoal.xPosition > state.PiecePosition.x)
            move.direction = Move.Direction.Right;

        if (currentGoal.rotation - state.PieceRotation >= 3)
            move.rotation = Move.Rotation.Counterclockwise;
        else if (currentGoal.rotation != state.PieceRotation)
            move.rotation = Move.Rotation.Clockwise;
    }

    private float EvaluateBoard(BoardState boardState, float startingHoleScore, bool debugBest = false)
    {
        if (boardState.GameOver) return float.MinValue;

        var holeScore = EvaluateHoleScore(boardState);
        var maxHeight = EvaluateMaxHeight(boardState);
        var bumpiness = EvaluateBumpiness(boardState);
        var lastColumnHeight = GetColumnHeight(boardState, boardState.Columns - 1);

        float score = 0;
        score += holeScore * holesMultiplier;
        score += maxHeight * maxHeightMultiplier;
        score += bumpiness * bumpinessMultiplier;
        score += lastColumnHeight * lastColumnHeightMultiplier;

#if UNITY_EDITOR
        if (fastMode && maxHeight > slowDownHeight)
            fastMode = false;

        if (debugBest && holeScore > startingHoleScore)
            print($"debugging turn {turn}");

        if (debugBest)
            fastMode = fastMode;
#endif

        return score;
    }

    private float EvaluateClearedRows(int clearedRows)
    {
        if (clearedRows == 4)
            return clearFourScore;

        if (clearedRows != 0)
            return (4 - clearedRows) * clearLessThanFourMultiplier;

        return 0;
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
            if (boardState.Tiles[column, y] != TileState.Empty)
                return y + 1;
        }

        return 0;
    }

    private static float EvaluateHoleScore(BoardState boardState)
    {
        var holeScore = 0f;

        for (var x = 0; x < boardState.Columns; ++x)
        {
            var columnHeight = GetColumnHeight(boardState, x);
            for (var y = 0; y < columnHeight; ++y)
            {
                if (boardState.Tiles[x, y] == TileState.Empty)
                    holeScore += 1 + (columnHeight - y) * 0.1f;
            }
        }

        return holeScore;
    }
}
