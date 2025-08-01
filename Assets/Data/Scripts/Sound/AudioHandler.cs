using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum SoundType {
    UISelect, SwordImpact, Jump, Slide
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

    private Dictionary<SoundType, AudioSource> soundDict;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        soundDict = new Dictionary<SoundType, AudioSource>();
        foreach (var linkedSound in sounds) {
            if (linkedSound != null && !soundDict.ContainsKey(linkedSound.Type)) {
                soundDict.Add(linkedSound.Type, linkedSound.audioSource);
            }
        }
    }

    public void PlaySound(SoundType sound) {
        if (soundDict.TryGetValue(sound, out var audioSource)) {
            audioSource.Play();
        } else {
            Debug.LogWarning($"No AudioSource found for SoundType {sound}");
        }
    }

    public void SetPlaySoundLoop(SoundType sound, bool play) {
        if (soundDict.TryGetValue(sound, out var audioSource)) {
            if (play && !audioSource.isPlaying) audioSource.Play();
            else if (audioSource.isPlaying) audioSource.Stop();
        } else Debug.LogWarning($"No AudioSource found for SoundType {sound}");
    }

    public void SetPauseAll(bool pauseAll) {
        foreach (var kvp in soundDict) {
            // Only pause/unpause if NOT a UI sound
            if (kvp.Key != SoundType.UISelect) {
                if (pauseAll) kvp.Value.Pause();
                else kvp.Value.UnPause();
            }
        }
    }

}