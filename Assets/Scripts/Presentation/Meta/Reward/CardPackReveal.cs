using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CardPackReveal : MonoBehaviour
{
    private enum Phase { Idle, Enter, WaitToOpen, Open, WaitToCollect, FlyCard, TurnIntoStar, FlyStar }

    [SerializeField] private RectTransform _pack;
    [SerializeField] private RectTransform _lid;
    [SerializeField] private Image[] _cards;
    [SerializeField] private Button _input;
    [SerializeField] private TMP_Text _prompt;
    [SerializeField] private string _openMessage = "Tap to open";
    [SerializeField] private string _collectMessage = "Tap to collect";
    [SerializeField] private Sprite _star;

    private RectTransform _stage;
    private Vector2 _packCenter;
    private Vector2 _lidRest;
    private Vector3 _from;
    private Vector3 _destination;
    private Vector2 _flightStart;
    private MenuTabButton _collectionTab;
    private WheelProgressBar _progressBar;
    private Action _onComplete;
    private CardPackResult _result;
    private Phase _phase;
    private float _elapsed;
    private int _cardIndex;
    private readonly StringBuilder _summary = new StringBuilder(160);

    private void Awake()
    {
        _stage = (RectTransform)transform;
        _packCenter = _pack.anchoredPosition;
        _lidRest = _lid.anchoredPosition;
        _input.onClick.AddListener(Advance);
    }

    public void Play(CardPackResult result, Vector3 from, MenuTabButton collectionTab,
        WheelProgressBar progressBar, Action onComplete)
    {
        _prompt.gameObject.SetActive(false);
        _input.interactable = false;
        gameObject.SetActive(true);
        _result = result;
        _from = from;
        _collectionTab = collectionTab;
        _progressBar = progressBar;
        _onComplete = onComplete;
        _pack.SetAsLastSibling();
        _pack.gameObject.SetActive(true);
        _pack.position = _from;
        _pack.localScale = Vector3.one * .18f;
        _lid.anchoredPosition = _lidRest;
        _lid.localRotation = Quaternion.identity;
        for (int index = 0; index < _cards.Length; index++)
        {
            Image card = _cards[index];
            card.sprite = result.Cards[index].Card.Art;
            card.color = Color.white;
            card.rectTransform.localScale = Vector3.one;
            card.gameObject.SetActive(false);
        }

        Begin(Phase.Enter);
    }

    public void Hide()
    {
        _phase = Phase.Idle;
        _result = default;
        _onComplete = null;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        _elapsed += Time.unscaledDeltaTime;
        switch (_phase)
        {
            case Phase.Enter:
                Enter();
                break;
            case Phase.Open:
                Open();
                break;
            case Phase.FlyCard:
            case Phase.FlyStar:
                Fly();
                break;
            case Phase.TurnIntoStar:
                TurnIntoStar();
                break;
        }
    }

    private void Enter()
    {
        float progress = Progress(.48f);
        float eased = EaseOut(progress);
        Vector3 center = _pack.parent.TransformPoint(_packCenter);
        _pack.position = Vector3.LerpUnclamped(_from, center, eased);
        float pop = Mathf.Sin(progress * Mathf.PI) * .16f;
        _pack.localScale = Vector3.one * (Mathf.Lerp(.18f, 1f, eased) + pop);
        if (progress < 1f) return;

        AudioManager.Play(SoundId.RewardReveal);
        Haptics.Vibrate();
        WaitForTap(Phase.WaitToOpen, _openMessage);
    }

    private void Advance()
    {
        _input.interactable = false;
        _prompt.gameObject.SetActive(false);
        if (_phase == Phase.WaitToOpen)
        {
            AudioManager.Play(SoundId.ButtonClick);
            Begin(Phase.Open);
        }
        else if (_phase == Phase.WaitToCollect)
        {
            _cardIndex = -1;
            NextCard(false);
        }
    }

    private void Open()
    {
        const float lowerDuration = .24f;
        const float lidDuration = .20f;
        const float stagger = .16f;
        const float riseDuration = .44f;
        float revealStart = lowerDuration + lidDuration;
        const float clearMouth = .24f;
        float lastCardStart = revealStart + (_cards.Length - 1) * stagger;
        float exitStart = lastCardStart + riseDuration * clearMouth;
        Vector2 lowered = _packCenter + Vector2.down * (_pack.rect.height * .52f);
        Vector2 mouth = lowered + Vector2.up * (_pack.rect.yMax - _lid.rect.height);
        float lid = EaseOut(Mathf.Clamp01((_elapsed - lowerDuration) / lidDuration));
        float peel = Mathf.Clamp01((lid - .45f) / .55f);
        _lid.anchoredPosition = _lidRest + new Vector2(_lid.rect.width * .035f * peel,
            _lid.rect.height * .85f * lid);
        _lid.localRotation = Quaternion.Euler(0f, 0f, -7f * peel);
        for (int index = 0; index < _cards.Length; index++)
        {
            float progress = Mathf.Clamp01((_elapsed - revealStart - index * stagger) / riseDuration);
            if (progress <= 0f) continue;

            Image card = _cards[index];
            if (!card.gameObject.activeSelf)
            {
                card.gameObject.SetActive(true);
                AudioManager.Play(SoundId.StarPop);
            }

            RectTransform rect = card.rectTransform;
            float eased = EaseOut(progress);
            Vector2 start = mouth - Vector2.up * (rect.rect.height * .14f);
            Vector2 target = CardPosition(index);
            if (progress >= clearMouth && rect.GetSiblingIndex() < _pack.GetSiblingIndex())
                rect.SetAsLastSibling();

            float fan = EaseOut(Mathf.Clamp01((progress - clearMouth) / (1f - clearMouth)));
            float lift = Mathf.Sin(progress * Mathf.PI) * rect.rect.height * .4f;
            rect.anchoredPosition = new Vector2(Mathf.Lerp(start.x, target.x, fan),
                Mathf.Lerp(start.y, target.y, eased) + lift);
            rect.localScale = Vector3.one * Mathf.Lerp(.28f, 1f, eased);
        }

        float exit = Mathf.Clamp01((_elapsed - exitStart) / .26f);
        _pack.anchoredPosition = Vector2.Lerp(_packCenter, lowered, EaseOut(Progress(lowerDuration)))
            + Vector2.down * (_stage.rect.height * .6f * exit * exit);
        if (exit >= 1f) _pack.gameObject.SetActive(false);
        if (exit < 1f || _elapsed < lastCardStart + riseDuration) return;
        ShowCollectPrompt();
    }

    private void ShowCollectPrompt()
    {
        _summary.Clear();
        if (_result.DuplicateStars > 0)
            _summary.Append((char)('0' + _result.DuplicateStars)).Append(" stars received\n");

        for (int index = 0; index < _result.CompletedSets.Count; index++)
            _summary.Append(_result.CompletedSets[index]).Append(" complete! +3 of each booster\n");

        _summary.Append(_collectMessage);
        _prompt.SetText(_summary);
        EnableInput(Phase.WaitToCollect);
    }

    private Vector2 CardPosition(int index)
    {
        return new Vector2(_stage.rect.width * .29f * (index - (_cards.Length - 1) * .5f),
            _stage.rect.height * .06f);
    }

    private void NextCard(bool duplicate)
    {
        while (++_cardIndex < _cards.Length)
        {
            if (_result.Cards[_cardIndex].IsDuplicate != duplicate) continue;

            RectTransform card = _cards[_cardIndex].rectTransform;
            _flightStart = card.anchoredPosition;
            if (duplicate)
            {
                _destination = _progressBar.Rect.position;
                Begin(Phase.TurnIntoStar);
            }
            else
            {
                _destination = _collectionTab != null ? _collectionTab.Rect.position : _stage.position;
                Begin(Phase.FlyCard);
            }

            return;
        }

        if (!duplicate)
        {
            _cardIndex = -1;
            NextCard(true);
            return;
        }

        Action complete = _onComplete;
        Hide();
        complete?.Invoke();
    }

    private void TurnIntoStar()
    {
        float progress = Progress(.30f);
        Image card = _cards[_cardIndex];
        if (progress < .5f)
        {
            card.rectTransform.localScale = new Vector3(1f - progress * 2f, 1f, 1f);
            return;
        }

        if (card.sprite != _star)
        {
            card.sprite = _star;
            AudioManager.Play(SoundId.StarPop);
        }

        float grow = (progress - .5f) * 2f;
        float pop = Mathf.Sin(grow * Mathf.PI) * .16f;
        card.rectTransform.localScale = Vector3.one * (.7f * EaseOut(grow) + pop);
        if (progress >= 1f) Begin(Phase.FlyStar);
    }

    private void Fly()
    {
        bool duplicate = _phase == Phase.FlyStar;
        RectTransform card = _cards[_cardIndex].rectTransform;
        float progress = Progress(.42f);
        Vector3 start = _stage.TransformPoint(_flightStart);
        card.position = Vector3.Lerp(start, _destination, progress * progress);
        card.localScale = Vector3.one * Mathf.Lerp(duplicate ? .7f : 1f, .12f, progress);
        if (progress < 1f) return;

        card.gameObject.SetActive(false);
        AudioManager.Play(SoundId.RewardArrive);
        NextCard(duplicate);
    }

    private void WaitForTap(Phase phase, string text)
    {
        _prompt.SetText(text);
        EnableInput(phase);
    }

    private void EnableInput(Phase phase)
    {
        _prompt.transform.SetAsLastSibling();
        _prompt.gameObject.SetActive(true);
        _input.interactable = true;
        Begin(phase);
    }

    private void Begin(Phase phase)
    {
        _phase = phase;
        _elapsed = 0f;
    }

    private float Progress(float duration) => Mathf.Clamp01(_elapsed / duration);

    private static float EaseOut(float progress)
    {
        float remaining = 1f - progress;
        return 1f - remaining * remaining * remaining;
    }
}
