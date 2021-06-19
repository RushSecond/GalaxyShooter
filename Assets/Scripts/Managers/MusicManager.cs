using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [System.Serializable]
    class MusicData
    {
        [SerializeField]
        public AudioClip musicClip;
        [SerializeField]
        public float volume = -1;
        [SerializeField]
        public float pitch = -1;
        [SerializeField]
        public float fadeInTime = 0;
    }

    [SerializeField]
    MusicData _deathMusic;
    [SerializeField]
    MusicData _bossMusic;
    [SerializeField]
    MusicData _victoryMusic;

    AudioSource _myAudio;
    float _defaultVolume;
    float _defaultPitch;

    // Start is called before the first frame update
    void Start()
    {
        _myAudio = GetComponent<AudioSource>();
        if (_myAudio == null) Debug.LogError("Music Manager does not have an audio source");
        _defaultVolume = _myAudio.volume;
        _defaultPitch = _myAudio.pitch;
    }

    public void OnPlayerDeath()
    {
        StartCoroutine(MusicFadeInRoutine(_deathMusic));
    }

    public void OnBossAppear()
    {
        StartCoroutine(MusicFadeInRoutine(_bossMusic));
    }

    public void OnVictory()
    {
        StartCoroutine(MusicFadeInRoutine(_victoryMusic));
    }

    IEnumerator MusicFadeInRoutine(MusicData musicData)
    {
        float elapsedTime = 0;
        float progress = 0;
        float maxVolume = musicData.volume < 0 ? _defaultVolume : musicData.volume;
        _myAudio.pitch = musicData.pitch < 0 ? _defaultPitch : musicData.pitch;
        _myAudio.clip = musicData.musicClip;
        _myAudio.Play();
        while (progress < 1)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            progress = elapsedTime / musicData.fadeInTime;
            _myAudio.volume = Mathf.Lerp(0, maxVolume, progress);
        }
    }

    public void StopMusic(float fadeOutTime)
    {
        StartCoroutine(MusicFadeOut(fadeOutTime));
    }

    IEnumerator MusicFadeOut(float fadeOutTime)
    {
        float elapsedTime = 0;
        float progress = 0;
        float myVolume = _myAudio.volume;
        while (progress < 1)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            progress = elapsedTime / fadeOutTime;
            _myAudio.volume = Mathf.Lerp(myVolume, 0, progress);
        }
    }
}
