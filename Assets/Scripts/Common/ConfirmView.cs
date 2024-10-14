using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class ConfirmView : MonoBehaviour
{
    [SerializeField]
    Image Content_Img;
    [SerializeField]
    Button Close_Btn, Cancel_Btn, Confirm_Btn;
    [SerializeField]
    TextMeshProUGUI Title_Txt, Content_Txt,
                    CancelBtn_Txt, ConfirmBtn_Txt;

    private LobbyView lobbyView;

    /// <summary>
    /// 設定確認介面內容
    /// </summary>
    /// <param name="titleStr">標題文字</param>
    /// <param name="contentStr">內容文字</param>
    /// <param name="contentImg">內容圖片</param>
    public void SetContent(string titleStr, string contentStr, Sprite contentSp = null)
    {
        Title_Txt.text = titleStr;
        Content_Txt.text = contentStr;

        Content_Img.gameObject.SetActive(contentSp != null);
        Content_Img.sprite = contentSp;

        CancelBtn_Txt.text = LanguageManager.Instance.GetText("Cancel");
        ConfirmBtn_Txt.text = LanguageManager.Instance.GetText("Confirm");
    }

    public void Awake()
    {
        lobbyView = GameObject.FindAnyObjectByType<LobbyView>();
    }

    /// <summary>
    /// 設定確認介面按鈕
    /// </summary>
    /// <param name="confirmCallback">確認按鈕回傳</param>
    /// <param name="showCancel">顯示取消</param>
    /// <param name="cancelCallback">取消按鈕回傳</param>
    public void SetBnt(UnityAction confirmCallback, bool showCancel = true, UnityAction cancelCallback = null)
    {
        //確認按鈕執行
        Confirm_Btn.onClick.AddListener(() =>
        {
            confirmCallback?.Invoke();
            Destroy(gameObject);
            //SoundToggleGroup.IsPlayAudio(lobbyView.audioSource);
        });

        Cancel_Btn.gameObject.SetActive(showCancel);
        //取消按鈕執行
        if (cancelCallback != null)
        {
            Cancel_Btn.onClick.AddListener(() =>
            {
                cancelCallback?.Invoke();
                Destroy(gameObject);
            });

            Close_Btn.onClick.AddListener(() =>
            {
                cancelCallback?.Invoke();
                Destroy(gameObject);
            });
        }
        else
        {
            //未設定關閉介面
            Cancel_Btn.onClick.AddListener(() =>
            {
                Destroy(gameObject);
            });

            Close_Btn.onClick.AddListener(() =>
            {
                Destroy(gameObject);
            });
        }
    }
}
