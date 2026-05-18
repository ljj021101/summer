using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class Signpost : MonoBehaviour
{
    [TextArea(2, 5)]
    [SerializeField] private string message = "Hello!";
    [SerializeField] private string playerTag = "Player";

    [Header("Text")]
    [SerializeField] private TextMesh textMesh;
    [SerializeField] private Vector2 textLocalOffset = new Vector2(0f, 1.5f);
    [SerializeField] private float characterSize = 0.18f;
    [SerializeField] private int fontSize = 32;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private float fadeDuration = 0.18f;

    [Header("Gizmo")]
    [SerializeField] private Color gizmoColor = new Color(0.1f, 0.85f, 1f, 0.22f);

    private readonly HashSet<Transform> playersInRange = new HashSet<Transform>();
    private BoxCollider2D signCollider;
    private Coroutine fadeRoutine;

    private BoxCollider2D SignCollider
    {
        get
        {
            if (signCollider == null)
            {
                signCollider = GetComponent<BoxCollider2D>();
            }

            return signCollider;
        }
    }

    private void Reset()
    {
        signCollider = GetComponent<BoxCollider2D>();
        signCollider.isTrigger = true;
        signCollider.size = new Vector2(2f, 2f);

        EnsureTextMesh();
        ApplyTextSettings();
        SetTextAlpha(0f);
    }

    private void Awake()
    {
        EnsureTextMesh();
        ApplyTextSettings();
        SetTextAlpha(0f);
    }

    private void OnValidate()
    {
        BoxCollider2D currentCollider = GetComponent<BoxCollider2D>();

        if (currentCollider != null)
        {
            currentCollider.isTrigger = true;
        }

        if (textMesh != null)
        {
            ApplyTextSettings();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Transform player = GetPlayerRoot(other);

        if (player == null || !playersInRange.Add(player))
        {
            return;
        }

        ShowText();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Transform player = GetPlayerRoot(other);

        if (player == null)
        {
            return;
        }

        playersInRange.Remove(player);

        if (playersInRange.Count == 0)
        {
            HideText();
        }
    }

    private Transform GetPlayerRoot(Collider2D other)
    {
        PlatformerPlayerController playerController = other.GetComponentInParent<PlatformerPlayerController>();

        if (playerController != null)
        {
            return playerController.transform;
        }

        return other.CompareTag(playerTag) ? other.transform : null;
    }

    private void ShowText()
    {
        EnsureTextMesh();
        ApplyTextSettings();
        FadeTextTo(1f);
    }

    private void HideText()
    {
        FadeTextTo(0f);
    }

    private void FadeTextTo(float targetAlpha)
    {
        if (textMesh == null)
        {
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeTextRoutine(targetAlpha));
    }

    private IEnumerator FadeTextRoutine(float targetAlpha)
    {
        float startAlpha = textMesh.color.a;

        if (fadeDuration <= 0f)
        {
            SetTextAlpha(targetAlpha);
            fadeRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeDuration);
            SetTextAlpha(Mathf.Lerp(startAlpha, targetAlpha, progress));
            yield return null;
        }

        SetTextAlpha(targetAlpha);
        fadeRoutine = null;
    }

    private void EnsureTextMesh()
    {
        if (textMesh != null)
        {
            return;
        }

        Transform existingText = transform.Find("SignText");

        if (existingText != null)
        {
            textMesh = existingText.GetComponent<TextMesh>();
        }

        if (textMesh != null)
        {
            return;
        }

        GameObject textObject = new GameObject("SignText");
        textObject.transform.SetParent(transform);
        textObject.transform.localPosition = textLocalOffset;
        textObject.transform.localRotation = Quaternion.identity;
        textMesh = textObject.AddComponent<TextMesh>();
    }

    private void ApplyTextSettings()
    {
        textMesh.text = message;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = characterSize;
        textMesh.fontSize = fontSize;
        textMesh.transform.localPosition = textLocalOffset;

        Renderer textRenderer = textMesh.GetComponent<Renderer>();

        if (textRenderer != null)
        {
            textRenderer.sortingOrder = 100;
        }
    }

    private void SetTextAlpha(float alpha)
    {
        Color currentColor = textColor;
        currentColor.a = alpha;
        textMesh.color = currentColor;
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D currentCollider = GetComponent<BoxCollider2D>();

        if (currentCollider == null)
        {
            return;
        }

        Bounds bounds = currentCollider.bounds;
        Color previousColor = Gizmos.color;

        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(bounds.center, bounds.size);

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.TransformPoint(textLocalOffset), 0.12f);

        Gizmos.color = previousColor;
    }
}
