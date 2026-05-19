using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapCollider2D))]
public class HazardTilemap : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private TilemapCollider2D tilemapCollider;

    private void Reset()
    {
        tilemapCollider = GetComponent<TilemapCollider2D>();
        tilemapCollider.isTrigger = true;
    }

    private void Awake()
    {
        tilemapCollider = GetComponent<TilemapCollider2D>();
        tilemapCollider.isTrigger = true;
    }

    private void OnValidate()
    {
        TilemapCollider2D currentCollider = GetComponent<TilemapCollider2D>();

        if (currentCollider != null)
        {
            currentCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHurtbox hurtbox = other.GetComponent<PlayerHurtbox>();

        if (hurtbox != null)
        {
            if (hurtbox.CheckpointController != null)
            {
                hurtbox.CheckpointController.Die();
            }

            return;
        }

        PlayerCheckpointController checkpointController = other.GetComponentInParent<PlayerCheckpointController>();

        if (checkpointController != null && checkpointController.GetComponentInChildren<PlayerHurtbox>() != null)
        {
            return;
        }

        if (other.isTrigger)
        {
            return;
        }

        if (checkpointController == null || !other.CompareTag(playerTag))
        {
            return;
        }

        checkpointController.Die();
    }
}
