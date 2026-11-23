using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RemoteAwareLevelSource : ILevelSource
{
    private readonly ILevelSource _bundled;
    private readonly Dictionary<int, int> _revisions = new Dictionary<int, int>();
    private int _maxLevelId;
    
    public RemoteAwareLevelSource(ILevelSource bundled)
    {
        _bundled = bundled;

        string indexPath = Path.Combine(Application.persistentDataPath, "levels", "index.json");
        if (!File.Exists(indexPath)) return;

        RemoteIndex index = JsonUtility.FromJson<RemoteIndex>(File.ReadAllText(indexPath));
        //Debug.Log($"[LevelDebug] indexExists={File.Exists(indexPath)} path={indexPath}");
        if (index?.levels == null) return;
        
        foreach (RemoteLevelEntry entry in index.levels)
        {
            _maxLevelId = Mathf.Max(_maxLevelId, entry.levelId);
            _revisions[entry.levelId] = entry.contentRevision;
        }
    }

    public int Count => Mathf.Max(_bundled.Count, _maxLevelId);

    public LevelData Get(int index)
    {
        int levelId = index + 1;

        if (_revisions.TryGetValue(levelId, out int revision))
        {
            string json = RemoteLevelSync.TryReadCachedLevelJson(levelId, revision);
            LevelData data = LevelJson.FromJson(json); 
            if (data != null) return data;
        }

        return _bundled.Get(index);
    }
}
