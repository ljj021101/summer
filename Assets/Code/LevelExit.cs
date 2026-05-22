using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string levelSelectSceneName = "Level Select";

    [Header("Exit Effect")]
    [SerializeField] private float swirlDuration = 1.2f;
    [SerializeField] private float swirlStartSpinSpeed = 180f;
    [SerializeField] private float swirlEndSpinSpeed = 1080f;
    [SerializeField, Range(0f, 1f)] private float swirlFinalScale = 0.05f;
    [SerializeField] private float returnDelay = 0.35f;
    [SerializeField] private Color gizmoColor = new Color(0.5f, 0.9f, 1f, 0.25f);

    private bool isCompleting;

    private void Reset()
    {
        Collider2D exitCollider = GetComponent<Collider2D>();
        if (exitCollider != null)
        {
            exitCollider.isTrigger = true;
        }
    }

    private void OnValidate()
    {
        Collider2D exitCollider = GetComponent<Collider2D>();
        if (exitCollider != null)
        {
            exitCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCompleting)
        {
            return;
        }

        PlatformerPlayerController playerController = other.GetComponentInParent<PlatformerPlayerController>();
        bool isPlayer = playerController != null || other.CompareTag(playerTag);

        if (!isPlayer)
        {
            return;
        }

        Transform player = playerController != null ? playerController.transform : other.transform;
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        PlayerCheckpointController checkpointController = player.GetComponent<PlayerCheckpointController>();
        Animator playerAnimator = player.GetComponentInChildren<Animator>();

        StartCoroutine(CompleteLevelRoutine(player, playerController, checkpointController, playerRb, playerAnimator, GetSwirlCenter()));
    }

    private IEnumerator CompleteLevelRoutine(
        Transform player,
        PlatformerPlayerController playerController,
        PlayerCheckpointController checkpointController,
        Rigidbody2D playerRb,
        Animator playerAnimator,
        Vector3 swirlCenter)
    {
        isCompleting = true;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RefreshCoinTotalFromScene();
            LevelManager.Instance.MarkCompleted();
        }

        GameProgressStore.SaveToPlayerPrefs();

        if (RoomCameraController.Instance != null)
        {
            RoomCameraController.Instance.SetLocked(true);
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (checkpointController != null)
        {
            checkpointController.enabled = false;
        }

        float originalAnimatorSpeed = 1f;

        if (playerAnimator != null)
        {
            originalAnimatorSpeed = playerAnimator.speed;
            playerAnimator.speed = 0f;
        }

        float originalGravityScale = 0f;
        bool originalSimulated = true;

        if (playerRb != null)
        {
            originalGravityScale = playerRb.gravityScale;
            originalSimulated = playerRb.simulated;
            playerRb.gravityScale = 0f;
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.simulated = false;
        }

        Vector3 startPosition = player.position;
        Quaternion startRotation = player.rotation;
        Vector3 startScale = player.localScale;
        Vector3 endScale = startScale * swirlFinalScale;
        float elapsed = 0f;
        float currentAngle = 0f;

        while (elapsed < swirlDuration)
        {
            elapsed += Time.deltaTime;
            float progress = swirlDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / swirlDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            float spinSpeed = Mathf.Lerp(swirlStartSpinSpeed, swirlEndSpinSpeed, easedProgress);

            currentAngle += spinSpeed * Time.deltaTime;

            if (player != null)
            {
                player.position = Vector3.Lerp(startPosition, swirlCenter, easedProgress);
                player.rotation = startRotation * Quaternion.Euler(0f, 0f, currentAngle);
                player.localScale = Vector3.Lerp(startScale, endScale, easedProgress);
            }

            yield return null;
        }

        if (player != null)
        {
            player.position = swirlCenter;
            player.rotation = startRotation * Quaternion.Euler(0f, 0f, currentAngle);
            player.localScale = endScale;
        }

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.gravityScale = originalGravityScale;
            playerRb.simulated = originalSimulated;
        }

        if (playerAnimator != null)
        {
            playerAnimator.speed = originalAnimatorSpeed;
        }

        if (returnDelay > 0f)
        {
            yield return new WaitForSeconds(returnDelay);
        }

        if (!string.IsNullOrWhiteSpace(levelSelectSceneName))
        {
            SceneManager.LoadScene(levelSelectSceneName);
        }
    }

    private Vector3 GetSwirlCenter()
    {
        Collider2D exitCollider = GetComponent<Collider2D>();

        if (exitCollider != null)
        {
            return exitCollider.bounds.center;
        }

        return transform.position;
    }

    private void OnDrawGizmos()
    {
        Collider2D exitCollider = GetComponent<Collider2D>();
        if (exitCollider == null)
        {
            return;
        }

        Color previousColor = Gizmos.color;
        Bounds bounds = exitCollider.bounds;

        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(bounds.center, bounds.size);

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.color = previousColor;
    }
}
