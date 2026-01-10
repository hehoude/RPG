using System.Collections;
using System.Collections.Generic;
using System.IO;//文件读写
using UnityEngine;
//实例化时机：抽卡商店被创建时
//public class PlayerData : MonoBehaviour
public class PlayerData : MonoSingleton<PlayerData>
{
    //public static event System.Action DataLoaded;//定义事件，使用static变为静态全局事件，其它脚本可以直接访问
    //public event System.Action PlayerLoaded;//定义非静态事件，需要实例才能访问
    public List<Card> PlayerCardList = new List<Card>();//玩家卡组
    //public TextAsset playerData;//加载玩家数据
    private CardStore CardStore;//卡牌商店脚本
    private Global_PlayerData Global_PlayerData;
    //public int coins;//持有的金币数
    //public int maxhp;//玩家最大生命
    //public int hp;//玩家生命
    //public int id;//角色id
    public List<int> MateList = new List<int>();//伙伴列表
    public List<Card> MateCardList0 = new List<Card>();//队友0的卡组
    public List<Card> MateCardList1 = new List<Card>();//队友1的卡组
    public List<Card> MateCardList2 = new List<Card>();//队友2的卡组
    public List<Card> MateCardList3 = new List<Card>();//队友3的卡组
    public List<int> ComboList = new List<int>();//连携列表

    protected override void Awake()
    {
        base.Awake();
        CardStore = CardStore.Instance;
        Global_PlayerData = Global_PlayerData.Instance;
        //核心数据加载提前（确保不被MapManager抢先加载）
        CardStore.LoadCardData();//调用商店脚本加载所有卡牌数据
        CardStore.LoadMateList();//加载队友卡牌数据
        LoadPlayerData();//加载玩家数据
    }
    void Start()
    {
        
        // 触发事件，通知其他脚本“我已加载完成”
        //DataLoaded?.Invoke();
        //PlayerLoaded?.Invoke();
    }

    void Update()
    {
        
    }

    //加载玩家所有数据
    public void LoadPlayerData()
    {
        LoadBaseData();//加载基础数据
        LoadCardData();//加载卡组数据
        //Debug.Log("PlayerData：玩家信息加载完成：" + PlayerCardList.Count);
    }

    //加载玩家基础数据
    public void LoadBaseData()
    {
        string fullPath = Application.dataPath + "/Datas/PlayerData.csv";//读取路径
        string fileContent = File.ReadAllText(fullPath, System.Text.Encoding.UTF8);//转换为字符串
        string[] dataRow = fileContent.Split('\n');//拆分
        //string[] dataRow = playerData.text.Split('\n');原有读写存在bug，弃用改为位置读写
        int _x = 0;
        int _y = 0;
        foreach (var row in dataRow)
        {
            string cleanRow = row.Trim();// 去除首尾的空格、\r、\n、制表符等
            string[] rowArray = cleanRow.Split(',');
            if (rowArray[0] == "#")
            {
                continue;
            }
            else if (rowArray[0] == "coins")//金币
            {
                Global_PlayerData.coins = int.Parse(rowArray[1]);
            }
            else if (rowArray[0] == "maxhp")//血量上限
            {
                Global_PlayerData.maxhp = int.Parse(rowArray[1]);
                Debug.Log("玩家血量上限设置为："+ Global_PlayerData.maxhp);
            }
            else if (rowArray[0] == "hp")//血量
            {
                Global_PlayerData.hp = int.Parse(rowArray[1]);
                Debug.Log("玩家血量设置为：" + Global_PlayerData.hp);
            }
            else if (rowArray[0] == "id")//玩家ID
            {
                Global_PlayerData.id = int.Parse(rowArray[1]);
            }
            else if (rowArray[0] == "seed")//地图种子
            {
                Global_PlayerData.seed = int.Parse(rowArray[1]);
            }
            else if (rowArray[0] == "floor")//地图层级
            {
                Global_PlayerData.floor = int.Parse(rowArray[1]);
            }
            else if (rowArray[0] == "progress")//游戏进度
            {
                Global_PlayerData.progress = int.Parse(rowArray[1]);
            }
            else if (rowArray[0] == "x")//逻辑位置x
            {
                _x = int.Parse(rowArray[1]);
            }
            else if (rowArray[0] == "y")//逻辑位置y
            {
                _y = int.Parse(rowArray[1]);
            }
            else if (rowArray[0] == "teammate")//队友ID
            {
                MateList.Add(int.Parse(rowArray[1]));//加入伙伴列表
            }
            else if (rowArray[0] == "combo")//连携
            {
                ComboList.Add(int.Parse(rowArray[1]));//加入连携列表
            }
        }
        //将坐标加载至全局变量（这几个_x,_y写到foreach外面才行呀，排了老半天问题才发现）
        Global_PlayerData.currentRoom = new Vector2Int(_x, _y);
        Debug.Log("数据加载完成，坐标为x:" + _x + ",y:" + _y);
    }

    //加载玩家与队友的卡组
    public void LoadCardData()
    {
        string fullPath = Application.dataPath + "/Datas/PlayerCard.csv";//读取路径
        LoadCardList(fullPath, PlayerCardList);//加载玩家卡组
        if (MateList.Count > 0)
        {
            fullPath = Application.dataPath + "/Datas/MateCard0.csv";
            LoadCardList(fullPath, MateCardList0);
        }
        if (MateList.Count > 1)
        {
            fullPath = Application.dataPath + "/Datas/MateCard1.csv";
            LoadCardList(fullPath, MateCardList1);
        }
        if (MateList.Count > 2)
        {
            fullPath = Application.dataPath + "/Datas/MateCard2.csv";
            LoadCardList(fullPath, MateCardList2);
        }
        if (MateList.Count > 3)
        {
            fullPath = Application.dataPath + "/Datas/MateCard3.csv";
            LoadCardList(fullPath, MateCardList3);
        }
        //最多支持4名队友
    }

    //将指定路径的卡组加载进指定容器里
    public void LoadCardList(string fullPath, List<Card> _CardList)
    {
        _CardList.Clear();//清空容器防止重复加载
        string fileContent = File.ReadAllText(fullPath, System.Text.Encoding.UTF8);//转换为字符串
        string[] dataRow = fileContent.Split('\n');//拆分
        foreach (var row in dataRow)
        {
            string cleanRow = row.Trim();// 去除首尾的空格、\r、\n、制表符等
            string[] rowArray = cleanRow.Split(',');
            if (rowArray[0] == "#")
            {
                continue;
            }
            else if (rowArray[0] == "card")
            {
                int id = int.Parse(rowArray[1]);
                Card cardObject = CardStore.CopyCard(id);//复制指定id的卡牌
                //检测卡牌是否升级
                if ((rowArray[2]) == "1")//直接读字符串
                {
                    //Debug.Log("读取到升级卡");
                    Card upcard = Upgrade(cardObject);//升级以修改基础属性
                    _CardList.Add(upcard);//加入卡组
                }
                else
                {
                    //Debug.Log("读取到普通卡");
                    _CardList.Add(cardObject);//加入卡组
                }
            }
        }
    }

    //保存玩家所有数据
    public void SavePlayerData()
    {
        SaveBaseData();
        SaveCardData();
        Debug.Log("保存完成");
    }

    //保存基础数据
    public void SaveBaseData()
    {
        string path = Application.dataPath + "/Datas/PlayerData.csv";
        //预存到datas里
        List<string> datas = new List<string>();
        //保存血量上限
        datas.Add("maxhp," + Global_PlayerData.maxhp.ToString());
        Debug.Log("玩家血量上限保存为：" + Global_PlayerData.maxhp);
        //保存血量
        datas.Add("hp," + Global_PlayerData.hp.ToString());
        //保存金币
        datas.Add("coins," + Global_PlayerData.coins.ToString());
        //保存ID
        datas.Add("id," + Global_PlayerData.id.ToString());
        //保存种子
        datas.Add("seed," + Global_PlayerData.seed.ToString());
        //保存游戏进度
        datas.Add("floor," + Global_PlayerData.floor.ToString());
        //保存游戏进度
        datas.Add("progress," + Global_PlayerData.progress.ToString());
        //保存逻辑位置
        datas.Add("x," + Global_PlayerData.currentRoom.x.ToString());
        datas.Add("y," + Global_PlayerData.currentRoom.y.ToString());
        //保存伙伴列表
        for (int i = 0; i < MateList.Count; i++)
        {
            datas.Add("teammate," + MateList[i].ToString());
        }
        //保存连携列表
        for (int i = 0; i < ComboList.Count; i++)
        {
            datas.Add("combo," + ComboList[i].ToString());
        }
        //Debug.Log("玩家基础数据已保存");
        //保存数据
        File.WriteAllLines(path, datas);
    }

    //保存卡组数据
    public void SaveCardData()
    {
        string path = Application.dataPath + "/Datas/PlayerCard.csv";
        SaveCardList(path, PlayerCardList);
        if (MateList.Count > 0)
        {
            path = Application.dataPath + "/Datas/MateCard0.csv";
            SaveCardList(path, MateCardList0);
        }
        if (MateList.Count > 1)
        {
            path = Application.dataPath + "/Datas/MateCard1.csv";
            SaveCardList(path, MateCardList1);
        }
        if (MateList.Count > 2)
        {
            path = Application.dataPath + "/Datas/MateCard2.csv";
            SaveCardList(path, MateCardList2);
        }
        if (MateList.Count > 3)
        {
            path = Application.dataPath + "/Datas/MateCard3.csv";
            SaveCardList(path, MateCardList3);
        }
    }

    //将指定容器的卡组保存到指定路径
    public void SaveCardList(string path, List<Card> _CardList)
    {
        //将ID和升级状况储存
        List<string> deckDatas = new List<string>();
        for (int i = 0; i < _CardList.Count; i++)
        {
            deckDatas.Add("card," + _CardList[i].id.ToString() + "," + (_CardList[i].upgrade ? "1" : "0"));
        }
        File.WriteAllLines(path, deckDatas);
    }

    //升级卡牌
    public Card Upgrade(Card _card)
    {
        _card.upgrade = true;//升级标志位True
        switch (_card.id)
        {
            case 0://盾击
                _card.attack += 2;
                _card.defense += 1;
                break;
            case 1://铁壁
                _card.defense += 4;
                break;
            case 2://转守为攻
                break;
            case 3://戳刺
                _card.attack += 3;
                break;
            case 4://防御
                _card.defense += 3;
                break;
            case 5://暗器
                _card.attack += 2;
                break;
            case 6://极速突袭
                _card.attack += 3;
                break;
            case 7://燃烧
                break;
            case 8://烈焰打击
                _card.fire += 4;
                break;
            case 9://爆燃
                _card.spend -= 1;
                break;
            case 10://无情之阳
                _card.fire += 10;
                break;
            case 11://涅槃
                break;
            case 12://毒刺
                _card.toxin += 2;
                break;
            case 13://以毒攻毒
                _card.toxin += 3;
                break;
            case 14://瘟疫手雷
                _card.toxin += 2;
                break;
            case 15://雷光斩
            case 16://电能释放
            case 17://雷枪
                _card.electricity += 1;
                break;
            case 18://火球术
                _card.fire += 3;
                break;
            case 19://荆棘之甲
                _card.toxin += 1;
                _card.defense += 2;
                break;
            case 20://元素补剂
                break;
            case 21://提纯
                break;
            case 22://穿透打击
                _card.attack += 2;
                break;
            case 23://旋风锤
                _card.attack += 5;
                break;
            case 24://火中取栗
                break;
            case 25://毒液
                _card.toxin += 2;
                break;
            case 26://刺骨寒毒
                break;
            case 27://脉冲拳
                _card.attack += 2;
                _card.electricity += 2;
                break;
            case 28://燃烧之手
                break;
            case 29://喷火
                _card.fire += 3;
                break;
            case 30://备用护盾
                _card.defense += 3;
                break;
            case 31://完美弹反
                _card.keep = 2;
                break;
            case 32://钢筋铁骨
                break;
            case 33://干电池
                _card.defense += 3;
                break;
            case 34://石化药剂
                break;
        }
        return _card;
    }

    //将数组中的卡组加入玩家卡组
    public void AddPlayerCardList(int[] newList)
    {
        foreach (var cardID in newList)
        {
            Card cardObject = CardStore.CopyCard(cardID);//复制指定id的卡牌
            PlayerCardList.Add(cardObject);//加入卡组
        }
    }



}
