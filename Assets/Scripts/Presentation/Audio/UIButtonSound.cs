using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private SoundId _sound = SoundId.ButtonClick;

    private void OnValidate()
    {
        if (_button == null) _button = GetComponent<Button>();
    }

    private void OnEnable() => _button.onClick.AddListener(PlaySound);
    private void OnDisable() => _button.onClick.RemoveListener(PlaySound);

    private void PlaySound()
    {
        AudioManager.Play(_sound);
    }
}