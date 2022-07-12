using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Vector2Int boardSize = new(10, 20);
    public Holder holder;
    public TetrominoQueue tetrominoQueue;
    public GameObject gameOverMenu;

    private bool holdingLocked;

    public Piece ActivePiece { get; private set; }
    public Tilemap Tilemap { get; private set; }
    private AudioSource audioSource;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private int lineClearCount;
    private int totalLinesCleared;

    private void Awake()
    {
        Tilemap = GetComponentInChildren<Tilemap>();
        ActivePiece = GameManager.Instance.Mode == GameMode.Ai
            ? gameObject.AddComponent<AiPiece>()
            : gameObject.AddComponent<PlayerPiece>();

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        SpawnNextPiece();
    }

    public void LockPiece()
    {
        audioSource.Play();
        Utilities.SetPiece(Tilemap, ActivePiece);
        ClearLines();
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
            spawnPosition.y -= 1;

        if (!IsValidPosition(data.Cells, spawnPosition))
        {
            gameOverMenu.SetActive(true);

        }

        ActivePiece.Initialize(this, spawnPosition, data);
        holdingLocked = false;
    }

    private void Clear(Piece piece)
    {
        foreach (var cell in piece.Cells)
        {
            Tilemap.SetTile((Vector3Int)(cell + piece.Position), null);
        }
    }

    public bool IsValidPosition(IEnumerable<Vector2Int> cells, Vector2Int position)
    {
        return cells.All(
            cell =>
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
            return;

        lineClearCount += 1;
        totalLinesCleared += linesToClear.Count();
        print($"avg lines cleared per clear {1.0f * totalLinesCleared / lineClearCount}");

        var linesCleared = 0;
        for (var y = linesToClear[0]; y < Bounds.yMax; ++y)
        {
            var clearing = linesToClear.Contains(y);
            for (var x = Bounds.xMin; x < Bounds.xMax; ++x)
            {
                var position = new Vector3Int(x, y, 0);
                if (!clearing)
                    Tilemap.SetTile(
                        new Vector3Int(x, y - linesCleared, 0),
                        Tilemap.GetTile(position)
                    );

                Tilemap.SetTile(position, null);
            }

            if (clearing)
                linesCleared += 1;
        }
    }

    public void HoldPiece()
    {
        if (holdingLocked)
            return;

        var prevHeldPiece = holder.HeldPiece;

        holder.SetHeldPiece(ActivePiece.Data);
        Clear(ActivePiece);

        if (prevHeldPiece != null)
            SpawnPiece(prevHeldPiece);
        else
            SpawnNextPiece();

        holdingLocked = true;
    }
}
