using Nethereum.Web3;
using Reown.AppKit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WalletConnect : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Button ConnectBtn, OpenNetWorkBtn, OpneAccountBtn, SendTransactionBtn, DisconnectBtn;
    public GameObject ButtonParent;
    private List<ButtonState> ButtonList;
    string abi = "";

    private async void Start()
    {
        ListenerEvent();
        ButtonList = ButtonParent.GetComponentsInChildren<ButtonState>().ToList();

        GetAbi();
        //abi = File.ReadAllText("C:\\Users\\xuanyu\\WalletConnect\\Assets\\erc20.abi.json");

        UpdatehButtons();
        AppKit.ChainChanged += (_, e) =>
        {
            UpdatehButtons();

            if (e.NewChain == null)
            {
                Debug.LogError("Unsupported chain");
                return;
            }
        };

        AppKit.AccountConnected += async (_, e) => UpdatehButtons();

        AppKit.AccountDisconnected += (_, _) => UpdatehButtons();

        AppKit.AccountChanged += (_, e) => UpdatehButtons();

        // After the scene and UI are loaded, try to resume the session from the storage
        var sessionResumed = await AppKit.ConnectorController.TryResumeSessionAsync();
        Debug.Log($"Session resumed: {sessionResumed}");
    }
    private void UpdatehButtons()
    {
        foreach (ButtonState button in ButtonList)
        {
            button.SetButton(true);
            if (button.isNeedAccount)
            {
                if (!AppKit.IsAccountConnected)
                {
                    button.SetButton(false);
                }
            }
        }
    }

    // Update is called once per frame
    private void ListenerEvent()
    {
        ConnectBtn.onClick.AddListener(() =>
        {
            OpenAppKitModal();
        });
        OpenNetWorkBtn.onClick.AddListener(() => {
            OpenNetwork();
        });
        OpneAccountBtn.onClick.AddListener(() =>
        {
            OpenAccount();
        });
        SendTransactionBtn.onClick.AddListener(() =>
        {
            SendTransaction();
        });
        DisconnectBtn.onClick.AddListener(() =>
        {
            OnDisconnect();
        });
    }
    private void OpenAppKitModal()
    {
        AppKit.OpenModal();
    }
    private void OpenNetwork()
    {
        AppKit.OpenModal(ViewType.NetworkSearch);
    }
    private void OpenAccount()
    {
        AppKit.OpenModal(ViewType.Account);
        // 連接成功，處理後續操作
    }
    private async void SendTransaction()
    {
        if (AppKit.NetworkController.ActiveChain.ChainId != "eip155:1")
        {
            Debug.LogError(AppKit.NetworkController.ActiveChain.ChainId);
            Debug.LogError("請切換到Ethereum");
        }
        const string toAddress = "0x300a5f472588449b6724157b532507573048e8a0";
        BigInteger amount = Web3.Convert.ToWei(0.001);

        try
        {
            Debug.Log("正在發送交易...");

            var value = Web3.Convert.ToWei(0.001);
            var result = await AppKit.Evm.SendTransactionAsync(toAddress, value);
            Debug.Log("交易hash: " + result);

            Debug.Log("交易已發送");
        }
        catch (Exception e)
        {
            Debug.Log($"發送交易失敗\n{e.Message}");
            Debug.LogException(e, this);
        }
    }
    private async void OnDisconnect()
    {
        Debug.Log("[AppKit Sample] OnDisconnectButton");

        try
        {
            await AppKit.DisconnectAsync();
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }
    private void GetAbi()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Json/erc20.abi");
        string jsonString = jsonFile.text;
        abi = jsonString;
    }
}
