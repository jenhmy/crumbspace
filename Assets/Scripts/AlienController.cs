using System.Collections;
using UnityEngine;

public class AlienController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D alienCollider;
    [SerializeField] private GameObject thruster;

    [Header("Movimiento")]
    [SerializeField] private float followSpeed = 1.5f;
    [SerializeField] private float distanceFromLeftEdge = 1.5f;
    [SerializeField] private float entryOffset = 2.5f;
    [SerializeField] private float entrySpeed = 2f;
    [SerializeField] private float swayAngle = 6f;
    [SerializeField] private float swaySpeed = 2.5f;
    [SerializeField, Range(0f, 1f)] private float entryAlpha = 0.3f;

    [Header("Muerte")]
    [SerializeField] private float respawnTime = 2f;
    [SerializeField, Min(0)] private int points = 25;

    private bool isEntering;
    private bool isDead;
    private float swayTime;
    private float normalAlpha = 1f;
    private Coroutine respawnCoroutine;
    private ParticleSystem thrusterParticles;

    private float positionX;
    private float entryStartX;

    private void Awake()
    {
        if (spriteRenderer != null)
        {
            normalAlpha = spriteRenderer.color.a;
        }

        if (thruster != null)
        {
            thrusterParticles =
                thruster.GetComponentInChildren<ParticleSystem>();
        }
    }

    private void StartEntry()
    {
        Camera cam = Camera.main;

        if (cam != null)
        {
            float leftEdge = cam.ViewportToWorldPoint(
                new Vector3(0f, 0.5f, 0f)).x;

            positionX = leftEdge + distanceFromLeftEdge;
            entryStartX = leftEdge - entryOffset;
        }

        isDead = false;
        isEntering = true;
        swayTime = 0f;

        Vector3 position = transform.position;
        position.x = entryStartX;
        transform.position = position;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        SetColliderEnabled(false);
        SetThrusterEnabled(true);
        SetAlpha(entryAlpha);
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameState.Playing ||
            isDead)
        {
            return;
        }

        swayTime += Time.deltaTime * swaySpeed;

        if (isEntering)
        {
            MoveIntoPosition();
        }
        else
        {
            FollowPlayerVertically();
        }

        ApplySway();
    }

    private void MoveIntoPosition()
    {
        Vector3 position = transform.position;
        position.x = Mathf.MoveTowards(
            position.x,
            positionX,
            entrySpeed * Time.deltaTime);
        transform.position = position;

        FollowPlayerVertically();

        if (Mathf.Approximately(position.x, positionX))
        {
            isEntering = false;
            SetColliderEnabled(true);
            SetAlpha(normalAlpha);
        }
    }

    private void FollowPlayerVertically()
    {
        if (playerTransform == null)
        {
            return;
        }

        Vector3 position = transform.position;
        position.y = Mathf.MoveTowards(
            position.y,
            playerTransform.position.y,
            followSpeed * Time.deltaTime);
        transform.position = position;
    }

    private void ApplySway()
    {
        Vector3 rotation = transform.eulerAngles;
        rotation.z = Mathf.Sin(swayTime) * swayAngle;
        transform.eulerAngles = rotation;
    }

    public void KillAlien()
    {
        if (isDead ||
            GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        isDead = true;
        isEntering = false;
        ExplodeVisual();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddAlienKill(points);
        }

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }

        respawnCoroutine = StartCoroutine(RespawnAfterDelay());
    }

    public void ExplodeWithoutScore()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        isEntering = false;

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }

        ExplodeVisual();
    }

    private void ExplodeVisual()
    {
        AudioManager.Instance?.PlaySFX(SfxType.AlienExplosion);
        SetColliderEnabled(false);
        SetThrusterEnabled(false);

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        if (PoolManager.Instance != null)
        {
            GameObject explosion =
                PoolManager.Instance.Get(PoolId.AlienExplosion);

            if (explosion != null)
            {
                explosion.transform.position = transform.position;
                explosion.transform.rotation = Quaternion.identity;
                explosion.SetActive(true);
            }
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        float elapsed = 0f;

        while (elapsed < respawnTime)
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameState.Playing)
            {
                elapsed += Time.deltaTime;
            }

            yield return null;
        }

        respawnCoroutine = null;
        StartEntry();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Asteroid"))
        {
            KillAlien();
        }
    }

    private void SetColliderEnabled(bool isEnabled)
    {
        if (alienCollider != null)
        {
            alienCollider.enabled = isEnabled;
        }
    }

    private void SetThrusterEnabled(bool isEnabled)
    {
        if (thruster == null)
        {
            return;
        }

        thruster.SetActive(isEnabled);

        if (thrusterParticles != null)
        {
            if (isEnabled)
            {
                thrusterParticles.Play();
            }
            else
            {
                thrusterParticles.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    public void BeginGame()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }

        StartEntry();
    }
}
