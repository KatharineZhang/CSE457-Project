using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Call this on Start button
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Call this on Quit button
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit pressed");
    }
}