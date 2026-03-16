using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityEngine;
using UnityEngine.UI;

public class TeammateState : MonoBehaviour
{
    [Header("基本属性")]
    public int id;
    [Header("UI")]
    public Text nameText;//名字
    private string matename;
    public GameObject Image;//图片对象
    [Header("意图")]
    public Text intent;//意图文本
    public int attack;//攻击意图
    public int allattack;//群体攻击意图
    public int defense;//防御意图
    public int build;//强化意图
    public int negative;//负面意图
    public int special1;//特殊意图1
    public int special2;//特殊意图2
    public int special3;//特殊意图3
    //意图表：type为种类、count为数值
    private List<int> Tent_type;
    private int runningIntent = 0;//当前意图序列
    [Header("其它")]
    public PlayerState playerState;//玩家类，由战斗管理器赋值
    public BattleManager battleManager;
    //使用TextAsset方法读取文本，将目标文本拖到插槽即可
    public TextAsset mateList;

    //由战斗管理器调用的替代初始化函数
    public void MateStart()
    {
        LoadImage(id);//加载图片
        LoadMateMessage(id);//加载信息
        nameText.text = matename;//显示名字
        RefreshIntent();//显示意图
        battleManager = BattleManager.Instance;
    }

    //图片加载函数
    public void LoadImage(int _id)
    {
        string imageEnemy = Application.dataPath + "/Image/Mate/" + _id.ToString() + ".png";
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

    //加载指定ID的队友信息
    public void LoadMateMessage(int enemyID)
    {
        Tent_type = new List<int>();
        string[] datarow = mateList.text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var row in datarow)//遍历元素
        {
            string[] rowArray = row.Split(',');//再创建字符串数组，指定逗号为分隔符
            if (rowArray[1] == enemyID.ToString())//如果找到符合ID的行
            {
                //读取表格内容赋值
                matename = rowArray[2];
                for (int i = 0;i<4;i++)
                {
                    Tent_type.Add(int.Parse(rowArray[2*i+3]));
                    Tent_type.Add(int.Parse(rowArray[2*i+4]));
                }
                return;
            }
        }
    }

    //滚动意图（可被外部调用）
    public void EnterTent()
    {
        runningIntent++;
        if (runningIntent>3 || 0 == Tent_type[runningIntent * 2])
        {
            runningIntent = 0;
        }
    }

    //更新意图文本
    public void RefreshIntent()
    {
        string _intent;
        int _current = Tent_type[runningIntent*2];
        int _count = Tent_type[runningIntent * 2+1];
        switch (_current)
        {
            case 1://攻击意图
                _intent = "攻击：" + (_count);
                break;
            case 2://群体攻击意图
                _intent = "群体攻击：" + (_count);
                break;
            case 3://格挡意图
                _intent = "防御：" + _count;
                break;
            case 4://特殊意图1
                _intent = MateText1(id);
                break;
            case 5://特殊意图2
                _intent = MateText2(id);
                break;
            case 6://特殊意图3
                _intent = MateText3(id);
                break;
            default:
                _intent = "未知意图";
                break;
        }
        intent.text = _intent;//显示层更改意图
    }

    //执行意图
    public void Action()
    {
        int _current = Tent_type[runningIntent * 2];
        int _count = Tent_type[runningIntent * 2 + 1];
        //数据层执行意图
        switch (_current)
        {
            case 1://攻击意图
                battleManager.MateAttack(_count, false);
                break;
            case 2://群体攻击意图
                battleManager.MateAttack(_count, true);
                break;
            case 3://格挡意图
                playerState.GetArmor(_count);
                break;
            case 4://特殊意图1
                Special1();
                break;
            case 5://特殊意图2
                //还没写---------
                break;
            case 6://特殊意图3
                //还没写---------
                break;
            default:

                break;
        }
        EnterTent();//滚动到下一个意图
        RefreshIntent();//更新意图文本
    }

    //特殊意图1文本
    public String MateText1(int _id)//接受敌人ID与敌人意图代号
    {
        String _Text = String.Empty;
        switch (_id)
        {
            case 1://巫毒猎手
                _Text = "抽一张牌";
                break;
            default:

                break;
        }
        return _Text;
    }
    //特殊意图2文本
    public String MateText2(int _id)//接受敌人ID与敌人意图代号
    {
        String _Text = String.Empty;
        switch (_id)
        {
            default:

                break;
        }
        return _Text;
    }
    //特殊意图3文本
    public String MateText3(int _id)//接受敌人ID与敌人意图代号
    {
        String _Text = String.Empty;
        switch (_id)
        {
            default:

                break;
        }
        return _Text;
    }

    //特殊意图1
    public void Special1()
    {
        switch (id)
        {
            case 1://巫毒猎手
                battleManager.DrawCard(1);//给玩家抽一张牌
                break;
            default:

                break;
        }
    }

}
