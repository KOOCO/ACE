using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class InternetCheck : MonoBehaviour
{
    public GameObject popupPanel; // Reference to the popup panel
    public Button confirmButton;  // Reference to the Confirm button
    private bool isOffline = false;
    public Canvas canvas;

    void Start()
    {
        // Ensure the popup is hidden at the start
        if (popupPanel != null)
        {
            canvas.gameObject.SetActive(false);
            popupPanel.SetActive(false);
        }

        // Add the listener for the Confirm button
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClick);

        // Start monitoring internet connection
        InvokeRepeating("CheckInternetConnection", 0f, 5f); // Check every 5 seconds
    }

    void CheckInternetConnection()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable && !isOffline)
        {
            // Internet is disconnected
            ShowPopup();
            isOffline = true;
        }
        else if (Application.internetReachability != NetworkReachability.NotReachable && isOffline)
        {
            // Internet is reconnected
            HidePopup();
            isOffline = false;
        }
    }

    void ShowPopup()
    {
        if (popupPanel != null)
        {
            Debug.Log(nameof(ShowPopup));
            canvas.gameObject.SetActive(true);
            popupPanel.SetActive(true);
        }
    }

    void HidePopup()
    {
        if (popupPanel != null)
        {
            canvas.gameObject.SetActive(false);
            popupPanel.SetActive(false);
        }
    }

    void OnConfirmClick()
    {
        // Close the popup when the Confirm button is clicked
        HidePopup();
        OnClickConfirmBtn();
        Debug.Log("User acknowledged the disconnection.");
    }

    [DllImport("__Internal")]
    private static extern void JS_WindowClose();

    private void CallJSWindowClose(string unused)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        JS_WindowClose();
#endif
    }

    public void OnClickConfirmBtn()
    {
        AppApi.LogoutRequest(CallJSWindowClose);
    }
}
