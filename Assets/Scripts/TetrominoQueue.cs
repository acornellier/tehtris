using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TetrominoQueue : MonoBehaviour
{
    public List<TetrominoData> datas;
    private Tilemap tilemap;
    private readonly List<TetrominoData> nextTetrominos = new();

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();

        foreach (var data in datas)
            data.Initialize();

        FillQueue();
    }

    public TetrominoData PopNextTetromino()
    {
        tilemap.ClearAllTiles();

        var nextTetromino = nextTetrominos[0];
        nextTetrominos.RemoveAt(0);
        FillQueue();

        Draw();

        return nextTetromino;
    }

    private void FillQueue()
    {
        while (nextTetrominos.Count < 5)
        {
            nextTetrominos.Add(datas[Random.Range(0, datas.Count)]);
        }
    }

    private void Draw()
    {
        for (int i = 0; i < nextTetrominos.Count; ++i)
        {
            var tetromino = nextTetrominos[i];
            var position = new Vector2Int(0, i * -3);
            Utilities.SetCells(tilemap, tetromino.cells, tetromino.tile, position);
        }
    }
}
