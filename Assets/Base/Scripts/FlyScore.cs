using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class FlyScore : MonoBehaviour
{
    public Flyqiu m_flyqiuPrefab;
    public FlySave m_FlySave;
    public Image m_MainImage;        // 主物体的Image组件
    public Image m_ChildImage;       // 子物体的Image组件
    public Sprite[] m_Sprites;       // 精灵数组
    public Flygolg m_flygolgPrefab;  // 金币预制体
    private RectTransform m_rectTransform;
    private float m_moveSpeed = 200f;
    private float m_amplitude = 100f;
    private float m_frequency = 2f;
    private float m_duration = 3f;
    private float m_distance = 400f;
    private bool m_isMoving = false;
    private bool m_isFalling = false;
    private float m_fallSpeed = 800f; // 下落速度
    private List<Flyqiu> m_flyqiuList = new List<Flyqiu>();
    private Rigidbody2D m_rigidbody;
    public Text scoreText;
    private Sequence m_currentSequence; // 当前动画序列
    private bool m_isDestroying = false; // 是否正在销毁中

    private int[] scores = new int[]{ 20,25,30,35,40,45,50};

    // 爆炸相关参数
    private float m_explosionForce = 2f; // 爆炸力度

    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
        m_rigidbody = GetComponent<Rigidbody2D>();
        if (m_rigidbody == null)
        {
            m_rigidbody = gameObject.AddComponent<Rigidbody2D>();
        }
        // 初始时禁用重力和碰撞
        m_rigidbody.gravityScale = 0;
        m_rigidbody.simulated = false;
        m_rigidbody.bodyType = RigidbodyType2D.Dynamic;

        // 确保子物体Image初始时是关闭的
        if (m_ChildImage != null)
        {
            m_ChildImage.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // 清理所有动画
        if (m_currentSequence != null)
        {
            m_currentSequence.Kill();
            m_currentSequence = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Balloon"))
        {
            // 清除当前速度
            m_rigidbody.velocity = Vector2.zero;
            // 向北（上）方向施加爆炸力
            m_rigidbody.AddForce(Vector2.up * m_explosionForce, ForceMode2D.Impulse);
        }
    }

    private void PlayGoldImageAnimation(Image goldImage)
    {
        if (goldImage == null) return;

        // 设置初始状态
        goldImage.gameObject.SetActive(true);
        goldImage.color = new Color(1f, 1f, 1f, 1f);
        RectTransform goldRect = goldImage.GetComponent<RectTransform>();
        Vector2 startPos = goldRect.anchoredPosition;

        // 创建上升和淡出动画
        Sequence goldSequence = DOTween.Sequence();
        goldSequence.Append(goldRect.DOAnchorPosY(startPos.y + 150f, 0.8f).SetEase(Ease.OutQuad));
        goldSequence.Join(goldImage.DOFade(0f, 0.8f));
        goldSequence.OnComplete(() => {
            goldImage.gameObject.SetActive(false);
        });
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_isDestroying) return;

        // 检查碰撞的Layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("BottomWall"))
        {
            m_isDestroying = true;
            // 停止所有移动
            m_isMoving = false;
            m_isFalling = false;
            StopAllCoroutines();
            A_AudioManager.Instance.PlaySound("chuxian",1f);
            // 禁用刚体和碰撞
            if (m_rigidbody != null)
            {
                m_rigidbody.velocity = Vector2.zero;
                m_rigidbody.angularVelocity = 0f;
                m_rigidbody.simulated = false;
            }

            // 清除旋转
            if (m_rectTransform != null)
            {
                m_rectTransform.localRotation = Quaternion.identity;

                // 设置位置到碰撞物体的Y位置
                Vector2 currentPos = m_rectTransform.anchoredPosition;
                RectTransform collisionRect = collision.gameObject.GetComponent<RectTransform>();
                if (collisionRect != null)
                {
                    currentPos.y = collisionRect.anchoredPosition.y+40f;
                    m_rectTransform.anchoredPosition = currentPos;
                }
            }

            int aniScore = 0;
            // 显示子物体Image并随机设置精灵
            if (m_ChildImage != null && m_Sprites != null && m_Sprites.Length > 0)
            {
                // 关闭箱子
                if (m_FlySave != null)
                {
                    m_FlySave.gameObject.SetActive(false);
                }

                m_ChildImage.gameObject.SetActive(true);
                int randomIndex = UnityEngine.Random.Range(0, m_Sprites.Length);
                m_ChildImage.sprite = m_Sprites[randomIndex];
                aniScore = scores[randomIndex];
                
                // 设置分数文本初始位置和透明度
                if (scoreText != null)
                {
                    scoreText.gameObject.SetActive(true);
                    scoreText.text = string.Format("+{0}", aniScore);
                    scoreText.color = new Color(1f, 1f, 1f, 1f);
                    RectTransform scoreRect = scoreText.GetComponent<RectTransform>();
                    if (scoreRect != null)
                    {
                        Vector2 startPos = scoreRect.anchoredPosition;
                        
                        // 清理之前的动画
                        if (m_currentSequence != null)
                        {
                            m_currentSequence.Kill();
                        }
                        
                        // 创建飘动和淡出动画
                        m_currentSequence = DOTween.Sequence();
                        
                        // 使用安全的动画创建方式
                        var moveTween = scoreRect.DOAnchorPosY(startPos.y + 200f, 0.8f).SetEase(Ease.OutQuad);
                        var fadeTween = scoreText.DOFade(0f, 0.8f);
                        
                        m_currentSequence.Append(moveTween);
                        m_currentSequence.Join(fadeTween);
                        m_currentSequence.OnComplete(() => {
                            // 使用静态方法安全地处理动画完成
                            SafeHandleAnimationComplete(scoreText);
                        });
                        
                        // 设置动画的目标检查
                        moveTween.SetTarget(scoreRect);
                        fadeTween.SetTarget(scoreText);
                    }
                }
            }
        
            // 触发全局加分事件
            GameEventManager.TriggerScoreAdded(aniScore);
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("JumpBad"))
        {
            // 触发全局加分事件
            GameEventManager.TriggerScoreAdded(25);
            // 触发金币增加事件
            A_AudioManager.Instance.PlaySound("qian",1f);
            GameEventManager.TriggerGoldAdded(1);
            // 停止所有移动
            m_isMoving = false;
            m_isFalling = false;
            StopAllCoroutines();

            // 清除所有力
            if (m_rigidbody != null)
            {
                m_rigidbody.velocity = Vector2.zero;
                m_rigidbody.angularVelocity = 0f;
                // 确保刚体是启用的
                m_rigidbody.simulated = true;
                // 添加上升力
                m_rigidbody.AddForce(Vector2.up * m_explosionForce * 2f, ForceMode2D.Impulse);
            }

            // 生成金币并播放动画
            if (m_flygolgPrefab != null)
            {
                Flygolg flygolg = Instantiate(m_flygolgPrefab, transform.parent);
                flygolg.transform.position = transform.position;
                flygolg.PlayGoldAnimation();
            }
        }
    }

    // 静态方法安全地处理动画完成
    private static void SafeHandleAnimationComplete(Text scoreText)
    {
        try
        {
            if (scoreText != null && scoreText.gameObject != null)
            {
                scoreText.gameObject.SetActive(false);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"SafeHandleAnimationComplete error: {e.Message}");
        }
    }

    private IEnumerator DestroyNextFrame()
    {
        yield return null;
        try
        {
            if (this != null && gameObject != null)
            {
                Destroy(gameObject);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"DestroyNextFrame error: {e.Message}");
        }
    }

    public void Init(int qiuCount)
    {
        m_FlySave.Init();
        CreateFlyqiu(qiuCount);
        
        // 为所有Flyqiu设置销毁事件
        foreach (var qiu in m_flyqiuList)
        {
            qiu.OnDestroy = () =>
            {
                // 触发生命值减少事件
                GameEventManager.TriggerLifeLost();
                Destroy(gameObject);
            };
        }
    }

    private void CreateFlyqiu(int count)
    {
        // 清除现有的Flyqiu
        foreach (var qiu in m_flyqiuList)
        {
            if (qiu != null)
            {
                Destroy(qiu.gameObject);
            }
        }
        m_flyqiuList.Clear();

        // 根据数量创建新的Flyqiu
        switch (count)
        {
            case 1:
                // 竖直向上
                CreateSingleFlyqiu(0);
                break;
            case 2:
                // 45度分开
                CreateSingleFlyqiu(-40);
                CreateSingleFlyqiu(40);
                break;
            case 3:
                // 30度间隔
                CreateSingleFlyqiu(-30);
                CreateSingleFlyqiu(0);
                CreateSingleFlyqiu(30);
                break;
        }
    }

    private void CreateSingleFlyqiu(float angle)
    {
        Flyqiu flyqiu = Instantiate(m_flyqiuPrefab, transform);
        flyqiu.Init();
        
        // 设置旋转
        RectTransform qiuRect = flyqiu.GetComponent<RectTransform>();
        qiuRect.localRotation = Quaternion.Euler(0, 0, angle);
        
        // 设置事件
        flyqiu.OnQiu = () => 
        {
            // 关闭这个Flyqiu
            flyqiu.gameObject.SetActive(false);
            // 从列表中移除
            m_flyqiuList.Remove(flyqiu);
            // 如果所有Flyqiu都被关闭，开始下落
            if (m_flyqiuList.Count == 0)
            {
                StartFalling();
            }
        };
        
        m_flyqiuList.Add(flyqiu);
    }

    private void StartFalling()
    {
        m_isMoving = false;
        m_isFalling = true;
        // 停止所有协程
        StopAllCoroutines();

        // 启用重力和碰撞
        if (m_rigidbody != null)
        {
            m_rigidbody.gravityScale = 1;
            m_rigidbody.simulated = true;
        }
    }

    private void Update()
    {
        if (m_isMoving)
        {
            // 持续向上移动
            Vector2 currentPos = m_rectTransform.anchoredPosition;
            currentPos.y += m_moveSpeed * Time.deltaTime;
            m_rectTransform.anchoredPosition = currentPos;
        }
        else if (m_isFalling)
        {
            // 快速下落
            Vector2 currentPos = m_rectTransform.anchoredPosition;
            currentPos.y -= m_fallSpeed * Time.deltaTime;
            m_rectTransform.anchoredPosition = currentPos;

            // 如果落到地面（y <= 0），销毁物体
            if (currentPos.y <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void MoveStraightUp()
    {
        m_isMoving = true;
    }

    public void MoveZigzag()
    {
        m_isMoving = true;
        StartCoroutine(ZigzagMovement());
    }

    public void MoveS()
    {
        m_isMoving = true;
        StartCoroutine(SMovement());
    }

    private IEnumerator ZigzagMovement()
    {
        float time = 0f;
        Vector2 startPos = m_rectTransform.anchoredPosition;
        
        while (m_isMoving)
        {
            time += Time.deltaTime;
            float xOffset = Mathf.Sin(time * m_frequency) * m_amplitude;
            Vector2 currentPos = m_rectTransform.anchoredPosition;
            currentPos.x = startPos.x + xOffset;
            m_rectTransform.anchoredPosition = currentPos;
            yield return null;
        }
    }

    private IEnumerator SMovement()
    {
        float time = 0f;
        Vector2 startPos = m_rectTransform.anchoredPosition;
        
        while (m_isMoving)
        {
            time += Time.deltaTime;
            float xOffset = Mathf.Sin(time * m_frequency * 2) * m_amplitude;
            Vector2 currentPos = m_rectTransform.anchoredPosition;
            currentPos.x = startPos.x + xOffset;
            m_rectTransform.anchoredPosition = currentPos;
            yield return null;
        }
    }
}
