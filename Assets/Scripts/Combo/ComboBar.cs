using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ComboBar : MonoBehaviour
{
    public GameObject[] ComboStone;//指示器
    private ComboStone[] ComboStoneList;//指示器脚本
    public int ComboType;//连携种类（需要创建连携条时赋值进来）
    public List<int> comboList;//连携数组
    private int ComboSch;//连携进度（目前仅支持每回合一次连携，后续需要再修改）
    public GameObject Image;//连携图片对象

    void Awake()
    {
        ComboSch = 0;
        //初始化指示器脚本数组
        ComboStoneList = new ComboStone[3];
        //获取三个指示器的脚本
        for (int i = 0; i < 3; i++)
        {
            ComboStoneList[i] = ComboStone[i].GetComponent<ComboStone>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //加载图片
        LoadImage(ComboType);
        //加载连携数组
        comboList = new List<int>(ComboList.GetComboArray(ComboType));
        //加载介绍文本
        Image.GetComponent<ComboMessage>().TipText = ComboList.GetText(ComboType);
        Image.GetComponent<ComboMessage>().LoadTip();
        //根据连携数组设置三颗指示宝石的颜色
        //获取三个指示器的脚本
        for (int i = 0; i < comboList.Count; i++)
        {
            switch (comboList[i])
            {
                case 1://火
                    ComboStoneList[i].Stone.color = Color.red;
                    break;
                case 2://毒
                    ComboStoneList[i].Stone.color = Color.green;
                    break;
                case 3://电
                    ComboStoneList[i].Stone.color = Color.blue;
                    break;
                default:
                    ComboStoneList[i].Stone.color = Color.white;
                    break;
            }
        }
        Event.SendCard += SendCard1;//订阅出牌事件
        Event.TurnEnd += Clear;//订阅回合结束事件
    }

    //当收到出牌事件时
    public void SendCard1(int type)
    {
        //Debug.Log("收到信号："+ type);
        //如果连携没有走完
        if (ComboSch < 3)
        {
            //则比对是否符合连携数组
            if (type == comboList[ComboSch] || type == 10)
            {
                if (ComboStoneList[ComboSch] != null)
                {
                    //推进连携进度
                    ComboStoneList[ComboSch].Fire.SetActive(true);
                    ComboSch++;
                } 
            }
            else
            {
                Clear();//比对失败则清空连携进度
                //清空后再尝试对比第一个
                if (type == comboList[ComboSch])
                {
                    if (ComboStoneList[ComboSch] != null)
                    {
                        //推进连携进度
                        ComboStoneList[ComboSch].Fire.SetActive(true);
                        ComboSch++;
                    }
                }
            }
        }
    }

    //回合结束清除连携指示器
    public void Clear()
    {
        for(int i = 0;i < 3;i++)
        {
            if (ComboStoneList[i] != null)
            {
                ComboStoneList[i].Fire.SetActive(false);
            }
        }
        ComboSch = 0;
    }

    //图片加载函数
    public void LoadImage(int _id)
    {
        string imageEnemy = Application.dataPath + "/Image/Combo/" + _id.ToString() + ".png";
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


}
