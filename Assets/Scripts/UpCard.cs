using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpCard : MonoBehaviour, IPointerDownHandler
{
    private Card card;
    
    void Start()
    {
        //找到CardDisplay获取card类实例
        card = gameObject.GetComponent<CardDisplay>().card;
    }
    //在点击卡牌时
    public void OnPointerDown(PointerEventData eventData)
    {
        //判断此卡牌有没有升级
        if (!card.upgrade)
        {
            //找到升级管理器，生成此id的卡牌
            GameObject.Find("UpManager").GetComponent<UpManager>().CreateCard(card.id);
        }
    }
}
