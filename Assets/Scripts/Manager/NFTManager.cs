using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
//using Thirdweb;

public class NFTManager : UnitySingleton<NFTManager>
{
    public List<NFTData> NFTDataList;
    public List<Sprite> NFTImageList;

    /// <summary>
    /// 所有練名稱
    /// </summary>
    string[] allChainName = new string[]
    {
        "arbitrum",
        "arbitrum_nova",
        "avalanche",
        "base",
        "blast",
        "bsc",
        "ethereum",
        "klaytn",
        "matic",
        "optimism",
        "solana",
        "zora",
    };

    public override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 開始執行更新處理
    /// </summary>
    public void StartHandleUpdate()
    {
        InvokeRepeating(nameof(UpdateNFT), 1, 600);
    }

    /// <summary>
    /// 取消更新
    /// </summary>
    public void CancelUpdate()
    {
        CancelInvoke(nameof(UpdateNFT));
    }

    /// <summary>
    /// 更新NFT資料
    /// </summary>
    public void UpdateNFT()
    {
        if (string.IsNullOrEmpty(DataManager.UserWalletAddress))
        {
            return;
        }

        StartCoroutine(IUpdateNFT());
    }

    /// <summary>
    /// 更新NFT資料
    /// </summary>
    /// <returns></returns>
    private IEnumerator IUpdateNFT()
    {
        NFTDataList = new List<NFTData>();
        NFTImageList = new List<Sprite>();

        foreach (var chainName in allChainName)
        {
            int limit = 20;
            string walletAddress = DataManager.UserWalletAddress;
            string apiUrl = $"https://api.opensea.io/api/v2/chain/{chainName}/account/{walletAddress}/nfts?limit={limit}";

            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                request.SetRequestHeader("accept", "application/json");
                request.SetRequestHeader("x-api-key", DataManager.NFTApiKey);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error: " + request.error);
                }
                else
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"Get {chainName} NFT Data:{jsonResponse}");
                    NFTAssets response = JsonUtility.FromJson<NFTAssets>(jsonResponse);
                    foreach (var data in response.nfts)
                    {
                        NFTData nftData = new NFTData();
                        nftData.name = data.name;
                        nftData.updated_at = data.updated_at;
                        nftData.description = data.description;
                        nftData.display_image_url = data.display_image_url;
                        nftData.identifier = data.identifier;

                        yield return Utils.ImageUrlToSprite(nftData.display_image_url, (sp) =>
                        {
                            NFTDataList.Add(nftData);
                            NFTImageList.Add(sp);
                        });                        
                    }
                }
            }
        }

        Debug.Log("NFT Loaded!!!");

        //更新MyNFTView介面
        GameObject nftView = GameObject.Find("MyNFTView");
        if (nftView != null)
        {
            nftView.GetComponent<MyNFTView>().DisplayNFT();
        }
    }

    private void OnDestroy()
    {
        CancelUpdate();        
    }
}
