using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public float timeLimit = 10f;            // Seconds (only for Timed mode)
    public int _brokenProgress;
    public int _breakableCount;

    [Header("UI References")]
    public TMP_Text brokenCountText;         // Shows "Broken: X / Y"
    public TMP_Text timerText;                // Shows time remaining (Timed mode only)
    public TMP_Text winMessageText;           // Shows "YOU WIN!" 
    public GameObject gameOverPanel;           // Panel with replay button
    public Button replayButton;

    private float timer;
    private bool gameActive = true;
    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Hide game over panel at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Start()
    {
        // Find all breakable objects in the scene (in case they didn't register in time)
        _breakableCount = FindObjectsByType<BreakableObject>(FindObjectsSortMode.None).Length;
        _brokenProgress = _breakableCount;

        // Initialize timer
        timer = timeLimit;
        UpdateUI();
    }

    private void Update()
    {
        if (!gameActive || gameEnded) return;

        // Timed mode countdown
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = 0;
            EndGame();
        }
        UpdateUI();
    }

    public void ObjectBroken(BreakableObject obj)
    {
        if (!gameActive || gameEnded) return;

        _brokenProgress--;
        int bonus = Random.Range(1,11);
        timer += bonus;
        UpdateUI();
        // Check win condition for ClearAll mode
        if (_brokenProgress <= 0)
        {
            WinGame();
        }
    }

    private void EndGame()
    {
        gameActive = false;
        gameEnded = true;

        // Determine win/loss in Timed mode
        if (_brokenProgress <= 0)
            WinGame();
        else
            LoseGame();
    }

    private void WinGame()
    {
        gameActive = false;
        gameEnded = true;

        if (winMessageText != null)
            winMessageText.text = "YOU WIN!";

        ShowGameOverPanel();
    }

    private void LoseGame()
    {
        gameActive = false;
        gameEnded = true;

        if (winMessageText != null)
            winMessageText.text = "TIME'S UP!";

        ShowGameOverPanel();
    }

    private void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    private void UpdateUI()
    {
        if (brokenCountText != null)
            brokenCountText.text = $"Remaining: {_brokenProgress}";

        if (timerText != null)
        {
            if (gameActive)
            {
                int minutes = Mathf.FloorToInt(timer / 60f);
                int seconds = Mathf.FloorToInt(timer % 60f);
                timerText.text = $"Time: {minutes:00}:{seconds:00}";
            }
            else
            {
                timerText.text = "";
            }
        }
    }

    // Called by the replay button
    public void Replay()
    {
        // Reload the current scene � this resets all objects to their initial positions,
        // effectively "repopulating" the breakable objects.

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    // Helper for BreakableObject to check if game is active
    public bool IsGameActive()
    {
        return gameActive && !gameEnded;
    }
}