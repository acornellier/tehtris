using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class Board : MonoBehaviour
{
    public TetrominoData[] tetrominoDatas;
    public Vector2Int boardSize = new(10, 20);
    public Holder holder;

    public Piece activePiece { get; private set; }
    public Tilemap tilemap { get; private set; }

    private bool holdingLocked;

    public RectInt bounds
    {
        get
        {
            Vector2Int position = new(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tetrominoDatas.Length; ++i)
        {
            tetrominoDatas[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnRandomPiece();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            HoldPiece();
        }
    }

    public void SpawnRandomPiece()
    {
        SpawnPiece(tetrominoDatas[Random.Range(0, tetrominoDatas.Length)]);
    }

    public void SpawnPiece(TetrominoData data)
    {
        var spawnPosition = new Vector2Int(-1, bounds.yMax - 2);
        if (data.tetromino == Tetromino.I)
            spawnPosition.y -= 1;

        activePiece.Initialize(this, spawnPosition, data);
        holdingLocked = false;
        Utilities.SetPiece(tilemap, activePiece);
    }

    public void Clear(Piece piece)
    {
        foreach (var cell in piece.cells)
        {
            tilemap.SetTile((Vector3Int)(cell + piece.position), null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector2Int position)
    {
        return Enumerable.All(
            piece.cells,
            cell =>
            {
                var tilePosition = position + cell;
                return bounds.Contains(tilePosition) && !tilemap.HasTile((Vector3Int)tilePosition);
            }
        );
    }

    public void ClearLines()
    {
        List<int> linesToClear = Enumerable
            .Range(bounds.yMin, bounds.size.y)
            .Where(
                y =>
                    Enumerable
                        .Range(bounds.xMin, bounds.size.x)
                        .All(x => tilemap.HasTile(new Vector3Int(x, y, 0)))
            )
            .ToList();

        if (!linesToClear.Any())
            return;

        int linesCleared = 0;
        for (int y = linesToClear[0]; y < bounds.yMax; ++y)
        {
            var clearing = linesToClear.Contains(y);
            for (int x = bounds.xMin; x < bounds.xMax; ++x)
            {
                var position = new Vector3Int(x, y, 0);
                if (!clearing)
                    tilemap.SetTile(
                        new Vector3Int(x, y - linesCleared, 0),
                        tilemap.GetTile(position)
                    );

                tilemap.SetTile(new Vector3Int(x, y, 0), null);
            }

            if (clearing)
                linesCleared += 1;
        }
    }

    private void HoldPiece()
    {
        if (holdingLocked)
            return;

        var prevHeldPiece = holder.heldPiece;

        holder.SetHeldPiece(activePiece.data);
        Clear(activePiece);

        if (prevHeldPiece.HasValue)
            SpawnPiece(prevHeldPiece.Value);

        holdingLocked = true;
    }
}
