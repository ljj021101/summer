using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RoomCameraController : MonoBehaviour
{
    public static RoomCameraController Instance { get; private set; }

    [Header("Follow")]
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.08f;
    [SerializeField] private float roomSwitchDuration = 0.35f;
    [SerializeField] private Vector2 followOffset;
    [SerializeField] private float cameraZPosition = -10f;

    [Header("Room")]
    [SerializeField] private Room currentRoom;

    private Camera roomCamera;
    private Vector3 velocity;
    private Vector3 roomSwitchStartPosition;
    private float roomSwitchTimer;
    private bool isSwitchingRooms;
    private bool isLocked;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        roomCamera = GetComponent<Camera>();

        Vector3 position = transform.position;
        position.z = cameraZPosition;
        transform.position = position;
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (isLocked)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        UpdateCurrentRoom();

        Vector3 desiredPosition = GetDesiredPosition();

        if (isSwitchingRooms)
        {
            roomSwitchTimer += Time.deltaTime;
            float progress = roomSwitchDuration <= 0f ? 1f : Mathf.Clamp01(roomSwitchTimer / roomSwitchDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            transform.position = Vector3.Lerp(roomSwitchStartPosition, desiredPosition, easedProgress);

            if (progress >= 1f)
            {
                isSwitchingRooms = false;
                velocity = Vector3.zero;
            }

            return;
        }

        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        transform.position = ClampPositionToCurrentRoom(smoothedPosition);
    }

    public void EnterRoom(Room room, Transform roomTarget)
    {
        if (room == currentRoom)
        {
            return;
        }

        currentRoom = room;

        if (target == null)
        {
            target = roomTarget;
        }

        roomSwitchStartPosition = transform.position;
        roomSwitchTimer = 0f;
        isSwitchingRooms = true;
        velocity = Vector3.zero;
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        velocity = Vector3.zero;
        isSwitchingRooms = false;
    }

    private void UpdateCurrentRoom()
    {
        if (isSwitchingRooms)
        {
            return;
        }

        Room bestRoom = null;
        float bestDistance = float.PositiveInfinity;

        foreach (Room room in Room.Rooms)
        {
            if (room == null || !room.ContainsPoint(target.position))
            {
                continue;
            }

            float distance = room.SqrDistanceFromCenter(target.position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestRoom = room;
            }
        }

        if (bestRoom != null && bestRoom != currentRoom)
        {
            EnterRoom(bestRoom, target);
        }
    }

    private Vector3 GetDesiredPosition()
    {
        Vector3 desiredPosition = target.position + (Vector3)followOffset;
        desiredPosition.z = cameraZPosition;

        return ClampPositionToCurrentRoom(desiredPosition);
    }

    private Vector3 ClampPositionToCurrentRoom(Vector3 position)
    {
        if (currentRoom == null)
        {
            return position;
        }

        Bounds roomBounds = currentRoom.Bounds;
        Vector2 cameraHalfSize = GetCameraHalfSize();

        float minX = roomBounds.min.x + cameraHalfSize.x;
        float maxX = roomBounds.max.x - cameraHalfSize.x;
        float minY = roomBounds.min.y + cameraHalfSize.y;
        float maxY = roomBounds.max.y - cameraHalfSize.y;

        position.x = ClampToRoomAxis(position.x, roomBounds.center.x, minX, maxX);
        position.y = ClampToRoomAxis(position.y, roomBounds.center.y, minY, maxY);

        return position;
    }

    private Vector2 GetCameraHalfSize()
    {
        if (roomCamera.orthographic)
        {
            float halfHeight = roomCamera.orthographicSize;
            return new Vector2(halfHeight * roomCamera.aspect, halfHeight);
        }

        return Vector2.zero;
    }

    private static float ClampToRoomAxis(float value, float roomCenter, float min, float max)
    {
        if (min > max)
        {
            return roomCenter;
        }

        return Mathf.Clamp(value, min, max);
    }
}
