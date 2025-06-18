using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyGold : MonoBehaviour
{
    private RectTransform m_rectTransform;
    private float m_moveSpeed = 200f; // 上升速度
    private bool m_isMoving = true;

    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("FlyBaby"))
        {
            // 触发金币增加事件
            GameEventManager.TriggerGoldAdded(1);
            Destroy(gameObject);
        }
    }
}
