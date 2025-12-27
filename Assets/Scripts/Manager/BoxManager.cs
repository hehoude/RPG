using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoxManager : MonoSingleton<BoxManager>
{
    //卡槽
    public GameObject CardBlock1;
    public GameObject CardBlock2;
    public GameObject CardBlock3;
    //卡牌预制体
    public GameObject ChooseCard_Prefab;
    //数据管理器
    //public GameObject DataManager;
    private PlayerData PlayerData;
    private CardStore CardStore;
    //所有可以在宝箱中出现的卡牌（数组）
    private List<int> White_Cards;
    private List<int> Blue_Cards;
    private List<int> Gold_Cards;
    //待选的三张卡牌ID
    private int Card1_id;
    private int Card2_id;
    private int Card3_id;

    private Global_PlayerData Global_PlayerData;

    protected override void Awake()
    {
        base.Awake();
        PlayerData = PlayerData.Instance;
        CardStore = CardStore.Instance;
        Global_PlayerData = Global_PlayerData.Instance;
        this.White_Cards = CardStore.White_Cards;
        this.Blue_Cards = CardStore.Blue_Cards;
        this.Gold_Cards = CardStore.Gold_Cards;
    }

    void Start()
    {
        //随机挑选三张卡牌的ID
        Card1_id = RandomCard(GetWeightedRandom());
        Card2_id = RandomCard(GetWeightedRandom());
        Card3_id = RandomCard(GetWeightedRandom());
        //展示到槽内
        CreateCard(Card1_id, CardBlock1, 0);
        CreateCard(Card2_id, CardBlock2, 1);
        CreateCard(Card3_id, CardBlock3, 2);
    }

    //带权重的随机数（随机卡牌的稀有度）
    public int GetWeightedRandom()
    {
        // 1. 定义权重数组（顺序对应选项A、B、C）
        int[] weights = { 9, 3, 1 };
        // 2. 计算总权重
        int totalWeight = 0;
        foreach (int w in weights) totalWeight += w;
        // 3. 生成0~总权重的随机数（左闭右开，所以用totalWeight）
        int randomValue = Random.Range(0, totalWeight);

        // 4. 遍历权重，找随机数落在的区间
        int currentSum = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            currentSum += weights[i];
            if (randomValue < currentSum)
            {
                return i + 1; // 找到对应索引，最终返回1-3的值
            }
        }
        return 0;
    }

    //根据稀有度随机卡牌
    public int RandomCard(int level)
    {
        int n = 0;
        int id = 0;
        if (level == 1)
        {
            n = Random.Range(0, White_Cards.Count);
            id = White_Cards[n];
            White_Cards.RemoveAt(n);//删除成员以避免重复
        }
        else if (level == 2)
        {
            n = Random.Range(0, Blue_Cards.Count);
            id = Blue_Cards[n];
            Blue_Cards.RemoveAt(n);
        }
        else if (level == 3)
        {
            n = Random.Range(0, Gold_Cards.Count);
            id = Gold_Cards[n];
            Gold_Cards.RemoveAt(n);
        }
        return id;
    }

    //根据ID创建卡牌
    public void CreateCard(int _id, GameObject CardBlock, int code)
    {
        //找到容器生成自己的复制品
        GameObject newCard = Instantiate(ChooseCard_Prefab, CardBlock.transform);
        //给复制品赋予card类实例
        newCard.GetComponent<CardDisplay>().card = CardStore.CopyCard(_id);
        //赋予选项编号
        newCard.GetComponent<ChooseCard>().code = code;
        //卡牌与卡槽关联
        CardBlock.GetComponent<Block>().obj = newCard;
    }

    //当卡牌被选择时（由卡牌被点击后触发）
    public void Choose(int _c)
    {
        int _id = 0;
        if (_c == 0)
        {
            _id = Card1_id;
        }
        else if (_c == 1)
        {
            _id = Card2_id;
        }
        else if (_c == 2)
        {
            _id = Card3_id;
        }
        else { Debug.Log("卡牌选择错误"); }
        //复制卡牌
        Card cardObject = CardStore.CopyCard(_id);
        //加入玩家卡组
        PlayerData.PlayerCardList.Add(cardObject);
        //结束选牌
        FinishUp();
    }

    //跳过并获得金币
    public void GetCoin()
    {
        Global_PlayerData.coins += 20;
        FinishUp();
    }

    public void FinishUp()
    {
        //保存数据
        PlayerData.SavePlayerData();
        //通知地图管理器删除当前
        if (MapManager.Instance != null)
        {
            //如果是主城就不删了，游戏地图上才删除
            MapManager.Instance.DeleteCurrentObject(3);//3代表宝箱
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
