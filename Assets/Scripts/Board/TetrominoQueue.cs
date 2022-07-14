using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

public class TetrominoQueue : MonoBehaviour
{
    public List<TetrominoData> datas;
    public List<TetrominoData> NextTetrominos { get; private set; } = new();
    private Tilemap tilemap;

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();

        foreach (var data in datas)
        {
            data.Initialize();
        }

        FillQueueAndDraw();
        Draw();
    }

    public void UpdateQueue(IEnumerable<TetrominoData> newQueue)
    {
        NextTetrominos = new List<TetrominoData>(newQueue);
        FillQueueAndDraw();
        Draw();
    }

    private void FillQueueAndDraw()
    {
        if (NextTetrominos.Count >= 5)
            return;

        var rnd = new Random(0);
        NextTetrominos.AddRange(datas.OrderBy(_ => rnd.Next()));
    }

    private void Draw()
    {
        tilemap.ClearAllTiles();

        for (var i = 0; i < 5 && i < NextTetrominos.Count; ++i)
        {
            var tetromino = NextTetrominos[i];
            var position = new Vector2Int(0, i * -3);
            Utilities.SetCells(tilemap, tetromino.Cells, tetromino.tile, position);
        }
    }
}
