using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ComboMessage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //介绍弹窗
    public GameObject TipWindow;
    //文本内容（创建时需要赋予）
    public string TipText = "";
    //弹窗文本对象
    public Text Tip;
    void Start()
    {
        //LoadTip();//加载弹窗文本（这个由ComboBar赋值后触发）
        TipWindow.SetActive(false);//关闭提示弹窗
    }
    //这里的函数名必须这个才能正常接入鼠标事件
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        TipWindow.SetActive(true);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        TipWindow.SetActive(false);
    }

    //加载弹窗文本（由ComboBar赋值后调用）
    public void LoadTip()
    {
        Tip.text = TipText;//设置文本
    }
}
