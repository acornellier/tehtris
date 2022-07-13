using System;
using UnityEngine;

public class Move
{
    public enum Direction
    {
        None,
        Left,
        Right,
        Down,
    }

    public enum Rotation
    {
        None,
        Clockwise,
        Counterclockwise,
    }

    public bool hold;
    public bool hardDrop;
    public Direction direction = Direction.None;
    public Rotation rotation = Rotation.None;

    public static Vector2Int DirectionToTranslation(Direction direction)
    {
        return direction switch
        {
            Direction.None => Vector2Int.down,
            Direction.Left => Vector2Int.left,
            Direction.Right => Vector2Int.right,
            Direction.Down => Vector2Int.down,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
    }

    public static int RotationToAmount(Rotation rotation)
    {
        return rotation switch
        {
            Rotation.None => 0,
            Rotation.Counterclockwise => -1,
            Rotation.Clockwise => +1,
            _ => throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null),
        };
    }
}

public class MoveResults
{
    public bool held;
    public bool locked;
    public int linesCleared;
}
