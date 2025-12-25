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
        string imageEnemy = Application.dataPath + "/Image/State/" + _id.ToString() + ".png";
        ChangeImage(Image, imageEnemy);
    }

    //替换图片
    public void ChangeImage(GameObject blockImage, string imagePath)
    {
        Image imageComponent = blockImage.GetComponent<Image>();
        if (imageComponent == null)
        {
            Debug.LogError("blockImage上未挂载Image组件！");
            return;
        }
        // 检查文件是否存在
        if (!File.Exists(imagePath))
        {
            Debug.LogError($"图片文件不存在：{imagePath}");
            return;
        }
        byte[] imageBytes = File.ReadAllBytes(imagePath);// 读取图片文件字节数据
        // 创建纹理并加载图片数据
        Texture2D texture = new Texture2D(2, 2); // 初始尺寸任意，LoadImage会自动调整
        if (texture.LoadImage(imageBytes))
        {
            // 将纹理转换为精灵
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height), // 精灵矩形区域（整个纹理）
                new Vector2(0.5f, 0.5f) // 精灵中心点（中心位置）
            );

            // 设置Image组件的精灵
            imageComponent.sprite = sprite;
        }
        else
        {
            Debug.LogError($"加载图片数据失败：{imagePath}");
            Destroy(texture); // 释放无效纹理
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
        }
        Tip.text = _text;//设置文本
    }

}
