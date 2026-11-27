using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WheelPage : MonoBehaviour
{
    [SerializeField] private GameAssets _assets;
    [SerializeField] private WheelView _wheel;
    [SerializeField] private WheelProgressBar _progressBar;
    [SerializeField] private MenuTabBar _tabBar;
    [SerializeField] private Button _spinButton;
    [SerializeField] private TMP_Text _spinsText;
    [SerializeField] private CardPackReveal _packReveal;
    
    private readonly Dictionary<WheelRewardType, IRewardPresenter> _presenters = new Dictionary<WheelRewardType, IRewardPresenter>();
    private readonly IRandom _random = new SystemRandom();
    private bool _built;
    private bool _busy;
    private Action _finishReward;
    private Action _presentSpin;
    private WheelSpinResult _pendingSpin;
    
    private void OnValidate()
    {
        if (_wheel == null) _wheel = GetComponentInChildren<WheelView>(true);
        if (_progressBar == null) _progressBar = GetComponentInChildren<WheelProgressBar>(true);
        if (_tabBar == null) _tabBar = AutoRef.InScene<MenuTabBar>();
        if (_spinButton == null) _spinButton = AutoRef.Child<Button>(this, "ui_button_spin");
        if (_spinsText == null) _spinsText = AutoRef.Child<TMP_Text>(this, "ui_text_spins_count");
        if (_packReveal == null) _packReveal = GetComponentInChildren<CardPackReveal>(true);
#if UNITY_EDITOR
        if (_assets == null) _assets = AutoRef.Asset<GameAssets>();
#endif
    }

    private void Awake()
    {
        _finishReward = Finish;
        _presentSpin = PresentSpin;
        foreach (IRewardPresenter presenter in GetComponentsInChildren<IRewardPresenter>(true))
        {
            _presenters.TryAdd(presenter.Type, presenter);
        }
    }
    
    internal void Prepare()
    {
        if (_built) return;

        _wheel.Build(_assets.Wheel.Segments);
        _built = true;
    }

    private void OnEnable()
    {
        Prepare();
        _spinButton.onClick.AddListener(OnSpin);
        WheelProgress.Changed += OnProgressChanged;
        Refresh(true);
    }

    private void OnDisable()
    {
        _spinButton.onClick.RemoveListener(OnSpin);
        WheelProgress.Changed -= OnProgressChanged;
        if (_packReveal != null) _packReveal.Hide();
        _pendingSpin = default;
        _busy = false;
        _tabBar.Locked = false;
    }

    private void OnProgressChanged()
    {
        if (!_busy) Refresh(true);
    }
    
    private void Refresh(bool animate)
    {
        for (int i = 0; i < _assets.Wheel.Segments.Length; i++)
            _wheel.SetSegment(i, WheelService.RewardAt(_assets.Wheel, i));

        int spins = WheelProgress.Spins;
        _progressBar.Show(WheelProgress.StarBank, _assets.Wheel.StarsPerSpin, animate);
        _spinsText.text = "x" + spins;
        _spinButton.interactable = spins > 0 && !_busy;
    }

    private void OnSpin()
    {
        if (_busy) return;
        int spinsBefore = WheelProgress.Spins;
        _busy = true;
        if (!WheelService.TrySpin(_assets.Wheel, _random, out WheelSpinResult result))
        {
            _busy = false;
            
            Refresh(false);
            return;
        }
        
        _tabBar.Locked = true;
        _spinButton.interactable = false;
        _spinsText.text = "x" + (spinsBefore - 1);
        AudioManager.Play(SoundId.ButtonClick);
        _pendingSpin = result;
        _wheel.SpinTo(result.SegmentIndex, _presentSpin);
    }

    private void PresentSpin()
    {
        WheelSpinResult result = _pendingSpin;
        _pendingSpin = default;
        Vector3 from = _wheel.SlotPosition(result.SegmentIndex);
        if (result.Reward.Type == WheelRewardType.CardPack)
        {
            if (_packReveal != null)
                _packReveal.Play(result.Pack, from, _tabBar.GetButton(MenuPage.Collection), _progressBar, _finishReward);
            else
                Finish();
            return;
        }

        if (!_presenters.TryGetValue(result.Reward.Type, out IRewardPresenter presenter))
        {
            Finish();
            return;
        }

        presenter.Present(new RewardContext(result.Reward, from), _finishReward);
    }

    private void Finish()
    {
        _busy = false;
        _tabBar.Locked = false;
        _wheel.ResumeIdle();
        Refresh(true);
    }
}
