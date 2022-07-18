using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Tetromino
{
    I,
    O,
    T,
    J,
    L,
    S,
    Z,
}

public enum TileState
{
    Empty,
    Garbage,
    I,
    O,
    T,
    J,
    L,
    S,
    Z,
}

[Serializable]
public class TetrominoData
{
    [field: SerializeField] public Tetromino Tetromino { get; private set; }
    [field: SerializeField] public Tile Tile { get; private set; }

    public TileState TileState { get; private set; }
    public Vector2Int[] Cells { get; private set; }
    public Vector2Int[,] WallKicks { get; private set; }
    public int MaxRotation { get; private set; }

    public void Initialize()
    {
        Cells = Data.Cells[Tetromino];
        WallKicks = Data.WallKicks[Tetromino];
        MaxRotation = Data.MaxRotationIndexes[Tetromino];
        TileState = Data.TileStates[Tetromino];
    }
}
