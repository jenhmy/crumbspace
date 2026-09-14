using System;
using UnityEngine;

[Serializable]
public class LevelConfig
{
    public int levelNumber = 1;

    [Min(0.01f)]
    public float asteroidSpawnInterval = 2f;

    [Min(0.01f)]
    public float collectibleSpawnMultiplier = 1f;

    public PoolId[] allowedAsteroids =
    {
        PoolId.Asteroid01,
        PoolId.Asteroid02,
        PoolId.Asteroid03,
        PoolId.Asteroid04,
        PoolId.Asteroid05
    };

    [Min(0)] public int magnetCount;
    [Min(0)] public int missileCount;
    [Min(0)] public int snowCount;
    [Min(0)] public int dynamiteCount;
    [Min(0)] public int invisibilityCount;

    public bool isBonusLevel;
}

public class LevelManager : MonoBehaviour
{
    [SerializeField, Min(0.1f)]
    private float levelDuration = 30f;

    [SerializeField]
    private LevelConfig[] levels =
    {
        new LevelConfig { levelNumber = 1 },
        new LevelConfig { levelNumber = 2 },
        new LevelConfig { levelNumber = 3 },
        new LevelConfig { levelNumber = 4 },
        new LevelConfig { levelNumber = 5 },
        new LevelConfig { levelNumber = 6 },
        new LevelConfig { levelNumber = 7 }
    };

    private int lastLevelNumber;

    private void Awake()
    {
        lastLevelNumber = 0;

        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null &&
                levels[i].levelNumber > lastLevelNumber)
            {
                lastLevelNumber = levels[i].levelNumber;
            }
        }
    }

    public LevelConfig GetLevelConfig(int levelNumber)
    {
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null &&
                levels[i].levelNumber == levelNumber)
            {
                return levels[i];
            }
        }

        Debug.LogError(
            $"No level configuration exists for level {levelNumber}.",
            this);

        return null;
    }

    public LevelConfig GetCurrentLevelConfig()
    {
        if (RunManager.Instance == null)
        {
            return null;
        }

        return GetLevelConfig(
            RunManager.Instance.CurrentLevel);
    }

    public float GetCurrentLevelDuration()
    {
        return levelDuration;
    }

    public int GetLastLevelNumber()
    {
        return lastLevelNumber;
    }
}