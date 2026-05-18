using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCheckpointController : MonoBehaviour
{
    [SerializeField] private Checkpoint startingCheckpoint;
    [SerializeField] private bool useInitialPositionAsFallback = true;

    [Header("Death Effect")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private int dustCount = 28;
    [SerializeField] private float dustSize = 0.08f;
    [SerializeField] private float dustGravityScale = 1.6f;
    [SerializeField] private Vector2 dustHorizontalSpeed = new Vector2(-2.5f, 2.5f);
    [SerializeField] private Vector2 dustVerticalSpeed = new Vector2(1.5f, 4f);
    [SerializeField] private float dustLifetime = 1.4f;
    [SerializeField] private float respawnDelay = 0.45f;
    [SerializeField] private Color dustColorA = new Color(0.72f, 0.6f, 0.75f, 1f);
    [SerializeField] private Color dustColorB = new Color(0.28f, 0.18f, 0.32f, 1f);
    [SerializeField] private Color dustColorC = Color.white;

    private Rigidbody2D rb;
    private PlatformerPlayerController playerController;
    private Checkpoint currentCheckpoint;
    private Vector3 fallbackRespawnPosition;
    private float respawnZPosition;
    private Renderer[] visualRenderers;
    private static Sprite dustSprite;
    private bool isRespawning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlatformerPlayerController>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (visualRoot == null && spriteRenderer != null)
        {
            visualRoot = spriteRenderer.transform;
        }

        visualRenderers = visualRoot != null ?
            visualRoot.GetComponentsInChildren<Renderer>() :
            GetComponentsInChildren<Renderer>();

        fallbackRespawnPosition = transform.position;
        respawnZPosition = transform.position.z;

        if (startingCheckpoint != null)
        {
            currentCheckpoint = startingCheckpoint;
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RespawnWithEffect();
        }
    }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null)
        {
            return;
        }

        currentCheckpoint = checkpoint;
    }

    public void Die()
    {
        RespawnWithEffect();
    }

    public void Respawn()
    {
        RespawnAtCheckpoint();
    }

    public void RespawnWithEffect()
    {
        if (isRespawning)
        {
            return;
        }

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        SpawnDeathDust();
        SetPlayerActive(false);

        yield return new WaitForSeconds(respawnDelay);

        RespawnAtCheckpoint();
        SetPlayerActive(true);

        isRespawning = false;
    }

    private void RespawnAtCheckpoint()
    {
        Vector3 respawnPosition = GetRespawnPosition();
        respawnPosition.z = respawnZPosition;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = respawnPosition;
    }

    private void SetPlayerActive(bool active)
    {
        if (playerController != null)
        {
            playerController.enabled = active;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = active;

        foreach (Renderer visualRenderer in visualRenderers)
        {
            if (visualRenderer != null)
            {
                visualRenderer.enabled = active;
            }
        }
    }

    private void SpawnDeathDust()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Bounds bounds = spriteRenderer.bounds;

        for (int i = 0; i < dustCount; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                transform.position.z
            );

            GameObject dust = new GameObject("Death Dust");
            dust.transform.position = position;
            dust.transform.localScale = Vector3.one * dustSize;

            SpriteRenderer dustRenderer = dust.AddComponent<SpriteRenderer>();
            dustRenderer.sprite = GetDustSprite();
            dustRenderer.color = GetDustColor();
            dustRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            dustRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;

            Rigidbody2D dustRb = dust.AddComponent<Rigidbody2D>();
            dustRb.gravityScale = dustGravityScale;
            dustRb.linearVelocity = new Vector2(
                Random.Range(dustHorizontalSpeed.x, dustHorizontalSpeed.y),
                Random.Range(dustVerticalSpeed.x, dustVerticalSpeed.y)
            );
            dustRb.angularVelocity = Random.Range(-360f, 360f);

            Destroy(dust, dustLifetime);
        }
    }

    private Color GetDustColor()
    {
        int colorIndex = Random.Range(0, 3);

        if (colorIndex == 0)
        {
            return dustColorA;
        }

        return colorIndex == 1 ? dustColorB : dustColorC;
    }

    private static Sprite GetDustSprite()
    {
        if (dustSprite != null)
        {
            return dustSprite;
        }

        Texture2D texture = new Texture2D(1, 1);
        texture.filterMode = FilterMode.Point;
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        dustSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 16f);
        return dustSprite;
    }

    private Vector3 GetRespawnPosition()
    {
        if (currentCheckpoint != null)
        {
            return currentCheckpoint.RespawnPosition;
        }

        return useInitialPositionAsFallback ? fallbackRespawnPosition : transform.position;
    }
}
