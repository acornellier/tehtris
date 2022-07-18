using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject pauseMenuUi;
    public GameObject optionsMenuUi;

    private void Awake()
    {
        GameManager.OnGamePauseChange += OnGamePauseChange;
    }

    private void OnDestroy()
    {
        GameManager.OnGamePauseChange -= OnGamePauseChange;
    }

    private void OnGamePauseChange(bool paused)
    {
        if (paused) PauseCallback();
        else ResumeCallback();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            GameManager.Instance.Paused = !GameManager.Instance.Paused;
    }

    private void PauseCallback()
    {
        pausePanel.SetActive(true);
        pauseMenuUi.SetActive(true);
        optionsMenuUi.SetActive(false);
    }

    private void ResumeCallback()
    {
        pausePanel.SetActive(false);
    }

    public void Resume()
    {
        GameManager.Instance.Paused = false;
    }

    public void Options()
    {
        pauseMenuUi.SetActive(false);
        optionsMenuUi.SetActive(true);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("StartMenu");
    }

    public void Quit()
    {
        GameUtilities.Quit();
    }
}
