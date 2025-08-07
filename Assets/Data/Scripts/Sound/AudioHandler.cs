using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum SoundType {
    UISelect, SwordImpact, Jump, Slide, Footstep
}

[System.Serializable]
public class LinkedSound {
    public SoundType Type;
    public AudioSource audioSource;
}


[DefaultExecutionOrder(-2)]
public class AudioHandler : MonoBehaviour {
    public static AudioHandler Instance { get; private set; }

    [SerializeField] private List<LinkedSound> sounds;

    private Dictionary<SoundType, List<AudioSource>> soundDict;

    private int randomIndex;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        soundDict = new Dictionary<SoundType, List<AudioSource>>();

        foreach (var linkedSound in sounds) {
            if (linkedSound != null) {
                if (!soundDict.ContainsKey(linkedSound.Type)) {
                    soundDict[linkedSound.Type] = new List<AudioSource>();
                }
                soundDict[linkedSound.Type].Add(linkedSound.audioSource);
            }
        }
    }

    /// <summary>
    /// Play a sound from the given SoundType.
    /// Only plays the first sound of the LinkedSound
    /// </summary>
    /// <param name="sound"></param>
    public void PlaySound(SoundType sound, bool allowOverlap = true) {
        if (soundDict.TryGetValue(sound, out var audioSource)) {
            // If not allowing sound to overlap, and the audio is playing, return
            if (!allowOverlap && audioSource[0].isPlaying) return;
            audioSource[0].Play();

        } else {
            Debug.LogWarning($"No AudioSource found for SoundType {sound}");
        }
    }

    /// <summary>
    /// Play a sound at random with a given SoundType.
    /// Use "PlaySound" for single sounds.
    /// </summary>
    /// <param name="sound"></param>
    public void PlaySoundRND(SoundType sound) {
        if (soundDict.TryGetValue(sound, out var audioSources) && audioSources.Count > 0) {
            if (audioSources[randomIndex].isPlaying) return;
            randomIndex = Random.Range(0, audioSources.Count);
            audioSources[randomIndex].Play();
        } else {
            Debug.LogWarning($"No AudioSources found for SoundType {sound}");
        }
    }

    /// <summary>
    /// Toggle a looping sound.
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="play"></param>
    public void SetPlaySoundLoop(SoundType sound, bool play) {
        if (soundDict.TryGetValue(sound, out var audioSource)) {
            if (play && !audioSource[0].isPlaying) audioSource[0].Play();
            else if (audioSource[0].isPlaying) audioSource[0].Stop();
        } else Debug.LogWarning($"No AudioSource found for SoundType {sound}");
    }

    /// <summary>
    /// Pause all sound effects except for UI.
    /// </summary>
    /// <param name="pauseAll"></param>
    public void SetPauseAll(bool pauseAll) {
        foreach (var kvp in soundDict) {
            // Skip UI sounds
            if (kvp.Key == SoundType.UISelect)
                continue;

            // Loop through all AudioSources for this type
            foreach (var audioSource in kvp.Value) {
                if (pauseAll) audioSource.Pause();
                else audioSource.UnPause();
            }
        }
    }

    public void SetVolume(float volume) {
        foreach (var kvp in soundDict) {
            foreach (var audioSource in kvp.Value) {
                audioSource.volume = volume;
            }
        }
    }
}