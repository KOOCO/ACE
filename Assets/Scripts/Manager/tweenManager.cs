using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class tweenManager : MonoBehaviour
{
    public static tweenManager inst;
    public List<GameObject> Cards;

    //public Transform Target;
    [Header("發牌")]
    public Transform dealCard;
    public List<Transform> playerSeats;

    [Header("棄牌")]
    public Transform foldCard;

    [Header("公牌")]
    public Transform communityCard;
    public List<Transform> Targets3;
    public Transform communityCard5;
    public List<Transform> Targets5;
    
    //[Header("下注")]
    //public Transform localBet;
    //public Transform playerBet;
    
    //[Header("補籌碼")]
    //public Transform localFetch;
    //public Transform playerFetch;
    
    [Header("D按鈕")]
    public Transform D_Btn;
    public List<Transform> D_Targets;
    public List<Transform> D_TargetsActive;

    bool isComplete;

    //.Sequence cardSequence;

    //Test
    //public Transform obj;
    //public Transform target;

    private void Awake()
    {
        inst = this;
        print(inst == null);

        foreach (var obj in Cards)
            obj.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //foreach (var obj in D_Targets)
        //{
        //    if (obj.gameObject.activeInHierarchy)
        //        D_TargetsActive.Add(obj);
        //}
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setSeats()
    {
        List<Transform>Seats = new List<Transform>();
        foreach(var obj in playerSeats)
        {
            if (obj.gameObject.activeSelf)
                Seats.Add(obj);
        }
        foreach(var info in Seats)
        {
            if (!info.GetComponent<GamePlayerInfo>().IsPlaying)
                Seats.RemoveAt(Seats.IndexOf(info));
        }

        dealCard.gameObject.SetActive(true);
        StartCoroutine(animChain(Seats));
    }

    public void playFold()
    {
        foldCard.gameObject.SetActive(true);
        StartCoroutine(animChain(foldCard));
    }
    
    public void playBet(Transform Target)
    {
        Target.gameObject.SetActive(true);
        StartCoroutine(animChain(Target));
    }
    
    public void playCommunity()
    {
        communityCard.gameObject.SetActive(true);
        StartCoroutine(communityChain());
    }
    
    public void playCommunity5()
    {
        communityCard5.gameObject.SetActive(true);
        StartCoroutine(communityChain5());
    }

    public void dealAnim(Transform obj, Transform target)
    {
        Sequence cardSequence = DOTween.Sequence();
        cardSequence.Append(obj.DOMove(target.position, 1)).Insert(0, obj.DOLookAt2D(target.position, 0));
        cardSequence.OnComplete(()=>
        {
            obj.GetComponent<pokerAnim>().onComplete(true);
        });
    }
    
    public void foldAnim(Transform obj, Transform target)
    {
        Sequence cardSequence = DOTween.Sequence();
        cardSequence.Append(obj.DOMove(target.position, 1)).Insert(0, obj.DORotate(new Vector3(20f, 0, 0), 1f)).Insert(0, obj.DORotate(new Vector3(0, 0, 360f), 1, RotateMode.FastBeyond360)).Insert(0, obj.DOScale(0.75f, 1)).OnComplete(() =>
        {
            obj.GetComponent<pokerAnim>().onComplete(false);
        });
    }
    
    public void betAnim(Transform obj, Transform target)
    {
        Sequence cardSequence = DOTween.Sequence();
        cardSequence.Append(obj.DOMove(target.position, 0.25f)).OnComplete(()=>
        {
            obj.GetComponent<pokerAnim>().onComplete(false);
        });
    }

    public void biggerAnim(Transform obj, bool isSolid = false)
    {
        Sequence cardSequence = DOTween.Sequence();
        cardSequence.Append(obj.DOScale(obj.localScale.x + 0.15f, 0.5f)).OnComplete(()=>
        {
            if (isSolid)
                obj.localScale = new Vector2(1.15f, 1.15f);
        });
    }
    
    public void communityAnim(Transform obj, Transform target)
    {
        isComplete = false;
        Sequence cardSequence = DOTween.Sequence();
        cardSequence.Append(obj.DOMove(target.position, 0.25f)).OnComplete(()=>
        {
            isComplete = true;
            if (obj.name == "CommuniyCards last")
            {
                for (int i = Targets3.Count-1; i > -1; i--)
                {
                    int index = i; 
                    Targets3[index].GetComponent<pokerAnim>().onComplete(false);
                }
            }
        });
    }
    
    public void communityAnim5(Transform obj, Transform target)
    {
        isComplete = false;
        Sequence cardSequence = DOTween.Sequence();
        cardSequence.Append(obj.DOMove(target.position, 0.15f)).OnComplete(()=>
        {
            isComplete = true;
            if (obj.name == "CommuniyCards last")
            {
                for (int i = Targets5.Count-1; i > -1; i--)
                {
                    int index = i; 
                    Targets5[index].GetComponent<pokerAnim>().onComplete(false);
                }
            }
        });
    }

    public void addTargets()
    {
        if (D_TargetsActive.Count > 0)
            D_TargetsActive.Clear();

        foreach (var obj in D_Targets)
        {
            if (obj.gameObject.activeInHierarchy)
                D_TargetsActive.Add(obj);
        }
    }
    public void initDPos()
    {
        GameView gameView = GetComponent<GameView>();
        GamePlayerInfo info = null;
        addTargets();

        if (gameView != null)
            info = gameView.SeatGamePlayerInfoList.FirstOrDefault(x => x.getDPos() != null);

        if (info != null)
        {
            D_Btn.SetParent(info.getDPos());
            D_Btn.localPosition = Vector2.zero;
        }
        else
            print("尚無開啟D位");
    }

    public void DPosAnim()
    {
        Transform parent = D_Btn.parent;
        int i = D_TargetsActive.IndexOf(parent);
        i = (i+1 >= D_TargetsActive.Count) ? 0 : i+1;
        print(i);
        Transform target = D_TargetsActive[i];
        Sequence cardSequence = DOTween.Sequence();
        cardSequence.Append(D_Btn.DOMove(target.position, 0.5f)).OnComplete(() =>
        {
            D_Btn.SetParent(target);
        });
    }

    public void fetchChipAnim(Transform obj)
    {
        Vector3 randomRotation = new Vector3(
            UnityEngine.Random.Range(0f, 360f), // X軸的隨機角度
            UnityEngine.Random.Range(0f, 360f), // Y軸的隨機角度
            0f                      // Z軸 (保留0或依需求隨機)
        );
        Sequence cardSequence = DOTween.Sequence();
        cardSequence.Append(obj.DOMove(D_Btn.parent.position, 0.5f)).Insert(0, obj.DORotate(randomRotation, 0.5f, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutQuad)).OnComplete(() =>
        {
            obj.GetComponent<pokerAnim>().onComplete(false);
        });
    }

    public void getAnim(string Name)
    {
        gameObject.SendMessage(Name);
    }
    public void getAnim(string Name, Transform obj)
    {
        gameObject.SendMessage(Name, obj);
    }

    IEnumerator animChain(Transform PTrans)
    {
        for (int i = 1; i < 3; i++)
        {
            int index = i;
            PTrans.GetChild(index).gameObject.SetActive(true);

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);
        PTrans.gameObject.SetActive(false);
    }
    IEnumerator communityChain()
    {
        for (int i = 0; i < Targets3.Count; i++)
        {
            int index = i;
            Targets3[index].gameObject.SetActive(true);

            yield return new WaitUntil(()=> isComplete);
            isComplete = false;
        }

        yield return new WaitForSeconds(1f);
        communityCard.gameObject.SetActive(false);
    }
    IEnumerator communityChain5()
    {
        for (int i = 0; i < Targets5.Count; i++)
        {
            int index = i;
            Targets5[index].gameObject.SetActive(true);

            yield return new WaitUntil(()=> isComplete);
            isComplete = false;
        }

        yield return new WaitForSeconds(1f);
        communityCard5.gameObject.SetActive(false);
    }
    IEnumerator animChain(List<Transform> seats)
    {
        while (seats.Count > 0)
        {
            for (int i = 0; i < 2; i++)
            {
                GameObject animObj = Instantiate(dealCard.GetChild(0).gameObject, dealCard);
                animObj.GetComponent<pokerAnim>().getTrans = seats[0];
                animObj.SetActive(true);

                yield return new WaitForSeconds(0.1f);
            }

            seats.RemoveAt(0);
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(1f);

        if (seats.Count <= 0)
        {
            dealCard.gameObject.SetActive(false);
            //for (int i = 1; i < dealCard.childCount; i++)
            //    Destroy(dealCard.GetChild(i).gameObject);
        }
    }
}

public static class DOTween2DExtensions
{
    /// <summary>
    /// 讓物件在2D空間朝向目標
    /// </summary>
    /// <param name="target">要旋轉的物件</param>
    /// <param name="destination">目標位置</param>
    /// <param name="duration">動畫時間</param>
    /// <returns>Tweener</returns>
    public static Tweener DOLookAt2D(this Transform target, Vector2 destination, float duration)
    {
        Vector2 direction = (destination - (Vector2)target.position).normalized; // 計算方向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // 方向轉角度
        angle -= 90f;
        return target.DORotate(new Vector3(0, 0, angle), duration); // 只旋轉 Z 軸
    }
}
