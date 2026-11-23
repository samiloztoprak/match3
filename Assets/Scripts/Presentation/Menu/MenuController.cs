using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private TMP_Text _playText;
    [SerializeField] private LevelCatalog _levels;
    
    private void OnValidate()
    {
        if (_playButton == null) _playButton = AutoRef.Child<Button>(this, "ui_button_play");
        if (_playText == null) _playText = AutoRef.Child<TMP_Text>(this, "ui_text_play_value");
#if UNITY_EDITOR
        if (_levels == null) _levels = AutoRef.Asset<LevelCatalog>();
#endif
    }

    private void OnEnable()
    {
        _playButton.onClick.AddListener(OnPlay);
    }

    private void OnDisable()
    {
        _playButton.onClick.RemoveListener(OnPlay);
        _playButton.transform.DOKill();
    }

    private void Start()
    {
        AudioManager.EnsureCreated();
        
        _playText.text = "LEVEL " + (LevelProgress.CurrentIndex + 1);
        _playButton.interactable = true;

        PlayIntro();
    }

    private void PlayIntro()
    {
        Transform play = _playButton.transform;
        play.localScale = Vector3.zero;
        play.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetDelay(0.3f);
    }

    private void OnPlay()
    {
        _playButton.interactable = false;
        SceneLoader.LoadGame();
    }
}