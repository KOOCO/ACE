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
    TextMeshProUGUI Nickname_Txt, Point_Txt, Award_Txt, Rank_Txt;

    /// <summary>
    /// 設置排名資料
    /// </summary>
    /// <param name="rankData"></param>
    /// <param name="rank">排名</param>
    /// <param name="pointStr">點數文字</param>
    public void SetRankData(RankData rankData, int rank, string pointStr)
    {
        int avatarIndex = rankData.nickname == DataManager.UserNickname ?
                          DataManager.UserAvatarIndex :
                          rankData.avatar;
        Avatar_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album[avatarIndex];
        Nickname_Txt.text = rankData.nickname == DataManager.UserNickname ?
                            $"<color=#E6C94E>{rankData.nickname}</color>" :
                            $"<color=#FFFFFF>{rankData.nickname}</color>";
        Point_Txt.text = $"{rankData.point.ToString()}";
        Award_Txt.text = rankData.award.ToString();
        Rank_Txt.text = rank.ToString();
    }
}
