using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

public class EnemyState : MonoBehaviour, IPointerDownHandler
{
    [Header("基础属性")]
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
    [Header("意图")]
    public int attack;//攻击意图
    public int defense;//防御意图
    public int build;//强化意图
    public int negative;//负面意图
    public int special1;//特殊意图1
    public int special2;//特殊意图2
    public int special3;//特殊意图3
    public int runningIntent = 1;//即将执行的意图
    [Header("UI")]
    public Text nameText;
    public Text hpText;
    public Text maxhpText;
    public Slider hpbar;
    public Text intent;
    public Text armorText;
    public GameObject Image;//图片对象
    public GameObject Target;//攻击目标
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
    public bool choose = false;//是否可被选择
    public bool Acing = false;//是否正在行动
    public EnemyType enemyType;

    void Awake() // 用 Awake() 初始化，比 OnEnable() 早，避免时机问题
    {
        State_Objects = new Dictionary<int, GameObject>();//初始化字典
    }

    void Start()
    {
        StartShow();
        Refresh();
        RefreshIntent();//初始化完再随机意图
        hpbar.value = 1;
        life = true;
    }
    void Update()
    {
        if (choose)
        {
            nameText.color = Color.red;
        }
        else
        {
            nameText.color = Color.white;
        }
    }

    //初始化显示层
    public void StartShow()
    {
        id = enemyType.id;//加载id
        LoadImage(id);//加载图片
        nameText.text = enemyType.name;//显示名字
        maxhp = enemyType.maxhp;//加载最大生命值
        maxhpText.text = maxhp.ToString();//显示最大生命
        hp = enemyType.hp;//加载初始生命值

        //加载意图
        attack = enemyType.attack;
        defense = enemyType.defense;
        build = enemyType.build;
        negative = enemyType.negative;
        special1 = enemyType.special1;
        special2 = enemyType.special2;
        special3 = enemyType.special3;
    }

    //图片加载函数
    public void LoadImage(int _id)
    {
        string imageEnemy = Application.dataPath + "/Image/Enemy/"+_id.ToString()+".png";
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

    //更新显示层
    public void Refresh()
    {
        hpText.text = hp.ToString();//更新当前生命
        hpbar.value = (float)hp / (float)maxhp;//更新血条
        armorText.text = armor.ToString();//更新格挡
    }

    //被鼠标点击时将触发这个函数
    public void OnPointerDown(PointerEventData eventData)
    {
        //当这个敌人可选，且鼠标左键时（否则右键取消时会误选择）
        if (choose && Input.GetMouseButton(0))
        {
            //Debug.Log("成功选中敌人");
            //告知战斗管理器自己被选中
            BattleManager.Instance.AttackConfirm(gameObject);
        }
    }

    //更新意图
    public void RefreshIntent()
    {
        //查看是否有指定意图
        if (enemyType.start == 0)
        {
            runningIntent = RandomIntent();//随机抽取一个意图作为现有意图
        }
        else
        {
            runningIntent = enemyType.start;//变为指定意图
            enemyType.start = 0;
        }
        string _intent;
        switch (runningIntent)
        {
            case 0://攻击意图
                int hurt = attack + strength;
                if (hurt < 0) { hurt = 0; }
                _intent = "攻击：" + (hurt);
                break;
            case 1://格挡意图
                _intent = "防御：" + defense;
                break;
            case 2://强化意图
                _intent = "强化";
                break;
            case 3://负面意图
                _intent = "负面";
                break;
            case 4://特殊意图1
                _intent = EnemyText(id, 4);
                break;
            case 5://特殊意图2
                _intent = EnemyText(id, 5);
                break;
            case 6://特殊意图3
                _intent = EnemyText(id, 6);
                break;
            default:
                _intent = "未知意图";
                break;
        }
        intent.text = _intent;//显示层更改意图
    }

    //随机意图
    public int RandomIntent()
    {
        int[] intents = new int[7] { attack, defense, build, negative, special1, special2, special3 };
        int _intent = UnityEngine.Random.Range(0, intents.Length);//这是开集，不会选中末尾
        if (intents[_intent] <= 0)//判断此敌人是否有这种意图
        {
            return RandomIntent();//无效意图则递归
        }
        else
        {
            return _intent;
        }
    }

    //伤害函数
    public void TakeDamage(int damage)
    {
        //雷电附加
        if (electricity>0)
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
            Acing = false;//通知协程函数行动结束
            //通知战斗管理器验证一次是否胜利
            BattleManager.Instance.CheckWin();
        }
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
    public void GetStrength(int _count)
    {
        strength += _count;
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

    //回合开始（这个由战斗管理器触发，并传入玩家对象）
    public void Execute(GameObject player)
    {
        if (life)
        {
            CleanArmor();//清空自身格挡
            Target = player;//确定目标
            if (toxin > 0)//判断中毒
            {
                ToxinAnim();//播放中毒动画
                Invoke("ToxinSolve", 1f);//动画完毕后结算中毒
            }
            else{ActionAnim();}//立即行动
        }
    }

    //中毒结算
    public void ToxinSolve()
    {
        TakeDamage(toxin);//结算一次毒素伤害
        GetToxin(-3);//减少三层毒素
        ActionAnim();//结算中毒完成后进入行动
    }

    //播放行动动画
    public void ActionAnim()
    {
        Image.GetComponent<EnemyImage>().Donghua();//播放行动动画（1秒）
    }

    //行动结算（这个函数会在动画播放时由EnemyImage触发）
    public void Action()
    {
        //数据层执行意图
        PlayerState target = Target.GetComponent<PlayerState>();
        switch (runningIntent)
        {
            case 0://攻击意图
                EnemyAttack(attack, target);//基础数值即为攻击值
                break;
            case 1://格挡意图
                GetArmor(defense);
                break;
            case 2://强化意图
                EnemyBuild();
                break;
            case 3://负面意图
                EnemyNega(target);
                break;
            case 4://特殊意图1
                EnemySpe1(target);
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
        RefreshIntent();//执行完毕后重新选择意图
        EnemySkillBack();//恢复部分技能冷却
        if (fire > 0)//判断燃烧
        {
            Invoke("FireAnim", 1f);//播放燃烧动画
            Invoke("FireSolve", 2f);//动画完毕后结算燃烧
        }
        else
        {
            Acing = false;//没有燃烧则直接结束行动
        }
    }

    //燃烧结算
    public void FireSolve()
    {
        int damage = fire - (fire / 2);
        TakeDamage(damage);//结算一次燃烧伤害
        GetFire(-damage);//减少一半燃烧
        Acing = false;//燃烧结算完毕，结束行动
    }

    //播放燃烧动画（由于卡牌也会调用这个动画，所以不自动结算燃烧）
    public void FireAnim()
    {
        Instantiate(BoomAnim_Prefab, EffectPlace);
    }

    //播放中毒动画
    public void ToxinAnim()
    {
        Instantiate(ToxinAnim_Prefab, EffectPlace);
    }

    //敌人攻击意图
    public void EnemyAttack(int _damage, PlayerState target)
    {
        int hurt = _damage + strength;//基础数值+力量
        if (hurt > 0)//大于零才会打出伤害
        {
            //造成伤害
            target.TakeDamage(hurt);
            //是否有火焰附加
            if (fireAdd > 0)
            {
                target.GetFire(fireAdd);//额外施加火焰
            }
        }
    }

    //敌人强化意图
    public void EnemyBuild()
    {
        switch (id)
        {
            case 0://恶魔
                GetStrength(5);
                build = -2;//隔2回合才能使用
                break;
            case 3://火苗
                GetFireAdd(10);
                build = 0;//不可再使用，不会恢复
                break;
            default:

                break;
        }
    }
    
    //敌人负面意图
    public void EnemyNega(PlayerState _Target)
    {
        switch (id)
        {
            case 1://幽灵
                _Target.GetStrength(-2);//减少目标2力量
                negative = -3;//隔3回合才能使用
                break;
            case 4://魔蛛
                _Target.GetToxin(10);//施加10中毒
                negative = -3;//隔3回合才能使用
                break;
            default:

                break;
        }
    }

    //敌人特殊意图1
    public void EnemySpe1(PlayerState _Target)
    {
        switch (id)
        {
            case 2://小鸡战士
                EnemyAttack(attack - 4, _Target);
                GetArmor(8);
                special1 = -1;//隔1回合才能使用
                break;
            case 6://杀手蝎
                EnemyAttack(attack +20, _Target);
                special1 = -3;//隔3回合才能使用
                break;
            default:

                break;
        }
    }

    //部分敌人回合结束需要恢复技能冷却
    public void EnemySkillBack()
    {
        switch (id)
        {
            case 0://恶魔
                build += 1;
                break;
            case 1://幽灵
                negative += 1;
                break;
            case 2://小鸡战士
                special1 += 1;
                break;
            case 4://魔蛛
                negative += 1;
                break;
            case 6://杀手蝎
                special1 += 1;
                if (special1>0)//如果CD满了
                {
                    enemyType.start = 4;//下一个意图必定为特殊意图1
                }
                break;
            default:

                break;
        }
    }

    //敌人特殊意图文本
    public String EnemyText(int _id, int _type)//接受敌人ID与敌人意图代号
    {
        String _Text = String.Empty;
        int hurt;
        switch (_id)
        {
            case 2://小鸡战士
                hurt = attack + strength - 4;
                if (hurt < 0) { hurt = 0; }
                _Text = "攻击：" + (hurt) + "\n防御：8";
                break;
            case 6://杀手蝎
                hurt = attack + strength +20;
                if (hurt < 0) { hurt = 0; }
                _Text = "攻击：" + (hurt);
                break;
            default:

                break;
        }
        return _Text;
    }

}
