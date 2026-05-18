using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(DeathZone))]
public class DeathZoneEditor : Editor
{
    private readonly BoxBoundsHandle boundsHandle = new BoxBoundsHandle();

    private void OnSceneGUI()
    {
        DeathZone deathZone = (DeathZone)target;
        Transform zoneTransform = deathZone.transform;
        BoxCollider2D zoneCollider = deathZone.GetComponent<BoxCollider2D>();

        DrawMoveHandle(zoneTransform);
        DrawColliderHandle(zoneCollider);
    }

    private static void DrawMoveHandle(Transform targetTransform)
    {
        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(targetTransform.position, targetTransform.rotation);

        if (!EditorGUI.EndChangeCheck())
        {
            return;
        }

        Undo.RecordObject(targetTransform, "Move Death Zone");
        targetTransform.position = newPosition;
        EditorUtility.SetDirty(targetTransform);
    }

    private void DrawColliderHandle(BoxCollider2D zoneCollider)
    {
        if (zoneCollider == null)
        {
            return;
        }

        using (new Handles.DrawingScope(zoneCollider.transform.localToWorldMatrix))
        {
            boundsHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y;
            boundsHandle.center = zoneCollider.offset;
            boundsHandle.size = zoneCollider.size;
            boundsHandle.handleColor = new Color(1f, 0.1f, 0.05f, 0.9f);
            boundsHandle.wireframeColor = new Color(1f, 0.1f, 0.05f, 1f);

            EditorGUI.BeginChangeCheck();
            boundsHandle.DrawHandle();

            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            Undo.RecordObject(zoneCollider, "Resize Death Zone");
            zoneCollider.offset = boundsHandle.center;
            zoneCollider.size = new Vector2(
                Mathf.Max(0.01f, boundsHandle.size.x),
                Mathf.Max(0.01f, boundsHandle.size.y)
            );
            EditorUtility.SetDirty(zoneCollider);
        }
    }
}
