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
#endif

        var startingHoleScore = EvaluateHoleScore(boardState);
        currentGoal = FindBestGoalForState(boardState, startingHoleScore, 0).Item1;
    }

    private Tuple<Goal, float> FindBestGoalForState(
        BoardState boardState,
        int startingHoleScore,
        int depth,
        bool prevHeld = false)
    {
        var isMaxDepth = depth >= maxDepth;
        var best = new Tuple<Goal, float>(new Goal(), float.MinValue);

        Goal goal;
        for (var rotation = 0; rotation <= boardState.PieceData.MaxRotation; ++rotation)
        {
            boardState.Rotate(rotation);

            for (var x = -1; x < boardState.Columns; ++x)
            {
                var boardStateClone = boardState.DeepClone();
                goal = new Goal { xPosition = x, rotation = rotation, };
                var translation = new Vector2Int(goal.xPosition - boardStateClone.PiecePosition.x, 0);
                if (!boardStateClone.MoveTranslation(translation))
                    continue;

                var clearedRows = boardStateClone.HardDrop(isMaxDepth);
                var score = EvaluateClearedRows(clearedRows);

                if (isMaxDepth)
                {
                    score += EvaluateBoard(boardStateClone, startingHoleScore);
                }
                else
                {
                    var tempScore = EvaluateBoard(boardStateClone, startingHoleScore);
                    if (tempScore < best.Item2 - 50)
                        score = float.MinValue;
                    else
                        score += FindBestGoalForState(boardStateClone, startingHoleScore, depth + 1).Item2;
                }

                if (score > best.Item2)
                    best = new Tuple<Goal, float>(goal, score);
            }

            boardState.Rotate(-rotation);
        }

        if (prevHeld)
            return best;

        boardState.HoldPiece();
        var holdBest = FindBestGoalForState(boardState, startingHoleScore, depth, true);

        if (!(holdBest.Item2 > best.Item2))
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

    private float EvaluateBoard(BoardState boardState, int startingHoleScore, bool debugBest = false)
    {
        var holeScore = EvaluateHoleScore(boardState);
        var maxHeight = EvaluateMaxHeight(boardState);
        var bumpiness = EvaluateBumpiness(boardState);
        var lastColumnHeight = GetColumnHeight(boardState, boardState.Columns - 1);

        float score = 0;
        score += maxHeight * maxHeightMultiplier;
        score += bumpiness * bumpinessMultiplier;
        score += holeScore * holesMultiplier;
        score += lastColumnHeight * lastColumnHeightMultiplier;

        // reducing number of holes is ALWAYS top priority
        if (holeScore < startingHoleScore)
            score += float.MaxValue;

#if UNITY_EDITOR
        if (fastMode && maxHeight > slowDownHeight)
            fastMode = false;

        if ((debugBest && holeScore > startingHoleScore) || turn == debugTurn)
            print("debugging11");
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
}
