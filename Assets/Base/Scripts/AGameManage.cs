using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;

public class AGameManage : MonoBehaviour
{
    public FlyBaby m_flyBaby;
    public BottomArea m_bottomArea;
    public ShotArea m_shotArea;
    public Button m_startButton;

    public GameOverPanel m_FuhuoPanelScript;
    public GameOverPanel m_GameOverPanelScript;
    public Button m_settingButton;
    public GameObject m_settingPanel;
    bool m_isFuhuo = false;

    void Start()
    {
         m_settingButton.onClick.AddListener(()=>
        { A_AudioManager.Instance.PlaySound("anniu",1f);
            Time.timeScale = 0;
            m_settingPanel.SetActive(true);
        });
        m_startButton.onClick.AddListener(()=>
        { A_AudioManager.Instance.PlaySound("anniu",1f);
            m_startButton.gameObject.SetActive(false);
            m_isFuhuo = false;
            m_flyBaby.StartFly();
            StartShotArea();
            StartBottomArea();
        });
        m_FuhuoPanelScript.OnJiesuan = ()=>
        {
            m_GameOverPanelScript.Show(m_bottomArea.GetScore(), m_bottomArea.GetGold());
        };

        // 注册事件监听
        GameEventManager.OnGameOver += HandleGameOver;
        GameEventManager.OnGameRestart += HandleGameRestart;
        GameEventManager.OnGameContinue += HandleGameContinue;
    }

    private void OnDestroy()
    {
        // 取消事件监听
        GameEventManager.OnGameOver -= HandleGameOver;
        GameEventManager.OnGameRestart -= HandleGameRestart;
        GameEventManager.OnGameContinue -= HandleGameContinue;
    }

    private void HandleGameOver()
    {
        if (m_isFuhuo){
             A_AudioManager.Instance.PlaySound("jiesuan",1f);
            m_GameOverPanelScript.Show(m_bottomArea.GetScore(), m_bottomArea.GetGold());
        }else{
             A_AudioManager.Instance.PlaySound("jiesuan",1f);

            m_FuhuoPanelScript.Show(m_bottomArea.GetScore(), m_bottomArea.GetGold());
        }
        // 显示游戏结束界面
        // 停止游戏
        StopGame();
    }

    private void HandleGameRestart() // 重新开始
    {
         m_isFuhuo = false;
         m_flyBaby.Rest();
         m_flyBaby.StartFly();
         m_bottomArea.RestUI();
         m_shotArea.ResetUI();
        // 开始游戏
        StartGame();
    }

    private void HandleGameContinue()// 继续游戏
    {
         m_isFuhuo = true;
         m_flyBaby.Rest();
          m_flyBaby.StartFly();
        // 恢复生命值
        m_bottomArea.RestoreLife();
        // 继续游戏
        StartGame();
    }

    public void StartGame()
    {
        // 开始BottomArea的生成
        if (m_bottomArea != null)
        {
            m_bottomArea.StartGenerating();
        }
    }

    public void StopGame()
    {
        // 停止BottomArea的生成
        if (m_bottomArea != null)
        {
            m_bottomArea.StopGenerating();
        }
    }

    private void StartShotArea()
    {
        if (m_shotArea != null)
        {
            m_shotArea.gameObject.SetActive(true);
            m_shotArea.enabled = true;
            m_shotArea.StartShooting();
        }
    }

    private void StopShotArea()
    {
        if (m_shotArea != null)
        {
            m_shotArea.StopShooting();
            m_shotArea.enabled = false;
            m_shotArea.gameObject.SetActive(false);
        }
    }

    private void StartBottomArea()
    {
        if (m_bottomArea != null)
        {
            m_bottomArea.gameObject.SetActive(true);
            m_bottomArea.enabled = true;
            m_bottomArea.StartGenerating();
        }
    }

    private void StopBottomArea()
    {
        if (m_bottomArea != null)
        {
            m_bottomArea.StopGenerating();
            m_bottomArea.enabled = false;
            m_bottomArea.gameObject.SetActive(false);
        }
    }

    public void RestGame()
    {
        m_flyBaby.Rest();
        m_bottomArea.RestUI();
        StopShotArea();
        StopBottomArea();
    }
}
