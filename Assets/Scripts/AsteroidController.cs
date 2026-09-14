using UnityEngine;

public class AsteroidController : MonoBehaviour
{
    [SerializeField] private float horizontalSpeed = 5f;
    [SerializeField] private bool verticalMovement;
    [SerializeField] private float verticalSpeed = 1f;
    [SerializeField] private float verticalRange = 1f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private bool randomizeRotationDirection = true;
    [SerializeField] private float despawnX = -20f;
    [SerializeField] private float speedTransition = 2f;

    private float baseY;
    private float verticalPhase;
    private float rotationDirection;
    private float currentSpeedMultiplier = 1f;
    private float targetSpeedMultiplier = 1f;
    private float bonusSpeedMultiplier = 1f;

    private void OnEnable()
    {
        currentSpeedMultiplier = 1f;
        targetSpeedMultiplier = 1f;
        bonusSpeedMultiplier = 1f;

        baseY = transform.position.y;
        verticalPhase = 0f;

        rotationDirection =
            randomizeRotationDirection && Random.value < 0.5f
                ? -1f
                : 1f;

        transform.rotation =
            Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.RegisterAsteroid(this);
        }
    }

    private void OnDisable()
    {
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.UnregisterAsteroid(this);
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        float deltaTime = Time.deltaTime;

        currentSpeedMultiplier = Mathf.MoveTowards(
            currentSpeedMultiplier,
            targetSpeedMultiplier,
            speedTransition * deltaTime
        );

        Vector3 position = transform.position;
        position.x -=
            horizontalSpeed *
            currentSpeedMultiplier *
            bonusSpeedMultiplier *
            deltaTime;

        if (verticalMovement)
        {
            verticalPhase +=
                verticalSpeed *
                currentSpeedMultiplier *
                bonusSpeedMultiplier *
                deltaTime;

            position.y =
                baseY +
                Mathf.Sin(verticalPhase) * verticalRange;
        }

        transform.position = position;

        transform.Rotate(
            Vector3.forward,
            rotationSpeed * rotationDirection * deltaTime
        );

        if (transform.position.x <= despawnX)
        {
            gameObject.SetActive(false);
        }
    }

    public void Explode()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        AudioManager.Instance?.PlaySFX(SfxType.AsteroidExplosion);

        if (PoolManager.Instance != null)
        {
            GameObject explosion =
                PoolManager.Instance.Get(PoolId.AsteroidExplosion);

            if (explosion != null)
            {
                explosion.transform.position = transform.position;
                explosion.transform.rotation = Quaternion.identity;
                explosion.SetActive(true);
            }
        }

        gameObject.SetActive(false);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        targetSpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ResetSpeedMultiplier()
    {
        targetSpeedMultiplier = 1f;
    }

    public void SetBonusSpeedMultiplier(float multiplier)
    {
        bonusSpeedMultiplier = Mathf.Max(0.1f, multiplier);
    }
}