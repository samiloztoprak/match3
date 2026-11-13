using System;
using UnityEngine;

public class BoosterBarController : MonoBehaviour
{
    [SerializeField] private BoosterButtonView[] _buttons;

    private bool _interactable = true;

    public BoosterType? Selected { get; private set; }
    public event Action<BoosterType> InstantBoosterRequested;

    private void OnValidate()
    {
        _buttons = GetComponentsInChildren<BoosterButtonView>(true);
    }

    private void OnEnable()
    {
        foreach (BoosterButtonView button in _buttons)
        {
            button.Clicked += OnButtonClicked;
            button.SetCount(BoosterInventory.Get(button.Type));
        }
        BoosterInventory.Changed += OnInventoryChanged;
    }

    private void OnDisable()
    {
        foreach (BoosterButtonView button in _buttons)
        {
            button.Clicked -= OnButtonClicked;
        }
        BoosterInventory.Changed -= OnInventoryChanged;
    }

    public bool TryConsume(BoosterType type)
    {
        if (!BoosterInventory.TryUse(type))
        {
            return false;
        }
        if (Selected == type)
        {
            ClearSelection();
        }
        return true;
    }

    public void ClearSelection()
    {
        Select(null);
    }

    public void SetInteractable(bool interactable)
    {
        _interactable = interactable;
        if (!interactable)
        {
            ClearSelection();
        }
    }

    private void OnButtonClicked(BoosterType type)
    {
        if (!_interactable)
        {
            return;
        }

        if (BoosterInventory.Get(type) <= 0)
        {
            Find(type)?.PlayDenied();
            return;
        }

        if (type == BoosterType.Shuffle)
        {
            ClearSelection();
            InstantBoosterRequested?.Invoke(type);
            return;
        }

        Select(Selected == type ? (BoosterType?)null : type);
    }

    private void Select(BoosterType? type)
    {
        Selected = type;
        foreach (BoosterButtonView button in _buttons)
        {
            button.SetSelected(type.HasValue && button.Type == type.Value);
        }
    }

    private void OnInventoryChanged(BoosterType type, int count)
    {
        Find(type)?.SetCount(count);
    }

    private BoosterButtonView Find(BoosterType type)
    {
        foreach (BoosterButtonView button in _buttons)
        {
            if (button.Type == type)
            {
                return button;
            }
        }
        return null;
    }
}