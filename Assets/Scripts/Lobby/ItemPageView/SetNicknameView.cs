using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Proyecto26;

public class SetNicknameView : MonoBehaviour
{
    [SerializeField]
    Button Subimt_Btn;
    [SerializeField]
    TMP_InputField SetNickname_If;
    [SerializeField]
    TextMeshProUGUI SetNicknameTitle_Txt, Title_Txt, SetNicknameIf_Placeholder,
                    Error_Txt, SubimtBtn_Txt;

    [Header("暱稱檢測")]
    [SerializeField]
    GameObject CheckNickname_Obj;
    [SerializeField]
    Image CheckNickname1_Img, CheckNickname2_Img;
    [SerializeField]
    TextMeshProUGUI CheckNickname1_Txt, CheckNickname2_Txt;

    bool isNucknameCorrect;                                     //是否暱稱正確
    string currNickname;                                        //當前暱稱

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        SetNicknameTitle_Txt.text = LanguageManager.Instance.GetText("Set Your Nickname");
        Title_Txt.text = LanguageManager.Instance.GetText("Nickname");
        SetNicknameIf_Placeholder.text = LanguageManager.Instance.GetText("Enter nickname");
        SubimtBtn_Txt.text = LanguageManager.Instance.GetText("SUBMIT");
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
    }

    private void Awake()
    {
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);

        Error_Txt.text = "";
        CheckNickname_Obj.SetActive(true);

        ListenerEvent();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        //提交
        Subimt_Btn.onClick.AddListener(() =>
        {
            OnSubmit();
        });

        //暱稱輸入變化
        SetNickname_If.onValueChanged.AddListener((value) =>
        {
            Error_Txt.text = "";
            bool check1 = GameUtils.CnahgeCheckIcon(value.Length > 0, CheckNickname1_Img);
            bool check2 = GameUtils.CnahgeCheckIcon(SetNickname_If.text.Length <= 10, CheckNickname2_Img);
        });
    }

    private void Start()
    {
        if (!DataManager.IsMobilePlatform)
        {
            SetNickname_If.Select();
        }        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            OnSubmit();
        }
    }

    /// <summary>
    /// 提交
    /// </summary>
    private void OnSubmit()
    {
        if (SetNickname_If.text.Trim().Length <= 0)
        {
            Error_Txt.text = LanguageManager.Instance.GetText("Please Enter A Nickname.");
        }
        else
        {
            currNickname = SetNickname_If.text.Trim();

#if UNITY_EDITOR

            //修改資料
            Dictionary<string, object> dataDic = new()
            {
                { FirebaseManager.NICKNAME, currNickname },
            };
            JSBridgeManager.Instance.UpdateDataFromFirebase(
                $"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserId}",
                dataDic,
                gameObject.name,
                nameof(SetNicknameSuccess));
            return;
#endif

            string nickname = SetNickname_If.text.Trim();
            currNickname = nickname;
            ViewManager.Instance.OpenWaitingView(transform);
            JSBridgeManager.Instance.CheckUserDataExist(FirebaseManager.NICKNAME,
                                                        currNickname,
                                                        gameObject.name,
                                                        nameof(CheckNicknameCallback));
        }
    }

    /// <summary>
    /// 暱稱重複檢查
    /// </summary>
    /// <param name="jsonData">回傳結果(true/false)</param>
    public void CheckNicknameCallback(string jsonData)
    {
        var data = JsonUtility.FromJson<CheckUserData>(jsonData);

        if (data.exists == "true")
        {
            Error_Txt.text = LanguageManager.Instance.GetText("Duplicate Nickname, Please Try Again.");
            return;
        }

        //修改資料
        Dictionary<string, object> dataDic = new()
        {
            { FirebaseManager.NICKNAME, currNickname },
        };
        JSBridgeManager.Instance.UpdateDataFromFirebase(
            $"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserId}",
            dataDic);

        SetNicknameSuccess();
    }

    /// <summary>
    /// 設置暱稱成功
    /// </summary>
    private void SetNicknameSuccess()
    {
        DataManager.UserNickname = currNickname;
        GameObject.FindFirstObjectByType<LobbyView>().UpdateUserData();
        Destroy(gameObject);
    }
}
