using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MusicSwitchBtn : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Image _imageoOn;
    [SerializeField] private Image _imageOff;
    [SerializeField] public Button btn;
    [SerializeField] public static bool isSoundOpen;
    void Start()
    {
        showImg(false);
        btn.onClick.AddListener(() =>
            {
                showImg(true);
                var lobbyView = GameObject.FindAnyObjectByType<LobbyView>();
                MusicSwitchBtn.IsPlayAudio(lobbyView.audioSource);
            }
            
        );
    }

    private void showImg(bool ifNeedSwitch)
    {
        if (ifNeedSwitch)
        {
            changeCurrentState();
        }

        var isMusicOpen = PlayerPrefs.GetInt(nameof(isSoundOpen));
        if (isMusicOpen == 0)
        {
            _imageOff.enabled = false;
            _imageoOn.enabled = true;
            return;
        }

        if (isMusicOpen == 1)
        {
            _imageOff.enabled = true;
            _imageoOn.enabled = false;
            return;
        }
    }

    public int changeCurrentState()
    {
        var switchNum= PlayerPrefs.GetInt(nameof(isSoundOpen));
        if (switchNum == 0)
        {
            PlayerPrefs.SetInt(nameof(isSoundOpen),1);
            return 0;
        }

        if (switchNum == 1)
        {
            PlayerPrefs.SetInt(nameof(isSoundOpen),0);
            return 1;
        }

        return 0;
    }
    
    public static void IsPlayAudio(AudioSource audioSource)
    {
        int playsoundNum = PlayerPrefs.GetInt(nameof(isSoundOpen));
        if (playsoundNum == 0)
        {
            audioSource.Play();
        }

        if (playsoundNum==1)
        {
            audioSource.Stop();
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
}
