using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [SerializeField] private Transform playerTransform;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private AlienController alienController;
    [SerializeField] private float snowSpeedMultiplier = 0.5f;
    [SerializeField] private PlayerController playerController;

    private readonly HashSet<CollectibleController> activeCollectibles =
        new HashSet<CollectibleController>();
    private readonly HashSet<AsteroidController> activeAsteroids =
        new HashSet<AsteroidController>();
    private readonly HashSet<PowerUpController> activePowerUps =
        new HashSet<PowerUpController>();
    private readonly List<AsteroidController> asteroidBuffer =
    new List<AsteroidController>();

    private Coroutine magnetCoroutine;
    private Coroutine snowCoroutine;
    private bool magnetWindowActive;
    private bool snowWindowActive;
    private Coroutine invisibilityCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterCollectible(CollectibleController collectible)
    {
        if (collectible == null)
        {
            return;
        }

        activeCollectibles.Add(collectible);

        if (magnetWindowActive)
        {
            collectible.SetMagnetTarget(playerTransform);
        }

        if (snowWindowActive)
        {
            collectible.SetSpeedMultiplier(snowSpeedMultiplier);
        }
    }

    public void UnregisterCollectible(CollectibleController collectible)
    {
        if (collectible != null)
        {
            activeCollectibles.Remove(collectible);
        }
    }

    public void RegisterAsteroid(AsteroidController asteroid)
    {
        if (asteroid == null)
        {
            return;
        }

        activeAsteroids.Add(asteroid);

        if (snowWindowActive)
        {
            asteroid.SetSpeedMultiplier(snowSpeedMultiplier);
        }
    }

    public void UnregisterAsteroid(AsteroidController asteroid)
    {
        if (asteroid != null)
        {
            activeAsteroids.Remove(asteroid);
        }
    }

    public void RegisterPowerUp(PowerUpController powerUp)
    {
        if (powerUp == null)
        {
            return;
        }

        activePowerUps.Add(powerUp);

        if (snowWindowActive)
        {
            powerUp.SetSpeedMultiplier(snowSpeedMultiplier);
        }
    }

    public void UnregisterPowerUp(PowerUpController powerUp)
    {
        if (powerUp != null)
        {
            activePowerUps.Remove(powerUp);
        }
    }

    public bool TryActivate(PowerUpType powerUpType, float duration)
    {
        if (powerUpType == PowerUpType.Magnet)
        {
            if (!IsMagnetAvailable() || playerTransform == null)
            {
                return false;
            }

            AudioManager.Instance?.PlaySFX(SfxType.Magnet);

            if (magnetCoroutine != null)
            {
                StopCoroutine(magnetCoroutine);
            }

            magnetCoroutine = StartCoroutine(ActivateMagnet(duration));
            return true;
        }

        if (powerUpType == PowerUpType.Missile)
        {
            if (!IsMissileAvailable() || alienController == null)
            {
                return false;
            }

            alienController.KillAlien();
            return true;
        }

        if (powerUpType == PowerUpType.Invisibility)
        {
            if (!IsInvisibilityAvailable() || playerController == null)
            {
                return false;
            }

            if (invisibilityCoroutine != null)
            {
                StopCoroutine(invisibilityCoroutine);
            }

            invisibilityCoroutine =
                StartCoroutine(ActivateInvisibility(duration));

            return true;
        }

        if (powerUpType == PowerUpType.Snow)
        {
            if (!IsSnowAvailable())
            {
                return false;
            }

            AudioManager.Instance?.PlaySFX(SfxType.Snow);

            if (snowCoroutine != null)
            {
                StopCoroutine(snowCoroutine);
            }

            snowCoroutine = StartCoroutine(ActivateSnow(duration));
            return true;
        }

        if (powerUpType == PowerUpType.Dynamite)
        {
            if (!IsDynamiteAvailable())
            {
                return false;
            }

            asteroidBuffer.Clear();
            asteroidBuffer.AddRange(activeAsteroids);

            for (int i = 0; i < asteroidBuffer.Count; i++)
            {
                AsteroidController asteroid = asteroidBuffer[i];

                if (asteroid != null && asteroid.isActiveAndEnabled)
                {
                    asteroid.Explode();
                }
            }

            return true;
        }

        return false;
    }

    private IEnumerator ActivateMagnet(float duration)
    {
        magnetWindowActive = true;
        SetMagnetTargetForCollectibles(playerTransform);

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

        magnetWindowActive = false;
        magnetCoroutine = null;
    }

    private IEnumerator ActivateInvisibility(float duration)
    {
        playerController.SetInvisible(true);

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

        playerController.SetInvisible(false);
        invisibilityCoroutine = null;
    }

    private bool IsInvisibilityAvailable()
    {
        if (levelManager == null)
        {
            return false;
        }

        LevelConfig levelConfig = levelManager.GetCurrentLevelConfig();
        return levelConfig != null &&
               levelConfig.invisibilityCount > 0;
    }

    private IEnumerator ActivateSnow(float duration)
    {
        snowWindowActive = true;

        ApplySnowMultiplierToAsteroids();
        ApplySnowMultiplierToCollectibles();
        ApplySnowMultiplierToPowerUps();

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

        ResetSnowMultiplierForAsteroids();
        ResetSnowMultiplierForCollectibles();
        ResetSnowMultiplierForPowerUps();

        snowWindowActive = false;
        snowCoroutine = null;
    }

    private bool IsMagnetAvailable()
    {
        if (levelManager == null)
        {
            return false;
        }

        LevelConfig levelConfig = levelManager.GetCurrentLevelConfig();
        return levelConfig != null && levelConfig.magnetCount > 0;
    }

    private bool IsMissileAvailable()
    {
        if (levelManager == null)
        {
            return false;
        }

        LevelConfig levelConfig = levelManager.GetCurrentLevelConfig();
        return levelConfig != null && levelConfig.missileCount > 0;
    }

    private bool IsSnowAvailable()
    {
        if (levelManager == null)
        {
            return false;
        }

        LevelConfig levelConfig = levelManager.GetCurrentLevelConfig();
        return levelConfig != null && levelConfig.snowCount > 0;
    }

    private bool IsDynamiteAvailable()
    {
        if (levelManager == null)
        {
            return false;
        }

        LevelConfig levelConfig = levelManager.GetCurrentLevelConfig();
        return levelConfig != null && levelConfig.dynamiteCount > 0;
    }

    private void SetMagnetTargetForCollectibles(Transform target)
    {
        foreach (CollectibleController collectible in activeCollectibles)
        {
            if (collectible != null && collectible.isActiveAndEnabled)
            {
                collectible.SetMagnetTarget(target);
            }
        }
    }

    private void ApplySnowMultiplierToAsteroids()
    {
        foreach (AsteroidController asteroid in activeAsteroids)
        {
            if (asteroid != null)
            {
                asteroid.SetSpeedMultiplier(snowSpeedMultiplier);
            }
        }
    }

    private void ApplySnowMultiplierToCollectibles()
    {
        foreach (CollectibleController collectible in activeCollectibles)
        {
            if (collectible != null)
            {
                collectible.SetSpeedMultiplier(snowSpeedMultiplier);
            }
        }
    }

    private void ApplySnowMultiplierToPowerUps()
    {
        foreach (PowerUpController powerUp in activePowerUps)
        {
            if (powerUp != null)
            {
                powerUp.SetSpeedMultiplier(snowSpeedMultiplier);
            }
        }
    }

    private void ResetSnowMultiplierForAsteroids()
    {
        foreach (AsteroidController asteroid in activeAsteroids)
        {
            if (asteroid != null)
            {
                asteroid.ResetSpeedMultiplier();
            }
        }
    }

    private void ResetSnowMultiplierForCollectibles()
    {
        foreach (CollectibleController collectible in activeCollectibles)
        {
            if (collectible != null)
            {
                collectible.ResetSpeedMultiplier();
            }
        }
    }

    private void ResetSnowMultiplierForPowerUps()
    {
        foreach (PowerUpController powerUp in activePowerUps)
        {
            if (powerUp != null)
            {
                powerUp.ResetSpeedMultiplier();
            }
        }
    }

}
