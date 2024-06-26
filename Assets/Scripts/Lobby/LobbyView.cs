using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Thirdweb;
using System.Threading.Tasks;

public class LobbyView : MonoBehaviour
{
    [SerializeField]
    Request_LobbyView baseRequest;

    [Header("用戶訊息")]
    [SerializeField]
    Button Avatar_Btn;
    [SerializeField]
    Text Stamina_Txt, CryptoChips_Txt;

    [Header("用戶資源列表")]
    [SerializeField]
    Button ShowAssets_Btn;
    [SerializeField]
    GameObject AssetList_Obj;
    [SerializeField]
    Image ShowAssetsBtn_Img;

    [Header("加密貨幣桌")]
    [SerializeField]
    GameObject CryptoTableBtnSample;
    [SerializeField]
    RectTransform CryptoTableParent;

    [Header("虛擬貨幣桌")]
    [SerializeField]
    GameObject VCTableBtnSample;
    [SerializeField]
    RectTransform VCTableParent;


    [Header("積分房")]
    [SerializeField]
    Button Integral_Btn;
    [SerializeField]
    Text IntegralBtn_Txt;

    [Header("提示")]
    [SerializeField]
    Text Tip_Txt;

    Coroutine tipCorutine;
    bool isShowAssetList;               //是否顯示用戶資源列表

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
       
    }

    private void Awake()
    {
        battleData = new BattleData();

        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage);
        ListenerEvent();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        //顯示用戶資源列表
        ShowAssets_Btn.onClick.AddListener(() =>
        {
            isShowAssetList = !isShowAssetList;
            SetIsShowAssetList = isShowAssetList;
        });

        //積分房
        Integral_Btn.onClick.AddListener(() =>
        {  
            if (battleData.isPairing)
            {
                //正在配對取消配對
                EndPair();
            }
            else
            {
                if (GameRoomManager.Instance.JudgeIsCanBeCreateRoom())
                {
                    //開始配對
                    battleData.isPairing = true;
                    battleData.startPairTime = DateTime.Now;
                }
                else
                {
                    //房間數已達上限
                    ShowMaxRoomTip();                    
                }                
            }
        });
    }

    private void OnEnable()
    {
        Color tipColor = Tip_Txt.color;
        tipColor.a = 0;
        Tip_Txt.color = tipColor;

        isShowAssetList = false;
        SetIsShowAssetList = isShowAssetList;

        UpdateUserInfo();
#if !UNITY_EDITOR
        WalletManager.Instance.StartCheckConnect();
#endif
    }

    private void Start()
    {
        CreateRoomBtn();
    }

    private void Update()
    {
        //等待計時器
        if (battleData.isPairing)
        {
            TimeSpan waitingTime = DateTime.Now - battleData.startPairTime;
            IntegralBtn_Txt.text = $"Pairing:{(int)waitingTime.TotalMinutes} : {waitingTime.Seconds:00}";

            if (waitingTime.Seconds >= 3)
            {
                baseRequest.SendRequest_InBattleRoom();
                EndPair();
            }
        }

#if UNITY_EDITOR

        if (Input.GetKeyDown(KeyCode.E))
        {
            WalletManager.Instance.OnWalletDisconnect();
            LoadSceneManager.Instance.LoadScene(SceneEnum.Login);
        }

#endif
    }

    /// <summary>
    /// 是否顯示用戶資源列表
    /// </summary>
    private bool SetIsShowAssetList
    {
        set
        {
            AssetList_Obj.SetActive(value);
            ShowAssetsBtn_Img.sprite = value == false ?
                           AssetsManager.Instance.GetAlbumAsset(AlbumEnum.ArrowAlbum).album[1] :
                           AssetsManager.Instance.GetAlbumAsset(AlbumEnum.ArrowAlbum).album[0];
        }
    }

    /// <summary>
    /// 更新用戶訊息
    /// </summary>
    private void UpdateUserInfo()
    {
        Avatar_Btn.image.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album[DataManager.UserAvatar];
        Stamina_Txt.text = $"{DataManager.UserEnergy}/50";
        CryptoChips_Txt.text = string.IsNullOrEmpty(DataManager.UserWalletBalance) ? "0" : DataManager.UserWalletBalance;
    }

    /// <summary>
    /// 創建房間按鈕
    /// </summary>
    private void CreateRoomBtn()
    {
        //加密貨幣桌        
        CryptoTableBtnSample.SetActive(false);
        float cryptoSpacing = CryptoTableParent.GetComponent<HorizontalLayoutGroup>().spacing;
        Rect cryptoRect = CryptoTableBtnSample.GetComponent<RectTransform>().rect;
        CryptoTableParent.sizeDelta = new Vector2((cryptoRect.width + cryptoSpacing) * DataManager.CryptoSmallBlindList.Count, cryptoRect.height);
        foreach (var smallBlind in DataManager.CryptoSmallBlindList)
        {
            RectTransform rt = Instantiate(CryptoTableBtnSample).GetComponent<RectTransform>();
            rt.gameObject.SetActive(true);
            rt.SetParent(CryptoTableParent);
            rt.GetComponent<CryptoTableBtn>().SetCryptoTableBtnInfo(smallBlind, this);
            rt.localScale = Vector3.one;
        }
        CryptoTableParent.anchoredPosition = Vector2.zero;

        //虛擬貨幣桌
        VCTableBtnSample.SetActive(false);
        float vcSpacing = VCTableParent.GetComponent<HorizontalLayoutGroup>().spacing;
        Rect vcRect = VCTableBtnSample.GetComponent<RectTransform>().rect;
        VCTableParent.sizeDelta = new Vector2((vcRect.width + vcSpacing) * DataManager.VCSmallBlindList.Count, vcRect.height);
        foreach (var smallBlind in DataManager.VCSmallBlindList)
        {
            RectTransform rt = Instantiate(VCTableBtnSample).GetComponent<RectTransform>();
            rt.gameObject.SetActive(true);
            rt.SetParent(VCTableParent);
            rt.GetComponent<VCTableBtn>().SetVCTableBtnInfo(smallBlind, this);
            rt.localScale = Vector3.one;
        }
        VCTableParent.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// 積分房資料
    /// </summary>
    private BattleData battleData;
    public class BattleData
    {
        public bool isPairing;              //是否正在配對中
        public DateTime startPairTime;      //開始配對時間
    }

    /// <summary>
    /// 結束配對
    /// </summary>
    private void EndPair()
    {
        IntegralBtn_Txt.text = "INTEGRAL";
        battleData.isPairing = false;
    }

    /// <summary>
    /// 顯示已達房間數量提示
    /// </summary>
    public void ShowMaxRoomTip()
    {
        if(tipCorutine != null) StopCoroutine(tipCorutine);
        tipCorutine = StartCoroutine(IShowTip(LanguageManager.Instance.GetText("MaxRoomTip")));
    }

    /// <summary>
    /// 顯示提示
    /// </summary>
    /// <param name="tipContent">提示內容</param>
    /// <returns></returns>
    public IEnumerator IShowTip(string tipContent)
    {
        float showTime = 0.5f;

        Tip_Txt.text = tipContent;
        Color tipColor = Tip_Txt.color;
        tipColor.a = 0;

        DateTime startTime = DateTime.Now;

        while ((DateTime.Now - startTime).TotalSeconds < showTime)
        {
            float progress = (float)(DateTime.Now - startTime).TotalSeconds / showTime;
            float alpha = Mathf.Lerp(0, 1, progress);
            tipColor.a = alpha;
            Tip_Txt.color = tipColor;

            yield return null;
        }

        tipColor.a = 1;
        Tip_Txt.color = tipColor;

        yield return new WaitForSeconds(2.5f);

        startTime = DateTime.Now;

        while ((DateTime.Now - startTime).TotalSeconds < showTime)
        {
            float progress = (float)(DateTime.Now - startTime).TotalSeconds / showTime;
            float alpha = Mathf.Lerp(1, 0, progress);
            tipColor.a = alpha;
            Tip_Txt.color = tipColor;

            yield return null;
        }

        tipColor.a = 0;
        Tip_Txt.color = tipColor;
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
        WalletManager.Instance.CancelCheckConnect();
    }
}
