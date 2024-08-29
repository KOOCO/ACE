using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwitchRoomBtn : MonoBehaviour
{
    [SerializeField]
    Button thisBtn;
    [SerializeField]
    GameObject CurrRoomIcon_Obj, CdIcon_Obj;
    [SerializeField]
    TextMeshProUGUI roomName_Txt;

    private string roomName;
    private string cdTimeStr;

    public int BtnIndex { get; set; }

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        if (!string.IsNullOrEmpty(roomName))
        {
            roomName_Txt.text = string.IsNullOrEmpty(cdTimeStr) ?
                                $"{LanguageManager.Instance.GetText(roomName)}" :
                                $"{LanguageManager.Instance.GetText(roomName)} : {cdTimeStr}";
        }
    }

    private void Awake()
    {
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
        CdIcon_Obj.SetActive(false);
        CurrRoomIcon_Obj.SetActive(false);
        roomName_Txt.text = "";
    }

    /// <summary>
    /// 設置選擇按鈕激活狀態
    /// </summary>
    public bool SetSelectFrameActive
    {
        set
        {
            CurrRoomIcon_Obj.SetActive(value);
            if (value)
            {             
                transform.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                CdIcon_Obj.SetActive(false);
            }
            else
            {              
                transform.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
            }
        }
    }

    /// <summary>
    /// 設置倒數時間
    /// </summary>
    /// <param name="cdTimeStr"></param>
    public void SetCdTimeText(string cdTimeStr)
    {
        this.cdTimeStr = cdTimeStr;
        roomName_Txt.text = string.IsNullOrEmpty(cdTimeStr) ?
                            $"{roomName}" :
                            $"{roomName} : {cdTimeStr}";

        if (!string.IsNullOrEmpty(cdTimeStr))
        {
            CdIcon_Obj.SetActive(!CurrRoomIcon_Obj.activeSelf);
        }
    }

    /// <summary>
    /// 設置切換房間按鈕訊息
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="btnIndex"></param>
    public void SetSwitchBtnInfo(string roomName, int btnIndex)
    {
        this.BtnIndex = btnIndex;
        this.roomName = roomName;
        roomName_Txt.text = roomName;
        thisBtn.onClick.AddListener(() =>
        {
            GameRoomManager.Instance.SwitchBtnClick(btnIndex);
        });
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
    }
}
