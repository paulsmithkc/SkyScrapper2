using UnityEngine;
using UnityEditor;

public class CopyPasteTransform : ScriptableObject
{
    private static Vector3 _position = Vector3.zero;
    private static Quaternion _rotation = Quaternion.identity;
    private static Vector3 _scale = Vector3.one;

    [MenuItem("Tools/Copy Transform", false, 10)]
    [MenuItem("CONTEXT/Transform/Copy Transform", false, 10)]
    [MenuItem("GameObject/Copy Transform", false, 11)]
    public static void CopyTransform()
    {
        if (Selection.activeTransform != null)
        {
            _position = Selection.activeTransform.localPosition;
            _rotation = Selection.activeTransform.localRotation;
            _scale = Selection.activeTransform.localScale;
        }

        Transform[] selections = Selection.transforms;
        if (selections != null)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append('[');

            for (int i = 0; i < selections.Length; ++i)
            {
                var sel = selections[i];
                var p = sel.transform.position;
                var r = sel.transform.localRotation.eulerAngles;
                var s = sel.transform.localScale;
                if (i > 0) { sb.Append(','); }
                sb.AppendFormat(
                    "{{Name:'{9}', Position:[{0},{1},{2}], Rotation:[{3},{4},{5}], Scale:[{6},{7},{8}]}}\n",
                    p.x, p.y, p.z,
                    r.x, r.y, r.z,
                    s.x, s.y, s.z,
                    sel.name
                );
            }

            sb.Append(']');
            EditorGUIUtility.systemCopyBuffer = sb.ToString();
        }
    }

    [MenuItem("Tools/Paste Transform", false, 11)]
    [MenuItem("CONTEXT/Transform/Paste Transform", false, 11)]
    [MenuItem("GameObject/Paste Transform", false, 12)]
    public static void PasteTransform()
    {
        Transform[] selections = Selection.transforms;
        if (selections != null)
        {
            foreach (Transform s in selections)
            {
                Undo.RecordObject(s, "Paste Transform");
                s.localPosition = _position;
                s.localRotation = _rotation;
                s.localScale = _scale;
            }
        }
    }

    [MenuItem("Tools/Paste Transform Position", false, 12)]
    [MenuItem("CONTEXT/Transform/Paste Transform Position", false, 12)]
    public static void PastePosition()
    {
        Transform[] selections = Selection.transforms;
        if (selections != null)
        {
            foreach (Transform s in selections)
            {
                Undo.RecordObject(s, "Paste Transform");
                s.localPosition = _position;
            }
        }
    }

    [MenuItem("Tools/Paste Transform Rotation", false, 13)]
    [MenuItem("CONTEXT/Transform/Paste Transform Rotation", false, 13)]
    public static void PasteRotation()
    {
        Transform[] selections = Selection.transforms;
        if (selections != null)
        {
            foreach (Transform s in selections)
            {
                Undo.RecordObject(s, "Paste Transform");
                s.localRotation = _rotation;
            }
        }

        if (Selection.activeTransform != null)
        {
            Undo.RecordObject(Selection.activeTransform, "Paste Transform Rotation");
            Selection.activeTransform.localRotation = _rotation;
        }
    }

    [MenuItem("Tools/Paste Transform Scale", false, 14)]
    [MenuItem("CONTEXT/Transform/Paste Transform Scale", false, 14)]
    public static void PasteScale()
    {
        Transform[] selections = Selection.transforms;
        if (selections != null)
        {
            foreach (Transform s in selections)
            {
                Undo.RecordObject(s, "Paste Transform");
                s.localScale = _scale;
            }
        }
    }

    [MenuItem("Tools/Set Y To Zero", false, 15)]
    [MenuItem("CONTEXT/Transform/Set Y To Zero", false, 15)]
    public static void SetYToZero()
    {
        Transform[] selections = Selection.transforms;
        if (selections != null)
        {
            foreach (Transform s in selections)
            {
                Undo.RecordObject(s, "Set Y To Zero");
                Vector3 p = s.position;
                p.y = 0.0f;
                s.position = p;
            }
        }
    }
}
