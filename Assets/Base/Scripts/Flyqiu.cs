using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Flyqiu : MonoBehaviour
{
    public Action OnQiu;
    public Action OnDestroy;
    public bool Iscolloder;
    public Sprite[] m_Sprites; // 图片数组
    private Image m_Image; // 自身的Image组件

    private void Awake()
    {
        // 获取自身的Image组件
        m_Image = GetComponent<Image>();
    }

    public void Init()
    {
        Iscolloder = false;
        // 随机选择一张图片
        if (m_Sprites != null && m_Sprites.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, m_Sprites.Length);
            m_Image.sprite = m_Sprites[randomIndex];
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Iscolloder)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("FlyBaby"))
            { 
                FlyBaby flyScore = collision.gameObject.GetComponent<FlyBaby>();
                if (flyScore != null){
                if (flyScore.m_canCollide == true)
                    A_AudioManager.Instance.PlaySound("qiubao",1f);
                    Iscolloder = true;
                    OnQiu?.Invoke();
                    Debug.Log("碰撞: " + collision.gameObject.name);
                    flyScore.StartCollisionCooldown();
                }
            }
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("TopWall"))
        {
            OnDestroy?.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("FlyBaby"))
        {
            Iscolloder = false;
        }
    }
}
