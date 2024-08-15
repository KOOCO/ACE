using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using UnityEngine.Events;

public class Entry : UnitySingleton<Entry>
{
    #region 測試
    public static GameServer CurrGameServer;
    public static class TestInfoData
    {      
        public static string LocalUserId = "LocalUser";
        public static string NickName = "LocalUserName";

        public static int newPlayerId = 10;
        public static double LocalUserCrypto = 11000;
        public static double LocalUserVirtual = 230200;

        public static DateTime foldTimd = DateTime.Now;
    }
    #endregion

    [Header("版本號")]
    public string version;
    [Header("發布環境")]
    public ReleaseEnvironmentEnum releaseType;
    [Header("使用測試重定向URL")]
    public bool isUsingTestRedirectUri;
    [Header("解析度")]
    public Vector2 resolution;
    [Header("Debug工具")]
    public bool isUsingDebug;
    [SerializeField] 
    GameObject ReporterObj;

    public override void Awake()
    {
#if !UNITY_EDITOR
        JSBridgeManager.Instance.SetupRecaptchaVerifier();
#endif

        base.Awake();
    }

    private IEnumerator Start()
    {
        if (isUsingDebug)
        {
            //Debug工具初始化
            Reporter.I.Initialize();
            Reporter.I.show = false;
        }

        LanguageManager.Instance.LoadLangageJson();

        yield return AssetsManager.Instance.ILoadAssets();
        AudioManager.Instance.StartLoadAudioAssets();

        LoadSceneManager.Instance.LoadScene(SceneEnum.Login);
    }

    private void Update()
    {
        #region 測試操作

        if (releaseType == ReleaseEnvironmentEnum.Test)
        {
            //NFT測試
            if (Input.GetKeyDown(KeyCode.RightControl))
            {
                DataManager.UserWalletAddress = "0xef279977cBC232C667082E06cfC252529513B738";
                NFTManager.Instance.UpdateNFT();
            }

            //移除手牌紀錄
            if (Input.GetKeyDown(KeyCode.F8))
            {
                HandHistoryManager.Instance.OnDeleteHistoryData();
            }
        }
        #endregion
    }

    #region Instagram登入

    /// <summary>
    /// 接收獲取IG用戶訊息
    /// </summary>
    /// <param name="mail"></param>
    public void ReceiveIGInfo(string info)
    {
        Debug.Log($"Get IG Info:{info}");
        DataManager.IGIUserIdAndName = info;
    }

    [System.Serializable]
    public class IGUserInfo
    {
        public string id;
        public string username;
    }

    /// <summary>
    /// Instagram 登入回傳
    /// </summary>
    /// <param name="code"></param>
    public void OnIGLoginCallback(string code)
    {
        Debug.Log("Receive Code: " + code);
        StartCoroutine(GetIGAccessToken(code));
    }

    [System.Serializable]
    public class TokenResponse
    {
        public string access_token;
        public string token_type;
        public string refresh_token;
        public string expires_in;
        public string scope;
        public string id_token;
    }

    public IEnumerator GetIGAccessToken(string accessCode)
    {
        string tokenUrl = "https://api.instagram.com/oauth/access_token";
        WWWForm data = new WWWForm();
        data.AddField("client_id", DataManager.InstagramChannelID);
        data.AddField("client_secret", DataManager.InstagramChannelSecret);
        data.AddField("grant_type", "authorization_code");
        data.AddField("redirect_uri", DataManager.InstagramRedirectUri);
        data.AddField("code", accessCode);

        using (UnityWebRequest www = UnityWebRequest.Post(tokenUrl, data))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                // 解析访问令牌响应
                var tokenResponse = JsonUtility.FromJson<TokenResponse>(www.downloadHandler.text);
                string idToken = tokenResponse.id_token;

                StartCoroutine(IG_VerifyIdToken(idToken));
            }
        }
    }
    /// <summary>
    /// 驗證Token返回用戶資訊
    /// </summary>
    /// <param name="idToken"></param>
    /// <returns></returns>
    public IEnumerator IG_VerifyIdToken(string idToken)
    {
        string tokenUrl = "https://graph.instagram.com/";
        WWWForm data = new WWWForm();
        data.AddField("user_id", DataManager.InstagramChannelID);
        data.AddField("access_token", idToken);

        using (UnityWebRequest www = UnityWebRequest.Post(tokenUrl, data))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                var userProfile = JsonUtility.FromJson<IGUserInfo>(www.downloadHandler.text);

                Debug.Log("User ID: " + userProfile.id);
                Debug.Log("Name: " + userProfile.username);
            }
        }

    }

    #endregion

    #region 邀請碼

    [System.Serializable]
    public class InvitationData
    {
        public string invitationCode;     //邀請人的邀請碼
        public string inviterId;          //邀請人ID
    }
    /// <summary>
    /// 接收邀請碼
    /// </summary>
    /// <param name="jsonData">回傳資料</param>
    public void ReceiveInvitationCode(string jsonData)
    {
        var data = JsonUtility.FromJson<InvitationData>(jsonData);
        Debug.Log($"Receive invitation code : {data.invitationCode} / inviterId: {data.inviterId}");
        DataManager.GetInvitationCode = data.invitationCode;
        DataManager.GetInviterId = data.inviterId;
    }

    #endregion

    #region 工具類 

    /// <summary>
    /// 網頁視窗失去焦點
    /// </summary>
    public void OnWindowBlur()
    {
        GameRoomManager.Instance.OnUpdateGame();
    }

    /// <summary>
    /// 網頁視窗獲得焦點
    /// </summary>
    public void OnWindowFocus()
    {
        //跳轉到下載錢包頁面回來
        if (DataManager.IsOpenDownloadWallet)
        {
            JSBridgeManager.Instance.Reload();
        }
    }

    /// <summary>
    /// 是否為移動平台
    /// </summary>
    public void IsMobilePlatform(string isMobile)
    {
        DataManager.IsMobilePlatform = isMobile == "true";
        Debug.Log($"IsMobilePlatform:{isMobile}");
    }

    /// <summary>
    /// 是否在預設瀏覽器內
    /// </summary>
    public void IsDefaultBrowser(string isDefaultBrowser)
    {
        DataManager.IsDefaultBrowser = isDefaultBrowser == "true";
        Debug.Log($"isDefaultBrowser:{isDefaultBrowser}");
    }

    /// <summary>
    /// 是否在Coinbase瀏覽器內
    /// </summary>
    public void IsInCoinbase(string isCoinbaseBrowser)
    {
        DataManager.IsInCoinbase = isCoinbaseBrowser == "true";
        Debug.Log($"isDefaultBrowser:{isCoinbaseBrowser}");
    }

    /// <summary>
    /// 網頁debug
    /// </summary>
    /// <param name="str"></param>
    public void HtmlDebug(string str)
    {
        Debug.Log($"Browser Debug: {str}");
    }

    #endregion
}
