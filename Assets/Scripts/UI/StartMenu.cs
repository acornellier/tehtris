using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public void PlayGame()
    {
        GameManager.Instance.Mode = GameMode.Solo;
    }

    public void WatchAi()
    {
        GameManager.Instance.Mode = GameMode.WatchAi;
    }

    public void Battle()
    {
        GameManager.Instance.Mode = GameMode.Battle;
    }

    public void Quit()
    {
        GameUtilities.Quit();
    }
}
