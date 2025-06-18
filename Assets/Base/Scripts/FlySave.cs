using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlySave : MonoBehaviour
{
    public Action OnSave;
    public bool Iscolloder;
    public void Init()
    {
        Iscolloder = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Iscolloder)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("FlyBaby"))
            {
                Iscolloder = true;
                OnSave?.Invoke();
                Debug.Log("触发区域被进入: " + collision.gameObject.name);
            }
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
