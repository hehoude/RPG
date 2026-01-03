using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

public class EnemyState : BattleState, IPointerDownHandler
{
    [Header("意图")]
    public Text intent;//意图文本
    public int attack;//攻击意图
    public int defense;//防御意图
    public int build;//强化意图
    public int negative;//负面意图
    public int special1;//特殊意图1
    public int special2;//特殊意图2
    public int special3;//特殊意图3
    public int runningIntent = 1;//即将执行的意图
    [Header("其它")]
    
    public GameObject Target;//攻击目标
    public bool choose = false;//是否可被选择
    
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
            if (runningIntent == 100) { runningIntent = 0; }//用100号初始意图代替攻击意图
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
                int def = defense + firm;
                if (def < 0) { def = 0; }
                _intent = "防御：" + def;
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

    //执行意图（这个函数会在动画播放时由EnemyImage触发）
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
                EnemyDefence(defense);
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
    public void EnemyAttack(int damage, PlayerState target)
    {
        int hurt = damage + strength;//基础数值+力量
        if (hurt > 0)//大于零才会打出伤害
        {
            //造成伤害（自身为伤害来源）
            target.TakeDamage(hurt, this);
            //是否有火焰附加
            if (fireAdd > 0)
            {
                target.GetFire(fireAdd);//额外施加火焰
            }
        }
        //攻击附加效果（部分敌人可以有）
        switch (id)
        {
            case 7:
                enemyType.start = 1;//指定下一个为防御意图
                break;
            default:
                break;
        }
    }

    //敌人防御意图
    public void EnemyDefence(int _defence)
    {
        int def = _defence + firm;//附加坚固
        GetArmor(def);
        //防御附加效果（部分敌人可以有）
        switch (id)
        {
            case 7:
                enemyType.start = 2;//指定下一个为强化意图
                break;
            default:
                break;
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
            case 7://哥布林英雄
                GetStrength(3);
                GetFirm(3);
                enemyType.start = 100;//指定下一个为攻击意图
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
                EnemyDefence(8);
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
                    enemyType.start = 4;//下一个意图为特殊意图1
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
