using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundToggleGroup : MonoBehaviour
{
    [SerializeField] public Toggle toggleOn;
    [SerializeField] Toggle toggleOff;
    [SerializeField] public static bool isSoundOpen;
    // Start is called before the first frame update
    void Start()
    {
        var isMusicOpen = PlayerPrefs.GetInt(nameof(isSoundOpen));
        if (isMusicOpen == 0)
        {
            toggleOn.isOn = true;
        }
        if (isMusicOpen != 0)
        {
            toggleOff.isOn = true;
        }

        toggleOn.onValueChanged.AddListener(
            (x) =>
            {

                if (x)
                {
                    //為了讓第一次進入還沒有設定過音樂開關的玩家
                    //能撥放音樂 所以以0為撥放音樂的代號
                    //(未設定時getInt會回傳0)
                    PlayerPrefs.SetInt(nameof(isSoundOpen), 0);
                }

                if (!x)
                {
                    PlayerPrefs.SetInt(nameof(isSoundOpen), 1);
                }
                Debug.Log("toggleOn is On=" + x + nameof(isSoundOpen) + "");


                //var lobbyView = GameObject.FindAnyObjectByType<LobbyView>();
                //SoundToggleGroup.IsPlayAudio(lobbyView.audioSource);
            });

        toggleOff.onValueChanged.AddListener(
            (x) =>
            {
                Debug.Log("toggleOn is On=" + x);
            });

    }

    public static void IsPlayAudio(AudioSource audioSource)
    {
        int playsoundNum = PlayerPrefs.GetInt(nameof(SoundToggleGroup.isSoundOpen));
        if (playsoundNum == 0)
        {
            audioSource.Play();
        }

        if (playsoundNum == 1)
        {
            audioSource.Stop();
        }
    }

}
