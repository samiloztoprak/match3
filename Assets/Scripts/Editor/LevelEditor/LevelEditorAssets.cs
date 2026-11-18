using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Asset work for the level editor: create, copy, delete, keep the catalog in order, play a level.
public static class LevelEditorAssets
{
    private const string NamePrefix = "Level-";

    public static LevelAsset[] All() => AutoRef.Assets<LevelAsset>(LevelCatalog.AssetFolder);

    public static LevelData NewLevelData()
    {
        var data = new LevelData();
        LevelEditing.EnsureRows(data);
        return data;
    }

    public static LevelData CopyData(LevelData data) => LevelJson.FromJson(LevelJson.ToJson(data));

    public static LevelAsset Create(LevelData data)
    {
        var asset = ScriptableObject.CreateInstance<LevelAsset>();
        asset.Data = data;
        AssetDatabase.CreateAsset(asset, $"{LevelCatalog.AssetFolder}/{NextName()}.asset");
        RefreshCatalog();
        return asset;
    }

    public static void Delete(LevelAsset asset)
    {
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
        RefreshCatalog();
    }

    public static void RefreshCatalog()
    {
        LevelCatalog catalog = AutoRef.Asset<LevelCatalog>();
        if (catalog == null)
        {
            Debug.LogError("[LevelEditor] LevelCatalog asset not found.");
            return;
        }
        catalog.Refresh();
        AssetDatabase.SaveAssets();
    }

    public static void Play(LevelAsset asset)
    {
        RefreshCatalog();
        int index = AutoRef.Asset<LevelCatalog>().IndexOf(asset);
        if (index < 0)
        {
            Debug.LogError($"[LevelEditor] {asset.name} is not in the LevelCatalog. Is it in {LevelCatalog.AssetFolder}?");
            return;
        }
        
        if (EditorBuildSettings.scenes.Length == 0 || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        LevelProgress.CurrentIndex = index;
        SessionState.SetBool(Boot.PlayLevelKey, true);
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);   // Boot
        EditorApplication.isPlaying = true;
    }

    private static string NextName()
    {
        int max = 0;
        foreach (LevelAsset asset in All())
        {
            Match match = Regex.Match(asset.name, @"(\d+)$");
            if (match.Success)
            {
                max = Mathf.Max(max, int.Parse(match.Groups[1].Value));
            }
        }
        return NamePrefix + (max + 1);
    }
}