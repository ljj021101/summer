using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Vector2 respawnLocalPosition;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Color triggerGizmoColor = new Color(0.2f, 1f, 0.45f, 0.2f);
    [SerializeField] private Color respawnGizmoColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private float respawnGizmoRadius = 0.25f;

    private BoxCollider2D checkpointCollider;

    public Vector3 RespawnPosition => respawnPoint != null ?
        respawnPoint.position :
        transform.TransformPoint(respawnLocalPosition);

    public Transform RespawnPointTransform => respawnPoint;

    private BoxCollider2D CheckpointCollider
    {
        get
        {
            if (checkpointCollider == null)
            {
                checkpointCollider = GetComponent<BoxCollider2D>();
            }

            return checkpointCollider;
        }
    }

    private void Reset()
    {
        checkpointCollider = GetComponent<BoxCollider2D>();
        checkpointCollider.isTrigger = true;
        checkpointCollider.size = new Vector2(1.5f, 2f);
        respawnLocalPosition = Vector2.zero;
    }

    public void SetRespawnPosition(Vector3 worldPosition)
    {
        if (respawnPoint != null)
        {
            respawnPoint.position = worldPosition;
            return;
        }

        respawnLocalPosition = transform.InverseTransformPoint(worldPosition);
    }

    private void OnValidate()
    {
        BoxCollider2D currentCollider = GetComponent<BoxCollider2D>();

        if (currentCollider != null)
        {
            currentCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerCheckpointController checkpointController = other.GetComponentInParent<PlayerCheckpointController>();
        bool isPlayer = checkpointController != null || other.CompareTag(playerTag);

        if (!isPlayer)
        {
            return;
        }

        if (checkpointController == null)
        {
            checkpointController = other.GetComponentInParent<PlayerCheckpointController>();
        }

        bool changedCheckpoint = false;
        if (checkpointController != null)
        {
            changedCheckpoint = checkpointController.SetCheckpoint(this);
        }

        bool bankedCoins = false;
        PlayerCoinCollector coinCollector = other.GetComponentInParent<PlayerCoinCollector>();
        if (coinCollector != null)
        {
            bankedCoins = coinCollector.BankCarriedCoins();
        }

        if (checkpointController != null && (changedCheckpoint || bankedCoins))
        {
            checkpointController.ShowSaveIcon();
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D currentCollider = GetComponent<BoxCollider2D>();

        if (currentCollider != null)
        {
            Bounds bounds = currentCollider.bounds;
            Color previousColor = Gizmos.color;

            Gizmos.color = triggerGizmoColor;
            Gizmos.DrawCube(bounds.center, bounds.size);

            Gizmos.color = new Color(triggerGizmoColor.r, triggerGizmoColor.g, triggerGizmoColor.b, 1f);
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            Gizmos.color = previousColor;
        }

        DrawRespawnGizmo();
    }

    private void DrawRespawnGizmo()
    {
        Vector3 position = RespawnPosition;
        Color previousColor = Gizmos.color;

        Gizmos.color = respawnGizmoColor;
        Gizmos.DrawSphere(position, respawnGizmoRadius);
        Gizmos.DrawLine(position + Vector3.left * respawnGizmoRadius * 2f, position + Vector3.right * respawnGizmoRadius * 2f);
        Gizmos.DrawLine(position + Vector3.down * respawnGizmoRadius * 2f, position + Vector3.up * respawnGizmoRadius * 2f);

        Gizmos.color = previousColor;
    }
}
