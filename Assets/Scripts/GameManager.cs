using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameMode mode = GameMode.Solo;

    private void Awake()
    {
        Instance = this;
    }
}

public enum GameMode
{
    Solo,
    Ai,
}
