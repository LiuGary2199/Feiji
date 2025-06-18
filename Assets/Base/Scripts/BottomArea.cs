using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomArea : MonoBehaviour
{
    public FlyScore m_flyScorePrefab;
    public FlyGold m_FlyGoldPrefab;

    public RectTransform m_movingObject; // 水平往复运动的物体
    public Text m_ScoreText;
    public Text m_GoldText;
    public Image m_Fireimg; 

    public Image[] m_LifeImages; // 生命值图片数组
    private RectTransform m_rectTransform;
    private int m_Score = 0;
    private int m_Gold;
    private int m_Life = 3; // 当前生命值
    private float m_width;
    private float m_height;
    
    // 水平运动参数
    private float m_moveSpeed = 2f; // 移动速度（降低到原来的1/4）
    private float m_moveRange = 350f; // 移动范围
    private float m_currentTime = 0f; // 当前时间
    private Vector2 m_startPosition; // 起始位置

    // 球数量概率的极限值
    private const float LIMIT_ONE_BALL = 0.30f;    // 1个球的极限概率
    private const float LIMIT_TWO_BALLS = 0.42f;   // 2个球的极限概率
    private const float LIMIT_THREE_BALLS = 0.28f; // 3个球的极限概率
    public int MaxScore = 1000;

    // 金币时间相关
    private bool m_isGoldTime = false;
    private float m_goldTimeRemaining = 0f;
    private const float GOLD_TIME_DURATION = 15f; // 金币时间持续15秒
    private const float GOLD_SPAWN_INTERVAL = 0.5f; // 金币生成间隔

    // 进度相关
    private float m_CurrentProgress = 0f;  // 当前进度值
    private int m_CurrentRoundScore = 0;  // 当前局数累计的分数

    private const float SPAWN_INTERVAL_1 = 5f; // 第一种生成间隔
    private const float SPAWN_INTERVAL_2 = 8f; // 第二种生成间隔
    private bool m_isGenerating = false; // 是否正在生成
    private List<FlyScore> m_activeFlyScores = new List<FlyScore>(); // 存储所有活动的FlyScore
    private Coroutine m_spawnFlyScoreCoroutine1; // 第一个生成协程
    private Coroutine m_spawnFlyScoreCoroutine2; // 第二个生成协程

    private const string GOLD_KEY = "PlayerGold"; // 金币存储的键名

    public Text shengmingtext;

    private int GetRandomBallCount()
    {
        // 如果分数小于500，只返回1个球
        if (m_Score < 500)
        {
            return 1;
        }

        // 计算基础概率变化
        float scoreFactor = Mathf.Min((m_Score - 500) / 500f, 14f); // 14是达到极限值需要的500分次数
        
        // 计算当前概率
        float oneBallProb = Mathf.Max(1f - (scoreFactor * 0.05f), LIMIT_ONE_BALL);
        float twoBallProb = Mathf.Min(scoreFactor * 0.03f, LIMIT_TWO_BALLS);
        float threeBallProb = Mathf.Min(scoreFactor * 0.02f, LIMIT_THREE_BALLS);
        
        // 生成随机数
        float random = Random.value;
        
        // 根据概率返回球的数量
        if (random < oneBallProb)
            return 1;
        else if (random < oneBallProb + twoBallProb)
            return 2;
        else
            return 3;
    }

    private void Start()
    {
        m_rectTransform = GetComponent<RectTransform>();
        m_width = m_rectTransform.rect.width;
        m_height = m_rectTransform.rect.height;
        
        // 记录起始位置
        if (m_movingObject != null)
        {
            m_startPosition = m_movingObject.anchoredPosition;
        }
        
        // 注册事件监听
        GameEventManager.OnScoreAdded += HandleScoreAdded;
        GameEventManager.OnLifeLost += HandleLifeLost;
        GameEventManager.OnGoldTimeStart += HandleGoldTimeStart;
        GameEventManager.OnGoldTimeEnd += HandleGoldTimeEnd;
        GameEventManager.OnGoldAdded += HandleGoldAdded;
        
        // 初始化生命值显示
        UpdateLifeDisplay();
        m_Fireimg.fillAmount = 0;
        m_CurrentRoundScore = 0;
        m_CurrentProgress = 0f;

        // 加载保存的金币数量
        LoadGold();
    }

    private void OnDestroy()
    {
        // 取消事件监听
        GameEventManager.OnScoreAdded -= HandleScoreAdded;
        GameEventManager.OnLifeLost -= HandleLifeLost;
        GameEventManager.OnGoldTimeStart -= HandleGoldTimeStart;
        GameEventManager.OnGoldTimeEnd -= HandleGoldTimeEnd;
        GameEventManager.OnGoldAdded -= HandleGoldAdded;
        
        // 停止所有生成协程
        if (m_spawnFlyScoreCoroutine1 != null)
        {
            StopCoroutine(m_spawnFlyScoreCoroutine1);
        }
        if (m_spawnFlyScoreCoroutine2 != null)
        {
            StopCoroutine(m_spawnFlyScoreCoroutine2);
        }
        // 清理所有FlyScore
        CleanupAllFlyScores();
    }

    private void HandleScoreAdded(int score)
    {
        // 全局分数始终累加
        m_Score += score;
        m_ScoreText.text = m_Score.ToString();
        
        // 只在非金币时间时更新当前局数分数和进度
        if (!m_isGoldTime)
        {
            m_CurrentRoundScore += score;
            m_CurrentProgress = (float)m_CurrentRoundScore / MaxScore;
            m_Fireimg.fillAmount = m_CurrentProgress;

            // 检查是否达到最大分数，触发金币时间
            if (m_CurrentRoundScore >= MaxScore)
            {
                GameEventManager.TriggerGoldTimeStart();
            }
        }
    }

    private void HandleLifeLost()
    {
        if (m_Life > 0)
        {
            m_Life--;
            UpdateLifeDisplay();
            
            // 如果生命值为0，通知游戏结束
            if (m_Life <= 0)
            {
                // 停止生成
                StopGenerating();
                // 如果正在金币时间，结束金币时间
                if (m_isGoldTime)
                {
                    GameEventManager.TriggerGoldTimeEnd();
                }
                // 通知游戏结束
                GameEventManager.TriggerGameOver();
            }
        }
    }

    private void HandleGoldTimeStart()
    {
        m_isGoldTime = true;
        m_goldTimeRemaining = GOLD_TIME_DURATION;
        
        // 启动fillAmount动画
        StartCoroutine(AnimateFillAmount());
        
        StartCoroutine(SpawnGold());
    }

    private IEnumerator AnimateFillAmount()
    {
        float startFillAmount = m_CurrentProgress;
        float elapsedTime = 0f;

        while (elapsedTime < GOLD_TIME_DURATION)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / GOLD_TIME_DURATION;
            
            // 使用插值计算当前的fillAmount
            m_Fireimg.fillAmount = Mathf.Lerp(startFillAmount, 0f, progress);
            
            yield return null;
        }

        // 确保最终值为0
        m_Fireimg.fillAmount = 0f;
        m_CurrentRoundScore = 0;  // 重置当前局数分数
        m_CurrentProgress = 0f;   // 重置进度
    }

    private void HandleGoldTimeEnd()
    {
        m_isGoldTime = false;
        m_CurrentRoundScore = 0;  // 重置当前局数分数
        m_CurrentProgress = 0f;   // 重置进度
        m_Fireimg.fillAmount = 0f;
        StopCoroutine(SpawnGold());  // 只停止生成金币的协程
        // 清理所有已生成的金币
        CleanupAllGold();
    }

    private void HandleGoldAdded(int goldAmount)
    {
        m_Gold += goldAmount;
        m_GoldText.text = m_Gold.ToString();
        SaveGold(); // 保存金币数量
    }

    private void LoadGold()
    {
        m_Gold = PlayerPrefs.GetInt(GOLD_KEY, 0);
        m_GoldText.text = m_Gold.ToString();
    }

    private void SaveGold()
    {
        PlayerPrefs.SetInt(GOLD_KEY, m_Gold);
        PlayerPrefs.Save();
    }

    private void UpdateLifeDisplay()
    {
        // 更新生命值显示
        for (int i = 0; i < m_LifeImages.Length; i++)
        {
            if (m_LifeImages[i] != null)
            {
                m_LifeImages[i].gameObject.SetActive(i < m_Life);
            }
        }
        shengmingtext.text = m_Life.ToString();
    }

    public void RestUI()
    {
        m_Score = 0;
        m_Life = 3;
        shengmingtext.text = m_Life.ToString();
        m_CurrentRoundScore = 0;  // 重置当前局数分数
        m_CurrentProgress = 0f;   // 重置进度
        m_Fireimg.fillAmount = 0f;
        m_ScoreText.text = "0";
        UpdateLifeDisplay();
        // 注意：这里不重置金币数量，因为金币是持久化的
    }

    private void Update()
    {
        if (m_movingObject != null)
        {
            // 使用正弦函数实现往复运动
            m_currentTime += Time.deltaTime;
            float xOffset = Mathf.Sin(m_currentTime * m_moveSpeed) * m_moveRange;
            
            // 更新位置
            Vector2 newPosition = m_startPosition;
            newPosition.x += xOffset;
            m_movingObject.anchoredPosition = newPosition;
        }

        // 更新金币时间
        if (m_isGoldTime)
        {
            m_goldTimeRemaining -= Time.deltaTime;
            if (m_goldTimeRemaining <= 0)
            {
                GameEventManager.TriggerGoldTimeEnd();
            }
        }
    }

    private IEnumerator SpawnFlyScore1()
    {
        while (m_isGenerating)
        {
            // 随机选择轨迹类型
            int trajectoryType = Random.Range(0, 3);
            // 根据分数计算球的数量
            int qiuCount = GetRandomBallCount();
            SpawnFlyScoreWithTrajectory(trajectoryType, qiuCount);
            
            // 固定等待5秒
            yield return new WaitForSeconds(SPAWN_INTERVAL_1);
        }
    }

    private IEnumerator SpawnFlyScore2()
    {
        while (m_isGenerating)
        {
            // 随机选择轨迹类型
            int trajectoryType = Random.Range(0, 3);
            // 根据分数计算球的数量
            int qiuCount = GetRandomBallCount();
            SpawnFlyScoreWithTrajectory(trajectoryType, qiuCount);
            
            // 固定等待8秒
            yield return new WaitForSeconds(SPAWN_INTERVAL_2);
        }
    }

    private void SpawnFlyScoreWithTrajectory(int trajectoryType, int qiuCount)
    {
        // 在宽度范围内随机生成位置
        float randomX = Random.Range(-m_width/2, m_width/2);
        Vector2 spawnPosition = new Vector2(randomX, 0+200);
        
        FlyScore flyScore = Instantiate(m_flyScorePrefab, transform);
        RectTransform flyScoreRect = flyScore.GetComponent<RectTransform>();
        flyScoreRect.anchoredPosition = spawnPosition;
        flyScore.Init(qiuCount);
        
        // 添加到活动列表
        m_activeFlyScores.Add(flyScore);
        
        // 根据轨迹类型启动不同的移动
        switch (trajectoryType)
        {
            case 0:
                flyScore.MoveStraightUp();
                break;
            case 1:
                flyScore.MoveZigzag();
                break;
            case 2:
                flyScore.MoveS();
                break;
        }
    }

    private IEnumerator SpawnGold()
    {
        while (m_isGoldTime)
        {
            // 在宽度范围内随机生成位置
            float randomX = Random.Range(-m_width/2, m_width/2);
            Vector2 spawnPosition = new Vector2(randomX, 0 + 200);
            
            // 生成金币
            FlyGold flyGold = Instantiate(m_FlyGoldPrefab, transform);
            RectTransform flyGoldRect = flyGold.GetComponent<RectTransform>();
            flyGoldRect.anchoredPosition = spawnPosition;
            
            // 等待生成间隔
            yield return new WaitForSeconds(GOLD_SPAWN_INTERVAL);
        }
    }

    public void StartGenerating()
    {
        m_isGenerating = true;
        // 启动两个生成协程
        m_spawnFlyScoreCoroutine1 = StartCoroutine(SpawnFlyScore1());
        m_spawnFlyScoreCoroutine2 = StartCoroutine(SpawnFlyScore2());
    }

    public void StopGenerating()
    {
        m_isGenerating = false;
        // 停止两个生成协程
        if (m_spawnFlyScoreCoroutine1 != null)
        {
            StopCoroutine(m_spawnFlyScoreCoroutine1);
            m_spawnFlyScoreCoroutine1 = null;
        }
        if (m_spawnFlyScoreCoroutine2 != null)
        {
            StopCoroutine(m_spawnFlyScoreCoroutine2);
            m_spawnFlyScoreCoroutine2 = null;
        }
        // 清理所有已生成的FlyScore
        CleanupAllFlyScores();
    }

    private void CleanupAllFlyScores()
    {
        foreach (var flyScore in m_activeFlyScores)
        {
            if (flyScore != null)
            {
                Destroy(flyScore.gameObject);
            }
        }
        m_activeFlyScores.Clear();
    }

    private void CleanupAllGold()
    {
        // 查找场景中所有的FlyGold对象并销毁
        FlyGold[] allGold = FindObjectsOfType<FlyGold>();
        foreach (var gold in allGold)
        {
            if (gold != null)
            {
                Destroy(gold.gameObject);
            }
        }
    }

    // 获取当前分数
    public int GetScore()
    {
        return m_Score;
    }

    // 获取当前金币
    public int GetGold()
    {
        return m_Gold;
    }

    // 恢复生命值
    public void RestoreLife()
    {
        m_Life = 3;
        shengmingtext.text = m_Life.ToString();
        UpdateLifeDisplay();
    }
}
