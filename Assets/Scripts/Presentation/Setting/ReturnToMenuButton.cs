using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ReturnToMenuButton : MonoBehaviour
{
    [SerializeField] private Button _button;

    private void OnValidate()
    {
        if (_button == null) _button = GetComponent<Button>();
    }

    private void OnEnable() => _button.onClick.AddListener(OnClick);
    private void OnDisable() => _button.onClick.RemoveListener(OnClick);

    private void OnClick()
    {
        _button.interactable = false;
        SceneLoader.LoadMenu();
    }
}