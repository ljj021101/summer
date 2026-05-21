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

    [Header("Save Icon")]
    [SerializeField] private Sprite saveIconSprite;
    [SerializeField] private Vector3 saveIconLocalOffset = new Vector3(0f, 1.25f, 0f);
    [SerializeField] private float saveIconScale = 1f;
    [SerializeField] private float saveIconVisibleDuration = 0.45f;
    [SerializeField] private float saveIconFadeDuration = 0.25f;
    [SerializeField] private Color saveIconColor = Color.white;

    private Rigidbody2D rb;
    private PlatformerPlayerController playerController;
    private PlayerCoinCollector coinCollector;
    private Checkpoint currentCheckpoint;
    private Vector3 fallbackRespawnPosition;
    private float respawnZPosition;
    private Renderer[] visualRenderers;
    private static Sprite dustSprite;
    private static Sprite generatedSaveIconSprite;
    private SpriteRenderer saveIconRenderer;
    private Coroutine saveIconRoutine;
    private bool isRespawning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlatformerPlayerController>();
        coinCollector = GetComponent<PlayerCoinCollector>();

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

    public bool SetCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null)
        {
            return false;
        }

        bool changedCheckpoint = currentCheckpoint == null || currentCheckpoint != checkpoint;
        currentCheckpoint = checkpoint;
        return changedCheckpoint;
    }

    public void ShowSaveIcon()
    {
        PlaySaveIcon();
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

        if (coinCollector != null)
        {
            coinCollector.ReturnCarriedCoins();
        }

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

    private void PlaySaveIcon()
    {
        EnsureSaveIconRenderer();

        if (saveIconRenderer == null)
        {
            return;
        }

        if (saveIconRoutine != null)
        {
            StopCoroutine(saveIconRoutine);
        }

        saveIconRoutine = StartCoroutine(ShowSaveIconRoutine());
    }

    private IEnumerator ShowSaveIconRoutine()
    {
        saveIconRenderer.enabled = true;
        SetSaveIconAlpha(1f);

        if (saveIconVisibleDuration > 0f)
        {
            yield return new WaitForSeconds(saveIconVisibleDuration);
        }

        if (saveIconFadeDuration > 0f)
        {
            float elapsed = 0f;

            while (elapsed < saveIconFadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(elapsed / saveIconFadeDuration);
                SetSaveIconAlpha(alpha);
                yield return null;
            }
        }

        SetSaveIconAlpha(0f);
        saveIconRenderer.enabled = false;
        saveIconRoutine = null;
    }

    private void EnsureSaveIconRenderer()
    {
        if (saveIconRenderer != null)
        {
            return;
        }

        GameObject iconObject = new GameObject("Save Icon");
        iconObject.transform.SetParent(transform, false);
        iconObject.transform.localPosition = saveIconLocalOffset;
        iconObject.transform.localScale = Vector3.one * GetSaveIconScale();

        saveIconRenderer = iconObject.AddComponent<SpriteRenderer>();
        saveIconRenderer.sprite = saveIconSprite != null ? saveIconSprite : GetGeneratedSaveIconSprite();
        saveIconRenderer.enabled = false;

        if (spriteRenderer != null)
        {
            saveIconRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            saveIconRenderer.sortingOrder = spriteRenderer.sortingOrder + 3;
        }

        SetSaveIconAlpha(0f);
    }

    private float GetSaveIconScale()
    {
        if (saveIconSprite != null)
        {
            return saveIconScale;
        }

        return Mathf.Max(0.75f, saveIconScale);
    }

    private void SetSaveIconAlpha(float alpha)
    {
        if (saveIconRenderer == null)
        {
            return;
        }

        Color color = saveIconColor;
        color.a *= alpha;
        saveIconRenderer.color = color;
    }

    private static Sprite GetGeneratedSaveIconSprite()
    {
        if (generatedSaveIconSprite != null)
        {
            return generatedSaveIconSprite;
        }

        Texture2D texture = new Texture2D(8, 8);
        texture.filterMode = FilterMode.Point;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color body = Color.white;
        Color detail = new Color(0.2f, 0.25f, 0.35f, 1f);

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        for (int y = 1; y < 7; y++)
        {
            for (int x = 1; x < 7; x++)
            {
                texture.SetPixel(x, y, body);
            }
        }

        texture.SetPixel(6, 6, clear);

        for (int x = 2; x < 6; x++)
        {
            texture.SetPixel(x, 5, detail);
        }

        texture.SetPixel(5, 5, body);

        for (int x = 2; x < 6; x++)
        {
            texture.SetPixel(x, 2, detail);
        }

        texture.SetPixel(2, 3, detail);
        texture.SetPixel(5, 3, detail);
        texture.Apply();

        generatedSaveIconSprite = Sprite.Create(texture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 8f);
        return generatedSaveIconSprite;
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
