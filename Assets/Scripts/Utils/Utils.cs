using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Events;
//using ZXing;
//using ZXing.QrCode;
using UnityEngine.EventSystems;
using TMPro;

public static class Utils
{
    //將 Color 轉換為十六進位字串
    public static string ColorToHex(Color color)
    {
        int r = Mathf.RoundToInt(color.r * 255f);
        int g = Mathf.RoundToInt(color.g * 255f);
        int b = Mathf.RoundToInt(color.b * 255f);
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    // 觸碰的UI
    private static GraphicRaycaster graphicRaycaster;
    private static EventSystem eventSystem;
    /// <summary>
    /// 初始化GraphicRaycaster和EventSystem
    /// </summary>
    private static void Initialize()
    {
        if (graphicRaycaster == null)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
                if (graphicRaycaster == null)
                {
                    Debug.LogError("Canvas物件中未找到GraphicRaycaster");
                }
            }
            else
            {
                Debug.LogError("未找到Canvas物件");
            }
        }

        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                Debug.LogError("未找到EventSystem");
            }
        }
    }
    /// <summary>
    /// 獲取觸碰的UI物件
    /// </summary>
    /// <returns></returns>
    public static GameObject GetTouchUIObj()
    {
        Initialize();

        if (graphicRaycaster == null || eventSystem == null)
        {
            return null;
        }

        PointerEventData eventData = new PointerEventData(eventSystem)
        {
            pressPosition = Input.mousePosition,
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                return result.gameObject;
            }
        }

        return null;
    }

    /// <summary>
    /// 載入url圖片
    /// </summary>
    /// <param name="url"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static IEnumerator ImageUrlToSprite(string url, UnityAction<Sprite> callback)
    {
        if (!string.IsNullOrEmpty(url))
        {
            Debug.Log($"Loading Texture:{url}");
            Sprite sprite = null;
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // 載入成功，將下載的紋理轉換為Sprite
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                callback?.Invoke(sprite);
            }
            else
            {
                Debug.LogError($"載入url圖片失敗:{www.error}");
            }
        }
    }

    /// <summary>
    /// 產生QRCode
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    //public static Sprite GenerateQRCodeTexture(string text)
    //{
        //BarcodeWriter barcodeWriter = new BarcodeWriter
        //{
        //    Format = BarcodeFormat.QR_CODE,
        //    Options = new QrCodeEncodingOptions
        //    {
        //        Height = 256,
        //        Width = 256
        //    }
        //};

        //Color32[] pixels = barcodeWriter.Write(text);
        //Texture2D qrCodeTexture = new Texture2D(256, 256);
        //qrCodeTexture.SetPixels32(pixels);
        //qrCodeTexture.Apply();

        //Sprite sprite = Sprite.Create(qrCodeTexture, new Rect(0, 0, qrCodeTexture.width, qrCodeTexture.height), Vector2.zero);
        //return sprite;
    //}

    /// <summary>
    /// 設置Dropdown項目
    /// </summary>
    /// <param name="dropdown"></param>
    /// <param name="options"></param>
    public static void SetOptionsToDropdown(TMP_Dropdown dropdown, List<string> options)
    {
        //清空當前選項
        dropdown.ClearOptions();

        //添加新的選項
        dropdown.AddOptions(options);
    }

    /// <summary>
    /// 日期轉時間戳
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static long ConvertDateTimeToTimestamp(DateTime dateTime)
    {
        // 確保DateTime對象是基於UTC
        DateTime utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

        // 使用DateTimeOffset將其轉換為Unix時間戳
        long timestamp = new DateTimeOffset(utcDateTime).ToUnixTimeSeconds();

        return timestamp;
    }

    /// <summary>
    /// 時間戳轉日期
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static DateTime ConvertTimestampToDate(long timestamp)
    {
        // 使用DateTimeOffset將Unix時間戳轉換為DateTime
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        DateTime dateTime = dateTimeOffset.DateTime;

        // 如果需要UTC時間，可以使用:
        // DateTime dateTime = dateTimeOffset.UtcDateTime;

        return dateTime;
    }
}
