using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class pokerAnim : MonoBehaviour
{
    public string tweenName;
    public Transform getTrans;
    Vector3 oriPos;
    Quaternion oriAng;

    private void Awake()
    {
        oriPos = transform.position;
        oriAng = transform.localRotation;
    }

    private void OnEnable()
    {
        switch (tweenName)
        {
            case "dealAnim_Local":
                if(getTrans != null)
                    tweenManager.inst.dealAnim_Local(transform, getTrans);
                break;
            case "dealAnim_Other":
                if(getTrans != null)
                    tweenManager.inst.dealAnim_Other(transform, getTrans);
                break;
            case "foldAnim":
                if(getTrans != null)
                    tweenManager.inst.foldAnim(transform, getTrans);
                break;
            case "Community":
                if(getTrans != null)
                    tweenManager.inst.communityAnim(transform, getTrans);
                break;
            case "Community5":
                if(getTrans != null)
                    tweenManager.inst.communityAnim5(transform, getTrans);
                break;
            case "Bet":
                if(getTrans != null)
                    tweenManager.inst.betAnim(transform, getTrans);
                break;
            case "Fetch":
                    tweenManager.inst.fetchChipAnim(transform);
                break;
        }
    }

    private void OnDisable()
    {
        transform.position = oriPos;
        transform.localRotation = oriAng;
    }

    public void Back()
    {
        transform.position = oriPos;
        transform.localRotation = oriAng;
    }

    public void onComplete(bool destroy)
    {
        transform.position = oriPos;
        transform.localRotation = oriAng;
        if (destroy)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);        
    }
}
