using System;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePot : MonoBehaviour
{
    [SerializeField]
    Image Pot_Img, winnerHandImage;
    [SerializeField]
    TextMeshProUGUI TotalPot_Txt, roomID_Txt, sbBlinds_Txt, winnerText;
    [SerializeField]
    GameObject WaitingTip_Txt, settleVFX_Obj;

    private GameData gameData;

    public void Init(string roomName)
    {
        gameData = GameRoomManager.Instance.GetGameData(roomName);
        //初始底池位置
        gameData.InitPotPointPos = Pot_Img.rectTransform.anchoredPosition;
        TotalPot_Txt.text = $"${StringUtils.SetChipsUnit(0)}";
        roomID_Txt.text = $"ID: {DataManager.RoomId}";
        EventListener();
    }
    private void EventListener()
    {
        roomID_Txt.GetComponent<Button>().onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            TextEditor editor = new TextEditor
            {
                text = roomID_Txt.text.Substring(4)
            };
            editor.SelectAll();
            editor.Copy();
#endif

            JSBridgeManager.Instance.CopyString(roomID_Txt.text.Substring(4));
            ViewManager.Instance.OpenTipMsgView(transform, messageStatus.Succesful, LanguageManager.Instance.GetText("Copy Success!"));
        });
    }
    public void SetWinnerStringTxt(string value)
    {
        if (winnerText != null)
        {

            if (value != "")
            {
                winnerHandImage?.gameObject.SetActive(true);
                winnerText.text = value;
                //settleVFX_Obj.SetActive(true);
            }
            else
            {
                winnerHandImage.gameObject.SetActive(false);
                //settleVFX_Obj.SetActive(false);
            }
        }
    }
    /// <summary>
    /// 設置底池顯示
    /// </summary>
    public bool PotActive
    {
        set
        {
            Pot_Img.enabled = value;
            Pot_Img.rectTransform.anchoredPosition = gameData.InitPotPointPos;
        }
    }
    /// <summary>
    /// 設置底池籌碼
    /// </summary>
    public double TotalPot
    {
        set
        {
            if (TotalPot_Txt.text != StringUtils.SetChipsUnit(value))
            {
                StringUtils.ChipsChangeEffect(TotalPot_Txt, Math.Floor(value), "$");
            }
            gameData.thisData.TotalPot = value;
        }
    }
    /// <summary>
    /// 設置底池籌碼文字
    /// </summary>
    public string TotalPotText
    {
        set
        {
            TotalPot_Txt.text = value;
        }
        get
        {
            return TotalPot_Txt.text;
        }
    }
    /// <summary>
    /// 獲取底池文字物件
    /// </summary>
    public TextMeshProUGUI TotalPotTextUI
    {
        get
        {
            return TotalPot_Txt;
        }
    }
    /// <summary>
    /// 設置大小盲
    /// </summary>
    public string SBBlinds
    {
        set
        {
            sbBlinds_Txt.text = value;
        }
    }
    /// <summary>
    /// 顯示等待開局
    /// </summary>
    public bool ShowWaitingTip
    {
        set
        {
            WaitingTip_Txt.gameObject.SetActive(value);
        }
    }
    /// <summart>
    /// 
    /// </summart>
    public Transform PotTransform
    {
        get
        {
            return Pot_Img.transform;
        }
    }
}
