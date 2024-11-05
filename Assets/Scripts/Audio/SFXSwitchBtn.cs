using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SFXSwitchBtn : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Image _imageoOn;
    [SerializeField] private Image _imageOff;
    [SerializeField] public Button btn;
    [SerializeField] public static bool isSFXOpen;
    void Start()
    {
        showImg(false);
        btn.onClick.AddListener(() =>
            {
                showImg(true);
                IsPlaySFX();
            }
            
        );
    }

    private void showImg(bool ifNeedSwitch)
    {
        if (ifNeedSwitch)
        {
            changeCurrentState();
        }

        var isMusicOpen = PlayerPrefs.GetInt(nameof(isSFXOpen));
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
        var switchNum= PlayerPrefs.GetInt(nameof(isSFXOpen));
        if (switchNum == 0)
        {
            PlayerPrefs.SetInt(nameof(isSFXOpen),1);
            return 0;
        }

        if (switchNum == 1)
        {
            PlayerPrefs.SetInt(nameof(isSFXOpen),0);
            return 1;
        }

        return 0;
    }
    
    public static void IsPlaySFX()
    {
        int playsoundNum = PlayerPrefs.GetInt(nameof(isSFXOpen));
        if (playsoundNum == 0)
        {
            AudioManager.Instance.setSFXOnOff(1);
        }

        if (playsoundNum==1)
        {
            AudioManager.Instance.setSFXOnOff(0);
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
}
