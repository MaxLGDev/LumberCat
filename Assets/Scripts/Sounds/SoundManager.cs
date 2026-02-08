using System;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    // �V���O���g���Q��
    public static SoundManager Instance;

    [SerializeField] private AudioMixer mixer;
    private const string MASTER_VOLUME = "MasterVolume";
    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";

    private const string MUSIC_PREF = "MusicVolume";
    private const string SFX_PREF = "SFXVolume";

    // ���y / ���ʉ��f�[�^
    public Sound[] musicSounds, effectSounds;

    // �Đ��p AudioSource
    public AudioSource musicSource, sfxSource;

    private bool isMuted;

    // Singleton ������
    private void Awake()
    {
        if (mixer == null)
            Debug.LogError("No mixer assigned", this);

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        float music = PlayerPrefs.GetFloat(MUSIC_PREF, 0.5f);
        float sfx = PlayerPrefs.GetFloat(SFX_PREF, 0.5f);

        Debug.Log($"[INIT] Music pref: {music}, SFX pref: {sfx}");
        SetMusicVolume(music);
        SetSFXVolume(sfx);

        DebugMixerValue(MUSIC_VOLUME);
        DebugMixerValue(SFX_VOLUME);
        DebugMixerValue(MASTER_VOLUME);
    }

    private void DebugMixerValue(string param)
    {
        if (mixer.GetFloat(param, out float value))
            Debug.Log($"[MIXER] {param} = {value} dB");
        else
            Debug.LogError($"[MIXER] PARAM NOT FOUND: {param}");
    }

    // �T�E���h�������Đ��̋��ʏ���
    private void PlaySound(
        Sound[] soundArray,
        AudioSource source,
        string name,
        bool useOneShot = false)
    {
        if (source == null)
        {
            Debug.LogWarning("AudioSource ���ݒ肳��Ă��܂���");
            return;
        }

        Sound sound = Array.Find(soundArray, s => s.soundName == name);

        if (sound == null)
        {
            Debug.LogWarning($"Sound '{name}' not found!");
            return;
        }

        if (useOneShot)
        {
            source.PlayOneShot(sound.audioClip);
        }
        else
        {
            source.clip = sound.audioClip;
            source.Play();
        }
    }

    // BGM �Đ�
    public void PlayMusic(string name)
    {
        if (musicSource.isPlaying)
            musicSource.Stop();

        PlaySound(musicSounds, musicSource, name);
    }

    public void PauseAll()
    {
        if (musicSource.isPlaying)
            musicSource.Pause();

        if(sfxSource.isPlaying)
            sfxSource.Pause();
    }

    public void ResumeAll()
    {
        if (!musicSource.isPlaying && musicSource.clip != null)
            musicSource.UnPause();

        if (!sfxSource.isPlaying && sfxSource.clip != null)
            sfxSource.UnPause();
    }

    // ���ʉ��Đ��i�s�b�`�w��j
    public void PlaySFX(string name, float pitch = 1f)
    {
        Sound sfxSound =
            Array.Find(effectSounds, s => s.soundName == name);

        if (sfxSound == null)
        {
            Debug.LogWarning($"Sound '{name}' not found!");
            return;
        }

        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(sfxSound.audioClip);
        sfxSource.pitch = 1f;
    }

    // WRAPPER FOR BUTTON USE
    public void PlaySFX(string name)
    {
        PlaySFX(name, 1f);
    }

    private void SetVolume(string parameter, float value)
    {
        // Slider safety
        value = Mathf.Clamp01(value);

        // Convert 0-1 to Db
        float db = value == 0 ? -80f : Mathf.Log10(value) * 20f;
        mixer.SetFloat(parameter, db);
    }

    public void SetMusicVolume(float value)
    {
        SetVolume(MUSIC_VOLUME, value);
        PlayerPrefs.SetFloat(MUSIC_PREF, value);
    }

    public void SetSFXVolume(float value)
    {
        Debug.Log($"[CALL] SetSFXVolume({value})");
        SetVolume(SFX_VOLUME, value);
        PlayerPrefs.SetFloat(SFX_PREF, value);
    }

    public void ToggleMute() => SetMuted(!isMuted);

    public void SetMuted(bool mute)
    {
        isMuted = mute;
        mixer.SetFloat(MASTER_VOLUME, mute ? -80f : 0f);
    }

    public bool IsMuted() => isMuted;

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            PlayerPrefs.Save();
    }
}
