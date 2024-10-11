using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class languageBtnBG : MonoBehaviour
{
    [SerializeField] private List<languageButton> languageButtons;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void unSelectAllBtn()
    {
        for (int i = 0; i < languageButtons.Count; i++)
        {
            languageButtons[i].notSelect();
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
