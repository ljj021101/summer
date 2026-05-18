using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(Checkpoint))]
public class CheckpointEditor : Editor
{
    private readonly BoxBoundsHandle boundsHandle = new BoxBoundsHandle();

    private void OnSceneGUI()
    {
        Checkpoint checkpoint = (Checkpoint)target;

        DrawMoveHandle(checkpoint.transform);
        DrawColliderHandle(checkpoint.GetComponent<BoxCollider2D>());
        DrawRespawnHandle(checkpoint);
    }

    private static void DrawMoveHandle(Transform targetTransform)
    {
        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(targetTransform.position, targetTransform.rotation);

        if (!EditorGUI.EndChangeCheck())
        {
            return;
        }

        Undo.RecordObject(targetTransform, "Move Checkpoint");
        targetTransform.position = newPosition;
        EditorUtility.SetDirty(targetTransform);
    }

    private void DrawColliderHandle(BoxCollider2D checkpointCollider)
    {
        if (checkpointCollider == null)
        {
            return;
        }

        using (new Handles.DrawingScope(checkpointCollider.transform.localToWorldMatrix))
        {
            boundsHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y;
            boundsHandle.center = checkpointCollider.offset;
            boundsHandle.size = checkpointCollider.size;
            boundsHandle.handleColor = new Color(0.2f, 1f, 0.45f, 0.9f);
            boundsHandle.wireframeColor = new Color(0.2f, 1f, 0.45f, 1f);

            EditorGUI.BeginChangeCheck();
            boundsHandle.DrawHandle();

            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            Undo.RecordObject(checkpointCollider, "Resize Checkpoint Bounds");
            checkpointCollider.offset = boundsHandle.center;
            checkpointCollider.size = new Vector2(
                Mathf.Max(0.01f, boundsHandle.size.x),
                Mathf.Max(0.01f, boundsHandle.size.y)
            );
            EditorUtility.SetDirty(checkpointCollider);
        }
    }

    private static void DrawRespawnHandle(Checkpoint checkpoint)
    {
        Vector3 currentPosition = checkpoint.RespawnPosition;

        Handles.color = Color.yellow;
        Handles.DrawWireDisc(currentPosition, Vector3.forward, 0.35f);
        Handles.Label(currentPosition + Vector3.up * 0.45f, "Respawn");

        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(currentPosition, Quaternion.identity);

        if (!EditorGUI.EndChangeCheck())
        {
            return;
        }

        Transform respawnPointTransform = checkpoint.RespawnPointTransform;

        if (respawnPointTransform != null)
        {
            Undo.RecordObject(respawnPointTransform, "Move Checkpoint Respawn Point");
        }
        else
        {
            Undo.RecordObject(checkpoint, "Move Checkpoint Respawn Point");
        }

        checkpoint.SetRespawnPosition(newPosition);
        EditorUtility.SetDirty(respawnPointTransform != null ? respawnPointTransform : checkpoint);
    }
}
