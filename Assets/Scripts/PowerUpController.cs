using UnityEngine;

public enum PowerUpType
{
    Magnet,
    Missile,
    Invisibility,
    Snow,
    Dynamite
}

public class PowerUpController : MonoBehaviour
{
    [SerializeField] private PowerUpType powerUpType = PowerUpType.Magnet;
    [SerializeField, Min(0f)] private float duration = 5f;
    [SerializeField] private float horizontalSpeed = 5f;
    [SerializeField] private float despawnX = -20f;

    private bool collected;
    private float currentSpeedMultiplier = 1f;

    private void OnEnable()
    {
        collected = false;
        currentSpeedMultiplier = 1f;
        PowerUpManager.Instance?.RegisterPowerUp(this);
    }

    private void OnDisable()
    {
        PowerUpManager.Instance?.UnregisterPowerUp(this);
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        Vector3 position = transform.position;
        position.x -= horizontalSpeed * currentSpeedMultiplier * Time.deltaTime;
        transform.position = position;

        if (transform.position.x <= despawnX)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected ||
            !other.CompareTag("Player") ||
            GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameState.Playing ||
            PowerUpManager.Instance == null)
        {
            return;
        }

        if (!PowerUpManager.Instance.TryActivate(powerUpType, duration))
        {
            return;
        }

        AudioManager.Instance?.PlaySFX(SfxType.CollectPowerUp);
        collected = true;

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

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ResetSpeedMultiplier()
    {
        currentSpeedMultiplier = 1f;
    }
}
