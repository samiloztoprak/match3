using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class AutoRef
{
    public static T Child<T>(Component owner, string childName) where T : Component
    {
        foreach (Transform t in owner.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == childName)
            {
                return t.GetComponent<T>();
            }
        }
        Debug.LogWarning($"[AutoRef] '{childName}' not found under '{owner.name}'", owner);
        return null;
    }

    public static T InScene<T>() where T : Object
    {
        return Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
    }

#if UNITY_EDITOR
    public static T Asset<T>() where T : Object
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
        return guids.Length == 0 ? null : AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    public static T[] Assets<T>(string folder) where T : Object
    {
        var list = new System.Collections.Generic.List<T>();
        foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder }))
        {
            list.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
        }
        list.Sort((a, b) => EditorUtility.NaturalCompare(a.name, b.name));
        return list.ToArray();
    }
#endif
}