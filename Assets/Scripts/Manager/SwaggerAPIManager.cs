using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using HtmlAgilityPack;
using System.Web;
using static LobbyMainPageView;

public class SwaggerAPIManager : UnitySingleton<SwaggerAPIManager>
{
    private const string BASE_URL = "https://admin.jf588.com/";           //API Base Url

    public override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 發送POST請求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="apiUrl">Api Url</param>
    /// <param name="data">傳遞的資料</param>
    /// <param name="callback">結果回傳</param>
    /// <param name="errCallback"></param>
    public void SendPostAPI<T1>(string apiUrl, T1 data, UnityAction<string> callback = null, UnityAction errCallback = null)
        where T1 : class
    {
        StartCoroutine(ISendPOSTRequest(apiUrl,data,callback,errCallback));
    }
    public void SendGetAPI(string apiUrl, UnityAction<string> callback = null, UnityAction errCallback = null)
    {
        StartCoroutine(ISendGetRequest(apiUrl, callback, errCallback));
    }
    [Serializable]
    public class LoginResponse
    {
        public string accessToken { get; set; }
        public string memberId { get; set; }
        public string memberStatus { get; set; }
        public int promotionCoin { get; set; }
        public int gold { get; set; }
        public int timer { get; set; }
        public int currentEnergy { get; set; }
        public int maxEnergy { get; set; }
        public decimal WalletAmount { get; set; }

    }
    public class GetBanner_File
    {
        public string imageName {  get; set; }

        public string imageUrl { get; set; }

        public string blobFileName { get; set; }

        public string startDate { get; set; }

        public string endDate { get; set; }

        public bool isEnabled { get; set; }

    }
    public class RegisterResponce
    {
        
    }
    public class ErrorResponse
    {
        public string error { get; set; }
        public string message { get; set; }
    }

    /// <summary>
    /// 發送POST請求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="apiUrl"></param>
    /// <param name="data"></param>
    /// <param name="callback"></param>
    /// <param name="errCallback"></param>
    /// <returns></returns>
    private IEnumerator ISendPOSTRequest<T1>(string apiUrl, T1 data, UnityAction<string> callback = null, UnityAction errCallback = null)
        where T1 : class
    {
        string fullUrl = BASE_URL + apiUrl;

        //發送的Json
        string jsonData = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        //創建POST請求
        UnityWebRequest request = new UnityWebRequest(fullUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {         
            //請求錯誤
            string errorJson = request.downloadHandler.text;
            Debug.LogError($"Error: {request.error}\nError Details: {errorJson}");
            Debug.LogError(errorJson);

            if (errorJson== "Invalid username or password!")
            {
                DataManager.TipText = LanguageManager.Instance.GetText("Invalid Username or Password!");
                DataManager.istipAppear = true;
                //Debug.Log("登入失敗");
            }
            errCallback?.Invoke();
        }
        else
        {
            string Response = request.downloadHandler.text;
            Debug.Log(Response);
          
            //Callback執行
            if (callback != null)
            {
                //T2 response = JsonUtility.FromJson<T2>(request.downloadHandler.text);
                callback?.Invoke(Response);
            }       
        }
    }
    public string ConvertHtmlToJson(string htmlString)
    {
        // 对HTML字符串进行编码，以防止特殊字符引起的JSON格式错误
        string encodedHtml = HttpUtility.HtmlEncode(htmlString);

        // 构建JSON格式的字符串
        string json = "{\"html\": \"" + encodedHtml + "\"}";

        return json;
    }
    public IEnumerator ISendGetRequest(string apiUrl, UnityAction<string> callback = null, UnityAction errCallback = null)
    {
        // 將GetBanner對象轉換為查詢字符串
        //string queryString = $"?Filter={Filter}&StartDate={StartDate}&EndDate={EndDate}&IsEnabled={IsEnabled}&Sorting={Sorting}&SkipCount={data.SkipCount}&MaxResultCount={data.MaxResultCount}";
        string fullUrl = BASE_URL + apiUrl; //+ queryString;

        // 創建GET請求
        UnityWebRequest GETrequest = UnityWebRequest.Get(fullUrl);
        GETrequest.downloadHandler = new DownloadHandlerBuffer();

        yield return GETrequest.SendWebRequest();

        if (GETrequest.result == UnityWebRequest.Result.ConnectionError || GETrequest.result == UnityWebRequest.Result.ProtocolError)
        {
            // 請求錯誤
            Debug.LogError($"Error: {GETrequest.error}\nError Details: {GETrequest.downloadHandler.text}");
            errCallback?.Invoke();
        }
        else
        {
            string response = GETrequest.downloadHandler.text;
            Debug.Log("Response: " + response);
            GetBanner getBanner = JsonConvert.DeserializeObject<GetBanner>(response);
            //ConvertHtmlToJson(response);


            // 執行Callback
            callback?.Invoke(response);
        }
    }
}

/// <summary>
/// 錢包登入資料
/// </summary>
public class passwordless_login
{
    public string walletAddress;
    public string ipAddress;
    public string machineCode;
}

/// <summary>
/// 錢包註冊資料
/// </summary>
public class register_passwordless
{
    public string userName;
    public string emailAddress;
    public string walletAddress;
}