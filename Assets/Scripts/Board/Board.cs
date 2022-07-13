using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Vector2Int boardSize = new(10, 20);
    public Tilemap tilemap;
    public Tilemap holderTilemap;
    public Tile ghostTile;
    public TetrominoQueue tetrominoQueue;
    public GameObject gameOverMenu;

    private AudioSource audioSource;
    private BoardState state;
    private Controller controller;

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
        controller = GameManager.Instance.Mode == GameMode.Ai
            ? GetComponent<AiController>()
            : GetComponent<PlayerController>();

        audioSource = GetComponent<AudioSource>();

        GameManager.OnGamePauseChange += OnGamePauseChange;
    }

    private void OnDestroy()
    {
        GameManager.OnGamePauseChange -= OnGamePauseChange;
    }

    private void OnGamePauseChange(bool paused)
    {
        enabled = !paused;
    }

    private void Start()
    {
        state = new BoardState(boardSize, tetrominoQueue.NextTetrominos);
        tetrominoQueue.UpdateQueue(state.Queue);
    }

    private void Update()
    {
        state.UpdateQueue(tetrominoQueue.NextTetrominos);

        var move = controller.GetMove(state.DeepClone());
        var moveResults = state.MakeMove(move);

        if (state.gameOver)
        {
            gameOverMenu.SetActive(true);
            enabled = false;
            return;
        }

        if (moveResults.held || moveResults.locked)
        {
            controller.NotifyNewPiece();
            tetrominoQueue.UpdateQueue(state.Queue);
        }

        if (moveResults.locked && !controller.Muted)
            audioSource.Play();

        if (moveResults.linesCleared > 0)
        {
            lineClearCount += 1;
            totalLinesCleared += moveResults.linesCleared;
            print($"avg lines cleared per clear {1.0f * totalLinesCleared / lineClearCount}");
        }

        UpdateTilemaps();
    }

    private void UpdateTilemaps()
    {
        tilemap.ClearAllTiles();

        SetBaseTiles();
        SetGhostTiles();
        SetPieceTiles();
        SetHolderTiles();
    }

    private void SetBaseTiles()
    {
        for (var x = 0; x < state.Columns; ++x)
        {
            for (var y = 0; y < state.Rows; ++y)
            {
                tilemap.SetTile(new Vector3Int(x + Bounds.xMin, y + Bounds.yMin, 0), state.Tiles[x, y]);
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
        SetPieceCells(state.PieceData.tile, state.PiecePosition);
    }

    private void SetPieceCells(Tile tile, Vector2Int position)
    {
        position.x += Bounds.xMin;
        position.y += Bounds.yMin;
        Utilities.SetCells(tilemap, state.PieceCells, tile, position);
    }

    private void SetHolderTiles()
    {
        if (state.HeldPiece == null)
            return;

        holderTilemap.ClearAllTiles();
        Utilities.SetCells(holderTilemap, state.HeldPiece.Cells, state.HeldPiece.tile);
    }
}
