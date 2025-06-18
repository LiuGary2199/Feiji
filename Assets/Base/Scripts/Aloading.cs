using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Aloading : MonoBehaviour
{
    public Text ProgressText;
    public Image ProgressImage;
    public Button startbtn;
    public GameObject progress;
    private float m_Progress;
    bool m_Running;
    private void Start()
    {
        m_Progress = 0;
        ProgressText.text = "0%";
        ProgressImage.fillAmount = 0;
        m_Running = false;
        startbtn.onClick.AddListener(() => 
        { A_AudioManager.Instance.PlaySound("anniu",1f);
            gameObject.SetActive(false);
        });
    }
    
    private void Update()
    {
        m_Progress += Time.deltaTime;
        ProgressImage.fillAmount = m_Progress;
        ProgressText.text = (int)(m_Progress * 100) + "%";
        if (m_Progress >= 1&& !m_Running)
        { 
            m_Running = true;
            progress.SetActive(false);
            startbtn.gameObject.SetActive(true);
            return;
        }
    }
}
