using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using yaSingleton;
using Random = System.Random;

[CreateAssetMenu(fileName = "Game Manager", menuName = "Singletons/GameManager")]
public class GameManager : Singleton<GameManager>
{
    public Random gen = new(1);

    private GameMode mode = GameMode.MainMenu;

    private bool paused;
    public static event Action<bool> OnGamePauseChange;

    private float aiTimeBetweenMoves = 0.2f;
    public static event Action<float> OnAiTimeBetweenMovesChange;

    public GameMode Mode
    {
        get => mode;
        set
        {
            mode = value;
            paused = false;
            SceneManager.LoadScene(mode == GameMode.Battle ? "Battle" : "Tetris");
        }
    }

    public bool Paused
    {
        get => paused;
        set
        {
            paused = value;
            OnGamePauseChange?.Invoke(value);
        }
    }

    public float AiTimeBetweenMoves
    {
        get => aiTimeBetweenMoves;
        set
        {
            aiTimeBetweenMoves = value;
            OnAiTimeBetweenMovesChange?.Invoke(value);
        }
    }
}

public enum GameMode
{
    MainMenu,
    Solo,
    WatchAi,
    Battle,
}
