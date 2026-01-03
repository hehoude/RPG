using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerState : BattleState
{
    [Header("其它")]
    //public GameObject DataManager;//从数据管理器获取玩家数据
    private PlayerData PlayerData;
    private Global_PlayerData Global_PlayerData;

    

    //public PlayerType playerType;

    void Awake() // 用 Awake() 初始化，比 OnEnable() 早，避免时机问题
    {
        PlayerData = PlayerData.Instance;
        Global_PlayerData = Global_PlayerData.Instance;
        State_Objects = new Dictionary<int, GameObject>();//初始化字典
    }
    void Start()
    {
        Debug.Log("所有信息加载完成，进入准备阶段");
        StartShow();//初始化信息
        Refresh();//刷新界面
    }
    void Update()
    {

    }

    //void OnEnable()
    //{
    //    //当脚本PlayerData的DataLoaded事件被触发后，执行OnScriptADataLoaded
    //    PlayerData.PlayerLoaded += OnDataLoaded;
    //}

    //void OnDataLoaded()
    //{
    //    Debug.Log("所有信息加载完成，进入准备阶段");
    //    StartShow();//初始化信息
    //    Refresh();//刷新界面
    //    PlayerData.PlayerLoaded -= OnDataLoaded;//取消事件订阅释放内存
    //}

    //初始化显示层
    public void StartShow()
    {
        id = Global_PlayerData.id;//加载id
        nameText.text = GetName(id);//显示名字
        maxhpText.text = Global_PlayerData.maxhp.ToString();//显示最大生命
        hp = Global_PlayerData.hp;//加载初始生命值
        maxhp = Global_PlayerData.maxhp;
    }

    //获取玩家职业名字
    public string GetName(int _id)
    {
        string name;
        switch (_id)
        {
            case 0:
                name = "无名";
                break;
            case 1:
                name = "重装战士";
                break;
            case 2:
                name = "刺客";
                break;
            case 3:
                name = "元素使";
                break;
            default:
                name = "";
                break;
        }
        return name;
    }

    //中毒结算
    public void ToxinSolve()
    {
        TakeDamage(toxin);//结算一次毒素伤害
        GetToxin(-3);//减少三层毒素
    }

    //燃烧结算
    public void FireSolve()
    {
        int damage = fire - (fire / 2);
        TakeDamage(damage);//结算一次燃烧伤害
        GetFire(-damage);//减少一半燃烧
    }

    //播放燃烧动画
    public void FireAnim()
    {
        Instantiate(BoomAnim_Prefab, EffectPlace);
        Invoke("FireSolve", 0.7f);
    }

    //播放中毒动画
    public void ToxinAnim()
    {
        Instantiate(ToxinAnim_Prefab, EffectPlace);
        Invoke("ToxinSolve", 0.7f);
    }

}
