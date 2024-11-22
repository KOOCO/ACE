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
using System.Security.Cryptography;

public class SwaggerAPIManager : UnitySingleton<SwaggerAPIManager>
{
    private const string devBASE_URL = "https://admin-d.jf588.com";     //API Base Url
    private const string prodBASE_URL = "https://ace.ap88.io";     //API Base Url

    private string url;

    public override void Awake()
    {
        base.Awake();
        if (Entry.Instance.releaseType == ReleaseEnvironmentEnum.Demo)
            url = devBASE_URL;
        else if (Entry.Instance.releaseType == ReleaseEnvironmentEnum.Prod)
            url = prodBASE_URL;
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
    public void SendPostEncryptAPI<T1>(T1 data, UnityAction<string> callback = null, UnityAction<string> errCallback = null)
        where T1 : class
    {
        StartCoroutine(ISendPostEncryptRequest(data, callback, errCallback));
    }
    public void SendGetAPI(string apiUrl, UnityAction<string> callback = null, UnityAction errCallback = null, bool addHeader = false)
    {
        StartCoroutine(ISendGetRequest(apiUrl, callback, errCallback, addHeader));
    }
    public string GetBaseUrl()
    {
        return url;
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
        string fullUrl = url + apiUrl;

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
            string jsonData = JsonConvert.SerializeObject(data);
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
                //DataManager.TipText = LanguageManager.Instance.GetText("Invalid Username or Password!");
                //DataManager.istipAppear = true;
                FindObjectOfType<LoginView>().openTip(messageStatus.Failed, "Invalid Username or Password!");
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

    IEnumerator ISendPostEncryptRequest<T1>(T1 data, UnityAction<string> callback = null, UnityAction<string> errCallback = null) 
    {
        // API 的 URL
        string url = "https://ace-admin-devs.azurewebsites.net/api/app/games/ace/table-chips-transaction";

        WWWForm form = new WWWForm();
        string jsonData = JsonConvert.SerializeObject(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        var encrypt = AppApi.EncryptJson(bodyRaw);
        form.AddField("text", encrypt);
        //print(encrypt);

        // 建立請求
        UnityWebRequest request = UnityWebRequest.Post(url, form);

        // 添加標頭
        request.SetRequestHeader("accept", "text/plain");
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        request.SetRequestHeader("RequestVerificationToken", "CfDJ8CdPAbU--ddBv_k4FpyUwXTR6hWzVqZqA8QsA9JWz-wCNfg04hMxdUjSlBQXqrhix_HA_LFkSOHEenlNNASRwIfMx-Wsgf9DzBadMnvjIhiKQFXHFlLL5Zh6bR7EuyqSuIZecuwzPkMWwOZSjHZBY9ACxPLlDaVYgGLjmeuSdWOMw_WcnqOvqsBW116SVFy4rQ");
        request.SetRequestHeader("X-Requested-With", "XMLHttpRequest");

        // 發送請求並等待回應
        yield return request.SendWebRequest();

        // 檢查請求狀態
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            string Response = request.downloadHandler.text;

            // Callback execution
            callback?.Invoke(Response);
        }
        else
        {
            string errorJson = request.downloadHandler.text;
            errCallback?.Invoke(request.responseCode.ToString());
            Debug.Log($"Error: {request.error}\nError Details: {errorJson}");
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
        string fullUrl = url + apiUrl; //+ queryString;
        Debug.Log($"Send Get to URL: {fullUrl}");
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
            //GetBanner getBanner = JsonConvert.DeserializeObject<GetBanner>(response);
            //ConvertHtmlToJson(response);


            // 執行Callback
            callback?.Invoke(response);
        }
    }
}
