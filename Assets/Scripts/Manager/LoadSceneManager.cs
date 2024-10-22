using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using TMPro;
public class LoadSceneManager : UnitySingleton<LoadSceneManager>
{
    [SerializeField]
    RectTransform lodingView;
    [SerializeField]
    Image Progress_Img;
    [SerializeField]
    TextMeshProUGUI Loading_Txt, Progress_Txt;

    [Header("開啟介面")]
    [SerializeField]
    GameObject LoginViewObj, LobbyViewObj;

    public bool isGetUserData { get; set; }

    DateTime startYieldTime;

    public override void Awake()
    {
        base.Awake();

        Loading_Txt.text = "Now Loading...";
        lodingView.gameObject.SetActive(false);
    }

    /// <summary>
    /// 載入場景
    /// </summary>
    /// <param name="sceneEnum">進入場景</param>
    public void LoadScene(SceneEnum sceneEnum)
    {
        if (SceneManager.GetActiveScene().name != "Entry")
        {
            StartCoroutine(ILoadScene(sceneEnum));
        }
        else if (SceneManager.GetActiveScene().name == "Login")
        {
            StartCoroutine(IEntryInToLobby(sceneEnum));
        }
        else
        {
            StartCoroutine(IEntryInToLogin(sceneEnum));
        }
    }

    /// <summary>
    /// Entry載入登入場景
    /// </summary>
    /// <returns></returns>
    private IEnumerator IEntryInToLogin(SceneEnum sceneEnum)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneEnum.ToString());
        asyncLoad.allowSceneActivation = false;

        // 等待加载完成
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
                yield return new WaitForSeconds(0.1f);

                DataManager.CurrScene = sceneEnum;
                ViewManager.Instance.Init();
                JudgeIntoScene(sceneEnum);
            }

            yield return null;
        }
    }

    /// <summary>
    /// 進入大廳場景
    /// </summary>
    /// <returns></returns>
    private IEnumerator IEntryInToLobby(SceneEnum sceneEnum)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneEnum.ToString());
        asyncLoad.allowSceneActivation = false;

        // 等待加载完成
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f && isGetUserData)
            {
                asyncLoad.allowSceneActivation = true;
                yield return new WaitForSeconds(0.1f);

                DataManager.CurrScene = sceneEnum;
                ViewManager.Instance.Init();
                JudgeIntoScene(sceneEnum);
            }

            yield return null;
        }
    }

    /// <summary>
    /// 載入場景
    /// </summary>
    /// <param name="sceneEnum">進入場景</param>
    /// <returns></returns>
    private IEnumerator ILoadScene(SceneEnum sceneEnum)
    {
        lodingView.gameObject.SetActive(true);
        Progress_Img.fillAmount = 0;

        // 异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneEnum.ToString());
        asyncLoad.allowSceneActivation = false;

        // 等待加载完成
        while (!asyncLoad.isDone)
        {
            startYieldTime = DateTime.Now;
            while (Progress_Img.fillAmount < 0.9f)
            {
                float progress = (float)(DateTime.Now - startYieldTime).TotalSeconds / 0.8f;
                Progress_Img.fillAmount = asyncLoad.progress < progress ?
                                          progress :
                                          Mathf.Lerp(0, 0.9f, progress);
                Progress_Txt.text = $"{(Progress_Img.fillAmount * 100):F0}%";

                yield return null;
            }

            if (asyncLoad.progress >= 0.9f && Progress_Img.fillAmount >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;

                DataManager.CurrScene = sceneEnum;

                startYieldTime = DateTime.Now;
                while ((DateTime.Now - startYieldTime).TotalSeconds < 0.5f)
                {
                    float progress = (float)(DateTime.Now - startYieldTime).TotalSeconds / 0.5f;
                    Progress_Img.fillAmount = Mathf.Lerp(0.9f, 1, progress);
                    Progress_Txt.text = $"{(Progress_Img.fillAmount * 100):F0}%";
                    yield return null;
                }

                ViewManager.Instance.Init();
                JudgeIntoScene(sceneEnum);
            }

            yield return null;
        }

        yield return new WaitForSeconds(1);

        lodingView.gameObject.SetActive(false);
    }
    LoginView loginView;
    /// <summary>
    /// 判斷進入場景
    /// </summary>
    /// <param name="sceneEnum">進入場景</param>
    private void JudgeIntoScene(SceneEnum sceneEnum)
    {
        switch (sceneEnum)
        {
            case SceneEnum.Login:
#if !UNITY_EDITOR
                JSBridgeManager.Instance.OpenRecaptchaTool();
#endif
                NFTManager.Instance.CancelUpdate();
                loginView = ViewManager.Instance.CreateViewInCurrCanvas<LoginView>(LoginViewObj);
                break;

            case SceneEnum.Lobby:
#if !UNITY_EDITOR
                JSBridgeManager.Instance.CloseRecaptchaTool();
#endif
                DataManager.ReciveRankData();
                ViewManager.Instance.CreateViewInCurrCanvas<LobbyView>(LobbyViewObj);
                break;

            case SceneEnum.Game:
                break;
        }
    }


    public void NoodleRegister(string loginString)
    {
        if (string.IsNullOrEmpty(loginString))
        {
            Debug.LogError("Invalid login string.");
            return;
        }

        // Call SwaggerAPIManager after a 3-second delay
        StartCoroutine(DelayedNoodleLogin(loginString));
    }

    // Coroutine to delay the execution
    private IEnumerator DelayedNoodleLogin(string loginString)
    {
        // Wait for 3 seconds
        yield return new WaitForSeconds(1f);

        // Call the Swagger API
        SwaggerAPIManager.Instance.SendPostAPI<LoginView>($"/api/app/games/ace/decrypt-session?session={loginString}", null, loginView.RegisterWithNoodle, (x) =>
        {
            Debug.Log("Noodle Login Failed: " + x);
        }, false, true);
    }
}
