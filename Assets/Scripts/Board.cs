using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Vector2Int boardSize = new(10, 20);
    public Holder holder;
    public TetrominoQueue tetrominoQueue;

    private bool holdingLocked;

    public Piece ActivePiece { get; private set; }
    public Tilemap Tilemap { get; private set; }

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        Tilemap = GetComponentInChildren<Tilemap>();
        ActivePiece = GetComponentInChildren<Piece>();
    }

    private void Start()
    {
        SpawnNextPiece();
    }

    public void SpawnNextPiece()
    {
        var nextTetromino = tetrominoQueue.PopNextTetromino();
        SpawnPiece(nextTetromino);
    }

    private void SpawnPiece(TetrominoData data)
    {
        var spawnPosition = new Vector2Int(-1, Bounds.yMax - 2);
        if (data.tetromino == Tetromino.I)
        {
            spawnPosition.y -= 1;
        }

        ActivePiece.Initialize(this, spawnPosition, data);
        holdingLocked = false;
        Utilities.SetPiece(Tilemap, ActivePiece);
    }

    private void Clear(Piece piece)
    {
        foreach (var cell in piece.Cells)
        {
            Tilemap.SetTile((Vector3Int)(cell + piece.Position), null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector2Int position)
    {
        return piece.Cells.All(cell =>
            {
                var tilePosition = position + cell;
                return Bounds.Contains(tilePosition) && !Tilemap.HasTile((Vector3Int)tilePosition);
            }
        );
    }

    public void ClearLines()
    {
        var linesToClear = Enumerable
            .Range(Bounds.yMin, Bounds.size.y)
            .Where(
                y =>
                    Enumerable
                        .Range(Bounds.xMin, Bounds.size.x)
                        .All(x => Tilemap.HasTile(new Vector3Int(x, y, 0)))
            )
            .ToList();

        if (!linesToClear.Any())
        {
            return;
        }

        var linesCleared = 0;
        for (var y = linesToClear[0]; y < Bounds.yMax; ++y)
        {
            var clearing = linesToClear.Contains(y);
            for (var x = Bounds.xMin; x < Bounds.xMax; ++x)
            {
                var position = new Vector3Int(x, y, 0);
                if (!clearing)
                {
                    Tilemap.SetTile(
                        new Vector3Int(x, y - linesCleared, 0),
                        Tilemap.GetTile(position)
                    );
                }

                Tilemap.SetTile(new Vector3Int(x, y, 0), null);
            }

            if (clearing)
            {
                linesCleared += 1;
            }
        }
    }

    public void HoldPiece()
    {
        if (holdingLocked)
        {
            return;
        }

        var prevHeldPiece = holder.HeldPiece;

        holder.SetHeldPiece(ActivePiece.Data);
        Clear(ActivePiece);

        if (prevHeldPiece != null)
        {
            SpawnPiece(prevHeldPiece);
        }
        else
        {
            SpawnNextPiece();
        }

        holdingLocked = true;
    }
}
