using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestPageBtn : MonoBehaviour
{
    [Header("領取按鈕")]
    [SerializeField]
    Button Receive;

    [Header("已領取")]
    [SerializeField]
    GameObject Received;

    [Header("任務名稱和報酬")]
    [SerializeField]
    TextMeshProUGUI QuestName;
    [SerializeField]
    TextMeshProUGUI CoinAmount;

    [Header("任務進程")]
    [SerializeField]
    GameObject schedule;
    [SerializeField]
    TextMeshProUGUI QuestProgress;


    [Header("紅利Point Icon")]
    [SerializeField]
    TextMeshProUGUI PointAmount;
    [SerializeField]
    int BonusAmount;


    /// <summary>
    /// 設置初始任務資料
    /// </summary>
    /// <param name="info">任物資料</param>
    public void SetQuestInfo(QuestInfo info)
    {
        if (info.Received)
        {
            Received.SetActive(true);
            ReceiveActive(false);
        }

        if (info.CurrentProgress >= info.FinishProgress && !info.Received)
            ReceiveActive(true);


        QuestName.text = info.QuestName;
        CoinAmount.text = $"* {info.GetCoin}";
        QuestProgress.text = $"{info.CurrentProgress}/{info.FinishProgress}";
        PointAmount.text = $"+{info.GetPoint}";

        BonusAmount = info.GetPoint;


        //  任務完成後領取獎勵
        Receive.onClick.AddListener(() =>
        {
            ReceiveActive(!Receive.gameObject.activeSelf);
            Received.SetActive(true);

            Debug.Log($"你領取了: {info.GetCoin} 籌碼");

            DataManager.CurrentBonusAmount += BonusAmount;
            info.Received = true;
            GameObject.FindFirstObjectByType<QuestBonus>().SetBonus();          //  查詢場景QuestBonus並更新
            GameObject.FindFirstObjectByType<QuestView>().ReceiveMaskActive();  //  生成領取遮罩

            Debug.Log($"你獲得了: {BonusAmount} 點數");
        });
    }

    /// <summary>
    /// 獲取任務名稱
    /// </summary>
    /// <returns></returns>
    public string GetQuestName() { return QuestName.text; }

    /// <summary>
    /// 獲取任務進程
    /// </summary>
    /// <returns></returns>
    public string GetQuestProgress() { return QuestProgress.text; }

    /// <summary>
    /// 設置任務進程
    /// </summary>
    /// <param name="Progress"></param>
    public void SetQuestProgress(string Progress) { QuestProgress.text = Progress; }

    /// <summary>
    /// 生成領取按鈕
    /// </summary>
    /// <param name="Active"></param>
    public void ReceiveActive(bool Active)
    {
        if (Receive != null)
            Receive.gameObject.SetActive(Active);
    }

    /// <summary>
    /// 生成已領取按鈕遮罩
    /// </summary>
    public void CreateReceived()
    {
        Received.SetActive(true);
    }

}
