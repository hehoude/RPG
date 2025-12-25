using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
//实例化时机：卡片被创建时
public class CardDisplay : MonoBehaviour
{
    //这里的Text是Unity的一个类，用于和UI界面上的文本相关联
    //使用此脚本的控件会出现相应的插槽，将UI界面上的控件拖入插槽，就可以与这里的实例关联
    public Text nameText;
    public Text spendText;
    public Text effectText;

    public Image backgroundImage;
    public Image stone;

    //这里声明一个Card类型的实例card，但没有赋值（等待其它代码执行赋值）
    public Card card;

    private string effect;//卡牌效果（文本）由此脚本负责生成

    void Start()
    {
        RefreshCard();
    }
    void Update()
    {
        
    }

    //刷新卡牌内容
    public void RefreshCard()
    {
        Effect();
        ShowCard();
    }

    //将类中的卡片信息呈现到UI上
    public void ShowCard()
    {
        //部分消耗为X的牌特殊处理
        if (999 == card.spend)
        {
            spendText.text = "X";
        }
        else
        {
            spendText.text = card.spend.ToString();
        }
        effectText.text = effect;
        if (card.upgrade)
        {
            nameText.text = (card.cardName + "+");
        }
        else
        {
            nameText.text = card.cardName;
        }
        //右上角宝石颜色
        switch (card.element)
        {
            case 1://火
                stone.color = Color.red;
                spendText.color = Color.white;
                break;
            case 2://毒
                stone.color = Color.green;
                spendText.color = Color.blue;
                break;
            case 3://电
                stone.color = Color.yellow;
                spendText.color = Color.blue;
                break;
            default:
                stone.color = Color.white;
                spendText.color = Color.blue;
                break;
        }
    }

    //根据卡牌id生成效果文本
    public void Effect()
    {
        int _id = card.id;
        if (card.upgrade)
        {
            _id += 1000;//已升级的卡延后1000位
        }
        switch (_id)
        {
            case 0://盾击
            case 1000:
                effect = "造成"+card.attack+ "点伤害，获得" + card.defense + "点格挡";
                break;
            case 1://铁壁
            case 1001:
                effect = "获得" + card.defense + "点格挡";
                break;
            case 2://转守为攻
                effect = "造成等同护甲的伤害，失去所有护甲";
                break;
            case 1002:
                effect = "造成等同护甲的伤害，失去一半护甲";
                break;
            case 3://戳刺
            case 1003:
                effect = "造成" + card.attack + "点伤害";
                break;
            case 4://防御
            case 1004:
                effect = "获得" + card.defense + "点格挡";
                break;
            case 5://暗器
            case 1005:
                effect = "造成" + card.attack + "点伤害";
                break;
            case 6://极速突袭
            case 1006:
                effect = "造成" + card.attack + "点伤害，抽2张牌";
                break;
            case 7://燃烧
                effect = "获得2点力量";
                break;
            case 1007:
                effect = "获得3点力量";
                break;
            case 8://烈焰打击
            case 1008:
                effect = "造成" + card.attack + "点伤害，施加"+ card.fire + "点燃烧";
                break;
            case 9://爆燃
            case 1009:
                effect = "立即结算目标的所有燃烧";
                break;
            case 10://无情之阳
            case 1010:
                effect = "施加" + card.fire + "点燃烧\n消耗";
                break;
            case 11://涅槃
                effect = "治疗15生命，获得15燃烧\n消耗";
                break;
            case 1011:
                effect = "治疗20生命，获得20燃烧\n消耗";
                break;
            case 12://毒刺
            case 1012:
                effect = "造成" + card.attack + "点伤害，施加" + card.toxin + "点中毒";
                break;
            case 13://以毒攻毒
                effect = "施加" + card.toxin + "点中毒，自己受到3点中毒";
                break;
            case 1013:
                effect = "施加" + card.toxin + "点中毒，自己受到4点中毒";
                break;
            case 14://瘟疫手雷
            case 1014:
                effect = "对敌方全体施加" + card.toxin + "点中毒\n消耗";
                break;
            case 15://雷光斩
            case 1015:
                effect = "造成" + card.attack + "点伤害\n施加" + card.electricity + "点雷电\n抽一张牌";
                break;
            case 16://电能释放
            case 1016:
                effect = "每点能量施加" + card.electricity + "点雷电\n消耗";
                break;
            case 17://雷枪
            case 1017:
                effect = "施加" + card.electricity + "点雷电，结束你的回合";
                break;
            case 18://火球术
            case 1018:
                effect = "造成" + card.attack + "点伤害，施加"+ card.fire + "点燃烧";
                break;
            case 19://荆棘之甲
            case 1019:
                effect = "获得" + card.defense + "点格挡，施加"+ card.toxin + "点中毒";
                break;
            case 20://元素补剂
                effect = "治疗4点生命，消耗\n可当任何元素牌使用";
                break;
            case 1020:
                effect = "治疗6点生命，消耗\n可当任何元素牌使用";
                break;
            case 21://提纯
                effect = "选择一张手牌消耗\n消耗";
                break;
            case 1021:
                effect = "选择一张手牌消耗\n抽一张牌\n消耗";
                break;
            case 22://穿透打击
                effect = "造成" + card.attack + "点伤害，如果目标有护甲，伤害+5";
                break;
            case 1022:
                effect = "造成" + card.attack + "点伤害，如果目标有护甲，伤害+7";
                break;
            case 23://旋风锤
                effect = "造成" + card.attack + "点伤害，自身力量-1";
                break;
            case 1023:
                effect = "造成" + card.attack + "点伤害，自身力量-1";
                break;
            case 24://火中取栗
                effect = "抽两张牌，获得3层燃烧\n虚无";
                break;
            case 1024:
                effect = "抽三张牌，获得3层燃烧\n虚无";
                break;
            case 25://毒液
            case 1025:
                effect = "施加" + card.toxin + "点中毒";
                break;
            case 26://刺骨寒毒
                effect = "如果目标中毒，减少目标2点力量\n消耗";
                break;
            case 1026:
                effect = "如果目标中毒，减少目标3点力量\n消耗";
                break;
            case 27://脉冲拳
            case 1027:
                effect = "造成" + card.attack + "点伤害\n施加" + card.electricity + "点雷电\n消耗";
                break;
        }
    }
}
