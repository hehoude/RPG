using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeleteCard : MonoBehaviour, IPointerDownHandler
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
        //找到删卡管理器，生成此id的卡牌
        GameObject.Find("DeleteManager").GetComponent<DeleteManager>().CreateCard(card);
    }
}
