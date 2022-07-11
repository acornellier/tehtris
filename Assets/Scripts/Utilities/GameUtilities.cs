using UnityEngine;

public class GameUtilities
{
    public static void Quit()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
