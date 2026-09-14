using UnityEngine;

public enum CollectibleType
{
    Star,
    Croissant,
    BonusCollectible
}

public class CollectibleController : MonoBehaviour
{
    [SerializeField] private CollectibleType collectibleType;
    [SerializeField] private float horizontalSpeed = 5f;
    [SerializeField, Min(0)] private int points = 1;
    [SerializeField] private float despawnX = -20f;
    [SerializeField] private bool canBeAttracted = true;
    [SerializeField] private float attractionSpeed = 8f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private Transform playerTarget;

    private bool isBeingAttracted;
    private bool collected;
    private float currentSpeedMultiplier = 1f;
    private bool rotationEnabled;
    private PlayerController playerController;

    private void Awake()
    {
        playerController = FindFirstObjectByType<PlayerController>();
    }

    private void OnEnable()
    {
        collected = false;
        isBeingAttracted = false;
        currentSpeedMultiplier = 1f;
        rotationEnabled = collectibleType == CollectibleType.Croissant;

        PowerUpManager.Instance?.RegisterCollectible(this);
    }

    private void OnDisable()
    {
        ClearMagnetTarget();
        PowerUpManager.Instance?.UnregisterCollectible(this);
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        Vector3 position = transform.position;

        if (canBeAttracted &&
            isBeingAttracted &&
            playerTarget != null)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                playerTarget.position,
                attractionSpeed * Time.deltaTime);
        }
        else if (canBeAttracted &&
                 playerController != null)
        {
            Vector2 difference =
                playerController.transform.position -
                transform.position;

            float magnetRange =
                playerController.PassiveMagnetRange;

            if (difference.sqrMagnitude <= magnetRange * magnetRange)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    playerController.transform.position,
                    playerController.PassiveMagnetSpeed * Time.deltaTime);
            }
            else
            {
                position.x -=
                    horizontalSpeed *
                    currentSpeedMultiplier *
                    Time.deltaTime;

                transform.position = position;
            }
        }
        else
        {
            position.x -=
                horizontalSpeed *
                currentSpeedMultiplier *
                Time.deltaTime;

            transform.position = position;
        }

        if (rotationEnabled)
        {
            transform.Rotate(
                Vector3.forward,
                rotationSpeed * Time.deltaTime);
        }

        if (transform.position.x <= despawnX)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetMagnetTarget(Transform target)
    {
        playerTarget = target;
        isBeingAttracted = target != null;
    }

    public void ClearMagnetTarget()
    {
        playerTarget = null;
        isBeingAttracted = false;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ResetSpeedMultiplier()
    {
        currentSpeedMultiplier = 1f;
    }

    public void SetRotationEnabled(bool enabled)
    {
        rotationEnabled = enabled;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected ||
            GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameState.Playing ||
            !other.CompareTag("Player"))
        {
            return;
        }

        if (ScoreManager.Instance == null)
        {
            Debug.LogError("Collectible requires a ScoreManager.", this);
            return;
        }

        collected = true;
        if (collectibleType == CollectibleType.Star)
        {
            ScoreManager.Instance.AddStar(points);
        }
        else if (collectibleType == CollectibleType.Croissant)
        {
            ScoreManager.Instance.AddCroissant(points);
        }
        else
        {
            ScoreManager.Instance.AddBonusCollectible(points);
        }

        AudioManager.Instance?.PlaySFX(
            collectibleType == CollectibleType.Star
                ? SfxType.CollectStar
                : SfxType.CollectCroissant);

        if (PoolManager.Instance != null)
        {
            GameObject collectEffect =
                PoolManager.Instance.Get(PoolId.CollectEffect);

            if (collectEffect != null)
            {
                collectEffect.transform.position =
                    transform.position;

                collectEffect.transform.rotation =
                    Quaternion.identity;

                collectEffect.SetActive(true);
            }
        }

        gameObject.SetActive(false);
    }
}
