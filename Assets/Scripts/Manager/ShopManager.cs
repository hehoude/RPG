using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoSingleton<ShopManager>
{
    //卡槽
    public GameObject[] CardBlock;
    //卡牌预制体
    public GameObject ShopCard_Prefab;
    //数据管理器
    private PlayerData PlayerData;
    private CardStore CardStore;
    //价格文本
    public Text[] Price;
    //所有可以在商店中出现的卡牌（数组）
    private List<int> White_Cards;
    private List<int> Blue_Cards;
    private List<int> Gold_Cards;
    //待选的卡牌ID
    private int[] Card_id;
    //购买卡牌所需金币
    private int[] SpendCoin;
    //商店暂停（查看卡组时开启，防止误触按钮）
    public bool stop = false;

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

        //初始化数组
        Card_id = new int[5];
        SpendCoin = new int[5];
    }

    void Start()
    {
        for(int i = 0; i < 5; i++)
        {
            int ran = GetWeightedRandom();
            Card_id[i] = RandomCard(ran);//随机挑选卡牌的ID
            CreateCard(Card_id[i], CardBlock[i], i);//展示到槽内
            SpendCoin[i] = RandomPrice(ran);//随机价格
            Price[i].text = SpendCoin[i].ToString();//展示价格
        }
    }

    //带权重的随机数（随机卡牌的稀有度）
    public int GetWeightedRandom()
    {
        // 1. 定义权重数组（顺序对应选项A、B、C）
        int[] weights = { 4, 2, 1 };
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
        }
        else if (level == 2)
        {
            n = Random.Range(0, Blue_Cards.Count);
            id = Blue_Cards[n];
        }
        else if (level == 3)
        {
            n = Random.Range(0, Gold_Cards.Count);
            id = Gold_Cards[n];
        }
        return id;
    }

    public int RandomPrice(int level)
    {
        int price = 0;
        if (level == 1)
        {
            price = Random.Range(40, 61);
        }
        else if (level == 2)
        {
            price = Random.Range(75, 96);
        }
        else if (level == 3)
        {
            price = Random.Range(110, 141);
        }
        return price;
    }

    //根据ID创建卡牌
    public void CreateCard(int _id, GameObject CardBlock, int code)
    {
        //找到容器生成自己的复制品
        GameObject newCard = Instantiate(ShopCard_Prefab, CardBlock.transform);
        //给复制品赋予card类实例
        newCard.GetComponent<CardDisplay>().card = CardStore.CopyCard(_id);
        //赋予选项编号
        newCard.GetComponent<ShopCard>().code = code;
        //卡牌与卡槽关联
        CardBlock.GetComponent<Block>().obj = newCard;
    }

    //当卡牌被选择时（由卡牌被点击后触发）
    public void Choose(int _c)
    {
        //检测玩家是否有足够的金币
        if (Global_PlayerData.coins >= SpendCoin[_c] && !stop)
        {
            //消耗金币
            Global_PlayerData.coins -= SpendCoin[_c];
            //复制卡牌
            Card cardObject = CardStore.CopyCard(Card_id[_c]);
            //加入玩家卡组
            PlayerData.PlayerCardList.Add(cardObject);
            //下架卡牌
            CardBlock[_c].GetComponent<Block>().obj.SetActive(false);
        }
    }

    public void FinishUp()
    {
        if (!stop)
        {
            //保存数据
            PlayerData.SavePlayerData();
            //通知地图管理器删除当前
            if (MapManager.Instance != null)
            {
                //如果是主城就不删了，游戏地图上才删除
                MapManager.Instance.DeleteCurrentObject(4);//4代表商店
            }
            //返回主城
            Exit();
        }
    }

    //退出按钮
    public void Exit()
    {
        if (!stop)
        {
            SceneChanger.Instance.GetMajorCity();
        }
    }

    //暂停与解除暂停（供外部按钮调用）
    public void SetStop(bool e)
    {
        stop = e;
    }


}
