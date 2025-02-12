using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameChat : MonoBehaviour
{
    [SerializeField]
    RectTransform ChatPage_Tr, ChatContent_Tr, NotReadChat_Tr;
    [SerializeField]
    Button Chat_Btn, ChatClose_Btn, ChatSend_Btn, NewMessage_Btn, Mask_Btn;
    [SerializeField]
    GameObject OtherChatSample, LocalChatSample;
    [SerializeField]
    ScrollRect ChatArea_Sr;
    [SerializeField]
    TMP_InputField Chat_If;
    [SerializeField]
    TextMeshProUGUI NotReadChat_Txt;

    bool isWating;
    private GameData gameData;
    int notReadMsgCount;                                        //未讀取數
    ObjPool objPool;

    public event Action<string, string> ShowChat;
    public event Action<string> UpdateChatMsg;
    private void Update()
    {
        //發送聊天訊息
        if ((Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter)) &&
            ChatPage_Tr.gameObject.activeSelf &&
            !string.IsNullOrEmpty(Chat_If.text))
        {
            SendChat();
            Chat_If.ActivateInputField();
            Chat_If.Select();
        }
    }
    public void Init(string roomName)
    {
        gameData = GameRoomManager.Instance.GetGameData(roomName);
        objPool = new ObjPool(transform, gameData.MaxChatCount);
        Mask_Btn.gameObject.SetActive(false);
        OtherChatSample.SetActive(false);
        LocalChatSample.SetActive(false);
        NewMessage_Btn.gameObject.SetActive(false);
        ChatPage_Tr.gameObject.SetActive(false);
        EvnetListener();
    }
    private void EvnetListener()
    {
        //遮罩按鈕
        Mask_Btn.onClick.AddListener(() =>
        {
            if (ChatPage_Tr.gameObject.activeSelf)
            {
                CloseChatPage();
            }
        });

        //開啟聊天
        Chat_Btn.onClick.AddListener(() =>
        {
            SetNotReadChatCount = 0;
            GameRoomManager.Instance.IsCanMoveSwitch = false;
            Mask_Btn.gameObject.SetActive(true);
            StartCoroutine(UnityUtils.Instance.IViewSlide(true,
                                                          ChatPage_Tr,
                                                          DirectionEnum.Left,
                                                          gameData.PageMoveTime));
            StartCoroutine(IYieldSetNewMessageActive());
        });

        //發送聊天訊息
        ChatSend_Btn.onClick.AddListener(() =>
        {
            SendChat();
        });

        //關閉聊天
        ChatClose_Btn.onClick.AddListener(() =>
        {
            CloseChatPage();
        });

        //聊天移動至最新訊息位置
        NewMessage_Btn.onClick.AddListener(() =>
        {
            StartCoroutine(IGoNewChatMessage());
        });

        //聊天區域
        ChatArea_Sr.onValueChanged.AddListener((value) =>
        {
            if (NewMessage_Btn.gameObject.activeSelf &&
                IsChatOnBottom())
            {
                NewMessage_Btn.gameObject.SetActive(false);
            }
        });
    }

    #region 聊天

    /// <summary>
    /// 關閉聊天
    /// </summary>
    private void CloseChatPage()
    {
        Mask_Btn.gameObject.SetActive(false);
        StartCoroutine(UnityUtils.Instance.IViewSlide(false,
        ChatPage_Tr,
        DirectionEnum.Left,
        gameData.PageMoveTime,
        () =>
        {
            //判斷保留訊息數量
            if (ChatContent_Tr.childCount > gameData.MaxChatCount)
            {
                int closeCount = ChatContent_Tr.childCount - gameData.MaxChatCount;
                for (int i = 0; i < closeCount; i++)
                {
                    if (ChatContent_Tr.GetChild(2 + i).gameObject.activeSelf)
                    {
                        ChatContent_Tr.GetChild(2 + i).gameObject.SetActive(false);
                        float chatHeight = ChatContent_Tr.GetChild(2 + i).GetComponent<RectTransform>().rect.height;
                        float reduce = Mathf.Max(0, ChatContent_Tr.anchoredPosition.y - chatHeight);
                        ChatContent_Tr.anchoredPosition = new Vector2(ChatContent_Tr.anchoredPosition.x,
                                                                    reduce);
                    }
                }
            }

            StartCoroutine(IGoNewChatMessage());
            GameRoomManager.Instance.IsCanMoveSwitch = true;
        }));
    }

    /// <summary>
    /// 設置未讀聊天訊息數
    /// </summary>
    public int SetNotReadChatCount
    {
        get
        {
            return notReadMsgCount;
        }
        set
        {
            notReadMsgCount = value;
            string countStr = value > 99 ?
                              "99+" :
                              $"{value}";
            NotReadChat_Txt.text = countStr;
            NotReadChat_Tr.gameObject.SetActive(value > 0);

            NotReadChat_Tr.sizeDelta = value > 99 ?
                                       new Vector2(20, 15) :
                                       new Vector2(15, 15);
        }
    }

    /// <summary>
    /// 延遲設置新訊息按鈕是否顯示
    /// </summary>
    /// <returns></returns>
    private IEnumerator IYieldSetNewMessageActive()
    {
        yield return null;
        NewMessage_Btn.gameObject.SetActive(!IsChatOnBottom());
    }

    /// <summary>
    /// 聊天移動至最新訊息位置
    /// </summary>
    /// <returns></returns>
    private IEnumerator IGoNewChatMessage()
    {
        yield return null;

        NewMessage_Btn.gameObject.SetActive(false);

        //顯示在最新訊息
        float chatAreaHeight = ChatArea_Sr.GetComponent<RectTransform>().rect.height;
        float currChatContentHeight = ChatContent_Tr.rect.height;
        float goPosY = Mathf.Max(0, currChatContentHeight - chatAreaHeight);
        ChatContent_Tr.anchoredPosition = new Vector2(ChatContent_Tr.anchoredPosition.x,
                                                      goPosY);

        yield return new WaitForSeconds(3);
        isWating = false;
    }

    /// <summary>
    /// 判斷聊天位置是否在最底部
    /// </summary>
    /// <returns></returns>
    private bool IsChatOnBottom()
    {
        float chatAreaHeight = ChatArea_Sr.GetComponent<RectTransform>().rect.height;
        float currChatContentHeight = ChatContent_Tr.rect.height;
        float bottomPosY = Mathf.Max(0, currChatContentHeight - chatAreaHeight);
        return ChatContent_Tr.anchoredPosition.y >= Mathf.Max(0, bottomPosY - 20);
    }

    /// <summary>
    /// 發送聊天訊息
    /// </summary>
    private void SendChat()
    {
        if (string.IsNullOrEmpty(Chat_If.text))
        {
            return;
        }

        if (isWating)
        {
            ViewManager.Instance.OpenTipMsgView(transform, messageStatus.Warning,
                                            LanguageManager.Instance.GetText("Messages sent too frequently"));
            return;
        }

        UpdateChatMsg.Invoke(Chat_If.text);

        Chat_If.text = "";

        if (!DataManager.IsMobilePlatform)
        {
            Chat_If.Select();
        }

        isWating = true;

        StartCoroutine(IGoNewChatMessage());
    }

    /// <summary>
    /// 接收聊天訊息
    /// </summary>
    /// <param name="chatData"></param>
    public void ReciveChat(ChatData chatData)
    {
        string id = chatData.userId;
        string nickname = chatData.nickname;
        string content = chatData.chatMsg;
        int avatar = chatData.avatarIndex;
        bool isLocal = id == DataManager.UserId;

        //判斷是否在最新訊息位置
        bool isBottom = IsChatOnBottom();
        if (ChatPage_Tr.gameObject.activeSelf)
        {
            NewMessage_Btn.gameObject.SetActive(!isBottom);
        }
        else
        {
            NewMessage_Btn.gameObject.SetActive(true);
        }

        CreateChatContent(avatar,
                          nickname,
                          content,
                          isLocal);

        if (isBottom)
        {
            StartCoroutine(IGoNewChatMessage());
        }

        //未開啟聊天頁面顯示新訊息提示
        if (!ChatPage_Tr.gameObject.activeSelf &&
            id != DataManager.UserId)
        {
            ShowChat.Invoke(id, content);
        }
    }

    /// <summary>
    /// 產生聊天內容
    /// </summary>
    /// <param name="avatar"></param>
    /// <param name="nickname"></param>
    /// <param name="content">聊天內容</param>
    /// <param name="isLocal">是否為本地玩家</param>
    private void CreateChatContent(int avatar, string nickname, string content, bool isLocal)
    {
        GameObject sample = isLocal ?
                            LocalChatSample :
                            OtherChatSample;

        ChatInfoSample chatInfo = objPool.CreateObj<ChatInfoSample>(sample, ChatContent_Tr);
        chatInfo.gameObject.SetActive(true);
        chatInfo.GetComponent<RectTransform>().SetSiblingIndex(ChatContent_Tr.childCount + 1);
        chatInfo.SetChatInfo(avatar,
                             nickname,
                             content);
    }

    #endregion
}
