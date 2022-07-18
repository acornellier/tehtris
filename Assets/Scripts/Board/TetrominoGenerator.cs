using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

public class TetrominoGenerator : MonoBehaviour
{
    [SerializeField] private Tile garbageTile;
    [SerializeField] private List<TetrominoData> datas;

    public Dictionary<TileState, Tile> TileStateToTile { get; } = new();

    private Random gen;

    private void Awake()
    {
        foreach (var data in datas)
        {
            data.Initialize();
            TileStateToTile[data.TileState] = data.Tile;
        }

        TileStateToTile[TileState.Empty] = null;
        TileStateToTile[TileState.Garbage] = garbageTile;

        gen = new Random();
    }

    public IEnumerable<TetrominoData> Generate()
    {
        return datas.OrderBy(_ => gen.Next());
    }
}
