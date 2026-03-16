using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField]
    private AudioSource _audioSourcePrefab;
    private IObjectPool<AudioSource> _audioSources;

    protected override void Awake() 
    {
        base.Awake();

        _audioSources = new ObjectPool<AudioSource>(
            createFunc: () => { return Instantiate(_audioSourcePrefab, transform); },
            actionOnGet: (audioSource) => audioSource.gameObject.SetActive(true),
            actionOnRelease: (audioSource) => audioSource.gameObject.SetActive(false),
            actionOnDestroy: (audioSource) => Destroy(audioSource.gameObject),
            maxSize: 10
        );
    }

    public async void PlayOneShot(AudioClip audioClip)
    {
        var audioSource = _audioSources.Get();
        audioSource.PlayOneShot(audioClip);

        // Prevent the await from accessing a destroyed AudioSource when exiting play mode.
        // When this MonoBehaviour is destroyed (e.g. stopping play mode), the cancellation token will cancel.
        var cancellationToken = this.GetCancellationTokenOnDestroy();

        try
        {
            await UniTask.WaitWhile(() => audioSource != null && audioSource.isPlaying, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // If we get here, the object was destroyed (play mode exited), so nothing more to do.
            return;
        }

        if (audioSource != null)
            _audioSources.Release(audioSource);
    }
}
