using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

public class TetrominoQueue : MonoBehaviour
{
    public List<TetrominoData> datas;
    public List<TetrominoData> NextTetrominos { get; } = new();
    private Tilemap tilemap;

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();

        foreach (var data in datas)
        {
            data.Initialize();
        }

        FillQueue();
    }

    public TetrominoData PopNextTetromino()
    {
        tilemap.ClearAllTiles();

        var nextTetromino = NextTetrominos[0];
        NextTetrominos.RemoveAt(0);

        if (NextTetrominos.Count < 5)
        {
            FillQueue();
        }

        Draw();

        return nextTetromino;
    }

    private void FillQueue()
    {
        var rnd = new Random();
        foreach (var _ in datas)
        {
            NextTetrominos.AddRange(datas.OrderBy(_ => rnd.Next()));
        }
    }

    private void Draw()
    {
        for (var i = 0; i < 5 && i < NextTetrominos.Count; ++i)
        {
            var tetromino = NextTetrominos[i];
            var position = new Vector2Int(0, i * -3);
            Utilities.SetCells(tilemap, tetromino.Cells, tetromino.tile, position);
        }
    }
}
