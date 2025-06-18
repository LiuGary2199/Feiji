using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ShotArea : MonoBehaviour
{
    public RectTransform m_heightReference; // 用于确定箭的高度
    public GameObject m_shotPrefab; // 发射的预制体
    public Image m_flashImage; // 闪烁的图片
    public float m_shotInterval = 15f; // 发射间隔改为15秒
    public float m_shotSpeed = 500f; // 发射速度
    public float m_flashDuration = 1f; // 闪烁持续时间
    public float m_offsetDistance = 75f; // 闪烁位置偏移距离
    public Text m_ShotCountText; // 射击次数显示
    public Button m_ShotButton; // 射击按钮

    private float m_screenHeight;
    private float m_screenWidth;
    private bool m_isFlashing = false;
    private float m_prefabWidth; // 预制体宽度
    private Coroutine m_shotCoroutine; // 存储发射协程的引用
    private int m_ShotCount = 0; // 射击次数
    public RectTransform m_jian; 
    private void Start()
    {
        // 使用Screen获取屏幕尺寸
        m_screenHeight = Screen.height;
        m_screenWidth = Screen.width;
        m_flashImage.gameObject.SetActive(false);

        // 获取预制体宽度
        if (m_shotPrefab != null)
        {
            RectTransform prefabRect = m_shotPrefab.GetComponent<RectTransform>();
            if (prefabRect != null)
            {
                m_prefabWidth = prefabRect.rect.width;
            }
        }
    }

    public void StartShooting()
    {
        if (m_shotCoroutine != null)
        {
            StopCoroutine(m_shotCoroutine);
        }
        m_shotCoroutine = StartCoroutine(ShotRoutine());
    }

    public void StopShooting()
    {
        if (m_shotCoroutine != null)
        {
            StopCoroutine(m_shotCoroutine);
            m_shotCoroutine = null;
        }
        m_flashImage.gameObject.SetActive(false);
    }

    private IEnumerator ShotRoutine()
    {
        // 等待15秒后开始第一次发射
        yield return new WaitForSeconds(15f);

        while (true)
        {
            // 随机选择发射方向（左到右或右到左）
            bool isLeftToRight = Random.value > 0.5f;
            
            // 使用参考RectTransform的Y位置作为箭的高度
            float arrowY = m_heightReference.anchoredPosition.y;
            
            // 设置闪烁位置（在屏幕内侧）
            Vector2 flashPos = new Vector2(
                isLeftToRight ? -m_screenWidth/2 + m_offsetDistance : m_screenWidth/2 - m_offsetDistance,
                arrowY
            );
            m_flashImage.rectTransform.anchoredPosition = flashPos;
            
            // 开始闪烁
            yield return StartCoroutine(FlashEffect());
            
            // 发射预制体
            ShootPrefab(isLeftToRight, arrowY);
            
            // 等待下一次发射
            yield return new WaitForSeconds(m_shotInterval);
        }
    }

    private IEnumerator FlashEffect()
    {
        m_isFlashing = true;
        m_flashImage.gameObject.SetActive(true);
        
        float elapsedTime = 0f;
        float flashInterval = 0.5f; // 初始闪烁间隔
        
        while (elapsedTime < m_flashDuration)
        {
            // 闪烁效果
            m_flashImage.DOFade(0f, flashInterval/2).SetEase(Ease.InOutQuad);
            yield return new WaitForSeconds(flashInterval/2);
            m_flashImage.DOFade(1f, flashInterval/2).SetEase(Ease.InOutQuad);
            yield return new WaitForSeconds(flashInterval/2);
            
            // 更新闪烁间隔，使闪烁越来越快
            flashInterval = Mathf.Lerp(0.5f, 0.1f, elapsedTime / m_flashDuration);
            elapsedTime += flashInterval;
        }
        
        m_flashImage.gameObject.SetActive(false);
        m_isFlashing = false;
    }

    private void ShootPrefab(bool isLeftToRight, float yPos)
    {
        GameObject shot = Instantiate(m_shotPrefab, transform);
        RectTransform shotRect = shot.GetComponent<RectTransform>();
        
        // 设置初始位置（在屏幕外）
        Vector2 startPos = new Vector2(
            isLeftToRight ? -m_screenWidth/2 - m_prefabWidth : m_screenWidth/2 + m_prefabWidth,
            yPos
        );
        shotRect.anchoredPosition = startPos;
        
        // 设置目标位置（完全离开屏幕）
        Vector2 targetPos = new Vector2(
            isLeftToRight ? m_screenWidth/2 + m_prefabWidth : -m_screenWidth/2 - m_prefabWidth,
            yPos
        );
        
        // 根据射击方向设置箭的旋转
        if (isLeftToRight)
        {
            // 从左往右射击时，水平翻转箭
            shotRect.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            // 从右往左射击时，保持原始方向
            shotRect.localScale = new Vector3(1, 1, 1);
        }
        
        // 计算移动时间
        float distance = Vector2.Distance(startPos, targetPos);
        float duration = distance / m_shotSpeed;
        
        // 移动预制体
        shotRect.DOAnchorPos(targetPos, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() => Destroy(shot));
    }

    // 重置UI状态
    public void ResetUI()
    {
        // 重置射击次数
        m_ShotCount = 0;
        // 更新射击次数显示
        if (m_ShotCountText != null)
        {
            m_ShotCountText.text = m_ShotCount.ToString();
        }
        // 重置射击按钮状态
        if (m_ShotButton != null)
        {
            m_ShotButton.interactable = true;
        }
    }

    // 重置射击计时
    public void ResetShotTimer()
    {
        // 停止当前射击协程
        if (m_shotCoroutine != null)
        {
            StopCoroutine(m_shotCoroutine);
            m_shotCoroutine = null;
        }
        // 隐藏闪烁效果
        m_flashImage.gameObject.SetActive(false);
        m_isFlashing = false;
    }
}
