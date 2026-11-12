using System.Collections;
using DG.Tweening;
using UnityEngine;

public class GoalFlyer : MonoBehaviour
{
    [SerializeField] private EffectSpawner _effects;
    [SerializeField] private GoalIconConfig _icons;
    [SerializeField] private SpriteRenderer _iconPrefab;
    [SerializeField] private GoalSpark _sparkPrefab;
    [SerializeField] private float _iconFlyDuration = 0.6f;
    [SerializeField] private float _sparkFlyDuration = 0.45f;
    [SerializeField] private float _arcHeight = 1.5f;
    [SerializeField] private float _arcSpread = 1.5f;

    private HudController _hud;
    private int _activeFlights;

    public void Bind(HudController hud)
    {
        _hud = hud;
    }

    public bool TryFly(Piece piece, Vector3 from, float delay)
    {
        if (_hud == null || piece == null)
        {
            return false;
        }

        GoalSlotView slot = _hud.FindSlot(piece);
        if (slot == null || !slot.TryReserve())
        {
            return false;
        }

        Vector3 to = UiToWorld(slot.Target);
        _icons.TryGet(slot.Type, slot.Color, out Sprite icon, out GoalFlyStyle style, out Color sparkColor);

        if (style == GoalFlyStyle.Spark)
        {
            FlySpark(from, to, sparkColor, delay, slot);
        }
        else
        {
            FlyIcon(icon, from, to, delay, slot);
        }
        return true;
    }

    public bool TryReserve(Piece piece, out GoalSlotView slot, out Vector3 target)
    {
        slot = null;
        target = Vector3.zero;
        if (_hud == null || piece == null)
        {
            return false;
        }

        GoalSlotView found = _hud.FindSlot(piece);
        if (found == null || !found.TryReserve())
        {
            return false;
        }

        slot = found;
        target = UiToWorld(found.Target);
        _activeFlights++;
        return true;
    }

    public void CompleteFlight(GoalSlotView slot)
    {
        _activeFlights = Mathf.Max(0, _activeFlights - 1);
        if (slot != null)
        {
            slot.OnIconArrived();
        }
    }

    public IEnumerator WaitForFlights(float timeout = 2f)
    {
        float elapsed = 0f;
        while (_activeFlights > 0 && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void FlyIcon(Sprite sprite, Vector3 from, Vector3 to, float delay, GoalSlotView slot)
    {
        SpriteRenderer icon = _effects.Spawn(_iconPrefab, from);
        icon.sprite = sprite;
        icon.sortingOrder = SortingOrders.Flight;

        Transform t = icon.transform;
        Vector3 baseScale = _iconPrefab.transform.localScale;
        t.localScale = Vector3.zero;
        _activeFlights++;

        Sequence seq = DOTween.Sequence().SetTarget(t);
        seq.AppendInterval(delay);
        seq.Append(t.DOScale(baseScale * 1.2f, 0.12f));
        seq.Append(t.DOMove(to, _iconFlyDuration).SetEase(Ease.InCubic));
        seq.Join(t.DOScale(baseScale * 0.6f, _iconFlyDuration).SetEase(Ease.InQuad));
        seq.OnComplete(() =>
        {
            CompleteFlight(slot);
            _effects.Release(icon);
        });
    }

    private void FlySpark(Vector3 from, Vector3 to, Color color, float delay, GoalSlotView slot)
    {
        GoalSpark spark = _effects.Spawn(_sparkPrefab, from);
        spark.Trail.Clear();
        spark.Sprite.color = color;
        spark.Sprite.sortingOrder = SortingOrders.Flight;
        spark.Trail.sortingOrder = SortingOrders.Flight;
        spark.Trail.startColor = color;
        spark.Trail.endColor = new Color(color.r, color.g, color.b, 0f);

        Transform t = spark.transform;
        Vector3 baseScale = _sparkPrefab.transform.localScale;
        t.localScale = baseScale;
        _activeFlights++;

        Vector3 mid = (from + to) * 0.5f;
        Vector3 side = Vector3.Cross(to - from, Vector3.forward).normalized;
        Vector3 control = mid + Vector3.up * _arcHeight + side * Random.Range(-_arcSpread, _arcSpread);

        float progress = 0f;
        Sequence seq = DOTween.Sequence().SetTarget(t);
        seq.AppendInterval(delay);
        seq.Append(t.DOPunchScale(baseScale * 0.4f, 0.12f, 4, 0.5f));
        seq.Append(DOTween.To(() => progress, v => progress = v, 1f, _sparkFlyDuration)
            .SetEase(Ease.InQuad)
            .OnUpdate(() =>
            {
                float u = 1f - progress;
                t.position = u * u * from + 2f * u * progress * control + progress * progress * to;
            }));
        seq.OnComplete(() =>
        {
            CompleteFlight(slot);
            _effects.Release(spark);
        });
    }

    public static Vector3 UiToWorld(RectTransform target)
    {
        Camera cam = Camera.main;
        Canvas canvas = target.GetComponentInParent<Canvas>();
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCamera, target.position);
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
        world.z = 0f;
        return world;
    }
}