﻿using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using yaSingleton;

[CreateAssetMenu(fileName = "Game Manager", menuName = "Singletons/GameManager")]
public class GameManager : Singleton<GameManager>
{
    private GameMode mode = GameMode.MainMenu;

    public GameMode Mode
    {
        get => mode;
        set
        {
            mode = value;
            paused = false;
            SceneManager.LoadScene("Tetris");
        }
    }

    private bool paused;
    public static event Action<bool> OnGamePauseChange;

    public bool Paused
    {
        get => paused;
        set
        {
            paused = value;
            OnGamePauseChange?.Invoke(value);
        }
    }
}

public enum GameMode
{
    MainMenu,
    Solo,
    Ai,
}