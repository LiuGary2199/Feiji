using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UIռ
using DG.Tweening;

public class FlyBaby : MonoBehaviour
{
    // 飞行控制参数
    public float forwardSpeed = 5f;     // 向前移动速度
    public float upForce = 10f;         // 向上的力
    public float maxAngle = 40f;        // 最大角度
    public float rotationSpeed = 5f;    // 旋转速度
    private float balloonCooldown = 0.4f;  // 气球碰撞后的冷却时间
    private float lastBalloonTime;         // 上次气球碰撞时间
    public float explosionForce = 30f;  // 爆炸力度
    public float explosionAngle = 30f;  // 爆炸角度范围

    private Rigidbody2D rb;
    public RectTransform birdImage;     // 小鸟图片的RectTransform

    // 音效组件引用
    [SerializeField] private AudioSource flySound;
    [SerializeField] private AudioSource hitSound;

    // 飞行方向控制
    private bool isFlyingRight = true;  // 默认向右方向

    // 层ID，在Start中初始化
    private int rightWallLayerID;
    private int leftWallLayerID;
    private int obstacleLayerID;
    private int balloonLayerID;
    private int BottomLayerID;
    private int ShejianLayerID;

    public bool m_canCollide = true; // 是否可以碰撞
    private RectTransform m_rectTransform;

    public Image m_MainImage;        // 主物体的Image组件
    public Image m_ChildImage;       // 子物体的Image组件
    public Sprite[] m_Sprites;       // 精灵数组
    private Sequence m_currentSequence; // 当前动画序列
    private bool m_isDestroying = false; // 是否正在销毁中

    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
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


    private IEnumerator DestroyNextFrame()
    {
        yield return null;
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    public void Rest()
    {
                rb.bodyType = RigidbodyType2D.Static;
        m_rectTransform.anchoredPosition = new Vector2(-450, 700);
        // 清理所有动画
        if (m_currentSequence != null)
        {
            m_currentSequence.Kill();
            m_currentSequence = null;
        }
        m_isDestroying = false;
    }

    public void StartFly()
    {m_canCollide = true; // 是否可以碰撞
        rb.bodyType = RigidbodyType2D.Dynamic;
        Jump();
    }
    // 启动碰撞冷却
    public void StartCollisionCooldown()
    {
        m_canCollide = false;
        StartCoroutine(CollisionCooldown());
    }

    private IEnumerator CollisionCooldown()
    {
        yield return new WaitForSeconds(0.1f);
        m_canCollide = true;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lastBalloonTime = -balloonCooldown; // 初始化时允许跳跃
        m_rectTransform = GetComponent<RectTransform>();
        // 初始化层ID
        rightWallLayerID = LayerMask.NameToLayer("RightWall");
        leftWallLayerID = LayerMask.NameToLayer("LeftWall");
        obstacleLayerID = LayerMask.NameToLayer("Obstacle");
        balloonLayerID = LayerMask.NameToLayer("Balloon");
        BottomLayerID = LayerMask.NameToLayer("BottomWall");
        ShejianLayerID= LayerMask.NameToLayer("JianShe");

        // 初始化刚体组件
        if (rb != null)
        {
            rb.gravityScale = 3f;       // 重力大小
            rb.drag = 0.5f;             // 空气阻力
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 连续碰撞检测
        }
        rb.bodyType = RigidbodyType2D.Static;
        m_rectTransform.anchoredPosition = new Vector2(-450, 700);
    }

    void Update()
    {
        // 向前移动的逻辑
        if (rb != null)
        {
            float xSpeed = isFlyingRight ? forwardSpeed : -forwardSpeed;
            rb.velocity = new Vector2(xSpeed, rb.velocity.y);
        }

        // 检测跳跃
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && 
            Time.time - lastBalloonTime >= balloonCooldown)
        {
            Jump();
        }

        // 更新角度
        UpdateBirdAngle();
    }

    // 小鸟跳跃
    void Jump()
    {
        if (rb != null)
        {
            // 垂直速度不施加向上的力
            float currentXSpeed = isFlyingRight ? forwardSpeed : -forwardSpeed;
            rb.velocity = new Vector2(currentXSpeed, 0);
            rb.AddForce(Vector2.up * upForce, ForceMode2D.Impulse);
            A_AudioManager.Instance.PlaySound("tan",1f);
            // 播放声音效果
            if (flySound != null)
            {
                flySound.Play();
            }
        }
    }

    // 更新小鸟角度
    void UpdateBirdAngle()
    {
        if (rb == null || birdImage == null) return;

        // 根据垂直速度计算目标角度
        float targetAngle = 0;

        // 当向上时，角度从0到maxAngle
        if (rb.velocity.y > 0)
        {
            targetAngle = Mathf.Lerp(0, maxAngle, rb.velocity.y / 10f);
        }
        else if (rb.velocity.y < 0)
        {
            targetAngle = Mathf.Lerp(0, -maxAngle, -rb.velocity.y / 10f);
        }

        // 旋转小鸟图片，使其localRotation
        birdImage.localRotation = Quaternion.Euler(0, 0, targetAngle);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == BottomLayerID)
        {
             GameEventManager.TriggerLifeLost();
       // 使用两倍的跳跃力
            float currentXSpeed = isFlyingRight ? forwardSpeed : -forwardSpeed;
            rb.velocity = new Vector2(currentXSpeed, 0);
            rb.AddForce(Vector2.up * (upForce * 2), ForceMode2D.Impulse);
            Debug.Log("触底");
        }
    }


    // 碰撞检测
    private void OnTriggerEnter2D(Collider2D other)
    {
        
         if (other.gameObject.layer == ShejianLayerID)
        {
            GameEventManager.TriggerLifeLost();
       // 使用两倍的跳跃力
            float currentXSpeed = isFlyingRight ? forwardSpeed : -forwardSpeed;
            rb.velocity = new Vector2(currentXSpeed, 0);
            rb.AddForce(Vector2.up * (upForce * 2), ForceMode2D.Impulse);
            Debug.Log("射箭");
        }
        // 使用层ID检测右墙
        if (other.gameObject.layer == rightWallLayerID)
        {
            ChangeDirection(false); // 碰到右墙向左飞
        }
        // 使用层ID检测左墙
        else if (other.gameObject.layer == leftWallLayerID)
        {
            ChangeDirection(true);  // 碰到左墙向右飞
        }
        // 检测障碍物
        else if (other.gameObject.layer == obstacleLayerID)
        {
            // 播放碰撞音效或处理其他逻辑
            if (hitSound != null)
            {
                hitSound.Play();
            }
        }
        // 检测气球
        else if (other.gameObject.layer == balloonLayerID)
        {
            // 清除当前速度
            rb.velocity = Vector2.zero;
            
            // 使用两倍的跳跃力
            float currentXSpeed = isFlyingRight ? forwardSpeed : -forwardSpeed;
            rb.velocity = new Vector2(currentXSpeed, 0);
            rb.AddForce(Vector2.up * (upForce * 2), ForceMode2D.Impulse);

            // 记录气球碰撞时间
            lastBalloonTime = Time.time;

            // 播放碰撞音效
            if (hitSound != null)
            {
                hitSound.Play();
            }
        }
    }

    // 改变飞行方向
    void ChangeDirection(bool flyRight)
    {
        isFlyingRight = flyRight;

        // 旋转小鸟图片，使其localScale
        if (birdImage != null)
        {
            Vector3 scale = birdImage.localScale;
            scale.x = flyRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            birdImage.localScale = scale;
        }

        // 碰撞效果
        if (hitSound != null)
        {
            hitSound.Play();
        }
    }
}