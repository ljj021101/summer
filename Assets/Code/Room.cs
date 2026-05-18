using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class Room : MonoBehaviour
{
    private static readonly List<Room> rooms = new List<Room>();

    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.7f, 1f, 0.2f);

    private BoxCollider2D roomCollider;

    public static IReadOnlyList<Room> Rooms => rooms;

    public Bounds Bounds => RoomCollider.bounds;

    private BoxCollider2D RoomCollider
    {
        get
        {
            if (roomCollider == null)
            {
                roomCollider = GetComponent<BoxCollider2D>();
            }

            return roomCollider;
        }
    }

    private void Reset()
    {
        roomCollider = GetComponent<BoxCollider2D>();
        roomCollider.isTrigger = true;
        roomCollider.size = new Vector2(16f, 9f);
    }

    private void OnEnable()
    {
        if (!rooms.Contains(this))
        {
            rooms.Add(this);
        }
    }

    private void OnDisable()
    {
        rooms.Remove(this);
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
        PlatformerPlayerController playerController = other.GetComponentInParent<PlatformerPlayerController>();
        bool isPlayer = playerController != null || other.CompareTag(playerTag);

        if (!isPlayer)
        {
            return;
        }

        RoomCameraController cameraController = RoomCameraController.Instance;

        if (cameraController != null)
        {
            Transform target = playerController != null ? playerController.transform : other.transform;
            cameraController.EnterRoom(this, target);
        }
    }

    public bool ContainsPoint(Vector3 point)
    {
        Bounds bounds = Bounds;

        return point.x >= bounds.min.x &&
            point.x <= bounds.max.x &&
            point.y >= bounds.min.y &&
            point.y <= bounds.max.y;
    }

    public float SqrDistanceFromCenter(Vector3 point)
    {
        Bounds bounds = Bounds;
        Vector2 offset = (Vector2)point - (Vector2)bounds.center;

        return offset.sqrMagnitude;
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
