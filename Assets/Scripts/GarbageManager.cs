using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GarbageManager : MonoBehaviour
{
    public static GarbageManager Instance { get; private set; }

    public int totalGarbageBags;
    public int totalFloorRubbish;
    private int collectedGarbageBags = 0;
    private int collectedFloorRubbish = 0;
    public Text Bags;
    public Text FloorRubbish;
    public Text Timer;
    public float timeLeft;
    public GameObject WinPanel;
    public GameObject LosePanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    private void Start()
    {
        UpdateScore();
    }
    

    private void Update()
    {
        UpdateTimer();
    }

    public void CollectGarbage()
    {
        collectedGarbageBags++;
        UpdateScore();
        CheckWinCondition();
    }

    public void CollectFloorRubbish()
    {
        collectedFloorRubbish++;
        UpdateScore();
        CheckWinCondition();
    }

    private void UpdateScore()
    {
        Bags.text = "Garbage Bags: " + collectedGarbageBags + "/" + totalGarbageBags;
        FloorRubbish.text = "Floor Rubbish: " + collectedFloorRubbish + "/" + totalFloorRubbish;
    }

    private void UpdateTimer()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);
            Timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else
        {
            // Delete timer text
            Timer.text = "";
            ShowLosePanel();
        }
    }

    private void CheckWinCondition()
    {
        if (collectedGarbageBags >= totalGarbageBags && collectedFloorRubbish >= totalFloorRubbish)
        {
            ShowWinPanel();
        }
    }

    private void ShowWinPanel()
    {
        WinPanel.SetActive(true);
        Time.timeScale = 0; // Pause the game
        UnlockCursor();
    }

    private void ShowLosePanel()
    {
        LosePanel.SetActive(true);
        Time.timeScale = 0; // Pause the game
        UnlockCursor();
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Restart the game by reloading the scene
    public void RestartGame()
    {
        Time.timeScale = 1; // Unpause the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Exit the game
    public void ExitGame()
    {
        Application.Quit();
    }
}
