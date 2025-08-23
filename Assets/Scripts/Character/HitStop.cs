// HitStop.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class HitStop : MonoBehaviour
{
    private static HitStop _i;
    private Coroutine _routine;
    private float _extend;

    [Header("Defaults (Global)")]
    [Tooltip("Varsayılan hit stop süresi (saniye)")]
    public float defaultDuration = 0.04f;

    [Tooltip("Varsayılan kamera sallama şiddeti")]
    public float defaultCamShake = 0.1f;

    [Header("Hooks")]
    [Tooltip("Kamera sarsıntısını bağlamak için (örn. Cinemachine impulse, custom shaker vs.)")]
    public UnityEvent<float> onShake;

    void Awake() => _i = this;

    /// <summary>
    /// Global değerlerle hit stop uygular (defaultDuration + defaultCamShake).
    /// </summary>
    public static void Do()
    {
        if (_i == null) return;
        _i.InternalShake(_i.defaultCamShake);
        _i.StartOrExtend(_i.defaultDuration);
    }

    /// <summary>
    /// İstersen sadece süreyi override edebilirsin; cam shake global kalır.
    /// </summary>
    public static void Do(float durationOverride)
    {
        if (_i == null) return;
        _i.InternalShake(_i.defaultCamShake);
        _i.StartOrExtend(Mathf.Max(0f, durationOverride));
    }

    /// <summary>
    /// Süre + cam shake ikisini de override etmek istersen.
    /// </summary>
    public static void Do(float durationOverride, float camShakeOverride)
    {
        if (_i == null) return;
        _i.InternalShake(Mathf.Max(0f, camShakeOverride));
        _i.StartOrExtend(Mathf.Max(0f, durationOverride));
    }

    private void InternalShake(float amount)
    {
        if (amount > 0f) onShake?.Invoke(amount);
    }

    private void StartOrExtend(float duration)
    {
        if (_routine != null)
        {
            _extend = Mathf.Max(_extend, duration);
            return;
        }
        _routine = StartCoroutine(Run(duration));
    }

    private IEnumerator Run(float duration)
    {
        Time.timeScale = 0f;
        float remaining = duration;
        _extend = 0f;

        while (remaining > 0f)
        {
            yield return null; // unscaled
            remaining -= Time.unscaledDeltaTime;
            if (_extend > 0f)
            {
                remaining = Mathf.Max(remaining, _extend);
                _extend = 0f;
            }
        }

        Time.timeScale = 1f;
        _routine = null;
    }
}
