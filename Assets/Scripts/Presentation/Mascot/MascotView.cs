using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MascotView : MonoBehaviour
{
    private const int NoReaction = -1;
    private const int IdleVariationPriority = 0;
    private const int HintPriority = 1;
    private const int CheerPriority = 2;
    private const int SurprisedPriority = 3;

    [SerializeField] private RectTransform _animRoot;
    [SerializeField] private Image _image;
    [SerializeField] private BoardController _board;
    [SerializeField] private GameAssets _assets;

    private MascotConfig Config => _assets.Mascot;

    private MascotMood _baseMood = MascotMood.Idle;
    private MascotMood _current = MascotMood.Idle;
    private int _currentPriority = NoReaction;
    private float _reactionEndsAt;
    private float _nextIdleVariationAt;
    private bool _levelEnded;

    private Vector2 _restPosition;
    private Sequence _action;
    private Tween _breath;

    private void OnValidate()
    {
        if (_animRoot == null) _animRoot = AutoRef.Child<RectTransform>(this, "ui_mascot_anim");
        if (_image == null) _image = AutoRef.Child<Image>(this, "ui_image_mascot");
        if (_board == null) _board = AutoRef.InScene<BoardController>();
#if UNITY_EDITOR
        if (_assets == null) _assets = AutoRef.Asset<GameAssets>();
#endif
    }

    private void OnEnable()
    {
        _board.TurnStarted += OnTurnStarted;
        _board.MovesChanged += OnMovesChanged;
        _board.HintShown += OnHintShown;
        _board.LevelEnded += OnLevelEnded;
        _board.LevelResumed += OnLevelResumed;
        
        _restPosition = _animRoot.anchoredPosition;
        _image.sprite = Config.Get(MascotMood.Idle);
        StartBreathing();
        ScheduleIdleVariation();
    }

    private void OnDisable()
    {
        _board.TurnStarted -= OnTurnStarted;
        _board.MovesChanged -= OnMovesChanged;
        _board.HintShown -= OnHintShown;
        _board.LevelEnded -= OnLevelEnded;
        _board.LevelResumed -= OnLevelResumed;
        
        _action?.Kill();
        _breath?.Kill();
    }

    private void Update()
    {
        if (_levelEnded)
        {
            return;
        }

        if (_currentPriority != NoReaction)
        {
            if (Time.time >= _reactionEndsAt)
            {
                ReturnToBase();
            }
        }
        else if (Time.time >= _nextIdleVariationAt)
        {
            MascotMood variation = Random.value < 0.5f ? MascotMood.Juggle : MascotMood.Think;
            Play(variation, IdleVariationPriority, Config.IdleVariationHold);
        }
    }
    
    private void OnTurnStarted(TurnSummary summary)
    {
        ScheduleIdleVariation();

        if (summary.HasLightBallCombo || summary.ClearedCount >= Config.HugeTurnClearCount)
        {
            Play(MascotMood.Surprised, SurprisedPriority, Config.ReactionHold);
        }
        else if (summary.HasCombo || summary.CreatedSpecial || summary.ToysCollected > 0
                 || summary.ClearedCount >= Config.GoodTurnClearCount)
        {
            Play(MascotMood.Cheer, CheerPriority, Config.ReactionHold);
        }
    }

    private void OnMovesChanged(int movesLeft)
    {
        MascotMood newBase = MascotMood.Idle;
        if (movesLeft <= Config.SadMovesThreshold)
        {
            newBase = MascotMood.Sad;
        }
        else if (movesLeft <= Config.WorriedMovesThreshold)
        {
            newBase = MascotMood.Think;
        }
        
        if (newBase == _baseMood)
        {
            return;
        }

        _baseMood = newBase;
        if (_currentPriority == NoReaction && !_levelEnded)
        {
            Show(_baseMood);
        }
    }

    private void OnHintShown()
    {
        Play(MascotMood.Point, HintPriority, Config.ReactionHold);
    }

    private void OnLevelEnded(bool won)
    {
        _levelEnded = true;
        _currentPriority = NoReaction;
        Show(won ? MascotMood.TipHat : MascotMood.Sad);
    }
    
    private void Play(MascotMood mood, int priority, float hold)
    {
        if (_levelEnded)
        {
            return;
        }
        bool busyWithHigher = _currentPriority > priority && Time.time < _reactionEndsAt;
        if (busyWithHigher)
        {
            return;
        }

        _currentPriority = priority;
        _reactionEndsAt = Time.time + hold;
        Show(mood);
    }

    private void ReturnToBase()
    {
        _currentPriority = NoReaction;
        Show(_baseMood);
        ScheduleIdleVariation();
    }

    private void ScheduleIdleVariation()
    {
        _nextIdleVariationAt = Time.time + Random.Range(Config.IdleVariationMin, Config.IdleVariationMax);
    }


    private void Show(MascotMood mood)
    {
        _action?.Kill();
        _animRoot.anchoredPosition = _restPosition;
        _animRoot.localRotation = Quaternion.identity;
        _animRoot.localScale = Vector3.one;

        bool changeSprite = mood != _current;
        _current = mood;

        _action = DOTween.Sequence().SetTarget(_animRoot);
        if (changeSprite)
        {
            _action.Append(_animRoot.DOScale(new Vector3(1.08f, 0.9f, 1f), 0.08f).SetEase(Ease.OutQuad));
            _action.AppendCallback(() => _image.sprite = Config.Get(mood));
            _action.Append(_animRoot.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
        }
        AppendMoodMotion(_action, mood);
    }

    private void AppendMoodMotion(Sequence seq, MascotMood mood)
    {
        switch (mood)
        {
            case MascotMood.Cheer:
                seq.Append(_animRoot.DOAnchorPosY(_restPosition.y + 25f, 0.15f).SetEase(Ease.OutQuad).SetLoops(4, LoopType.Yoyo));
                break;
            case MascotMood.Surprised:
                seq.Append(_animRoot.DOShakeAnchorPos(0.4f, 12f, 25, 90f, false, true));
                break;
            case MascotMood.Point:
                seq.Append(_animRoot.DOAnchorPosX(_restPosition.x + 8f, 0.2f).SetEase(Ease.InOutSine).SetLoops(4, LoopType.Yoyo));
                break;
            case MascotMood.TipHat:
                seq.Append(_animRoot.DORotate(new Vector3(0f, 0f, 8f), 0.25f).SetEase(Ease.OutQuad));
                seq.Append(_animRoot.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutBack));
                break;
            case MascotMood.Sad:
                seq.Append(_animRoot.DORotate(new Vector3(0f, 0f, -6f), 0.4f).SetEase(Ease.OutQuad));
                seq.Join(_animRoot.DOAnchorPosY(_restPosition.y - 8f, 0.4f).SetEase(Ease.OutQuad));
                break;
        }
    }

    private void StartBreathing()
    {
        Transform t = _image.transform;
        t.localScale = Vector3.one;
        _breath = t.DOScaleY(1.03f, 1.2f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }
    
    private void OnLevelResumed()
    {
        _levelEnded = false;
        ReturnToBase();
    }
}