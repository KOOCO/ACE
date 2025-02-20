using Newtonsoft.Json;
using System;
using UnityEngine;

public class MouseClickIdleDetector : MonoBehaviour
{
    [SerializeField]
    private float timeSinceLastClick = 0f; // 距离上次点击的时间累计
    [SerializeField]
    private float idleThreshold = 25f;    // 定义闲置时间阈值

    bool isIdle;

    void Update()
    {
        // 检测鼠标左键或右键的点击事件
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            timeSinceLastClick = 0f; // 重置时间累计
            PlayerPrefs.SetString("PlayerIsOnline", "True");
            WebAndroidHB.Instance.initHeartBeat(DataManager.UserId);
        }
        else
        {
            // 使用 Time.deltaTime 累积时间
            timeSinceLastClick += Time.deltaTime;
            //print("閒置時間: " + timeSinceLastClick);
        }

        // 检测是否超过闲置时间
        if (timeSinceLastClick > idleThreshold)
        {
            if (!isIdle)
            {
                Debug.Log("player is idle");
                WebAndroidHB.Instance.stopListenHB();
                isIdle = true;
            }

            if (timeSinceLastClick > 40)
            {
                JSBridgeManager.Instance.ReadDataFromFirebase(
                $"{Entry.Instance.releaseType}/{FirebaseManager.HEARTBEAT_DATA_PATH}/{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}/{DataManager.UserId}",
                gameObject.name,
                nameof(checkCallHeartbeat));
                timeSinceLastClick = 0;
                isIdle = false;
            }
        }
    }

    void checkCallHeartbeat(string jsonData)
    {
        print("is Online data: " + jsonData);
        if (!string.IsNullOrEmpty(jsonData) && jsonData != "null")
        {
            var hb = JsonConvert.DeserializeObject<heartbeatData>(jsonData);
            string pStatus = hb.isOnline.ToString();
            print("Player is Online: " + pStatus);
            PlayerPrefs.SetString("PlayerIsOnline", pStatus);
            PlayerPrefs.Save();
        }
    }
}
