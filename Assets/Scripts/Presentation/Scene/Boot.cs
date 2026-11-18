using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Boot : MonoBehaviour
{
    private const string LoadingScreenKey = "LoadingScreen";

#if UNITY_EDITOR
    public const string PlayLevelKey = "Match3.PlayLevelDirect";
#endif
    
    private IEnumerator Start()
    {
        yield return Addressables.InitializeAsync();

        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(LoadingScreenKey);
        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[Boot] Could not load '{LoadingScreenKey}': {handle.OperationException}");
            yield break;
        }

        LoadingScreen.Register(handle.Result.GetComponent<LoadingScreen>());
#if UNITY_EDITOR
        if (UnityEditor.SessionState.GetBool(PlayLevelKey, false))
        {
            UnityEditor.SessionState.EraseBool(PlayLevelKey);
            SceneLoader.LoadGame();
            yield break;
        }
#endif
        SceneLoader.LoadMenu();
    }
}