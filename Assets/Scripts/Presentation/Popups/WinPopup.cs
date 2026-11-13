using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinPopup : PopupView
{
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private Image[] _stars;
    [SerializeField] private Button _continueButton;

    private int _starCount;

    protected override void OnValidate()
    {
        base.OnValidate();
        if (_levelText == null) _levelText = AutoRef.Child<TMP_Text>(this, "ui_text_level_value");
        if (_continueButton == null) _continueButton = AutoRef.Child<Button>(this, "ui_button_continue");
        if (_stars == null || _stars.Length != 3)
        {
            _stars = new[]
            {
                AutoRef.Child<Image>(this, "ui_image_star_1"),
                AutoRef.Child<Image>(this, "ui_image_star_2"),
                AutoRef.Child<Image>(this, "ui_image_star_3")
            };
        }
    }

    private void OnEnable() => _continueButton.onClick.AddListener(OnContinue);
    private void OnDisable() => _continueButton.onClick.RemoveListener(OnContinue);

    public void Setup(int levelNumber, int stars)
    {
        _levelText.text = "LEVEL " + levelNumber;
        _starCount = stars;
        foreach (Image star in _stars)
        {
            star.transform.DOKill();
            star.transform.localScale = Vector3.zero;
        }
    }

    protected override void OnOpened()
    {
        for (int i = 0; i < _starCount && i < _stars.Length; i++)
        {
            Transform star = _stars[i].transform;
            star.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetDelay(i * 0.25f);
            star.DOPunchRotation(new Vector3(0f, 0f, 20f), 0.4f).SetDelay(i * 0.25f + 0.2f);
        }
    }

    private void OnContinue()
    {
        LevelProgress.CurrentIndex++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}