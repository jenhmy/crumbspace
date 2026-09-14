using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private enum DeathCause
    {
        Asteroid,
        DeathZone
    }

    [Header("Movimiento")]
    [SerializeField] private float flapForce = 4f;

    [Header("Referencias")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private AlienController alienController;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject thruster;
    [SerializeField] private float alienExplosionDelay = 1f;
    [SerializeField] private float maxY = 4.5f;
    [SerializeField] private float distanceFromLeftEdge = 4f;

    [Header("Imán pasivo")]
    [SerializeField, Min(0f)] private float passiveMagnetRange = 1f;
    [SerializeField, Min(0f)] private float passiveMagnetSpeed = 4f;

    public float PassiveMagnetRange => passiveMagnetRange;
    public float PassiveMagnetSpeed => passiveMagnetSpeed;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private bool isDead;
    private bool isInvisible;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            playerInput.actions.Enable();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        rb.simulated = false;
    }

    private void Start()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState == GameState.Playing)
        {
            BeginGame();
        }
    }

    private void Update()
    {
        if (isDead ||
            GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        if (playerInput != null &&
            playerInput.actions["Interact"].WasPressedThisFrame())
        {
            Flap();
        }
    }

    private void FixedUpdate()
    {
        Vector3 position = transform.position;

        if (position.y > maxY)
        {
            position.y = maxY;
            transform.position = position;

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Min(rb.linearVelocity.y, 0f)
            );
        }        
    }

    public void BeginGame()
    {
        Camera cam = Camera.main;

        if (cam != null)
        {
            float leftEdge = cam.ViewportToWorldPoint(
                new Vector3(0f, 0.5f, 0f)).x;

            Vector3 position = transform.position;
            position.x = leftEdge + distanceFromLeftEdge;
            transform.position = position;
        }

        isDead = false;
        isInvisible = false;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;

            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        if (thruster != null)
        {
            thruster.SetActive(true);

            ParticleSystem particles =
                thruster.GetComponentInChildren<ParticleSystem>();

            if (particles != null)
            {
                particles.Play();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead ||
            GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        if (other.CompareTag("Asteroid"))
        {
            if (!isInvisible)
            {
                Die(DeathCause.Asteroid);
            }
        }
        else if (other.CompareTag("DeathZone"))
        {
            Die(DeathCause.DeathZone);
        }
    }

    private void Die(DeathCause cause)
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        rb.simulated = false;
        AudioManager.Instance?.PlaySFX(SfxType.PlayerExplosion);
        AudioManager.Instance?.PlaySFX(SfxType.Death);

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        if (thruster != null)
        {
            thruster.SetActive(false);
        }

        if (PoolManager.Instance != null)
        {
            GameObject explosion =
                PoolManager.Instance.Get(PoolId.PlayerExplosion);

            if (explosion != null)
            {
                explosion.transform.position = transform.position;
                explosion.transform.rotation = Quaternion.identity;
                explosion.SetActive(true);
            }
        }

        if (alienController != null)
        {
            StartCoroutine(ExplodeAlienAfterDelay());
        }

        if (uiManager != null)
        {
            if (cause == DeathCause.DeathZone)
            {
                uiManager.ShowFeedback(
                    LocalizationManager.Get("DEATH_ZONE"));
            }
            else
            {
                uiManager.ShowFeedback(
                    LocalizationManager.Get("DEATH_ASTEROID"));
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    private IEnumerator ExplodeAlienAfterDelay()
    {
        yield return new WaitForSeconds(alienExplosionDelay);

        if (alienController != null)
        {
            alienController.ExplodeWithoutScore();
        }
    }

    private void Flap()
    {
        rb.linearVelocity =
            new Vector2(rb.linearVelocity.x, 0f);

        rb.AddForce(
            Vector2.up * flapForce,
            ForceMode2D.Impulse
        );
    }

    public void SetInvisible(bool invisible)
    {
        isInvisible = invisible;

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = invisible ? 0.35f : 1f;
            spriteRenderer.color = color;
        }
    }

    public void StopGameplay()
    {
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
    }
}