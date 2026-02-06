using UnityEngine;
using System;
using UnityEngine.Animations;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    // �V���O���g���Q��
    public static SoundManager Instance;

    [SerializeField] private AudioMixer mixer;
    private const string MASTER_VOLUME = "MasterVolume";

    // ���y / ���ʉ��f�[�^
    public Sound[] musicSounds, effectSounds;

    // �Đ��p AudioSource
    public AudioSource musicSource, sfxSource;

    private bool isMuted;

    // Singleton ������
    private void Awake()
    {
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
        => PlaySound(musicSounds, musicSource, name);

    // ���ʉ��̃~���[�g�؂�ւ�
    public void ToggleSFX()
        => sfxSource.mute = !sfxSource.mute;

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

    public void ToggleMute()
    {
        SetMuted(!isMuted);
    }

    public void SetMuted(bool mute)
    {
        isMuted = mute;
        mixer.SetFloat(MASTER_VOLUME, mute ? -80f : 0f);
    }

    public bool IsMuted() => isMuted;
}
