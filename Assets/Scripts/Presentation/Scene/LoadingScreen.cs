using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    private const float FadeDuration = 0.25f;
    private const float MinShowTime = 0.6f;
    private const float BarFillSpeed = 2.5f;

    private static LoadingScreen _prefab;
    private static LoadingScreen _instance;

    [SerializeField] private Canvas _canvas;
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private Image _barFill;
    [SerializeField] private TMP_Text _percentText;

    private float _displayed;
    private Vector2 _mascotStartPos;

    public static bool IsLoading { get; private set; }

    public static void Register(LoadingScreen prefab)
    {
        _prefab = prefab;
    }

    private static LoadingScreen Instance
    {
        get
        {
            if (_instance == null)
            {
                if (_prefab == null)
                {
                    Debug.LogError("[LoadingScreen] Not registered. Start the game from the Boot scene.");
                    return null;
                }
                _instance = Instantiate(_prefab);
                _instance.name = "LoadingScreen";
                DontDestroyOnLoad(_instance.gameObject);
                _instance.HideImmediate();
            }
            return _instance;
        }
    }

    private void OnValidate()
    {
        _canvas = GetComponent<Canvas>();
        _group = GetComponent<CanvasGroup>();
        _barFill = AutoRef.Child<Image>(this, "ui_image_bar_fill");
        _percentText = AutoRef.Child<TMP_Text>(this, "ui_text_percent");
    }

    public static void LoadScene(string sceneName)
    {
        if (IsLoading)
        {
            return;
        }
        LoadingScreen screen = Instance;
        if (screen == null)
        {
            return;
        }
        screen.StartCoroutine(screen.LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        IsLoading = true;
        _displayed = 0f;
        SetBar(0f);

        _canvas.enabled = true;
        _group.blocksRaycasts = true;
        yield return _group.DOFade(1f, FadeDuration).SetUpdate(true).WaitForCompletion();

        float startTime = Time.unscaledTime;
        AsyncOperationHandle<SceneInstance> handle =
            Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single, activateOnLoad: false);

        while (!handle.IsDone || _displayed < 1f || Time.unscaledTime - startTime < MinShowTime)
        {
            float target = handle.PercentComplete;
            _displayed = Mathf.MoveTowards(_displayed, target, BarFillSpeed * Time.unscaledDeltaTime);
            SetBar(_displayed);
            yield return null;
        }

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[LoadingScreen] Failed to load scene '{sceneName}': {handle.OperationException}");
            HideImmediate();
            IsLoading = false;
            yield break;
        }

        yield return handle.Result.ActivateAsync();
        yield return null;

        yield return _group.DOFade(0f, FadeDuration).SetUpdate(true).WaitForCompletion();
        HideImmediate();
        IsLoading = false;
    }

    private void SetBar(float value)
    {
        _barFill.fillAmount = value;
        _percentText.text = $"{Mathf.RoundToInt(value * 100f)}%";
    }
   

    private void HideImmediate()
    {
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        _canvas.enabled = false;
    }
}