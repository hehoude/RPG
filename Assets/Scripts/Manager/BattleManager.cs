using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;
using static UnityEngine.EventSystems.EventTrigger;

//各个阶段
public enum GamePhase
{
    gameStart, playerStart, playerWait, playerAction, playerOver, enemyStart, enemyAction
}

public class BattleManager : MonoSingleton<BattleManager>
{
    
    [Header("战场UI")]
    public GameObject cardPrefab;//战斗卡牌预制体
    public Text EnergyText;//能量显示文本
    public Transform playerHand;//手牌区
    public Text DrawText;//抽牌堆数量
    public Text DisText;//弃牌堆数量
    public Text ConText;//消耗牌堆数量
    public GameObject player;//玩家游戏对象（实体）
    private PlayerState playerState;//玩家状态类
    public GameObject enemyPrefab;//敌人游戏对象（预制体）
    public GameObject[] enemyBlocks;//敌人区域
    public GameObject ChooseCardBlock;//选牌显示窗
    public GameObject ComboPlace;//连携显示区域
    public GameObject ComboBar_Prefab;//连携指示条（预制体）
    [Header("管理器")]
    //public GameObject DataManager;//从数据管理器获取玩家数据
    private PlayerData PlayerData;
    private CardStore CardStore;
    public GameObject BattleReader;//敌人ID读取器
    private BattleReader battleReader;
    public GameObject EnemyManager;//从敌人管理器获取敌人数据
    private EnemyData EnemyData;
    public GridLayoutGroup HandGridLayout;//手牌布局管理器
    private Global_PlayerData Global_PlayerData;
    [Header("临时生成物")]
    private GameObject runningCard;//运行中的卡牌存这里
    public GameObject targetArrow;//存放目标箭头预制体
    private GameObject arrow;//存放场地中实例化的箭头
    public Transform Canvas;//传入场地（放置箭头时使用）
    public Transform AttackLight;//攻击特效创建处
    public GameObject AttackLightPrefab;//攻击特效预制体
    [Header("全局变量")]
    public int energy = 3;//玩家剩余能量 
    public bool execute = false;//卡牌执行标志位
    public List<Card> DrawCardList;//抽牌堆（直接复用PlayerData里的卡组）
    public List<Card> DisCardList = new List<Card>();//弃牌堆
    public List<Card> ConsumeList = new List<Card>();//消耗牌堆
    private List<int> playedCardNums = new List<int>();//连携记牌器
    private List<ComboRule> comboRules = new List<ComboRule>();// 连携规则数组
    [Header("其它")]
    public CardEffect cardEffect;
    public List<GameObject> activeEnemies;//可用敌人容器（用于遍历与随机）
    public bool Wait_TurnEnd = false;//即将结束回合
    public GamePhase GamePhase = GamePhase.gameStart;//战斗阶段枚举
    public UnityEvent phaseChangeEvent = new UnityEvent();//创建阶段切换事件
    public int HandCount = 0;//手牌数量（目前只有三种变化途径：抽牌、弃牌、打牌，后续可能有创生牌）
    public int RunCount = 0;//回合计数器
    public GameObject TargetCard;//被选择的卡牌（作为目标）
    private bool ChooseCardMode;//选牌模式
    // 自定义连携规则类（存储“目标组合”和“对应效果”）
    public class ComboRule
    {
        public int[] targetCombo; // 目标组合
        public Action<object> effect;     // 触发的效果
        public int maxUse;
        public int Use;
        public ComboRule(int[] combo, Action<object> eff, int maxuse)
        {
            targetCombo = combo;
            effect = eff;//连携达成触发的函数
            maxUse = maxuse;
            Use = maxuse;
        }
    }

    protected override void Awake() // 用 Awake() 初始化，比 OnEnable() 早，避免时机问题
    {
        base.Awake();
        PlayerData = PlayerData.Instance;
        CardStore = CardStore.Instance;
        Global_PlayerData = Global_PlayerData.Instance;
        EnemyData = EnemyManager.GetComponent<EnemyData>();
        battleReader = BattleReader.GetComponent<BattleReader>();
        playerState = player.GetComponent<PlayerState>();
        cardEffect = gameObject.GetComponent<CardEffect>();
    }

    // Start is called before the first frame updatedai'h
    void Start()
    {
        Debug.Log("所有信息加载完成，进入准备阶段");
        GameStart(); //执行一次游戏初始化
    }

    // Update is called once per frame
    void Update()
    {
        OnPlayerStart1();//玩家开始阶段
        OnEnemyAction();//敌人行动阶段
        //当点击鼠标右键时（0左键、1右键、2中键）
        if (Input.GetMouseButtonDown(1))//加Down只会触发一帧
        {
            if (ChooseCardMode)
            {
                //Debug.Log("退出选牌模式");
                runningCard.GetComponent<BattleCard>().choosed = false;//恢复卡牌可选
                if (ChooseCardBlock.GetComponent<Block>().obj != null)//如果有牌被选择
                {
                    GameObject _Card = ChooseCardBlock.GetComponent<Block>().obj;
                    //恢复被选择的卡牌可选
                    _Card.GetComponent<BattleCard>().choosed = false;//恢复卡牌可选
                    //将被选择的卡牌移动回手牌
                    _Card.transform.SetParent(playerHand.transform, false);
                    //恢复ZoomUI2
                    _Card.GetComponent<ZoomUI2>().enabled = true;
                    _Card.transform.localScale = Vector3.one; // 恢复缩放
                    //_Card.transform.localPosition = Vector3.zero;
                    ChooseCardBlock.GetComponent<Block>().obj = null;
                }
                TargetCard = null;//被选中的卡牌清除
                ChooseCardBlock.SetActive(false);//选牌显示窗隐藏
                ChooseCardMode = false;//关闭选牌模式
                execute = false;//由于选牌模式也会打开执行中信号，所以选牌模式退出时也要关闭
            }
            DestroyArrow();//箭头删除
            runningCard = null;//运行中的卡牌被清除
            EnemyChoose(false);//将所有敌人设为不可选
        }
    }

    //void OnEnable()
    //{
    //    //当脚本PlayerData的DataLoaded事件被触发后，执行OnScriptADataLoaded
    //    PlayerData.PlayerLoaded += OnDataLoaded;
    //}

    //void OnDataLoaded()
    //{
    //    Debug.Log("所有信息加载完成，进入准备阶段");
    //    GameStart(); //执行一次游戏初始化
    //    PlayerData.PlayerLoaded -= OnDataLoaded;//取消事件订阅释放内存
    //}

    //游戏初始化
    public void GameStart()
    {
        //赋予玩家初始状态
        playerState.maxhp = Global_PlayerData.maxhp;
        playerState.hp = Global_PlayerData.hp;
        playerState.life = true;
        playerState.Refresh();//刷新显示层
        //创建敌人
        CreateEnemy(enemyPrefab, enemyBlocks);
        //初始化可用敌人指示容器
        activeEnemies = new List<GameObject>();
        RefreshEnemyList();//刷新容器状态
        //读取卡组
        ReadDeck();
        //卡组洗牌
        ShuffletDeck();
        //更新显示层卡牌数量
        FreshCardCount();
        //手牌数量设置
        HandCount = 0;
        //初始化连携规则（以后需要写复杂一些）
        ComboStart();
        //进入下一阶段
        NextPhase();
    }

    //加载初始抽牌堆
    public void ReadDeck()
    {
        //加载玩家卡组
        DrawCardList = new List<Card>(); // 先初始化新列表
        foreach (var card in PlayerData.PlayerCardList)
        {
            AddDeck(card);
        }
        //将队友卡组插入玩家卡组中
        var allMateCardLists = new[]
        {
            PlayerData.MateCardList0,
            PlayerData.MateCardList1,
            PlayerData.MateCardList2,
            PlayerData.MateCardList3
        };
        int mateCount = PlayerData.MateList.Count;
        for (int i = 0; i < mateCount && i < allMateCardLists.Length; i++)
        {
            // 遍历当前索引的队友卡组，逐个加入玩家卡组
            foreach (var card in allMateCardLists[i])
            {
                AddDeck(card);
            }
        }
    }

    //在抽牌堆中加入卡牌（仅限加载初始数据时使用）
    public void AddDeck(Card _card)
    {
        // 调用Card类的复制方法，创建独立的新Card实例
        Card newCard = CardStore.CopyCard(_card.id);
        if (_card.upgrade) //升级卡牌
        {
            newCard = PlayerData.Upgrade(newCard);
        }
        DrawCardList.Add(newCard);
    }

    //抽牌堆洗牌
    public void ShuffletDeck()
    {
        for (int i = 0; i < DrawCardList.Count; i++)
        { 
            //随机一张牌，将第i张牌与第rad张牌调换
            int rad = UnityEngine.Random.Range(0, DrawCardList.Count);
            Card temp = DrawCardList[i];
            DrawCardList[i] = DrawCardList[rad];
            DrawCardList[rad] = temp;
        }
    }

    //更新显示层卡牌数量
    public void FreshCardCount()
    {
        DrawText.text = DrawCardList.Count.ToString();
        DisText.text = DisCardList.Count.ToString();
        ConText.text = ConsumeList.Count.ToString();
    }

    //创建敌人
    public void CreateEnemy(GameObject _enemy, GameObject[] _blocks)
    {
        for (int i=0;i< battleReader.enemies.Count;i++)
        {
            //获取指定ID的敌人EnemyType实例
            EnemyType enemyType = EnemyData.LoadEnemyMessage(battleReader.enemies[i]);
            //在卡槽中创建敌人游戏对象
            GameObject newEnemy = Instantiate(_enemy, _blocks[i].transform);
            //为游戏对象newEnemy赋予enemyType实例
            newEnemy.GetComponent<EnemyState>().enemyType = enemyType;
            //敌人对象与敌人卡槽关联（千万不要把预制体_enemy塞进去了）
            _blocks[i].GetComponent<Block>().obj = newEnemy;
        }
    }

    //初始化连携
    public void ComboStart()
    {
        foreach (int com in PlayerData.ComboList)
        {
            List<int> comboList;//预设连携数组（用于赋给指示条）
            switch (com)
            {
                case 0://重装战士连携
                    //在comboRules中创建连携规则
                    comboRules.Add(new ComboRule(new int[] { 1, 1, 1 }, _ => TriggerRGBEffect(com), 1));
                    comboList = new List<int>() { 1, 1, 1 };
                    break;
                case 1://刺客连携
                    comboRules.Add(new ComboRule(new int[] { 2, 2, 2 }, _ => TriggerRGBEffect(com), 1));
                    comboList = new List<int>() { 2, 2, 2 };
                    break;
                case 2://元素使连携
                    comboRules.Add(new ComboRule(new int[] { 1, 2, 3 }, _ => TriggerRGBEffect(com), 1));
                    comboList = new List<int>() { 1, 2, 3 };
                    break;
                default:
                    comboList = new List<int>() { 1, 1, 1 };
                    Debug.LogWarning("未知连携：" + com);
                    break;
            }
            //在显示层创建连携可视化指示条
            GameObject _ComboBar = Instantiate(ComboBar_Prefab, ComboPlace.transform);
            //赋予连携数组
            _ComboBar.GetComponent<ComboBar>().ComboList = comboList;
        }
    }

    //玩家开始阶段1（此时阶段为playerStart）
    public void OnPlayerStart1()
    {
        if (GamePhase == GamePhase.playerStart)
        {
            playerState.CleanArmor();//清空玩家格挡
            energy = 3;//重置能量
            RefreshEnergy();//更新能量文本
            //重置连携次数
            foreach (var rule in comboRules)
            {
                rule.Use = rule.maxUse;
            }
            //设定延迟时间
            float delayTime = 0.5f;
            //如果有中毒
            if (playerState.toxin > 0)
            {
                playerState.ToxinAnim();//播放中毒动画（玩家动画自动触发结算）
                delayTime += 1.0f;
            }
            Invoke("OnPlayerStart2", delayTime);
            NextPhase();//下一阶段
        }
    }

    //玩家开始阶段2（此时阶段为playerWait）
    public void OnPlayerStart2()
    {
        int draw = 5;//基础抽牌数
        //判断是不是第一回合
        if (RunCount == 0)
        {
            //遍历抽牌堆
            for(int i = 0;i < DrawCardList.Count;i++)
            {
                //查找固有牌
                if (DrawCardList[i].keep == 2)
                {
                    //在指定位置生成一张卡（游戏对象）
                    GameObject card = Instantiate(cardPrefab, playerHand);
                    //将card实例赋予游戏对象中的卡
                    card.GetComponent<CardDisplay>().card = DrawCardList[i];
                    //移出抽牌堆
                    DrawCardList.RemoveAt(i);
                    //手牌计数+1
                    HandCount += 1;
                    //抽牌数-1
                    draw--;
                }
                //查看手牌是否满了
                if (HandCount == 10) { break; }
            }
        }
        if (draw < 0) { draw = 0; }
        DrawCard(draw);//抽取手牌
        RunCount++;
        NextPhase();//下一阶段
    }

    //敌人行动阶段
    public void OnEnemyAction()
    {
        if (GamePhase == GamePhase.enemyStart)
        {
            //EnemyAction();//调用一次敌人行动函数（这里附带清空格挡）
            StartCoroutine(EnemyAction());//协程函数调用方法
            NextPhase();//下一阶段（避免协程函数在Update()中被重复执行）
        }
    }

    //抽卡
    public void DrawCard(int _count)
    {
        for (int i = 0; i < _count; i++)
        {
            if (HandCount > 10)
            {
                break;//手牌满了
            }
            // 如果卡组为空，触发下一轮洗牌
            if (DrawCardList == null || DrawCardList.Count == 0)
            {
                //确保弃牌堆里有牌
                if (DisCardList == null || DisCardList.Count == 0)
                {
                    break;//全部牌已经抽出
                }
                else
                {
                    //将弃牌堆的牌转移到抽牌堆
                    DrawCardList.AddRange(DisCardList);
                    DisCardList.Clear();
                    ShuffletDeck();
                }
            }
            //在指定位置生成一张卡（游戏对象）
            GameObject card = Instantiate(cardPrefab, playerHand);
            //将卡组顶部的card实例赋予游戏对象中的卡
            card.GetComponent<CardDisplay>().card = DrawCardList[0];
            //将顶部的卡除去
            DrawCardList.RemoveAt(0);
            //手牌计数+1
            HandCount += 1;
            //判断卡牌是否有出场效果
            if (card.GetComponent<CardDisplay>().card.other == 1)
            {
                //执行出场效果
                cardEffect.EnterEffect(card.GetComponent<CardDisplay>().card, playerState);
            }
        }
        FreshCardCount();//最后更新一次显示层
    }

    //回合结束（按钮）
    public void OnClickTurnEnd()
    {
        //只有玩家出牌阶段才能按
        if (GamePhase == GamePhase.playerAction)
        { TurnEnd(); }
    }

    //回合结束
    public void TurnEnd()
    {
        if (ChooseCardMode == false)//选牌模式时不可结束回合
        {
            float delayTime = 0.5f;
            Fold();//弃牌阶段
                   //燃烧结算
            if (playerState.fire > 0)
            {
                playerState.FireAnim();//播放燃烧动画（玩家动画自动触发结算）
                delayTime += 1.0f;
            }
            playedCardNums.Clear();//清空连携记录
            Event.CallTurnEnd();//发送回合结束信号（用于清除显示层连携指示器）
            NextPhase();
            Invoke("NextPhase", delayTime); //NextPhase执行一次会进入playerOver阶段,这里延迟0.5秒后再进入下一阶段enemyStart
        }
    }

    //弃牌阶段
    public void Fold()
    {
        //校对卡牌数量（调试用）
        if (playerHand.childCount != HandCount)
        {
            Debug.Log("手牌计数有误！计数："+ HandCount + "实际："+ playerHand.childCount);
        }
        // 创建子对象临时数组
        Transform[] handCardTransforms = new Transform[playerHand.childCount];
        for (int i = 0; i < playerHand.childCount; i++)
        {
            handCardTransforms[i] = playerHand.GetChild(i);
        }

        // 遍历所有手牌游戏对象
        foreach (var cardTransform in handCardTransforms)
        {
            //获取卡牌上的card实例
            Card _cardObj = cardTransform.GetComponent<CardDisplay>().card;
            //判断是否有回合结束弃置效果
            if (_cardObj.other == 3)
            {
                cardEffect.OverEffect(_cardObj, playerState);//执行回合结束弃置效果
            }
            if (_cardObj.keep == 1)//如果卡牌是虚无
            { ConsumeList.Add(_cardObj); }//放入消耗牌堆
            else if (_cardObj.keep == 2)//如果卡牌是保留
            { continue; }
            else
            { DisCardList.Add(_cardObj); }//放入弃牌堆

            //销毁卡牌游戏对象
            Destroy(cardTransform.gameObject);//这里不会连带销毁card
            HandCount -= 1;//手牌计数-1
        }
    }

    //跳转下一阶段
    public void NextPhase()
    {
        //如果是枚举量的最后一位
        if ((int)GamePhase == System.Enum.GetNames(GamePhase.GetType()).Length - 1)
        {
            GamePhase = GamePhase.playerStart;
        }
        else
        {
            GamePhase += 1;
        }
        phaseChangeEvent.Invoke();//发出回合切换信号
    }

    //出牌请求（由被点击卡牌触发）
    public void AttackRequest(GameObject _Card)
    {
        int _spend = _Card.GetComponent<CardDisplay>().card.spend;
        int _target = _Card.GetComponent<CardDisplay>().card.target;
        //判断费用是否足够，且没有卡牌在执行
        if ((energy >= _spend || _spend == 999) && !execute)
        {
            //将卡牌放入预备区
            runningCard = _Card;
            if (_target == 0)//如果没有目标
            {
                execute = true;//卡牌执行中
                //AttackConfirm(player);无目标时不能将玩家自身作为目标，因为玩家没有enemyState，可能空引用
                AttackConfirm(RandomEnemy());//随机敌人目标
            }
            else if (_target == 1)//如果是单体目标
            {
                //创建目标指示箭头
                CreateArrow(_Card.transform, targetArrow);
                //将所有敌人设为可选
                EnemyChoose(true);
            }
            else if (_target == 2)//如果是全体目标
            {
                execute = true;//卡牌执行中
                Card attackCard = _Card.GetComponent<CardDisplay>().card;//获取Card类
                int RunCount = 1;
                if (attackCard.spend == 999)
                {
                    RunCount = energy;
                    energy = 0;//费用X时直接清空能量
                }
                else
                {
                    energy -= attackCard.spend;//正常扣减能量
                }
                RefreshEnergy();
                //这里跳过了AttackConfirm函数，直接对所有敌人发动效果
                foreach (var block in enemyBlocks)
                {
                    //判断enemyBlocks里面有没有敌人
                    if (block.GetComponent<Block>().obj != null)
                    {
                        for (int i = 0; i < RunCount; i++)
                        {
                            Attack(_Card, block.GetComponent<Block>().obj);//发动卡牌效果
                        }
                    }
                }
                CardOver(_Card);//群体卡全部执行完成后结算
            }
            //类型3的卡牌可以在没目标的时候跳过，类型4不允许
            else if (_target == 3 || (_target == 4 && HandCount>1))//如果选择一张卡牌为目标
            {
                if (HandCount == 1)//判断手牌里是否有其它牌
                {
                    //没有可选牌时的处理方案
                    execute = true;//卡牌执行中
                    TargetCard = null;//确保没有牌被选择
                    AttackConfirm(player);//无目标时将玩家自身作为目标
                }
                else
                {
                    execute = true;//卡牌执行中
                    _Card.GetComponent<BattleCard>().choosed = true;//显示此牌已经被选中（防止重复选择）
                    ChooseCardMode = true;//开启选牌模式
                    ChooseCardBlock.SetActive(true);//选牌显示窗显示
                }
                
            }
        }
        else if (ChooseCardMode)//如果正在进行选牌模式
        {
            if (ChooseCardBlock.GetComponent<Block>().obj != null)//如果已经有牌被选中
            {
                Debug.Log("替换被选的牌");
                GameObject _OldCard = ChooseCardBlock.GetComponent<Block>().obj;//获取已选牌
                //恢复被选择的卡牌可选
                _OldCard.GetComponent<BattleCard>().choosed = false;//恢复卡牌可选
                //移动回手牌
                _OldCard.transform.SetParent(playerHand.transform, false);
                //恢复卡牌的ZoomUI2脚本
                _OldCard.GetComponent<ZoomUI2>().enabled = true;
                _OldCard.transform.localScale = Vector3.one;
                //_OldCard.transform.localPosition = Vector3.zero;
            }
            TargetCard = _Card;//将此牌作为选牌
            
            //禁用卡牌的ZoomUI2脚本
            _Card.GetComponent<ZoomUI2>().enabled = false;
            _Card.transform.localScale = Vector3.one; // 恢复缩放
            //更新选牌的父对象与位置
            _Card.transform.SetParent(ChooseCardBlock.transform, false);
            _Card.transform.localPosition = Vector3.zero;
            ChooseCardBlock.GetComponent<Block>().obj = _Card;//替换为新的牌
            _Card.GetComponent<BattleCard>().choosed = true;//显示此牌已经被选中
            //此时布局管理器可能被封锁，需要解封一下
            HandGridLayout.enabled = true;
        }
    }

    //一次性设置所有敌人可选状态
    public void EnemyChoose(bool en)
    {
        //遍历所有敌人槽
        foreach (var enemy in activeEnemies)
        {
            //敌人对象将其变为可选
            enemy.GetComponent<EnemyState>().choose = en;
            //Debug.Log("变更完成:"+en);
        }
    }

    //随机选择敌人
    public GameObject RandomEnemy()
    {
        if (activeEnemies.Count > 0)
        {
            return activeEnemies[UnityEngine.Random.Range(0, activeEnemies.Count)];
        }
        else
        {
            Debug.LogWarning("异常：无可用敌人!");
            return player;
        }
    }

    //刷新敌人指示容器状态（开始游戏时、敌人死亡时触发）
    public void RefreshEnemyList()
    {
        activeEnemies.Clear();//清除容器
        //遍历所有敌人槽
        foreach (var block in enemyBlocks)
        {
            //判断enemyBlocks1里面有没有敌人，且敌人存活
            if (block.GetComponent<Block>().obj != null && block.GetComponent<Block>().obj.activeSelf)
            {
                activeEnemies.Add(block.GetComponent<Block>().obj);//加入容器
            }
        }
    }

    //带选牌的执行（由外部按钮触发）
    public void ChooseFinish()
    {
        if (ChooseCardBlock.GetComponent<Block>().obj != null && TargetCard != null)
        {
            //恢复卡牌可选
            runningCard.GetComponent<BattleCard>().choosed = false;
            //放回手牌
            GameObject _Card = ChooseCardBlock.GetComponent<Block>().obj;
            _Card.GetComponent<BattleCard>().choosed = false;
            ChooseCardBlock.GetComponent<Block>().obj = null;//解除关联
            ChooseCardMode = false;//关闭选牌模式
            ChooseCardBlock.SetActive(false);//选牌显示窗隐藏

            //剩下按照无目标卡牌执行，被选卡牌的引用已经存储在TargetCard中
            execute = true;//卡牌执行中
            AttackConfirm(player);//无目标时将玩家自身作为目标
        }
    }

    //出牌确认（由被点击目标触发，或由无目标函数调用）
    public void AttackConfirm(GameObject _target)
    {
        execute = true;//卡牌执行中
        DestroyArrow();//删除箭头
        EnemyChoose(false);//将所有敌人设为不可选
        Card attackCard = runningCard.GetComponent<CardDisplay>().card;//获取Card类
        //扣减能量
        int RunCount = 1;
        if (attackCard.spend == 999)
        {
            RunCount = energy;
            energy = 0;//费用X时直接清空能量
        }
        else
        {
            energy -= attackCard.spend;//正常扣减能量
        }
        RefreshEnergy();
        for (int i = 0; i < RunCount; i++)
        {
            Attack(runningCard, _target);//发动卡牌效果
        }
        //卡牌结算
        CardOver(runningCard);
    }

    //创造指示箭头
    public void CreateArrow(Transform _startPoint, GameObject _prefab)
    {
        //删除前面的箭头
        DestroyArrow();
        //实例化，由于卡牌上并没有存放箭头的插槽，只能生成到Canvas里
        arrow = GameObject.Instantiate(_prefab, Canvas);
        //将位置信息传递给箭头
        arrow.GetComponent<Arrow>().SetStartPoint(new Vector2(_startPoint.position.x, _startPoint.position.y));
    }
    
    //删除所有箭头（召唤箭头与攻击箭头）
    public void DestroyArrow()
    {
        Destroy(arrow);
        //Destroy(attackArrow);
    }

    //更新能量文本显示
    public void RefreshEnergy()
    {
        EnergyText.text = energy.ToString();
    }

    //攻击动画
    public void Anim_Attack()
    {
        //生成攻击动画
        Instantiate(AttackLightPrefab, AttackLight);
    }

    //卡牌效果执行
    public void Attack(GameObject _Card, GameObject _target)
    {
        Card attackCard = _Card.GetComponent<CardDisplay>().card;
        //由于玩家只有一个，所有玩家类采用全局变量
        EnemyState enemyState = _target.GetComponent<EnemyState>();//获取敌人状态类
        

        //检测卡牌是否有前置效果
        if (attackCard.front == 1)
        {
            cardEffect.FrontEffect(attackCard, enemyState, playerState);
        }

        //向目标发起攻击
        if (attackCard.attack > 0)
        {
            Anim_Attack();//播放攻击动画
            //卡牌面板伤害一般为卡牌的攻击值+印记值
            Attack(attackCard.attack + attackCard.imprint, enemyState);
        }
        //向目标施加燃烧
        if (attackCard.fire > 0)
        { enemyState.GetFire(attackCard.fire);}
        //向目标施加毒素
        if (attackCard.toxin > 0)
        {enemyState.GetToxin(attackCard.toxin);}
        //向目标施加雷电
        if (attackCard.electricity > 0)
        {enemyState.GetElectricity(attackCard.electricity);}
        //对自身防御
        if (attackCard.defense > 0)
        { playerState.GetArmor(attackCard.defense);}

        //调用effect函数完成剩余效果
        cardEffect.Effect(attackCard, enemyState, playerState);
    }

    //攻击函数（由于攻击会附带很多效果，故封装成专门函数）
    public void Attack(int cardDamage, EnemyState enemyState)
    {
        enemyState.TakeDamage(cardDamage + playerState.strength);//卡牌面板伤害+玩家力量
        //是否有火焰附加
        if (playerState.fireAdd > 0)
        {
            enemyState.GetFire(playerState.fireAdd);//额外施加火焰
        }
    }

    //卡牌结算
    public void CardOver(GameObject _Card)
    {
        Card attackCard = _Card.GetComponent<CardDisplay>().card;
        //查看是否存在消耗词条
        if (attackCard.consume == 1)
        {
            ConsumeList.Add(attackCard);//加入消耗牌堆
        }
        else
        {
            //查看是否是能力牌
            if (attackCard.type != 2)
            {
                DisCardList.Add(attackCard);//加入弃牌堆
            }
            //能力牌直接消除，不进入任何地方
        }
        HandCount -= 1;//手牌数-1（这个要提前执行，否则连携可能会抽牌）
        //查看卡牌属性判断是否触发连携
        if (attackCard.element != 0)//如果不是普通牌则计入连携
        {
            playedCardNums.Add(attackCard.element);//计入连携
            Event.CallSendCard(attackCard.element);//向显示层所有的连携指示条发送信号
            if (playedCardNums.Count > 5) playedCardNums.RemoveAt(0);//最多记录5张
            CheckNumComboEffects();//检查连携
        }
        FreshCardCount();//更新显示层
        Destroy(_Card);//销毁卡牌对象
        execute = false;//卡牌执行完成

        //判断由卡牌执行结束回合的命令是否开启
        if (Wait_TurnEnd)
        {
            Wait_TurnEnd = false;
            //TurnEnd();//结束回合，不知道为什么这里会让“结束回合”效果的牌复制一份，所以延迟
            Invoke("TurnEnd", 0.3f);
        }
    }

    //敌人行动执行
    public IEnumerator EnemyAction()
    {
        //遍历所有敌人槽
        foreach (var block in enemyBlocks)
        {
            //判断enemyBlocks里面有没有敌人
            if (block.GetComponent<Block>().obj != null)
            {
                //获取敌人状态组件
                EnemyState enemyState = block.GetComponent<Block>().obj.GetComponent<EnemyState>();
                //判断敌人是否存活
                if (enemyState.life)
                {
                    //执行敌人意图前将Acing设为true
                    enemyState.Acing = true;
                    //执行敌人意图
                    enemyState.Execute(player);
                    //等待Action变为false（敌人执行完毕）
                    yield return new WaitUntil(() => !enemyState.Acing);
                    //等待部分特效执行完毕（0.5秒）
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
        //延迟0.5秒后进入下一阶段
        Invoke("NextPhase", 0.5f);
        //NextPhase();
    }

    //检测战斗是否结束（每有单位死亡会触发一次验证）
    public void CheckWin()
    {
        RefreshEnemyList();//刷新敌人指示容器

        if (activeEnemies.Count == 0)
        {
            Debug.Log("所有敌人已被击败，胜利！");
            //向Global_PlayerData修改玩家血量
            Global_PlayerData.hp = playerState.hp;
            //奖励金币战利品
            Global_PlayerData.coins += 20;
            //保存数据
            PlayerData.SavePlayerData();
            
            if (MapManager.Instance != null)
            {
                //通知地图管理器删除当前怪物
                MapManager.Instance.DeleteCurrentObject(Global_PlayerData.CurrentId);
                //验证房间是否清除，并执行后续流程
                MapManager.Instance.RoomClean();
            }
            else { Debug.Log("错误：没找到地图管理器"); }
            //调用SceneChanger切回主城
            SceneChanger.Instance.GetMajorCity();
        }
        else if (!playerState.life)
        {
            Debug.Log("玩家已阵亡，失败！");
            SceneChanger.Instance.BackManage();
        }
    }

    //连携检测函数
    private void CheckNumComboEffects()
    {
        foreach (var rule in comboRules)//遍历所有连携规则
        {
            int[] target = rule.targetCombo;//获取规则数组
            int targetLen = target.Length;
            int currentLen = playedCardNums.Count;

            if (currentLen < targetLen) continue;//当前记录长度不足以匹配该规则，跳过

            //连携次数用尽跳过
            if (rule.Use <= 0) continue;
            //从后往前匹配
            bool isMatch = true;
            for (int i = 0; i < targetLen; i++)
            {
                int targetNum = target[targetLen - 1 - i];
                int currentNum = playedCardNums[currentLen - 1 - i];
                if (targetNum != currentNum && 10 != currentNum)//10为万能元素
                {
                    //匹配失败退出连携
                    isMatch = false;
                    break;
                }
            }

            if (isMatch)
            {
                rule.effect.Invoke(null);//触发连携效果
                rule.Use--;//减少一次使用
                break; 
            }
        }
    }

    //连携触发函数
    public void TriggerRGBEffect(int id)
    {
        switch (id)
        {
            case 0://重甲兵
                playerState.GetArmor(6);//获得6点格挡
                break;
            case 1://刺客
                playerState.GetStrength(1);//获得1点力量
                break;
            case 2://元素使
                energy += 1;//获得1点能量
                RefreshEnergy();//更新能量文本
                break;
        }
                
    }


}