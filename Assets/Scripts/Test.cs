using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        string localIP = GetLocalIPAddress();
        Debug.Log("���� IP �a�}: " + localIP);
    }

    string GetLocalIPAddress()
    {
        string localIP = "";
        try
        {
            // ������� DNS ���
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                // �ˬd�O�_�� IPv4 �a�}�A�B���O�^�e�a�}
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            if (string.IsNullOrEmpty(localIP))
            {
                throw new System.Exception("�䤣�쥻�� IP �a�}�I");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("������� IP �a�}�ɥX��: " + ex.Message);
        }

        return localIP;
    }
}
