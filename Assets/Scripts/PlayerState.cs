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

public class PlayerState : MonoBehaviour
{
    [Header("基本属性")]
    public int id;
    public int hp;//生命值
    public int maxhp;//最大生命值
    public bool life = true;//生存状况
    public int armor = 0;//护甲
    [Header("主要状态")]
    public int strength = 0;//力量
    public int firm = 0;//坚固
    public int fire = 0;//燃烧层数
    public int toxin = 0;//毒素层数
    public int electricity = 0;//雷电层数
    public int fireAdd = 0;//火焰附加层数
    [Header("UI")]
    public Text nameText;
    public Text hpText;
    public Text maxhpText;
    public Text armorText;
    public Slider hpbar;
    public Image enemyImage;
    [Header("状态")]
    public Transform State;//状态栏
    private Dictionary<int, GameObject> State_Objects;//存储所有状态指示器的引用
    public GameObject State_Prefab;//状态图标预制体
    [Header("特效")]
    public Transform EffectPlace; //特效区域
    public GameObject BoomAnim_Prefab;//燃烧特效
    public GameObject ToxinAnim_Prefab;//中毒特效
    public GameObject HurtAnim_Prefab;//受伤数字特效
    public GameObject DefAnim_Prefab;//格挡特效
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
    //更新显示层
    public void Refresh()
    {
        hpText.text = hp.ToString();//更新当前生命
        hpbar.value = (float)hp / (float)maxhp;//更新血条
        armorText.text = armor.ToString();
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

    //伤害函数
    public void TakeDamage(int damage)
    {
        //雷电附加
        if (electricity > 0)
        {
            damage += electricity;
            GetElectricity(-1);
            if (electricity > 5) { GetElectricity(-1); }//大于5层额外扣减
        }
        //播放受伤特效
        GameObject hurt_count = Instantiate(HurtAnim_Prefab, EffectPlace);
        hurt_count.GetComponent<Text>().text = damage.ToString();
        //伤害结算
        armor -= damage;//掉甲
        if (armor < 0)
        {
            hp += armor;//扣血
            armor = 0;
        }
        if (hp < 0) hp = 0;
        Refresh();//更新显示层
        //死亡
        if (hp <= 0)
        {
            gameObject.SetActive(false);
            life = false;
            //通知战斗管理器验证一次是否胜利
            BattleManager.Instance.CheckWin();
        }
    }

    //放血函数
    public void LostHp(int damage)
    {
        hp -= damage;
        Refresh();
        //死亡
        if (hp <= 0)
        {
            gameObject.SetActive(false);
            life = false;
            //通知战斗管理器验证一次是否胜利
            BattleManager.Instance.CheckWin();
        }
    }

    //治疗函数
    public void Heal(int _count)
    {
        hp += _count;//回血
        if (hp > maxhp) { hp = maxhp; }
        Refresh();//更新显示层
    }

    //起甲函数
    public void GetArmor(int _armor)
    {
        if (_armor > 0)
        {
            armor += (_armor + firm);//正数起甲才受到坚固影响
            Instantiate(DefAnim_Prefab, EffectPlace);//播放动画
        }
        else
        {
            //零起甲或负数起甲可能是一些影响格挡的特殊效果，并非正常获取格挡
            armor += _armor;
        }
        Refresh();
    }

    //清空格挡函数
    public void CleanArmor()
    {
        armor = 0;
        Refresh();
    }

    //修改力量函数
    public void GetStrength(int _strength)
    {
        strength += _strength;
        FreshState(0);
    }

    //修改燃烧函数
    public void GetFire(int _count)
    {
        fire += _count;
        if (fire < 0) { fire = 0; }
        FreshState(1);
    }

    //修改毒素函数
    public void GetToxin(int _count)
    {
        toxin += _count;
        if (toxin < 0) { toxin = 0; }
        FreshState(2);
    }

    //修改雷电函数
    public void GetElectricity(int _count)
    {
        electricity += _count;
        if (electricity < 0) { electricity = 0; }
        FreshState(3);
    }

    //修改坚固函数
    public void GetFirm(int _firm)
    {
        firm += _firm;
        FreshState(4);
    }

    //修改火焰附加函数
    public void GetFireAdd(int _count)
    {
        fireAdd += _count;
        if (fireAdd < 0) { fireAdd = 0; }
        FreshState(5);
    }

    //刷新状态栏（出于性能考虑，每次调用只刷新一种状态）
    public void FreshState(int _state)
    {
        int state_Count = 0;
        GameObject state_Object = null;
        switch (_state)
        {
            case 0://力量
                state_Count = strength;
                break;
            case 1://燃烧
                state_Count = fire;
                break;
            case 2://中毒
                state_Count = toxin;
                break;
            case 3://雷电
                state_Count = electricity;
                break;
            case 4://坚固
                state_Count = firm;
                break;
            case 5://火焰附加
                state_Count = fireAdd;
                break;
            default://传入未知变量则用默认值
                //state_Count = strength;
                //state_Object = ref Strength_State;
                Debug.Log("状态栏刷新异常！");
                break;
        }
        if (state_Count != 0)//检测数值
        {
            // 尝试从字典取值
            bool hasStateInDict = State_Objects.TryGetValue(_state, out state_Object);

            // 校验对象是否有效（未销毁/非null）
            bool isStateObjValid = state_Object != null && state_Object;

            // 无有效对象 → 创建新图标
            if (!hasStateInDict || !isStateObjValid)
            {
                //调用状态管理器创建图标
                state_Object = AddState(_state, State.transform);
                //放入字典
                State_Objects[_state] = state_Object;
            }
            state_Object.GetComponent<StateDisplay>().FreshCount(state_Count);//更新数值文本
        }
        else if (state_Count == 0)
        {
            if (!State_Objects.TryGetValue(_state, out state_Object))
            {
                Debug.LogWarning($"状态栏{_state}不存在字典中！");
                return;
            }
            if (state_Object != null) // 先判断对象是否有效（避免重复销毁）
            {
                Destroy(state_Object);
            }
            // 清空字典中该状态的引用
            State_Objects[_state] = null;
        }
    }

    //添加状态主要图标，接收状态id与添加位置，返回图标对象
    public GameObject AddState(int _state, Transform _stateLab)
    {
        GameObject NewState = Instantiate(State_Prefab, _stateLab);//添加状态图标
        StateDisplay stateDisplay = NewState.GetComponent<StateDisplay>();//获取脚本
        stateDisplay.id = _state;//赋予状态id
        return NewState;
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
