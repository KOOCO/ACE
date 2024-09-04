using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WaitingView : MonoBehaviour
{
    DateTime startTime;
    float waitTime;
    Transform parent;

    private void Update()
    {
        if ((DateTime.Now - startTime).TotalSeconds >= waitTime)
        {
            ViewManager.Instance?.CloseWaitingView(parent);
        }
    }

    /// <summary>
    /// 設定等待資料
    /// </summary>
    /// <param name="parent">產生介面</param>
    /// <param name="waitTime">自動關閉時間</param>
    public void SetWaitingDate(Transform parent, float waitTime)
    {
        startTime = DateTime.Now;
        this.parent = parent;
        this.waitTime = waitTime;
    }
}
