using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public float volume = 1;
    public bool loop = false;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM")]
    public Sound[] bgmSounds;

    [Header("SE")]
    public Sound[] seSounds;

    private Dictionary<string, AudioSource> bgmSources = new Dictionary<string, AudioSource>();
    private Dictionary<string, Sound> seData = new Dictionary<string, Sound>();
    private Dictionary<string, AudioClip> seClips = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioSource> seSources = new Dictionary<string, AudioSource>(); // 루프 SE용
    private float originalBGMVolume = 1.0f;

    private AudioSource seSource; // 일반 SE용

    private void Start()
    {
        LoadAndApplyAudioSettings();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitSounds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitSounds()
    {
        // BGM 초기화
        foreach (var sound in bgmSounds)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = sound.clip;
            source.volume = sound.volume;
            source.loop = sound.loop;
            source.playOnAwake = false;

            bgmSources[sound.name] = source;
        }

        // 일반 SE용 AudioSource
        seSource = gameObject.AddComponent<AudioSource>();
        seSource.loop = false;
        seSource.playOnAwake = false;

        // SE 초기화
        foreach (var sound in seSounds)
        {
            seData[sound.name] = sound;
            seClips[sound.name] = sound.clip; // 기존 호환성 유지

            // 루프 SE는 전용 AudioSource 생성
            if (sound.loop)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.clip = sound.clip;
                source.volume = sound.volume;
                source.loop = true;
                source.playOnAwake = false;

                seSources[sound.name] = source;
                Debug.Log($"루프 SE '{sound.name}' 초기화 완료");
            }
        }
    }

    public void PlayBGM(string name)
    {
        if (bgmSources.TryGetValue(name, out AudioSource source))
        {
            StopAllBGM();
            source.Play();
        }
        else
        {
            Debug.LogWarning($"BGM '{name}' not found!");
        }
    }

    public void StopAllBGM()
    {
        foreach (var pair in bgmSources)
        {
            if (pair.Value.isPlaying)
                pair.Value.Stop();
        }
    }

    public void StopBGM(string name)
    {
        if (bgmSources.TryGetValue(name, out AudioSource source))
        {
            if (source.isPlaying)
            {
                source.Stop();
                Debug.Log($"BGM '{name}' 정지");
            }
        }
    }

    public void PlaySE(string name)
    {
        if (seData.TryGetValue(name, out Sound sound))
        {
            if (sound.loop)
            {
                // 루프 SE - 전용 AudioSource 사용
                if (seSources.TryGetValue(name, out AudioSource source))
                {
                    if (!source.isPlaying)
                    {
                        source.Play();
                        Debug.Log($"SE '{name}' 루프 재생 시작");
                    }
                }
            }
            else
            {
                // 일반 SE - PlayOneShot 사용 (기존 방식)
                StartCoroutine(PlaySoundAndWait(name));
            }
        }
        else
        {
            Debug.LogWarning($"SE '{name}' not found!");
        }
    }

    // 모든 SE 정지 (파라미터 없음)
    public void StopSE()
    {
        // 일반 SE 정지
        seSource.Stop();

        // 모든 루프 SE 정지
        foreach (var source in seSources.Values)
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
        }

        Debug.Log("모든 SE 정지");
    }

    // 특정 SE만 정지 (오버로드)
    public void StopSE(string name)
    {
        if (seSources.TryGetValue(name, out AudioSource source))
        {
            if (source.isPlaying)
            {
                source.Stop();
                Debug.Log($"SE '{name}' 정지");
            }
        }
        else
        {
            Debug.LogWarning($"루프 SE '{name}'을 찾을 수 없습니다.");
        }
    }

    private IEnumerator PlaySoundAndWait(string name)
    {
        if (seClips.TryGetValue(name, out AudioClip clip))
        {
            seSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }
    }

    public void SetBGMVolume(float volume)
    {
        foreach (var pair in bgmSources)
        {
            pair.Value.volume = volume;
        }
    }

    public void SetSEVolume(float volume)
    {
        seSource.volume = volume;

        // 루프 SE 볼륨도 조정
        foreach (var source in seSources.Values)
        {
            source.volume = volume;
        }
    }

    public void TemporarilyMuteBGM()
    {
        if (bgmSources.Count > 0)
        {
            var firstSource = bgmSources.Values.GetEnumerator();
            if (firstSource.MoveNext())
            {
                originalBGMVolume = firstSource.Current.volume;
            }
        }

        SetBGMVolume(0f);
        Debug.Log("BGM 음소거 활성화");
    }

    public void RestoreBGMVolume()
    {
        SetBGMVolume(originalBGMVolume);
        Debug.Log($"BGM 볼륨 복원: {originalBGMVolume}");
    }

    public IEnumerator TemporarilyMuteBGMForDuration(float duration)
    {
        TemporarilyMuteBGM();
        yield return new WaitForSeconds(duration);
        RestoreBGMVolume();
    }

    private void LoadAndApplyAudioSettings()
    {
        float savedBGMVolume = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
        SetBGMVolume(savedBGMVolume);

        float savedSEVolume = PlayerPrefs.GetFloat("SEVolume", 1.0f);
        SetSEVolume(savedSEVolume);

        Debug.Log($"오디오 설정 자동 로드 완료 - BGM: {savedBGMVolume}, SE: {savedSEVolume}");
    }
}