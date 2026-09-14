using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Waiting,
    Playing,
    LevelComplete,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float restartDelay = 2f;
    [SerializeField] private PlayerController player;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private AlienController alien;

    public GameState CurrentState { get; private set; }
    public float TimeRemaining { get; private set; }

    private int completedLevel;
    private bool currentLevelIsBonus;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentState = GameState.Waiting;
    }

    private void Start()
    {
        if (RunManager.Instance != null && RunManager.Instance.RunStarted)
        {
            BeginCurrentLevel();
        }
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing)
        {
            return;
        }

        if (currentLevelIsBonus)
        {
            return;
        }

        TimeRemaining -= Time.deltaTime;

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            CompleteCurrentLevel();
        }
    }

    public void StartGame()
    {
        if (CurrentState != GameState.Waiting)
        {
            return;
        }

        if (RunManager.Instance != null)
        {
            RunManager.Instance.StartNewRun();
        }

        BeginCurrentLevel();
    }

    public void ReturnToStart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void StartNextLevel()
    {
        if (CurrentState != GameState.LevelComplete ||
            RunManager.Instance == null ||
            levelManager == null ||
            completedLevel >= levelManager.GetLastLevelNumber())
        {
            return;
        }

        if (SceneTransition.Instance == null)
        {
            Debug.LogError("GameManager requires a SceneTransition.", this);
            return;
        }

        SceneTransition.Instance.FadeOutAndRun(LoadNextLevel);
    }

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver)
        {
            return;
        }

        if (currentLevelIsBonus)
        {
            CompleteCurrentLevel();
            return;
        }

        CurrentState = GameState.GameOver;

        if (RunManager.Instance != null)
        {
            RunManager.Instance.RegisterDeath();
        }

        StartCoroutine(RestartSceneAfterDelay());
    }

    private void BeginCurrentLevel()
    {
        if (levelManager == null ||
            RunManager.Instance == null)
        {
            return;
        }

        LevelConfig levelConfig = levelManager.GetCurrentLevelConfig();
        if (levelConfig == null)
        {
            return;
        }

        currentLevelIsBonus = levelConfig.isBonusLevel;

        TimeRemaining = levelManager.GetCurrentLevelDuration();
        CurrentState = GameState.Playing;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetLevelStats();
        }

        if (uiManager != null)
        {
            uiManager.ClearFeedback();
        }

        if (player != null)
        {
            player.BeginGame();
        }

        if (alien != null)
        {
            alien.BeginGame();
        }
    }

    public void CompleteCurrentLevel()
    {
        if (RunManager.Instance == null)
        {
            return;
        }

        completedLevel = RunManager.Instance.CurrentLevel;
        CurrentState = GameState.LevelComplete;

        if (player != null)
        {
            player.StopGameplay();
        }

        if (uiManager == null)
        {
            return;
        }

        if (levelManager != null &&
            completedLevel == levelManager.GetLastLevelNumber())
        {
            uiManager.ShowWinPanel();
        }
        else
        {
            uiManager.ShowLevelCompletePanel();
        }
    }

    public bool CurrentLevelIsBonus()
    {
        return currentLevelIsBonus;
    }

    private void LoadNextLevel()
    {
        if (RunManager.Instance == null)
        {
            return;
        }

        RunManager.Instance.CompleteLevel();

        RunManager.Instance.BeginLevel(
            RunManager.Instance.CurrentLevel);

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }

    private IEnumerator RestartSceneAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}