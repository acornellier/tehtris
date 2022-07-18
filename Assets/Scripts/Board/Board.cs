using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    private readonly Vector2Int boardSize = new(10, 20);

    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tilemap holderTilemap;
    [SerializeField] private Tilemap queueTilemap;
    [SerializeField] private Tile ghostTile;
    [SerializeField] private TetrominoGenerator tetrominoGenerator;
    [SerializeField] private GameObject gameOverMenu;

    private AudioSource audioSource;
    private BoardState state;
    private Controller controller;

    public static event Action<int, int> OnLinesClearedEvent;

    private RectInt Bounds
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
        controller = GameManager.Instance.Mode == GameMode.WatchAi
            ? GetComponent<AiController>()
            : GetComponent<PlayerController>();

        controller ??= gameObject.AddComponent<AiController>();

        audioSource = GetComponent<AudioSource>();

        GameManager.OnGamePauseChange += OnGamePauseChange;
        OnLinesClearedEvent += OnLinesCleared;
    }

    private void OnDestroy()
    {
        GameManager.OnGamePauseChange -= OnGamePauseChange;
        OnLinesClearedEvent -= OnLinesCleared;
    }

    private void OnGamePauseChange(bool paused)
    {
        enabled = !paused;
    }

    private void Start()
    {
        state = new BoardState(boardSize, tetrominoGenerator.Generate());
    }

    private void Update()
    {
        var move = controller.GetMove(state.DeepClone());
        var moveResults = state.MakeMove(move);

        if (state.GameOver)
        {
            UpdateTilemaps();
            gameOverMenu.SetActive(true);
            enabled = false;
            return;
        }

        if (state.Queue.Count < 7)
            state.PushToQueue(tetrominoGenerator.Generate());

        if (moveResults.held || moveResults.locked)
            controller.NotifyNewPiece();

        if (moveResults.locked && !controller.Muted)
            audioSource.Play();

        if (moveResults.linesCleared > 0)
            OnLinesClearedEvent?.Invoke(GetInstanceID(), moveResults.linesCleared);

        // if (GameManager.Instance.gen.NextDouble() > 0.8)
        // state.AddPendingGarbage((int)Math.Ceiling((float)GameManager.Instance.gen.Next(1, 3)));
        // lineClearCount += 1;
        // totalLinesCleared += moveResults.linesCleared;
        // print($"avg lines cleared per clear {1.0f * totalLinesCleared / lineClearCount}");

        UpdateTilemaps();
    }

    private void UpdateTilemaps()
    {
        tilemap.ClearAllTiles();
        SetBaseTiles();
        SetGhostTiles();
        SetPieceTiles();

        SetHolderTiles();

        SetQueueTiles();
    }

    private void SetBaseTiles()
    {
        for (var x = 0; x < state.Columns; ++x)
        {
            for (var y = 0; y < state.Rows; ++y)
            {
                var tile = tetrominoGenerator.TileStateToTile[state.Tiles[x, y]];
                tilemap.SetTile(new Vector3Int(x + Bounds.xMin, y + Bounds.yMin, 0), tile);
            }
        }
    }

    private void SetGhostTiles()
    {
        var position = state.PiecePosition;

        for (var y = position.y; y >= -1; --y)
        {
            var testPosition = position;
            testPosition.y = y;
            if (!state.IsValidPosition(testPosition))
                break;

            position = testPosition;
        }

        SetPieceCells(ghostTile, position);
    }

    private void SetPieceTiles()
    {
        SetPieceCells(state.PieceData.Tile, state.PiecePosition);
    }

    private void SetPieceCells(Tile tile, Vector2Int position)
    {
        position.x += Bounds.xMin;
        position.y += Bounds.yMin;
        Utilities.SetCells(tilemap, state.PieceCells, tile, position);
    }

    private void SetHolderTiles()
    {
        if (!holderTilemap || state.HeldPiece == null)
            return;

        holderTilemap.ClearAllTiles();
        Utilities.SetCells(holderTilemap, state.HeldPiece.Cells, state.HeldPiece.Tile);
    }

    private void SetQueueTiles()
    {
        if (!queueTilemap) return;

        queueTilemap.ClearAllTiles();

        for (var i = 0; i < 5 && i < state.Queue.Count; ++i)
        {
            var tetromino = state.Queue[i];
            var position = new Vector2Int(0, i * -3);
            Utilities.SetCells(queueTilemap, tetromino.Cells, tetromino.Tile, position);
        }
    }

    private void OnLinesCleared(int instanceId, int linesCleared)
    {
        if (GetInstanceID() == instanceId)
            return;

        var linesToSend = (int)Math.Ceiling(linesCleared / 2f);
        state.AddPendingGarbage(linesToSend);
    }
}
