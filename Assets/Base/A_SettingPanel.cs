using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>  </summary>
public class A_SettingPanel : MonoBehaviour
{
    public Sprite On;
    public Sprite Off;
    public Button MusicBtn;
    public Button SoundBtn;
    public Text VersionText;
    public Slider MusicSlider;    // 音乐音量滑块
    public Slider SoundSlider;    // 音效音量滑块
    public Text MusicVolumeText;  // 音乐音量文本
    public Text SoundVolumeText;  // 音效音量文本

    public Button CloseBtn;

    private void Start()
    {
        Application.targetFrameRate = 60;
        CloseBtn.onClick.AddListener(()=>{
            Time.timeScale = 1;
            gameObject.SetActive(false);
        });

        // 初始化开关状态
        MusicBtn.image.sprite = PlayerPrefs.GetInt("Music", 1) == 1 ? On : Off;
        SoundBtn.image.sprite = PlayerPrefs.GetInt("Sound", 1) == 1 ? On : Off;

        // 初始化音量滑块
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float soundVolume = PlayerPrefs.GetFloat("SoundVolume", 1f);
        
        MusicSlider.value = musicVolume;
        SoundSlider.value = soundVolume;
        
        UpdateVolumeTexts();

        // 添加滑块值改变事件
        MusicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        SoundSlider.onValueChanged.AddListener(OnSoundVolumeChanged);

        VersionText.text = "Version : " + Application.version;
    }

    private void OnDestroy()
    {
        // 移除事件监听
        MusicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        SoundSlider.onValueChanged.RemoveListener(OnSoundVolumeChanged);
    }

    private void UpdateVolumeTexts()
    {
        //MusicVolumeText.text = $"{(int)(MusicSlider.value * 100)}%";
       // SoundVolumeText.text = $"{(int)(SoundSlider.value * 100)}%";
    }

    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        A_AudioManager.Instance.SetMusicVolume(value);
        UpdateVolumeTexts();
    }

    private void OnSoundVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SoundVolume", value);
        A_AudioManager.Instance.SetSoundVolume(value);
        UpdateVolumeTexts();
    }

    public void Music()
    {
        int music = PlayerPrefs.GetInt("Music", 1);
        music = music == 1 ? 0 : 1;
        PlayerPrefs.SetInt("Music", music);
        MusicBtn.image.sprite = music == 1 ? On : Off;
        A_AudioManager.Instance.ToggleMusic();
    }

    public void Sound()
    {
        int sound = PlayerPrefs.GetInt("Sound", 1);
        sound = sound == 1 ? 0 : 1;
        PlayerPrefs.SetInt("Sound", sound);
        SoundBtn.image.sprite = sound == 1 ? On : Off;
        A_AudioManager.Instance.ToggleSound();
    }
}
