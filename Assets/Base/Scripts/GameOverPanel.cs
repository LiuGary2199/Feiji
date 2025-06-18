using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    public Button m_RestartButton;    // 重新开始按钮
    public Button m_AdContinueButton; // 看广告继续按钮
    public Text m_ScoreText;          // 显示最终分数
    public Button m_JieSuanButton; // 看广告继续按钮
    public Action OnJiesuan;
    public Action OnADFail;
    public GameObject FailPanel;
    private Coroutine m_failPanelCoroutine; // 存储FailPanel协程的引用

    private void Start()
    {
        // 确保FailPanel初始时是关闭的
        if (FailPanel != null)
        {
            FailPanel.SetActive(false);
        }

        if (m_RestartButton!=null){
          m_RestartButton.onClick.AddListener(() =>
        {
            A_AudioManager.Instance.PlaySound("anniu",1f);
           OnRestartClick();
        });
        }

        if (m_JieSuanButton!=null){
            m_JieSuanButton.onClick.AddListener(() =>
                {
                    A_AudioManager.Instance.PlaySound("anniu",1f);
                    gameObject.SetActive(false);
                    OnJiesuan?.Invoke();
                });
        }
        
        m_AdContinueButton.onClick.AddListener(() =>
        {
            A_ADManager.Instance.playRewardVideo((success) =>
            {
                if (success)
                {
                    A_AudioManager.Instance.PlaySound("anniu",1f);
                    Hide();
                    GameEventManager.TriggerGameContinue();
                }
                else
                {
                  OnADFail?.Invoke();
                  ShowFailPanel();
                }
            });
        });
    }

    private void OnDestroy()
    {
        // 取消按钮点击事件
         if (m_RestartButton!=null){
             m_RestartButton.onClick.RemoveListener(OnRestartClick);
         }
         if (m_AdContinueButton!=null){
            m_AdContinueButton.onClick.RemoveListener(OnAdContinueClick);
         }
        
        // 停止FailPanel协程
        if (m_failPanelCoroutine != null)
        {
            StopCoroutine(m_failPanelCoroutine);
        }
    }

    // 显示FailPanel
    private void ShowFailPanel()
    {
        if (FailPanel != null)
        {
            // 停止之前的协程
            if (m_failPanelCoroutine != null)
            {
                StopCoroutine(m_failPanelCoroutine);
            }
            
            FailPanel.SetActive(true);
            m_failPanelCoroutine = StartCoroutine(HideFailPanelAfterDelay(1f));
        }
    }

    // 延迟关闭FailPanel的协程
    private IEnumerator HideFailPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (FailPanel != null)
        {
            FailPanel.SetActive(false);
        }
        m_failPanelCoroutine = null;
    }

    // 显示游戏结束界面
    public void Show(int score, int gold)
    {
        gameObject.SetActive(true);
        // 确保FailPanel是关闭的
        if (FailPanel != null)
        {
            FailPanel.SetActive(false);
        }
        // 更新分数和金币显示
        m_ScoreText.text = score.ToString();
    }

    // 隐藏游戏结束界面
    public void Hide()
    {
        gameObject.SetActive(false);
        // 同时关闭FailPanel
        if (FailPanel != null)
        {
            FailPanel.SetActive(false);
        }
        // 停止FailPanel协程
        if (m_failPanelCoroutine != null)
        {
            StopCoroutine(m_failPanelCoroutine);
            m_failPanelCoroutine = null;
        }
    }

    // 重新开始按钮点击事件
    private void OnRestartClick()
    {
        // 隐藏游戏结束界面
        Hide();
        // 重置游戏
        GameEventManager.TriggerGameRestart();
    }

    // 看广告继续按钮点击事件
    private void OnAdContinueClick()
    {
        // TODO: 这里添加广告SDK的调用
        // 如果广告播放成功，继续游戏
        if (PlayAd())
        {
            // 隐藏游戏结束界面
            Hide();
            // 继续游戏
            GameEventManager.TriggerGameContinue();
        }
    }

    // 播放广告的方法（需要根据实际使用的广告SDK来实现）
    private bool PlayAd()
    {
        // TODO: 实现广告播放逻辑
        Debug.Log("播放广告");
        return true; // 临时返回true，实际应该根据广告播放结果返回
    }
} 