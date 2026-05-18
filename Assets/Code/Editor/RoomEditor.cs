using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(Room))]
public class RoomEditor : Editor
{
    private readonly BoxBoundsHandle boundsHandle = new BoxBoundsHandle();

    private void OnSceneGUI()
    {
        Room room = (Room)target;
        Transform roomTransform = room.transform;
        BoxCollider2D roomCollider = room.GetComponent<BoxCollider2D>();

        DrawMoveHandle(roomTransform);
        DrawColliderHandle(roomCollider);
    }

    private static void DrawMoveHandle(Transform targetTransform)
    {
        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(targetTransform.position, targetTransform.rotation);

        if (!EditorGUI.EndChangeCheck())
        {
            return;
        }

        Undo.RecordObject(targetTransform, "Move Room");
        targetTransform.position = newPosition;
        EditorUtility.SetDirty(targetTransform);
    }

    private void DrawColliderHandle(BoxCollider2D roomCollider)
    {
        if (roomCollider == null)
        {
            return;
        }

        using (new Handles.DrawingScope(roomCollider.transform.localToWorldMatrix))
        {
            boundsHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y;
            boundsHandle.center = roomCollider.offset;
            boundsHandle.size = roomCollider.size;
            boundsHandle.handleColor = new Color(0.2f, 0.7f, 1f, 0.9f);
            boundsHandle.wireframeColor = new Color(0.2f, 0.7f, 1f, 1f);

            EditorGUI.BeginChangeCheck();
            boundsHandle.DrawHandle();

            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            Undo.RecordObject(roomCollider, "Resize Room Bounds");
            roomCollider.offset = boundsHandle.center;
            roomCollider.size = new Vector2(
                Mathf.Max(0.01f, boundsHandle.size.x),
                Mathf.Max(0.01f, boundsHandle.size.y)
            );
            EditorUtility.SetDirty(roomCollider);
        }
    }
}
