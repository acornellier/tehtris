using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUi;

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
        pauseMenuUi.SetActive(true);
    }

    public void Resume()
    {
        GameManager.Instance.Paused = false;
    }

    private void ResumeCallback()
    {
        pauseMenuUi.SetActive(false);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        GameUtilities.Quit();
    }
}
