using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public void PlayGame()
    {
        GameManager.Instance.mode = GameMode.Solo;
        SceneManager.LoadScene("Tetris");
    }

    public void WatchAi()
    {
        GameManager.Instance.mode = GameMode.Ai;
        SceneManager.LoadScene("Tetris");
    }
}
