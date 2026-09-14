using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HighScoreEntry
{
    public string playerName;
    public int score;
}

[Serializable]
public class HighScoreList
{
    public List<HighScoreEntry> entries = new List<HighScoreEntry>();
}

public class SaveManager : MonoBehaviour
{
    public const string HighScoresKey = "HighScores";
    public const int MaxHighScores = 5;
    public const int MaxPlayerNameLength = 12;

    public bool SaveHighScore(string playerName, int score)
    {
        string normalizedName = NormalizePlayerName(playerName);
        if (string.IsNullOrEmpty(normalizedName))
        {
            return false;
        }

        HighScoreList highScoreList = LoadHighScores();
        highScoreList.entries.Add(new HighScoreEntry
        {
            playerName = normalizedName,
            score = score
        });

        SortAndLimit(highScoreList.entries);
        SaveHighScores(highScoreList);
        return true;
    }

    public bool SaveCurrentRunScore(string playerName)
    {
        if (RunManager.Instance == null)
        {
            Debug.LogError("SaveManager requires a RunManager to save the current score.", this);
            return false;
        }

        return SaveHighScore(playerName, RunManager.Instance.CurrentScore);
    }

    public List<HighScoreEntry> GetHighScores()
    {
        HighScoreList highScoreList = LoadHighScores();
        SortAndLimit(highScoreList.entries);
        return new List<HighScoreEntry>(highScoreList.entries);
    }

    public void ClearHighScores()
    {
        PlayerPrefs.DeleteKey(HighScoresKey);
        PlayerPrefs.Save();
    }

    private HighScoreList LoadHighScores()
    {
        if (!PlayerPrefs.HasKey(HighScoresKey))
        {
            return new HighScoreList();
        }

        string json = PlayerPrefs.GetString(HighScoresKey);
        if (string.IsNullOrEmpty(json))
        {
            return new HighScoreList();
        }

        HighScoreList highScoreList = JsonUtility.FromJson<HighScoreList>(json);
        return highScoreList ?? new HighScoreList();
    }

    private void SaveHighScores(HighScoreList highScoreList)
    {
        string json = JsonUtility.ToJson(highScoreList);
        PlayerPrefs.SetString(HighScoresKey, json);
        PlayerPrefs.Save();
    }

    private static string NormalizePlayerName(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return string.Empty;
        }

        string normalizedName = playerName.Trim();
        return normalizedName.Length <= MaxPlayerNameLength
            ? normalizedName
            : string.Empty;
    }

    private static void SortAndLimit(List<HighScoreEntry> entries)
    {
        entries.Sort((first, second) => second.score.CompareTo(first.score));

        if (entries.Count > MaxHighScores)
        {
            entries.RemoveRange(MaxHighScores, entries.Count - MaxHighScores);
        }
    }
}
