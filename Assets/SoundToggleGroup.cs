using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundToggleGroup : MonoBehaviour
{
    [SerializeField]public Toggle toggleOn;
    [SerializeField] Toggle toggleOff;
    [SerializeField] static bool isSoundOpen;
    // Start is called before the first frame update
    void Start()
    {
        toggleOn.onValueChanged.AddListener(
            (x) =>
            {
                if (x)
                {
                    PlayerPrefs.SetInt(nameof(isSoundOpen),1);
                }
                
                if (!x)
                {
                    PlayerPrefs.SetInt(nameof(isSoundOpen),0);
                }
                Debug.Log("toggleOn is On="+x+nameof(isSoundOpen)+"");
            });
        
        toggleOff.onValueChanged.AddListener(
            (x) =>
            {
                Debug.Log("toggleOn is On="+x);
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
