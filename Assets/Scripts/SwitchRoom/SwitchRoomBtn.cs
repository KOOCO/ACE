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
    TextMeshProUGUI roomName_Txt;
    [SerializeField]
    RectTransform selectFrame_Tr;

    private string roomName;

    public int BtnIndex { get; set; }

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        if (!string.IsNullOrEmpty(roomName))
        {
            roomName_Txt.text = LanguageManager.Instance.GetText(roomName);
        }
    }

    private void Awake()
    {
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
    }
    private void Update()
    {
    }
    /// <summary>
    /// 設置選擇按鈕激活狀態
    /// </summary>
    public bool SetSelectFrameActive
    {
        set
        {
            if(value)
            {
             
                transform.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
              
            }
            else
            {
              
                transform.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
               
            }

            // selectFrame_Tr.gameObject.SetActive(value);
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
