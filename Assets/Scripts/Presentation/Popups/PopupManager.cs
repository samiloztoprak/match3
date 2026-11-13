using UnityEngine;

public class PopupManager : MonoBehaviour
{
    [SerializeField] private PopupView[] _popups;

    private PopupView _current;

    public bool IsAnyOpen => _current != null && _current.IsOpen;

    private void OnValidate()
    {
        _popups = GetComponentsInChildren<PopupView>(true);
    }

    public T Show<T>() where T : PopupView
    {
        T popup = Find<T>();
        if (popup == null)
        {
            Debug.LogError($"[PopupManager] No {typeof(T).Name} under {name}");
            return null;
        }

        if (_current != null && _current != popup && _current.IsOpen)
        {
            _current.Close();
        }
        _current = popup;
        popup.Open();
        return popup;
    }

    public void CloseCurrent()
    {
        if (_current != null && _current.IsOpen)
        {
            _current.Close();
        }
    }

    private T Find<T>() where T : PopupView
    {
        foreach (PopupView popup in _popups)
        {
            if (popup is T typed)
            {
                return typed;
            }
        }
        return null;
    }
}