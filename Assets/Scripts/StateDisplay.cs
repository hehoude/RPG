using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;

public class StateDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //状态种类（创建时需要赋予）
    public int id;
    //数值
    public Text count;
    //介绍弹窗
    public GameObject TipWindow;
    //图片对象
    public GameObject Image;
    //弹窗文本
    public Text Tip;

    void Start()
    {
        LoadImage(id);//加载图片
        LoadTip();//加载弹窗文本
        TipWindow.SetActive(false);//关闭提示弹窗
    }

    //更新数值（由其它脚本调用）
    public void FreshCount(int _count)
    {
        count.text = _count.ToString();
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

    //图片加载函数
    public void LoadImage(int _id)
    {
        // 直接从单例缓存取精灵（无任何文件IO，耗时<0.1ms）
        Sprite sprite = StateImageLoad.Instance.GetStateSprite(_id);
        if (sprite == null)
        {
            Debug.LogWarning($"状态{_id}的图片缓存不存在");
            return;
        }

        // 直接赋值精灵（无需再调用ChangeImage）
        Image imageComponent = Image.GetComponent<Image>();
        if (imageComponent != null)
        {
            imageComponent.sprite = sprite;
        }
        else
        {
            Debug.LogError("Image对象上无Image组件！");
        }
    }

    //加载弹窗文本
    public void LoadTip()
    {
        string _text = "";
        switch (id)
        {
            case 0://力量
                _text = "力量\n每次攻击增加相应层数的伤害";
                break;
            case 1://燃烧
                _text = "燃烧\n回合结束受到层数一半的伤害，减少相应层数";
                break;
            case 2://中毒
                _text = "中毒\n回合开始受到层数相同的伤害，减少3层数";
                break;
            case 3://雷电
                _text = "雷电\n每次受到伤害额外增加等同层数的伤害并减少1层，如果层数大于5层则额外减少1层";
                break;
            case 4://坚固
                _text = "坚固\n每次获得格挡时额外获得相应层数的格挡";
                break;
            case 5://火焰附加
                _text = "火焰附加\n每次攻击会额外施加相应层数的燃烧";
                break;
            case 6://弹反
                _text = "弹反\n承受敌人攻击后，如果生命没有减少且格挡正好为0，对攻击者造成等同本次攻击的伤害";
                break;
            case 7://免疫
                _text = "免疫\n受到伤害时，减少等同层数的伤害";
                break;
            case 8://防护
                _text = "防护\n每层可以无视一次敌人攻击";
                break;
            case 9://虚弱
                _text = "虚弱\n减少25%的攻击伤害";
                break;
        }
        Tip.text = _text;//设置文本
    }

}
