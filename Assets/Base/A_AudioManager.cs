using UnityEngine;
using System.Collections.Generic;

public class A_AudioManager : MonoBehaviour
{
    public static A_AudioManager Instance { get; private set; }
    public List<AudioClip> Audios = new List<AudioClip>();
    // 音乐音频源
    private AudioSource musicAudioSource;
    // 音效音频源池
    private List<AudioSource> SoundAudioSources = new List<AudioSource>();
    // 音效音频源数量
    public int SoundPoolSize = 5;

    // 音乐开关
    private bool isMusicOn = true;
    // 音效开关
    private bool isSoundOn = true;
    // 音乐音量
    private float musicVolume = 1f;
    // 音效音量
    private float soundVolume = 1f;

    private void Awake()
    {
        Instance = this;

        isMusicOn = PlayerPrefs.GetInt("Music", 1) == 1;
        isSoundOn = PlayerPrefs.GetInt("Sound", 1) == 1;
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        soundVolume = PlayerPrefs.GetFloat("SoundVolume", 1f);

        // 初始化音乐音频源
        musicAudioSource = gameObject.AddComponent<AudioSource>();
        musicAudioSource.playOnAwake = false;
        musicAudioSource.loop = true;
        musicAudioSource.volume = musicVolume;

        // 初始化音效音频源池
        for (int i = 0; i < SoundPoolSize; i++)
        {
            AudioSource SoundSource = gameObject.AddComponent<AudioSource>();
            SoundSource.playOnAwake = false;
            SoundSource.volume = soundVolume;
            SoundAudioSources.Add(SoundSource);
        }

        PlayMusic("BGM");
    }

    // 设置音乐音量
    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = volume;
        }
    }

    // 设置音效音量
    public void SetSoundVolume(float volume)
    {
        soundVolume = volume;
        foreach (var source in SoundAudioSources)
        {
            if (source != null)
            {
                source.volume = volume;
            }
        }
    }

    // 切换音乐开关状态
    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        if (isMusicOn)
        {
            if (musicAudioSource.clip == null)
                musicAudioSource.clip = GetAudioClip("BGM");
            if (musicAudioSource.clip != null && !musicAudioSource.isPlaying)
            {
                musicAudioSource.Play();
            }
        }
        else
        {
            musicAudioSource.Pause();
        }
    }

    // 切换音效开关状态
    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
    }

    // 播放音乐
    public void PlayMusic(string name, float volume = 1f)
    {
        if (!isMusicOn) return;

        AudioClip musicClip = GetAudioClip(name);
        if (musicClip != null)
        {
            musicAudioSource.clip = musicClip;
            musicAudioSource.volume = musicVolume * volume;
            musicAudioSource.Play();
        }
    }

    // 播放音效
    public void PlaySound(string name, float volume = 1f)
    {
        if (!isSoundOn) return;

        AudioClip SoundClip = GetAudioClip(name);
        if (SoundClip != null)
        {
            foreach (AudioSource SoundSource in SoundAudioSources)
            {
                if (!SoundSource.isPlaying)
                {
                    SoundSource.clip = SoundClip;
                    SoundSource.volume = soundVolume * volume;
                    SoundSource.Play();
                    return;
                }
            }
            // 如果所有音效源都在使用，可以动态创建新的音频源
            AudioSource newSoundSource = gameObject.AddComponent<AudioSource>();
            newSoundSource.playOnAwake = false;
            newSoundSource.volume = soundVolume * volume;
            SoundAudioSources.Add(newSoundSource);
            newSoundSource.clip = SoundClip;
            newSoundSource.Play();
        }
    }

    private AudioClip GetAudioClip(string name)
    {
        foreach (var clip in Audios)
        {
            if (clip.name == name)
            {
                return clip;
            }
        }
        return null;
    }
}