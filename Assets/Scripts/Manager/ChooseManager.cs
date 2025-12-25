using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ChooseManager : MonoBehaviour
{
    //获取三个选择槽
    //public GameObject Choose1;
    //public GameObject Choose2;
    //public GameObject Choose3;
    //获取三个图片
    public GameObject BlockImage1;
    public GameObject BlockImage2;
    public GameObject BlockImage3;
    //获取头文本
    public Text HeadText;
    //获取三个介绍文本
    public Text BlockText1;
    public Text BlockText2;
    public Text BlockText3;
    [Header("管理器")]
    //public GameObject DataManager;//从数据管理器获取玩家数据
    private PlayerData PlayerData;
    private CardStore CardStore;
    private Global_PlayerData Global_PlayerData;

    void Awake()
    {
        PlayerData = PlayerData.Instance;
        CardStore = CardStore.Instance;
        Global_PlayerData = Global_PlayerData.Instance;
    }
    void Start()
    {
        LoadStartList();//测试用，后续删掉
    }

    void Update()
    {
        
    }

    //加载初始卡组选择
    public void LoadStartList()
    {
        //重步兵图片路径
        string imagePath_Armour = Application.dataPath + "/Image/RoleCard/Armour.png";
        //刺客图片路径
        string imagePath_Assassin = Application.dataPath + "/Image/RoleCard/Assassin.png";
        //元素使图片路径
        string imagePath_Element = Application.dataPath + "/Image/RoleCard/Element.png";
        //修改图片
        ChooseImage(BlockImage1, imagePath_Armour);
        ChooseImage(BlockImage2, imagePath_Assassin);
        ChooseImage(BlockImage3, imagePath_Element);
        //准备头文本
        string text_head = "选择你的职业";
        //准备介绍文本
        string text_Armour = "重步兵\n拥有坚固的防守与一把强力的重锤，从数值上压制对手。";
        string text_Assassin = "刺客\n通过快速过牌积蓄自身力量，打出多段爆发伤害击杀目标。";
        string text_Element = "元素使\n掌控各种元素的力量，通过卡牌之间的配合打出各种效果。";

        HeadText.text = text_head;
        BlockText1.text = text_Armour;
        BlockText2.text = text_Assassin;
        BlockText3.text = text_Element;
    }

    //替换图片
    public void ChooseImage(GameObject blockImage, string imagePath)
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

    //选择函数
    public void OnChoose(int choose)
    {
        if (true)
        {
            OnChooseCardList(choose);//加入卡组
            PlayerData.ComboList.Add(choose - 1);//加入连携
        }
    }

    //选择卡组
    public void OnChooseCardList(int choose)
    {
        int[] cardList;
        if (choose == 1)//重步兵
        {
            cardList = new int[] { 0, 0, 0, 0, 1, 1, 1, 1, 2 };
            Global_PlayerData.id = 1;
        }
        else if (choose == 2)//刺客
        {
            cardList = new int[] { 3, 3, 3, 3, 4, 4, 4, 4, 5, 6 };
            Global_PlayerData.id = 2;
        }
        else if (choose == 3)//元素使
        {
            cardList = new int[] { 18, 18, 18, 18, 19, 19, 19, 19, 20, 21 };
            Global_PlayerData.id = 3;
        }
        else
        {
            cardList = new int[] { };//空卡组
            Debug.Log("无效选择，未选择任何角色。");
        }
        PlayerData.AddPlayerCardList(cardList);

        FinishChoose();//完成并离开
    }

    //完成选择并离开
    public void FinishChoose()
    {
        //保存数据
        PlayerData.SavePlayerData();
        //通知地图管理器删除选择
        if (MapManager.Instance != null)
        {
            //如果是主城就不删了，游戏地图上才删除
            MapManager.Instance.DeleteCurrentObject(5);//5代表选择
            //推进游戏进程（这里已经包含了保存数据）
            MapManager.Instance.ProgressGame();
        }
        //返回主城
        Exit();
    }

    //退出按钮
    public void Exit()
    {
        SceneChanger.Instance.GetMajorCity();
    }


}
