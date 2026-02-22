using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

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
    //连携条预制体
    public GameObject ComboBar_Prefab;
    //获取三个连携条
    public GameObject ComboBar1;
    public GameObject ComboBar2;
    public GameObject ComboBar3;
    //随机三个队友ID
    public int[] Mate_id;
    //标志位：选择玩家角色还是队友
    private bool role;
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
        Mate_id = new int[3];
    }
    void Start()
    {
        //根据当前层数判断
        if (Global_PlayerData.Instance.floor == 0)
        {
            LoadStartList();//选择角色
            role = true;
        }
        else
        {
            RandomMateId();//随机队友ID
            ShowImages();//展示图片
            ShowText();//展示介绍文本
            MateComboBar();//展示连携条
            role = false;
        }
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
        //创建连携条
        CreateComboBar(0, ComboBar1);
        CreateComboBar(1, ComboBar2);
        CreateComboBar(2, ComboBar3);
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

    //选择函数（由外部按钮调用）
    public void OnChoose(int choose)
    {
        if (role)
        {
            OnChooseCardList(choose);//加入卡组
            PlayerData.ComboList.Add(choose - 1);//加入连携
        }
        else
        {
            int mateId = Mate_id[choose - 1];
            PlayerData.MateList.Add(mateId);//加入队友
            PlayerData.ComboList.Add(mateId + 10);//加入连携（队友连携为ID后推10位）
            AddCardList(mateId);//加入卡组
        }
        FinishChoose();//完成并离开
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
        SceneChanger.Instance.GetMajorCity(7);
    }

    //随机三个队友ID（后续还需要修改，不能和已有的队友相同ID）
    public void RandomMateId()
    {
        Mate_id[0] = Random.Range(0, CardStore.MateLists.Count);//随机队友1ID
        Mate_id[1] = Random.Range(0, CardStore.MateLists.Count);//随机队友2ID
        while (Mate_id[0] == Mate_id[1])//防止ID一致
        {
            Mate_id[1] = Random.Range(0, CardStore.MateLists.Count);
        }
        Mate_id[2] = Random.Range(0, CardStore.MateLists.Count);//随机队友3ID
        while (Mate_id[0] == Mate_id[2] || Mate_id[1] == Mate_id[2])//防止ID一致
        {
            Mate_id[2] = Random.Range(0, CardStore.MateLists.Count);
        }
    }

    //显示队友图片
    public void ShowImages()
    {
        string imagePath1 = Application.dataPath + "/Image/Mate/" + Mate_id[0].ToString() + ".png";
        string imagePath2 = Application.dataPath + "/Image/Mate/" + Mate_id[1].ToString() + ".png";
        string imagePath3 = Application.dataPath + "/Image/Mate/" + Mate_id[2].ToString() + ".png";
        //修改图片
        ChooseImage(BlockImage1, imagePath1);
        ChooseImage(BlockImage2, imagePath2);
        ChooseImage(BlockImage3, imagePath3);
    }

    //显示介绍文本
    public void ShowText()
    {
        //准备头文本
        string text_head = "选择你的队友";
        //准备介绍文本
        string text1 = GetText(Mate_id[0]);
        string text2 = GetText(Mate_id[1]);
        string text3 = GetText(Mate_id[2]);

        HeadText.text = text_head;
        BlockText1.text = text1;
        BlockText2.text = text2;
        BlockText3.text = text3;
    }

    //显示队友连携条
    public void MateComboBar()
    {
        //创建连携条（队友连携条为ID后推10位）
        CreateComboBar(Mate_id[0]+10, ComboBar1);
        CreateComboBar(Mate_id[1]+10, ComboBar2);
        CreateComboBar(Mate_id[2]+10, ComboBar3);
        
    }

    //根据队友ID返回介绍文本
    public string GetText(int _id)
    {
        string text = "";
        switch(_id)
        {
            case 0:
                text = "火龙战士\n用强大的火焰焚烧一切！";
                break;
            case 1:
                text = "巫毒猎手\n擅长用毒药耗尽对手的生命。";
                break;
            case 2:
                text = "雷电守卫\n使用电的力量辅助作战。";
                break;
        }
        return text;
    }

    //将CardStore的队友卡组加入到PlayerData中
    public void AddCardList(int _id)
    {
        List<Card> MateCardList;
        //根据队友数量判断要塞进哪张卡组里
        if (PlayerData.MateList.Count == 1)
        {
            MateCardList = PlayerData.MateCardList0;
        }
        else if (PlayerData.MateList.Count == 2)
        {
            MateCardList = PlayerData.MateCardList1;
        }
        else if (PlayerData.MateList.Count == 3)
        {
            MateCardList = PlayerData.MateCardList2;
        }
        else if (PlayerData.MateList.Count == 4)
        {
            MateCardList = PlayerData.MateCardList3;
        }
        else
        {
            Debug.LogWarning("队友卡组配置错误!");
            return;
        }
        //根据id在CardStore中获取相应卡组
        foreach (int card_id in CardStore.MateLists[_id])
        {
            //根据ID复制Card实例，加入到卡组中
            MateCardList.Add(CardStore.CopyCard(card_id));
        }
    }

    //创建连携条
    public void CreateComboBar(int com, GameObject ComboPlace)
    {
        //在显示层创建连携可视化指示条
        GameObject _ComboBar = Instantiate(ComboBar_Prefab, ComboPlace.transform);
        //赋予连携序号
        _ComboBar.GetComponent<ComboBar>().ComboType = com;
    }

}
