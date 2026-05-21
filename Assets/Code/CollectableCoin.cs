using UnityEngine;
using System;

public class CollectableCoin : MonoBehaviour
{
    [SerializeField] private string coinId;
    [SerializeField, Range(0f, 1f)] private float collectedAlpha = 0.45f;
    [SerializeField] private float carriedBehindDistance = 0.55f;
    [SerializeField] private float carriedHeight = 0.35f;
    [SerializeField] private Vector2 carriedExtraOffset = new Vector2(0.18f, 0.12f);
    [SerializeField] private float followStopDistance = 0.18f;
    [SerializeField] private float followFullSpeedDistance = 1.6f;
    [SerializeField] private float followMaxSpeed = 7f;
    [SerializeField] private int collectDustCount = 18;
    [SerializeField] private float collectDustSize = 0.07f;
    [SerializeField] private float collectDustGravityScale = 1.2f;
    [SerializeField] private Vector2 collectDustHorizontalSpeed = new Vector2(-1.8f, 1.8f);
    [SerializeField] private Vector2 collectDustVerticalSpeed = new Vector2(1f, 3.2f);
    [SerializeField] private float collectDustLifetime = 1f;
    [SerializeField] private Color collectDustColorA = new Color(1f, 0.86f, 0.22f, 1f);
    [SerializeField] private Color collectDustColorB = new Color(1f, 0.58f, 0.08f, 1f);
    [SerializeField] private Color collectDustColorC = Color.white;

    public string CoinId => coinId;

    private Collider2D coinCollider;
    private SpriteRenderer[] spriteRenderers;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private bool isCarried;
    private Transform followTarget;
    private Vector3 followOffset;
    private static Sprite dustSprite;

    private void Awake()
    {
        coinCollider = GetComponent<Collider2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;

        if (coinCollider != null)
        {
            coinCollider.isTrigger = true;
        }
    }

    private void Start()
    {
        ApplySavedVisualState();
    }

    private void Update()
    {
        if (!isCarried || followTarget == null)
        {
            return;
        }

        Vector3 targetPosition = followTarget.position + followOffset;
        Vector3 toTarget = targetPosition - transform.position;
        float distance = toTarget.magnitude;

        if (distance <= followStopDistance)
        {
            return;
        }

        float speedScale = Mathf.InverseLerp(followStopDistance, followFullSpeedDistance, distance);
        float step = followMaxSpeed * speedScale * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
    }

    private void Reset()
    {
        EnsureId();

        Collider2D currentCollider = GetComponent<Collider2D>();
        if (currentCollider != null)
        {
            currentCollider.isTrigger = true;
        }
    }

    private void OnValidate()
    {
        EnsureId();

        Collider2D currentCollider = GetComponent<Collider2D>();
        if (currentCollider != null)
        {
            currentCollider.isTrigger = true;
        }
    }

    public void EnsureId()
    {
        if (string.IsNullOrWhiteSpace(coinId))
        {
            RegenerateId();
        }
    }

    public void RegenerateId()
    {
        coinId = $"coin_{Guid.NewGuid():N}";
    }

    public void PickUp(Transform followTarget, int carriedIndex)
    {
        if (isCarried || followTarget == null)
        {
            return;
        }

        isCarried = true;
        this.followTarget = followTarget;
        UpdateCarrySlot(1, carriedIndex);

        if (coinCollider != null)
        {
            coinCollider.enabled = false;
        }
    }

    public void UpdateCarrySlot(int facingDirection, int carriedIndex)
    {
        int direction = facingDirection >= 0 ? 1 : -1;
        float behindX = -direction * carriedBehindDistance;
        Vector2 extraOffset = carriedExtraOffset * carriedIndex;
        extraOffset.x *= -direction;
        followOffset = new Vector3(behindX + extraOffset.x, carriedHeight + extraOffset.y, 0f);
    }

    public void ReturnToOriginalPosition()
    {
        isCarried = false;
        followTarget = null;
        followOffset = Vector3.zero;
        transform.SetParent(originalParent, true);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        if (coinCollider != null)
        {
            coinCollider.enabled = true;
        }

        ApplySavedVisualState();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerCoinCollector collector = other.GetComponentInParent<PlayerCoinCollector>();

        if (collector != null)
        {
            collector.TryPickUp(this);
        }
    }

    private void ApplySavedVisualState()
    {
        bool alreadyCollected = LevelManager.Instance != null && LevelManager.Instance.IsCoinCollected(coinId);
        float alpha = alreadyCollected ? collectedAlpha : 1f;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    public void CompleteCollection()
    {
        SpawnCollectDust();
        isCarried = false;
        followTarget = null;
        followOffset = Vector3.zero;
        transform.SetParent(originalParent, true);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        if (coinCollider != null)
        {
            coinCollider.enabled = true;
        }

        gameObject.SetActive(false);
    }

    private void SpawnCollectDust()
    {
        Bounds bounds = GetVisualBounds();

        for (int i = 0; i < collectDustCount; i++)
        {
            Vector3 position = new Vector3(
                UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                transform.position.z
            );

            GameObject dust = new GameObject("Coin Collect Dust");
            dust.transform.position = position;
            dust.transform.localScale = Vector3.one * collectDustSize;

            SpriteRenderer dustRenderer = dust.AddComponent<SpriteRenderer>();
            dustRenderer.sprite = GetDustSprite();
            dustRenderer.color = GetCollectDustColor();

            if (spriteRenderers.Length > 0 && spriteRenderers[0] != null)
            {
                dustRenderer.sortingLayerID = spriteRenderers[0].sortingLayerID;
                dustRenderer.sortingOrder = spriteRenderers[0].sortingOrder + 1;
            }

            Rigidbody2D dustRb = dust.AddComponent<Rigidbody2D>();
            dustRb.gravityScale = collectDustGravityScale;
            dustRb.linearVelocity = new Vector2(
                UnityEngine.Random.Range(collectDustHorizontalSpeed.x, collectDustHorizontalSpeed.y),
                UnityEngine.Random.Range(collectDustVerticalSpeed.x, collectDustVerticalSpeed.y)
            );
            dustRb.angularVelocity = UnityEngine.Random.Range(-360f, 360f);

            Destroy(dust, collectDustLifetime);
        }
    }

    private Bounds GetVisualBounds()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                return spriteRenderers[i].bounds;
            }
        }

        return new Bounds(transform.position, Vector3.one * 0.2f);
    }

    private Color GetCollectDustColor()
    {
        int colorIndex = UnityEngine.Random.Range(0, 3);

        if (colorIndex == 0)
        {
            return collectDustColorA;
        }

        return colorIndex == 1 ? collectDustColorB : collectDustColorC;
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
}
