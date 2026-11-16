using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private const int SfxSourceCount = 8;

    private static AudioManager _instance;

    private AudioConfig _config;
    private AudioSource _music;
    private readonly List<AudioSource> _sfx = new List<AudioSource>();
    private readonly Dictionary<SoundId, float> _lastPlayed = new Dictionary<SoundId, float>();
    private int _nextSfx;

    private static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("AudioManager");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<AudioManager>();
                _instance.Init(Resources.Load<AudioConfig>("AudioConfig"));
            }
            return _instance;
        }
    }

    public static void EnsureCreated()
    {
        _ = Instance;
    }

    public static void Play(SoundId id)
    {
        Instance.PlaySound(id);
    }

    private void Init(AudioConfig config)
    {
        _config = config;

        _music = gameObject.AddComponent<AudioSource>();
        _music.loop = true;
        _music.playOnAwake = false;

        for (int i = 0; i < SfxSourceCount; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            _sfx.Add(source);
        }

        GameSettings.Changed += OnSettingChanged;

        if (_config == null)
        {
            Debug.LogWarning("[AudioManager] Resources/AudioConfig not found, audio is off");
            return;
        }

        _music.clip = _config.Music;
        _music.volume = _config.MusicVolume;
        if (_music.clip != null && GameSettings.Get(SettingType.Music))
        {
            _music.Play();
        }
    }

    private void OnDestroy()
    {
        GameSettings.Changed -= OnSettingChanged;
    }

    private void OnSettingChanged(SettingType type, bool on)
    {
        if (type != SettingType.Music || _music.clip == null) return;
        if (on) _music.Play();
        else _music.Stop();
    }

    private void PlaySound(SoundId id)
    {
        if (_config == null || !GameSettings.Get(SettingType.Sound)) return;
        if (!_config.TryGet(id, out AudioConfig.SoundEntry entry) || entry.Clips == null || entry.Clips.Length == 0) return;

        float now = Time.unscaledTime;
        if (_lastPlayed.TryGetValue(id, out float last) && now - last < entry.MinInterval) return;
        _lastPlayed[id] = now;

        AudioSource source = _sfx[_nextSfx];
        _nextSfx = (_nextSfx + 1) % _sfx.Count;
        source.pitch = 1f + Random.Range(-entry.PitchVariation, entry.PitchVariation);
        source.PlayOneShot(entry.Clips[Random.Range(0, entry.Clips.Length)], entry.Volume);
    }
}