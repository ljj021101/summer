using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(Signpost))]
public class SignpostEditor : Editor
{
    private readonly BoxBoundsHandle boundsHandle = new BoxBoundsHandle();

    private void OnSceneGUI()
    {
        Signpost signpost = (Signpost)target;
        Transform signTransform = signpost.transform;
        BoxCollider2D signCollider = signpost.GetComponent<BoxCollider2D>();

        DrawMoveHandle(signTransform);
        DrawColliderHandle(signCollider);
    }

    private static void DrawMoveHandle(Transform targetTransform)
    {
        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(targetTransform.position, targetTransform.rotation);

        if (!EditorGUI.EndChangeCheck())
        {
            return;
        }

        Undo.RecordObject(targetTransform, "Move Signpost");
        targetTransform.position = newPosition;
        EditorUtility.SetDirty(targetTransform);
    }

    private void DrawColliderHandle(BoxCollider2D signCollider)
    {
        if (signCollider == null)
        {
            return;
        }

        using (new Handles.DrawingScope(signCollider.transform.localToWorldMatrix))
        {
            boundsHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y;
            boundsHandle.center = signCollider.offset;
            boundsHandle.size = signCollider.size;
            boundsHandle.handleColor = new Color(0.1f, 0.85f, 1f, 0.9f);
            boundsHandle.wireframeColor = new Color(0.1f, 0.85f, 1f, 1f);

            EditorGUI.BeginChangeCheck();
            boundsHandle.DrawHandle();

            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            Undo.RecordObject(signCollider, "Resize Signpost Range");
            signCollider.offset = boundsHandle.center;
            signCollider.size = new Vector2(
                Mathf.Max(0.01f, boundsHandle.size.x),
                Mathf.Max(0.01f, boundsHandle.size.y)
            );
            EditorUtility.SetDirty(signCollider);
        }
    }
}
