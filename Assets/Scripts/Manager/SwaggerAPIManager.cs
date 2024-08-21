using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.Events;

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
    public void SendPostAPI<T1, T2>(string apiUrl, T1 data, UnityAction<T2> callback = null, UnityAction errCallback = null) 
        where T1 : class 
        where T2 : class
    {
        StartCoroutine(ISendPOSTRequest(apiUrl,
                                        data,
                                        callback,
                                        errCallback));
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
    private IEnumerator ISendPOSTRequest<T1, T2>(string apiUrl, T1 data, UnityAction<T2> callback = null, UnityAction errCallback = null) 
        where T1 : class 
        where T2 : class
    {
        string fullUrl = BASE_URL + apiUrl;
        Debug.Log($"Send POST:{fullUrl}");

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
            errCallback?.Invoke();
        }
        else
        {
            //回傳結果
            Debug.Log("Response: " + request.downloadHandler.text);
            if (request.downloadHandler.text.Length > 1000)
            {
                LoadSceneManager.Instance.LoadScene(SceneEnum.Lobby);
            }
            //Callback執行
            if (callback != null)
            {
                T2 response = JsonUtility.FromJson<T2>(request.downloadHandler.text);
                callback?.Invoke(response);
            }
        }
    }
}
