using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameController : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Button _restartButton;

    private bool _isWin;
    
    private void Awake()
    {
        _panel.SetActive(false);
        _restartButton.onClick.AddListener(Restart);
    }

    public void ShowWin()
    {
        _messageText.text = "Level Complete!";
        _panel.SetActive(true);
        _isWin = true;
    }

    public void ShowLose()
    {
        _messageText.text = "Out of Moves";
        _panel.SetActive(true);
        _isWin = false;
    }

    public void Restart()
    {
        if (_isWin)
        {
            LevelProgress.CurrentIndex++;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}