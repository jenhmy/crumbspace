using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private float spawnX = 15f;
    [SerializeField] private float minSpawnY = -4f;
    [SerializeField] private float maxSpawnY = 4f;
    [SerializeField] private float starSpawnInterval = 3f;
    [SerializeField] private float croissantSpawnInterval = 7f;
    [SerializeField] private float initialStarDelay = 0.5f;
    [SerializeField] private float initialCroissantDelay = 1f;
    [SerializeField] private float starMinScale = 0.8f;
    [SerializeField] private float starMaxScale = 1.2f;
    [SerializeField] private float croissantMinScale = 0.8f;
    [SerializeField] private float croissantMaxScale = 1.2f;
    [SerializeField] private float bonusCollectibleSpawnInterval = 3f;
    [SerializeField] private float initialBonusCollectibleDelay = 0.5f;
    [SerializeField] private float bonusCollectibleStarMinScale = 0.3f;
    [SerializeField] private float bonusCollectibleStarMaxScale = 0.5f;
    [SerializeField] private float bonusCollectibleCroissantMinScale = 0.2f;
    [SerializeField] private float bonusCollectibleCroissantMaxScale = 0.3f;
    [SerializeField] private float powerUpAsteroidMinSeparation = 1.5f;
    [SerializeField] private float powerUpAsteroidCheckXRange = 4f;
    [SerializeField] private float collectibleAsteroidMinSeparation = 1.2f;
    [SerializeField] private float collectibleAsteroidCheckXRange = 4f;

    [Header("Bonus Difficulty")]
    [SerializeField] private float bonusMinAsteroidSpawnInterval = 1f;
    [SerializeField] private float bonusAsteroidStartDelay = 5f;

    private static readonly PoolId[] asteroidPoolIds =
    {
        PoolId.Asteroid01,
        PoolId.Asteroid02,
        PoolId.Asteroid03,
        PoolId.Asteroid04,
        PoolId.Asteroid05
    };

    private LevelConfig currentLevelConfig;
    private float spawnTimer;
    private float starSpawnTimer;
    private float croissantSpawnTimer;
    private float bonusElapsedTime;
    private bool initialSequenceStarted;
    private bool initialSequenceInProgress;
    private bool powerUpSequenceStarted;

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        if (!initialSequenceStarted)
        {
            initialSequenceStarted = true;
            initialSequenceInProgress = true;
            spawnTimer = 0f;
            starSpawnTimer = 0f;
            croissantSpawnTimer = 0f;
            bonusElapsedTime = 0f;

            currentLevelConfig = levelManager != null
                ? levelManager.GetCurrentLevelConfig()
                : null;

            if (IsBonusLevel())
            {
                StartCoroutine(SpawnInitialBonusCollectible());
            }
            else
            {
                SpawnCurrentLevelAsteroid();
                StartCoroutine(SpawnInitialCollectibles());
                StartPowerUpSequence();
            }
            return;
        }

        if (initialSequenceInProgress)
        {
            return;
        }

        spawnTimer += Time.deltaTime;
        starSpawnTimer += Time.deltaTime;
        croissantSpawnTimer += Time.deltaTime;

        bool isBonusLevel = IsBonusLevel();

        if (isBonusLevel)
        {
            bonusElapsedTime += Time.deltaTime;

            SpawnBonusObstacleIfDue();

            float bonusCollectibleInterval =
                GetAdjustedCollectibleSpawnInterval(
                    bonusCollectibleSpawnInterval);

            if (bonusCollectibleInterval > 0f &&
                starSpawnTimer >= bonusCollectibleInterval)
            {
                starSpawnTimer = 0f;
                SpawnCollectible(GetRandomBonusCollectiblePoolId());
            }

            return;
        }

        SpawnAsteroidIfDue();

        float adjustedStarInterval =
            GetAdjustedCollectibleSpawnInterval(starSpawnInterval);
        if (adjustedStarInterval > 0f &&
            starSpawnTimer >= adjustedStarInterval)
        {
            starSpawnTimer = 0f;
            SpawnCollectible(PoolId.Star);
        }

        float adjustedCroissantInterval =
            GetAdjustedCollectibleSpawnInterval(croissantSpawnInterval);
        if (adjustedCroissantInterval > 0f &&
            croissantSpawnTimer >= adjustedCroissantInterval)
        {
            croissantSpawnTimer = 0f;
            SpawnCollectible(PoolId.Croissant);
        }
    }

    private bool IsBonusLevel()
    {
        return currentLevelConfig != null &&
            currentLevelConfig.isBonusLevel;
    }

    private void SpawnBonusObstacle()
    {
        SpawnAsteroid(GetRandomBonusObstaclePoolId());
    }

    private void SpawnBonusObstacleIfDue()
    {
        if (bonusElapsedTime < bonusAsteroidStartDelay)
        {
            return;
        }

        if (currentLevelConfig == null ||
            currentLevelConfig.asteroidSpawnInterval <= 0f)
        {
            return;
        }

        float currentInterval = GetBonusAsteroidSpawnInterval(
            currentLevelConfig.asteroidSpawnInterval);

        if (spawnTimer >= currentInterval)
        {
            spawnTimer = 0f;
            SpawnBonusObstacle();
        }
    }

    private float GetBonusAsteroidSpawnInterval(float baseInterval)
    {
        float progress = Mathf.Clamp01(bonusElapsedTime / 60f);

        progress *= progress;

        return Mathf.Lerp(
            baseInterval,
            bonusMinAsteroidSpawnInterval,
            progress
        );
    }

    private IEnumerator SpawnInitialBonusCollectible()
    {
        yield return WaitForPlayingTime(
            GetAdjustedCollectibleSpawnInterval(
                initialBonusCollectibleDelay));

        SpawnCollectible(GetRandomBonusCollectiblePoolId());
        starSpawnTimer = 0f;
        initialSequenceInProgress = false;
    }

    private PoolId GetRandomBonusCollectiblePoolId()
    {
        return Random.Range(0, 2) == 0
            ? PoolId.BonusCollectibleStar
            : PoolId.BonusCollectibleCroissant;
    }

    private float GetAdjustedCollectibleSpawnInterval(float interval)
    {
        float multiplier = currentLevelConfig != null
            ? Mathf.Max(
                0.01f,
                currentLevelConfig.collectibleSpawnMultiplier)
            : 1f;

        return interval / multiplier;
    }

    private PoolId GetRandomBonusObstaclePoolId()
    {
        return Random.Range(0, 2) == 0
            ? PoolId.BonusObstacle01
            : PoolId.BonusObstacle02;
    }

    private void SpawnCurrentLevelAsteroid()
    {
        if (currentLevelConfig == null ||
            currentLevelConfig.allowedAsteroids == null ||
            currentLevelConfig.allowedAsteroids.Length == 0)
        {
            return;
        }

        SpawnAsteroid(currentLevelConfig.allowedAsteroids);
    }

    private void SpawnAsteroidIfDue()
    {
        if (currentLevelConfig == null ||
            currentLevelConfig.allowedAsteroids == null ||
            currentLevelConfig.allowedAsteroids.Length == 0 ||
            currentLevelConfig.asteroidSpawnInterval <= 0f)
        {
            return;
        }

        if (spawnTimer >= currentLevelConfig.asteroidSpawnInterval)
        {
            spawnTimer = 0f;
            SpawnAsteroid(currentLevelConfig.allowedAsteroids);
        }
    }

    private IEnumerator SpawnInitialCollectibles()
    {
        float elapsed = 0f;
        float starY = GetSafeSpawnY(
            collectibleAsteroidMinSeparation,
            collectibleAsteroidCheckXRange,
            false,
            0f);

        while (elapsed < initialStarDelay)
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameState.Playing)
            {
                elapsed += Time.deltaTime;
            }

            yield return null;
        }
        SpawnCollectible(PoolId.Star, starY);
        starSpawnTimer = 0f;

        while (elapsed < initialCroissantDelay)
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameState.Playing)
            {
                elapsed += Time.deltaTime;
            }

            yield return null;
        }

        SpawnCollectible(
            PoolId.Croissant,
            GetSafeSpawnY(
                collectibleAsteroidMinSeparation,
                collectibleAsteroidCheckXRange,
                true,
                starY));
        croissantSpawnTimer = 0f;
        initialSequenceInProgress = false;
    }

    private void StartPowerUpSequence()
    {
        if (powerUpSequenceStarted)
        {
            return;
        }

        powerUpSequenceStarted = true;
        StartCoroutine(SpawnAvailablePowerUps());
    }

    private IEnumerator SpawnAvailablePowerUps()
    {
        if (currentLevelConfig == null)
        {
            yield break;
        }

        List<PoolId> availablePowerUps =
            GetAvailablePowerUps(currentLevelConfig);

        if (availablePowerUps.Count == 0)
        {
            yield break;
        }

        for (int i = availablePowerUps.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            PoolId temp = availablePowerUps[i];
            availablePowerUps[i] = availablePowerUps[randomIndex];
            availablePowerUps[randomIndex] = temp;
        }

        float duration = levelManager.GetCurrentLevelDuration();
        float firstTime = duration * 0.2f;
        float lastTime = duration * 0.7f;
        float previousTime = 0f;

        for (int index = 0; index < availablePowerUps.Count; index++)
        {
            float spawnTime = availablePowerUps.Count == 1
                ? duration * 0.35f
                : Mathf.Lerp(
                    firstTime,
                    lastTime,
                    index / (float)(availablePowerUps.Count - 1));

            yield return WaitForPlayingTime(spawnTime - previousTime);
            SpawnPowerUp(availablePowerUps[index]);
            previousTime = spawnTime;
        }
    }

    private IEnumerator WaitForPlayingTime(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameState.Playing)
            {
                elapsed += Time.deltaTime;
            }

            yield return null;
        }
    }

    private List<PoolId> GetAvailablePowerUps(LevelConfig levelConfig)
    {
        List<PoolId> availablePowerUps = new List<PoolId>();

        for (int count = 0; count < levelConfig.magnetCount; count++)
        {
            availablePowerUps.Add(PoolId.Magnet);
        }

        for (int count = 0; count < levelConfig.missileCount; count++)
        {
            availablePowerUps.Add(PoolId.Missile);
        }

        for (int count = 0; count < levelConfig.invisibilityCount; count++)
        {
            availablePowerUps.Add(PoolId.Invisibility);
        }

        for (int count = 0; count < levelConfig.snowCount; count++)
        {
            availablePowerUps.Add(PoolId.Snow);
        }

        for (int count = 0; count < levelConfig.dynamiteCount; count++)
        {
            availablePowerUps.Add(PoolId.Dynamite);
        }

        return availablePowerUps;
    }

    private void SpawnPowerUp(PoolId poolId)
    {
        if (poolManager == null)
        {
            return;
        }

        GameObject powerUp = poolManager.Get(poolId);
        if (powerUp == null)
        {
            return;
        }

        float spawnY = GetSafeSpawnY(
            powerUpAsteroidMinSeparation,
            powerUpAsteroidCheckXRange,
            false,
            0f);
        powerUp.transform.position =
            new Vector3(spawnX, spawnY, transform.position.z);
        powerUp.SetActive(true);
    }

    private float GetSafeSpawnY(
        float minimumVerticalSeparation,
        float asteroidCheckXRange,
        bool hasReferenceY,
        float referenceY)
    {
        float bestY = Random.Range(minSpawnY, maxSpawnY);
        float bestSeparation = GetSpawnSeparation(
            bestY,
            minimumVerticalSeparation,
            asteroidCheckXRange,
            hasReferenceY,
            referenceY);

        for (int attempt = 0; attempt < 10; attempt++)
        {
            float candidateY = Random.Range(minSpawnY, maxSpawnY);
            float candidateSeparation = GetSpawnSeparation(
                candidateY,
                minimumVerticalSeparation,
                asteroidCheckXRange,
                hasReferenceY,
                referenceY);

            if (candidateSeparation > bestSeparation)
            {
                bestY = candidateY;
                bestSeparation = candidateSeparation;
            }

            if (candidateSeparation >= minimumVerticalSeparation)
            {
                return candidateY;
            }
        }

        return bestY;
    }

    private float GetSpawnSeparation(
        float candidateY,
        float minimumVerticalSeparation,
        float asteroidCheckXRange,
        bool hasReferenceY,
        float referenceY)
    {
        float separation = GetAsteroidSeparation(
            candidateY,
            minimumVerticalSeparation,
            asteroidCheckXRange);

        if (hasReferenceY)
        {
            separation = Mathf.Min(
                separation,
                Mathf.Abs(candidateY - referenceY));
        }

        return separation;
    }

    private float GetAsteroidSeparation(
        float candidateY,
        float minimumVerticalSeparation,
        float asteroidCheckXRange)
    {
        float closestSeparation = maxSpawnY - minSpawnY;
        bool foundAsteroid = false;

        if (poolManager == null)
        {
            return closestSeparation;
        }

        for (int poolIndex = 0;
             poolIndex < asteroidPoolIds.Length;
             poolIndex++)
        {
            foreach (GameObject asteroid in poolManager.GetActiveInstances(
                asteroidPoolIds[poolIndex]))
            {
                if (Mathf.Abs(asteroid.transform.position.x - spawnX) >
                    asteroidCheckXRange)
                {
                    continue;
                }

                foundAsteroid = true;
                float separation = Mathf.Abs(
                    asteroid.transform.position.y - candidateY);

                if (separation < closestSeparation)
                {
                    closestSeparation = separation;
                }
            }
        }

        return foundAsteroid ? closestSeparation : float.MaxValue;
    }

    private void SpawnAsteroid(PoolId[] allowedAsteroids)
    {
        if (poolManager == null)
        {
            return;
        }

        PoolId poolId =
            allowedAsteroids[Random.Range(0, allowedAsteroids.Length)];

        GameObject asteroid = poolManager.Get(poolId);

        if (asteroid == null)
        {
            return;
        }

        AsteroidController asteroidController =
            asteroid.GetComponent<AsteroidController>();

        if (asteroidController != null)
        {
            asteroidController.SetBonusSpeedMultiplier(
                IsBonusLevel()
                    ? GetBonusAsteroidSpeedMultiplier()
                    : 1f);
        }

        float spawnY = Random.Range(minSpawnY, maxSpawnY);

        asteroid.transform.position =
            new Vector3(spawnX, spawnY, transform.position.z);

        asteroid.SetActive(true);
    }

    private void SpawnAsteroid(PoolId poolId)
    {
        if (poolManager == null)
        {
            return;
        }

        GameObject asteroid = poolManager.Get(poolId);

        if (asteroid == null)
        {
            return;
        }

        AsteroidController asteroidController =
            asteroid.GetComponent<AsteroidController>();

        if (asteroidController != null)
        {
            asteroidController.SetBonusSpeedMultiplier(
                IsBonusLevel()
                    ? GetBonusAsteroidSpeedMultiplier()
                    : 1f);
        }

        float spawnY = Random.Range(minSpawnY, maxSpawnY);

        asteroid.transform.position =
            new Vector3(spawnX, spawnY, transform.position.z);

        asteroid.SetActive(true);
    }

    private void SpawnCollectible(PoolId poolId)
    {
        SpawnCollectible(
            poolId,
            GetSafeSpawnY(
                collectibleAsteroidMinSeparation,
                collectibleAsteroidCheckXRange,
                false,
                0f));
    }

    private void SpawnCollectible(PoolId poolId, float spawnY)
    {
        if (poolManager == null)
        {
            return;
        }

        GameObject collectible = poolManager.Get(poolId);
        if (collectible == null)
        {
            return;
        }

        ApplyCollectibleScale(collectible, poolId);
        collectible.transform.position = new Vector3(spawnX, spawnY, transform.position.z);
        collectible.SetActive(true);
        ConfigureCollectibleRotation(collectible, poolId);
    }

    private void ApplyCollectibleScale(GameObject collectible, PoolId poolId)
    {
        float minScale;
        float maxScale;

        if (poolId == PoolId.Star)
        {
            minScale = starMinScale;
            maxScale = starMaxScale;
        }
        else if (poolId == PoolId.Croissant)
        {
            minScale = croissantMinScale;
            maxScale = croissantMaxScale;
        }
        else if (poolId == PoolId.BonusCollectibleStar)
        {
            minScale = bonusCollectibleStarMinScale;
            maxScale = bonusCollectibleStarMaxScale;
        }
        else if (poolId == PoolId.BonusCollectibleCroissant)
        {
            minScale = bonusCollectibleCroissantMinScale;
            maxScale = bonusCollectibleCroissantMaxScale;
        }
        else
        {
            return;
        }

        float scale = Random.Range(minScale, maxScale);
        collectible.transform.localScale =
            new Vector3(scale, scale, 1f);
    }

    private void ConfigureCollectibleRotation(GameObject collectible, PoolId poolId)
    {
        CollectibleController controller =
            collectible.GetComponent<CollectibleController>();

        if (controller == null)
        {
            return;
        }

        bool shouldRotate = poolId == PoolId.Croissant ||
            poolId == PoolId.BonusCollectibleCroissant;

        controller.SetRotationEnabled(shouldRotate);
    }

    private float GetBonusAsteroidSpeedMultiplier()
    {
        float progress = Mathf.Clamp01(bonusElapsedTime / 60f);

        progress *= progress;

        return Mathf.Lerp(
            0.85f,
            2.2f,
            progress
        );
    }

}
