using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        string localIP = GetLocalIPAddress();
        Debug.Log("本機 IP 地址: " + localIP);
    }

    string GetLocalIPAddress()
    {
        string localIP = "";
        try
        {
            // 獲取本機 DNS 資料
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                // 檢查是否為 IPv4 地址，且不是回送地址
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            if (string.IsNullOrEmpty(localIP))
            {
                throw new System.Exception("找不到本機 IP 地址！");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("獲取本機 IP 地址時出錯: " + ex.Message);
        }

        return localIP;
    }
}
