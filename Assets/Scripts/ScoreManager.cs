using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore
    {
        get
        {
            return RunManager.Instance != null
                ? RunManager.Instance.CurrentScore
                : 0;
        }
    }

    public int StarsCollected { get; private set; }
    public int CroissantsCollected { get; private set; }
    public int AliensKilled { get; private set; }
    public int LevelScore { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddPoints(int points)
    {
        AddScore(points);
    }

    public void AddStar(int points)
    {
        StarsCollected++;
        AddScoreToLevel(points);
    }

    public void AddCroissant(int points)
    {
        CroissantsCollected++;
        AddScoreToLevel(points);
    }

    public void AddAlienKill(int points)
    {
        AliensKilled++;
        AddScoreToLevel(points);
    }

    public void AddBonusCollectible(int points)
    {
        AddScoreToLevel(points);
    }

    public void ResetLevelStats()
    {
        StarsCollected = 0;
        CroissantsCollected = 0;
        AliensKilled = 0;
        LevelScore = 0;
    }

    public int GetScore()
    {
        return CurrentScore;
    }

    public int RefreshScore()
    {
        return CurrentScore;
    }

    private void AddScoreToLevel(int points)
    {
        LevelScore += points;
        AddScore(points);
    }

    private void AddScore(int points)
    {
        if (RunManager.Instance == null)
        {
            Debug.LogError("ScoreManager requires a RunManager.", this);
            return;
        }

        RunManager.Instance.AddScore(points);
    }
}
