using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Flygolg : MonoBehaviour
{
    public Image goldImag;
    private RectTransform m_rectTransform;

    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
    }

    public void PlayGoldAnimation()
    {
        if (goldImag == null) return;

        // 设置初始状态
        goldImag.gameObject.SetActive(true);
        goldImag.color = new Color(1f, 1f, 1f, 1f);
        Vector2 startPos = m_rectTransform.anchoredPosition;

        // 创建上升和淡出动画
        Sequence goldSequence = DOTween.Sequence();
        goldSequence.Append(m_rectTransform.DOAnchorPosY(startPos.y + 150f, 0.8f).SetEase(Ease.OutQuad));
        goldSequence.Join(goldImag.DOFade(0f, 0.8f));
        goldSequence.OnComplete(() => {
            goldImag.gameObject.SetActive(false);
            // 销毁物体
            Destroy(gameObject);
        });
    }
}
