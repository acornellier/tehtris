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

[Serializable]
public class TetrominoData
{
    public Tetromino tetromino;
    public Tile tile;
    public Vector2Int[] Cells { get; private set; }
    public Vector2Int[,] WallKicks { get; private set; }
    public int MaxRotation { get; private set; }

    public void Initialize()
    {
        Cells = Data.Cells[tetromino];
        WallKicks = Data.WallKicks[tetromino];
        MaxRotation = Data.MaxRotationIndexes[tetromino];
    }
}
