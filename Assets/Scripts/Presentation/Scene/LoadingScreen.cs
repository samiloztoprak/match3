using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    private const string ResourcePath = "LoadingScreen";
    private const float FadeDuration = 0.25f;
    private const float MinShowTime = 0.6f;
    private const float BarFillSpeed = 2.5f;

    private static LoadingScreen _instance;

    [SerializeField] private Canvas _canvas;
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private Image _barFill;
    [SerializeField] private TMP_Text _percentText;
    [SerializeField] private RectTransform _mascot;

    private float _displayed;
    private Vector2 _mascotStartPos;
    private Tween _mascotTween;

    public static bool IsLoading { get; private set; }

    private static LoadingScreen Instance
    {
        get
        {
            if (_instance == null)
            {
                LoadingScreen prefab = Resources.Load<LoadingScreen>(ResourcePath);
                _instance = Instantiate(prefab);
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
        _mascot = AutoRef.Child<RectTransform>(this, "ui_image_mascot");
    }

    public static void LoadScene(string sceneName)
    {
        if (IsLoading)
        {
            return;
        }
        Instance.StartCoroutine(Instance.LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        IsLoading = true;
        _displayed = 0f;
        SetBar(0f);

        _canvas.enabled = true;
        _group.blocksRaycasts = true;
        StartMascot();
        yield return _group.DOFade(1f, FadeDuration).SetUpdate(true).WaitForCompletion();

        float startTime = Time.unscaledTime;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f || _displayed < 1f || Time.unscaledTime - startTime < MinShowTime)
        {
            float target = Mathf.Clamp01(operation.progress / 0.9f);
            _displayed = Mathf.MoveTowards(_displayed, target, BarFillSpeed * Time.unscaledDeltaTime);
            SetBar(_displayed);
            yield return null;
        }

        operation.allowSceneActivation = true;
        while (!operation.isDone)
        {
            yield return null;
        }
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

    private void StartMascot()
    {
        _mascotStartPos = _mascot.anchoredPosition;
        _mascotTween = _mascot.DOAnchorPosY(_mascotStartPos.y + 30f, 0.35f)
            .SetEase(Ease.OutQuad)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void HideImmediate()
    {
        _mascotTween?.Kill();
        if (_mascotTween != null)
        {
            _mascot.anchoredPosition = _mascotStartPos;
            _mascotTween = null;
        }
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        _canvas.enabled = false;
    }
}