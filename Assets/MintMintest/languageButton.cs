using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class languageButton : MonoBehaviour
{
    [SerializeField] private Button transparentBtn;
    [SerializeField] private Image darkCircle;
    [SerializeField] private Image lightCircle;
    [SerializeField] private Image solidCircle;

    [SerializeField] private TextMeshProUGUI languageName; 
    // Start is called before the first frame update
    void Start()
    {
        transparentBtn.onClick.AddListener(() =>
        {
            Debug.Log("hi");
            
        });
    }

    public void onSelected()
    {
        lightCircle.enabled = true;
        solidCircle.enabled = true;
        languageName.color = Color.white;
        
    }
    public void notSelect()
    {
        lightCircle.enabled = false;
        solidCircle.enabled = false;
        languageName.color = Color.gray;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
