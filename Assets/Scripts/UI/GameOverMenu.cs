using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public void Restart()
    {
        GameManager.Instance.Mode = GameMode.Solo;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        GameUtilities.Quit();
    }
}
