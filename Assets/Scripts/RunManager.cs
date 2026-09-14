using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    public bool RunStarted { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public int CurrentScore { get; private set; }
    public int LevelStartScore { get; private set; }

    private const int FirstLevel = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartNewRun()
    {
        RunStarted = true;
        CurrentLevel = FirstLevel;
        CurrentScore = 0;
        LevelStartScore = 0;
    }

    public void EndRun()
    {
        RunStarted = false;
        CurrentLevel = FirstLevel;
        CurrentScore = 0;
        LevelStartScore = 0;
    }

    public void AddScore(int points)
    {
        CurrentScore += points;
    }

    public void BeginLevel(int level)
    {
        if (level < FirstLevel)
        {
            Debug.LogError(
                $"Level must be {FirstLevel} or greater.",
                this);
            return;
        }

        CurrentLevel = level;
        LevelStartScore = CurrentScore;
    }

    public void RegisterDeath()
    {
        CurrentScore = LevelStartScore;
    }

    public void CompleteLevel()
    {
        CurrentLevel++;
    }
}
