using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class RaycastTargetCleaner
{
    [MenuItem("Tools/Match3/Disable Unneeded Raycast Targets")]
    private static void Clean()
    {
        int fixedCount = 0;
        foreach (Graphic graphic in Object.FindObjectsByType<Graphic>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!graphic.raycastTarget) continue;

            Selectable selectable = graphic.GetComponent<Selectable>();
            bool isClickTarget = selectable != null && selectable.targetGraphic == graphic;
            if (isClickTarget) continue;

            Undo.RecordObject(graphic, "Disable Raycast Target");
            graphic.raycastTarget = false;
            fixedCount++;
            Debug.Log($"Raycast Target disabled: {GetPath(graphic.transform)}", graphic);
        }
        Debug.Log($"[RaycastTargetCleaner] {fixedCount} graphics fixed");
    }

    private static string GetPath(Transform t)
    {
        return t.parent == null ? t.name : GetPath(t.parent) + "/" + t.name;
    }
}