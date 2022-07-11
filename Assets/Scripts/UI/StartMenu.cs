using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public void PlayGame()
    {
        GameManager.Instance.Mode = GameMode.Solo;
    }

    public void WatchAi()
    {
        GameManager.Instance.Mode = GameMode.Ai;
    }

    public void Quit()
    {
        GameUtilities.Quit();
    }
}
