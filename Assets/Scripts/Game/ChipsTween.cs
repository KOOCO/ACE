using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;

public class ChipsTween : MonoBehaviour
{
    public Transform PotPos;
    public List<GameObject> Stacks;

    private List<Transform> chips = new List<Transform>();

    private int curIndex = 0;
    public void GameInit()
    {
        foreach(Transform chip in chips)
        {
            if (chip != null)
            {
                Destroy(chip.gameObject);
            }
        }
        foreach(GameObject stack in Stacks)
        {
            stack.SetActive(false);
        }
        chips.Clear();
        curIndex = 0;
    }
    public void PlayBet(Transform obj,Vector3 target, Action callback)
    {
        chips.Add(obj);
        obj.DOMove(target, 0.5f).SetEase(Ease.Linear).OnComplete(() => {
            callback.Invoke();
        });
    }
    public void ConcentrateChips()
    {
        for (int i = curIndex; i < chips.Count; i++)
        {
            int index = i / 5;
            var item = chips[i];
            item.gameObject.SetActive(true);
            item.DOMove(PotPos.position, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
            {
                item.SetParent(Stacks[index].transform);
                Stacks[index].SetActive(false);
                Stacks[index].SetActive(true);
            });
        }
        curIndex = chips.Count;
    }
    public void Result(int winnerCount, int index, Vector3 winnerSeatPos)
    {
        int flyChipsCount = chips.Count / winnerCount;
        int startChipIndex = index * flyChipsCount;
        int endChipIndex = (index + 1) * flyChipsCount;
        for (int i = startChipIndex; i < endChipIndex; i++)
        {
            var obj = chips[i];
            obj.DOMove(winnerSeatPos, 0.5f).SetDelay(0.1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                Destroy(obj.gameObject);
            });
        }
    }
}
