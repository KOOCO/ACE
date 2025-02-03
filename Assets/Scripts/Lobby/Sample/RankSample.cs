using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RankSample : MonoBehaviour
{
    [SerializeField]
    Image Avatar_Img;
    [SerializeField]
    TextMeshProUGUI Nickname_Txt, status_Txt, Award_Txt, Rank_Txt;
    [SerializeField]
    bool isSelfBar;

    /// <summary>
    /// 設置排名資料
    /// </summary>
    /// <param name="rankData"></param>
    /// <param name="rank">排名</param>
    /// <param name="pointStr">點數文字</param>
    public void SetRankData(RankData rankData, string rank)
    {
        int avatarIndex = rankData.nickname == DataManager.UserNickname ?
                          DataManager.UserAvatarIndex :
                          rankData.avatar;
        Avatar_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album[avatarIndex];
        if (rankData.nickname == DataManager.UserNickname && !isSelfBar)
        {
            Nickname_Txt.text = $"<color=#E6C94E>{rankData.nickname}</color>";
        }
        else
        {
            Nickname_Txt.text = $"<color=#FFFFFF>{rankData.nickname}</color>";
        }
        status_Txt.text = rankData.status ? $"<color=#36D982>{LanguageManager.Instance.GetText("Online")}</color>" : $"<color=#EC6273>{LanguageManager.Instance.GetText("Offline")}</color>";
        Award_Txt.text = "$" + rankData.point.ToString();
        Rank_Txt.text = rank;
    }

}
