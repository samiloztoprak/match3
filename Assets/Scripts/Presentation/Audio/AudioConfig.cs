using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Match3/AudioConfig")]
public class AudioConfig : ScriptableObject
{
    [Serializable]
    public class SoundEntry
    {
        public SoundId Id;
        public AudioClip[] Clips;
        [Range(0f, 1f)] public float Volume = 1f;
        [Range(0f, 0.3f)] public float PitchVariation = 0.05f;
        public float MinInterval = 0.05f;
    }

    [Header("Music")]
    public AudioClip Music;
    [Range(0f, 1f)] public float MusicVolume = 0.5f;

    [Header("Sounds")]
    [SerializeField] private List<SoundEntry> _sounds = new List<SoundEntry>();

    public bool TryGet(SoundId id, out SoundEntry entry)
    {
        foreach (SoundEntry sound in _sounds)
        {
            if (sound.Id == id)
            {
                entry = sound;
                return true;
            }
        }
        entry = null;
        return false;
    }
}