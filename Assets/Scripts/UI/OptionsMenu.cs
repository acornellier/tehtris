using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public GameObject pauseMenuUi;
    public GameObject optionsMenuUi;

    public void Back()
    {
        pauseMenuUi.SetActive(true);
        optionsMenuUi.SetActive(false);
    }
}
