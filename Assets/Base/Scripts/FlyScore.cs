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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查碰撞的Layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("BottomWall"))
        {
            // 触发全局加分事件
            GameEventManager.TriggerScoreAdded(25);
            
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
            m_rectTransform.localRotation = Quaternion.identity;

            // 设置位置到碰撞物体的Y位置
            Vector2 currentPos = m_rectTransform.anchoredPosition;
            RectTransform collisionRect = collision.gameObject.GetComponent<RectTransform>();
            if (collisionRect != null)
            {
                currentPos.y = collisionRect.anchoredPosition.y+40f;
                m_rectTransform.anchoredPosition = currentPos;
            }

            // 隐藏主物体Image
            if (m_MainImage != null)
            {
                m_MainImage.gameObject.SetActive(false);
            }

            // 显示子物体Image并随机设置精灵
            if (m_ChildImage != null && m_Sprites != null && m_Sprites.Length > 0)
            {
                m_ChildImage.gameObject.SetActive(true);
                int randomIndex = UnityEngine.Random.Range(0, m_Sprites.Length);
                m_ChildImage.sprite = m_Sprites[randomIndex];
            }

            // 1秒后销毁物体
            StartCoroutine(DestroyAfterDelay(1f));
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("JumpBad"))
        {
            // 触发全局加分事件
            GameEventManager.TriggerScoreAdded(25);

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
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
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
