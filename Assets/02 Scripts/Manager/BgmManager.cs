using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BgmManager : Singleton<BgmManager>
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _bgmClip;

    [Range(0f, 1f)]
    [SerializeField] private float _volume = 0.4f;

    protected override void Init()
    {
        _audioSource.clip = _bgmClip;
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f;
        _audioSource.volume = _volume;

        Play();
    }

    public void Play()
    {
        if (_audioSource.isPlaying)
        {
            return;
        }

        _audioSource.Play();
    }
}