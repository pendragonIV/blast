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

        await UniTask.WaitWhile(() => audioSource.isPlaying);
        _audioSources.Release(audioSource);
    }
}
