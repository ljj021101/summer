using UnityEngine;

public class PlayerHurtbox : MonoBehaviour
{
    public PlayerCheckpointController CheckpointController { get; private set; }

    private void Awake()
    {
        CheckpointController = GetComponentInParent<PlayerCheckpointController>();
    }
}
