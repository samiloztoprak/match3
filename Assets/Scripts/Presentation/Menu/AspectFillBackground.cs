using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[ExecuteAlways]
public class AspectFillBackground : MonoBehaviour
{
    private Vector2 _lastParentSize = new Vector2(-1f, -1f);

    private void OnEnable()
    {
        _lastParentSize = new Vector2(-1f, -1f);
        Apply();
    }

    private void LateUpdate()
    {
        var parent = transform.parent as RectTransform;
        if (parent == null)
        {
            return;
        }

        if (parent.rect.size != _lastParentSize)
        {
            _lastParentSize = parent.rect.size;
            Apply();
        }
    }

    private void Apply()
    {
        var image = GetComponent<Image>();
        var rect = (RectTransform)transform;
        var parent = transform.parent as RectTransform;
        if (image.sprite == null || parent == null)
        {
            return;
        }

        Vector2 spriteSize = image.sprite.rect.size;
        float scale = Mathf.Max(parent.rect.width / spriteSize.x, parent.rect.height / spriteSize.y);

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = spriteSize * scale;
    }
}