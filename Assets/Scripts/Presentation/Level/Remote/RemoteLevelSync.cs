using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

[Serializable] public class RemoteIndex
{
    public int schemaVersion;
    public int indexRevision;
    public List<RemoteLevelEntry> levels = new List<RemoteLevelEntry>();
}

[Serializable] public class RemoteLevelEntry
{
    public int levelId;
    public int contentRevision;
    public string file;
    public string sha256;
}

public class RemoteLevelSync : MonoBehaviour
{
    private const string BaseUrl = "https://circus-royale-levels-host.web.app/levels/v1/";
    private static string CacheDir => Path.Combine(Application.persistentDataPath, "levels");

    public IEnumerator SyncLevels(Action onDone)
    {
        Directory.CreateDirectory(CacheDir);

        byte[] indexBytes = null;
        yield return Get(BaseUrl + "index.json", data => indexBytes = data);
        if (indexBytes == null)
        {
            onDone?.Invoke();
            yield break;
        }

        File.WriteAllBytes(Path.Combine(CacheDir, "index.json"), indexBytes);
        RemoteIndex index = JsonUtility.FromJson<RemoteIndex>(System.Text.Encoding.UTF8.GetString(indexBytes));
        if (index == null || index.levels == null)
        {
            onDone?.Invoke();
            yield break;
        }

        foreach (RemoteLevelEntry entry in index.levels)
        {
            string cachePath = CachePath(entry);
            if (File.Exists(cachePath)) continue;

            byte[] data = null;
            yield return Get(BaseUrl + entry.file, d => data = d);
            if (data == null) continue;

            if (!string.Equals(Sha256Hex(data), entry.sha256, StringComparison.OrdinalIgnoreCase)) continue;

            string tmp = cachePath + ".tmp";
            File.WriteAllBytes(tmp, data);
            if (File.Exists(cachePath)) File.Delete(cachePath);
            File.Move(tmp, cachePath);
        }

        onDone?.Invoke();
    }

    public static string TryReadCachedLevelJson(int levelId, int contentRevision)
    {
        string path = Path.Combine(CacheDir, $"{levelId:000}_r{contentRevision}.json");
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }

    private static string CachePath(RemoteLevelEntry entry) =>
        Path.Combine(CacheDir, $"{entry.levelId:000}_r{entry.contentRevision}.json");

    private static IEnumerator Get(string url, Action<byte[]> onDone)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.timeout = 10;
            yield return req.SendWebRequest();
            bool ok = req.result == UnityWebRequest.Result.Success;
            onDone(ok ? req.downloadHandler.data : null);
        }
    }

    private static string Sha256Hex(byte[] data)
    {
        using (SHA256 sha = SHA256.Create())
        {
            return BitConverter.ToString(sha.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
        }
    }
}