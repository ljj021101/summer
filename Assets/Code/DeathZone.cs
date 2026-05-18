using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DeathZone : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Color gizmoColor = new Color(1f, 0.1f, 0.05f, 0.25f);

    private BoxCollider2D deathCollider;

    private BoxCollider2D DeathCollider
    {
        get
        {
            if (deathCollider == null)
            {
                deathCollider = GetComponent<BoxCollider2D>();
            }

            return deathCollider;
        }
    }

    private void Reset()
    {
        deathCollider = GetComponent<BoxCollider2D>();
        deathCollider.isTrigger = true;
        deathCollider.size = new Vector2(2f, 1f);
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

        if (!isPlayer || checkpointController == null)
        {
            return;
        }

        checkpointController.Die();
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

        Gizmos.color = previousColor;
    }
}
