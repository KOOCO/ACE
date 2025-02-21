using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;

public class AndroidHB : MonoBehaviour
{
    public static AndroidHB Instance;

    bool isListenered;
    string serverStatus;

    private void Awake()
    {
        Instance = this;
    }

    #region 心跳 in Android
    private bool isHeartbeatScheduled = false;
    public void initHeartBeat(string userID)
    {
        if (!isListenered)
        {
            print("開啟心跳監聽");
            isListenered = true;
            JSBridgeManager.Instance.StartListeningForDataChanges(
                        $"{Entry.Instance.releaseType}/{FirebaseManager.HEARTBEAT_DATA_PATH}/{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}/{userID}",
                    gameObject.name,
                    nameof(delayCallHeartbeat));
            print("MenberID: " + userID);
        }
    }

    void StartHeartbeat()
    {
        heartbeatData HB = null;

        HB = new heartbeatData(true, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(), PlayerPrefs.GetString("PlayerStatus"), serverStatus);

        string data = JsonConvert.SerializeObject(HB);

        JSBridgeManager.Instance.UpdateDataToFirebase(
                    $"{Entry.Instance.releaseType}/{FirebaseManager.HEARTBEAT_DATA_PATH}/{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}/{DataManager.UserId}",
                    data,
                    gameObject.name,
                    nameof(checkUpdate));
        JSBridgeManager.Instance.GetPlayerIPAddress();
    }
    public void delayCallHeartbeat(string jsonData)
    {
        //print("After 5 second in Entry: " + jsonData);

        if (!string.IsNullOrEmpty(jsonData) && jsonData != "null")
        {
            var hb = JsonConvert.DeserializeObject<heartbeatData>(jsonData);
            string pStatus = hb.playerStatus;
            string sStatus = hb.serverStatus;
            PlayerPrefs.SetString("PlayerStatus", pStatus);
            serverStatus = sStatus;
            //PlayerPrefs.SetString("ServerStatus", sStatus);
            LoginView loginView = FindAnyObjectByType<LoginView>();
            if (loginView != null)
                loginView.checkIsMaintenance(serverStatus);
            LobbyView lobbyView = FindAnyObjectByType<LobbyView>();
            if (lobbyView != null)
                lobbyView.checkIsMaintenance(serverStatus);
            //print($"PS: {PlayerPrefs.GetString("PlayerStatus")}, SS: {PlayerPrefs.GetString("ServerStatus")}");
        }
        else
        {
            PlayerPrefs.SetString("PlayerStatus", "normal");
            serverStatus = "normal";
            //PlayerPrefs.SetString("ServerStatus", sStatus);
            LoginView loginView = FindAnyObjectByType<LoginView>();
            if (loginView != null)
                loginView.checkIsMaintenance(serverStatus);
            LobbyView lobbyView = FindAnyObjectByType<LobbyView>();
            if (lobbyView != null)
                lobbyView.checkIsMaintenance(serverStatus);
            PlayerPrefs.Save();
        }

        // 只在沒有排程時才開始倒數
        if (!isHeartbeatScheduled)
        {
            isHeartbeatScheduled = true;
            StartCoroutine(HeartbeatCooldown());
        }
    }
    public void checkUpdate(string s)
    {
        print("Result: " + s);
        int nullC = PlayerPrefs.GetInt("nullData");
        if (s == "false")
        {
            nullC++;
            PlayerPrefs.SetInt("nullData", nullC);
            PlayerPrefs.Save();
        }
        //print(nullC);
    }

    IEnumerator HeartbeatCooldown()
    {
        yield return new WaitForSeconds(5);
        StartHeartbeat();
        isHeartbeatScheduled = false;
    }

    public void stopListenHB()
    {
        JSBridgeManager.Instance.StopListeningForDataChanges(
                        $"{Entry.Instance.releaseType}/{FirebaseManager.HEARTBEAT_DATA_PATH}/{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}/{DataManager.UserId}");
        isListenered = false;
    }
    #endregion
}
