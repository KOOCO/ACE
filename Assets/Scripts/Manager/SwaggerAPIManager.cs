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
    public void SendPostAPI<T1>(string apiUrl, T1 data, UnityAction<string> callback = null, UnityAction<string> errCallback = null, bool addHeader = false, bool useParams = false)
        where T1 : class
    {
        StartCoroutine(ISendPOSTRequest(apiUrl, data, callback, errCallback, addHeader, useParams));
    }
    public void SendGetAPI(string apiUrl, UnityAction<string> callback = null, UnityAction errCallback = null, bool addHeader = false)
    {
        StartCoroutine(ISendGetRequest(apiUrl, callback, errCallback, addHeader));
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
    private IEnumerator ISendPOSTRequest<T1>(string apiUrl, T1 data, UnityAction<string> callback = null, UnityAction<string> errCallback = null, bool addHeader = false, bool useParams = false)
    where T1 : class
    {
        string fullUrl = BASE_URL + apiUrl;

        if (useParams)
        {
            // Create a list to hold the query parameters
            List<string> queryParams = new List<string>();

            // Serialize data to key-value pairs
            foreach (var prop in typeof(T1).GetProperties())
            {
                var value = prop.GetValue(data)?.ToString();
                if (value != null)
                {
                    // Add URL-encoded query parameters
                    queryParams.Add($"{Uri.EscapeDataString(prop.Name)}={Uri.EscapeDataString(value)}");
                }
            }

            // Append query parameters to the URL
            if (queryParams.Count > 0)
            {
                fullUrl += "?" + string.Join("&", queryParams);
            }
        }

        Debug.Log($"Send POST to URL: {fullUrl}");

        // Create POST request
        UnityWebRequest request = new UnityWebRequest(fullUrl, "POST");

        if (useParams)
        {
            // Empty body for POST request if using parameters in the URL
            request.uploadHandler = new UploadHandlerRaw(new byte[0]);
        }
        else
        {
            // Serialize data to JSON if not using URL parameters
            string jsonData = JsonUtility.ToJson(data);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        }

        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", useParams ? "application/x-www-form-urlencoded" : "application/json");

        if (addHeader)
        {
            request.SetRequestHeader("Authorization", "Bearer " + Services.PlayerService.GetAccessToken());
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            // Request error
            string errorJson = request.downloadHandler.text;
            Debug.Log($"Error: {request.error}\nError Details: {errorJson}");

            if (errorJson == "Invalid username or password!")
            {
                DataManager.TipText = LanguageManager.Instance.GetText("Invalid Username or Password!");
                DataManager.istipAppear = true;
            }
            errCallback?.Invoke(request.responseCode.ToString());
        }
        else
        {
            string Response = request.downloadHandler.text;

            // Callback execution
            callback?.Invoke(Response);
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
    public IEnumerator ISendGetRequest(string apiUrl, UnityAction<string> callback = null, UnityAction errCallback = null, bool addHeader = false)
    {
        // 將GetBanner對象轉換為查詢字符串
        //string queryString = $"?Filter={Filter}&StartDate={StartDate}&EndDate={EndDate}&IsEnabled={IsEnabled}&Sorting={Sorting}&SkipCount={data.SkipCount}&MaxResultCount={data.MaxResultCount}";
        string fullUrl = BASE_URL + apiUrl; //+ queryString;

        // 創建GET請求
        UnityWebRequest getRequest = UnityWebRequest.Get(fullUrl);
        getRequest.downloadHandler = new DownloadHandlerBuffer();
        if (addHeader)
            getRequest.SetRequestHeader("Authorization", "Bearer " + Services.PlayerService.GetAccessToken());
        yield return getRequest.SendWebRequest();

        if (getRequest.result == UnityWebRequest.Result.ConnectionError || getRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            // 請求錯誤
            Debug.LogError($"Error: {getRequest.error}\nError Details: {getRequest.downloadHandler.text}");
            errCallback?.Invoke();
        }
        else
        {
            string response = getRequest.downloadHandler.text;
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
    public string memberName;
    public string emailAddress;
    public string walletAddress;
}