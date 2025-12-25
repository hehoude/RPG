using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopCard : MonoBehaviour, IPointerDownHandler
{
    public int code = 0;
    //在点击卡牌时
    public void OnPointerDown(PointerEventData eventData)
    {
        //告诉选择管理器选项编号
        ShopManager.Instance.Choose(code);
        Debug.Log("选择完成，Code："+ code);
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
