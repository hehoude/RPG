using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CardEffect : MonoBehaviour
{
    public BattleManager BattleManager;
    private void Awake()
    {
        BattleManager = BattleManager.Instance;
    }
    public void Effect(Card attackCard, EnemyState enemyState, PlayerState playerState)
    {
        //发起剩余效果
        int effect_id = attackCard.id;
        if (attackCard.upgrade)
        {
            effect_id += 1000;//已升级的卡延后1000位
        }
        switch (effect_id)
        {
            case 0://盾击
            case 1000:
                break;
            case 1://铁壁
            case 1001:
                break;
            case 2://转守为攻
                Anim_Attack();
                Attack(playerState.armor, enemyState);
                playerState.GetArmor(-playerState.armor);
                break;
            case 1002:
                Anim_Attack();
                Attack(playerState.armor, enemyState);
                playerState.GetArmor(-(playerState.armor/2));
                break;
            case 3://戳刺
            case 1003:
                break;
            case 4://防御
            case 1004:
                break;
            case 5://暗器
            case 1005:
                break;
            case 6://极速突袭
            case 1006:
                DrawCard(2);
                break;
            case 7://燃烧
                playerState.GetStrength(2);
                break;
            case 1007:
                playerState.GetStrength(3);
                break;
            case 8://烈焰打击
            case 1008:
                break;
            case 9://爆燃
            case 1009:
                enemyState.FireAnim();//触发敌人的燃烧动画
                enemyState.TakeDamage(enemyState.fire);
                enemyState.fire = 0;
                enemyState.GetFire(0);//使其刷新一次燃烧值
                break;
            case 10://无情之阳
            case 1010:
                break;
            case 11://涅槃
                playerState.Heal(15);
                playerState.GetFire(15);
                break;
            case 1011:
                playerState.Heal(20);
                playerState.GetFire(20);
                break;
            case 12://毒刺
            case 1012:
                break;
            case 13://以毒攻毒
                playerState.GetToxin(3);
                break;
            case 1013:
                playerState.GetToxin(4);
                break;
            case 14://瘟疫手雷
            case 1014:
                break;
            case 15://雷光斩
            case 1015:
                DrawCard(1);
                break;
            case 16://电能释放
            case 1016:
                break;
            case 17://雷枪
            case 1017:
                TurnEnd();
                break;
            case 18://火球术
            case 1018:
                break;
            case 19://荆棘之甲
            case 1019:
                break;
            case 20://元素补剂
                playerState.Heal(4);
                break;
            case 1020:
                playerState.Heal(6);
                break;
            case 21://提纯
            case 1021:
                if (BattleManager.TargetCard != null)
                {
                    GameObject targetCard = BattleManager.TargetCard;//获取被选中的卡牌
                    CardConsume(targetCard);//消耗
                }
                if (effect_id == 1021)
                {
                    DrawCard(1);
                }
                break;
            case 22://穿透打击
                if (enemyState.armor > 0)
                {
                    attackCard.imprint += 5;
                }
                break;
            case 1022:
                if (enemyState.armor > 0)
                {
                    attackCard.imprint += 7;
                }
                break;
            case 23://旋风锤
            case 1023:
                playerState.GetStrength(-1);
                break;
            case 24://火中取栗
                playerState.GetFire(3);
                DrawCard(2);
                break;
            case 1024:
                playerState.GetFire(3);
                DrawCard(3);
                break;
            case 25://毒液
            case 1025:
                break;
            case 26://刺骨寒毒
                if (enemyState.toxin > 0)
                {
                    enemyState.GetStrength(-2);
                }
                break;
            case 1026:
                if (enemyState.toxin > 0)
                {
                    enemyState.GetStrength(-3);
                }
                break;
            case 27://脉冲拳
            case 1027:
                break;
            case 28://燃烧之手
                playerState.GetFireAdd(3);
                break;
            case 1028:
                playerState.GetFireAdd(4);
                break;
            case 29://喷火
            case 1029:
                break;
            case 30://备用护盾
            case 1030:
                break;
            case 31://完美弹反
            case 1031:
                playerState.beatBack += 1;
                playerState.FreshState(6);
                break;
            case 32://钢筋铁骨
                playerState.immune += 1;
                playerState.FreshState(7);
                break;
            case 1032:
                playerState.immune += 2;
                playerState.FreshState(7);
                break;
        }
    }

    //需要提前发动的效果
    public void FrontEffect(Card attackCard, EnemyState enemyState, PlayerState playerState)
    {
        //发起剩余效果
        int effect_id = attackCard.id;
        if (attackCard.upgrade)
        {
            effect_id += 1000;//已升级的卡延后1000位
        }
        switch (effect_id)
        {
            case 22://穿透打击
            case 1022:
                attackCard.imprint = 0;//清除临时加成
                break;
        }
    }

    //抽到时效果
    public void EnterEffect(Card attackCard, PlayerState playerState)
    {

    }

    //丢弃时效果
    public void LoseEffect(Card attackCard, PlayerState playerState)
    {

    }

    //回合结束丢弃效果
    public void OverEffect(Card attackCard, PlayerState playerState)
    {

    }

    //回调BattleManager的方法
    //抽牌
    public void DrawCard(int count)
    {
        BattleManager.DrawCard(count);
    }

    //攻击动画
    public void Anim_Attack()
    {
        BattleManager.Anim_Attack();
    }

    //这张卡结算完毕后回合结束
    public void TurnEnd()
    {
        BattleManager.Wait_TurnEnd = true;
    }

    //消耗指定卡牌
    public void CardConsume(GameObject targetCard)
    {
        Card _cardObj = targetCard.GetComponent<CardDisplay>().card;//获取卡牌脚本
        BattleManager.ConsumeList.Add(_cardObj);//放入消耗牌堆
        Destroy(targetCard.gameObject);//销毁对象
        BattleManager.HandCount -= 1;//别忘了修改手牌数量
    }

    //主动弃置卡牌
    public void ThrowCard(GameObject targetCard, PlayerState playerState)
    {
        Card _cardObj = targetCard.GetComponent<CardDisplay>().card;//获取卡牌脚本
        //检测是否有主动弃置效果
        if (_cardObj.other == 2)
        {
            //执行弃置效果
            LoseEffect(_cardObj, playerState);
        }
        BattleManager.ConsumeList.Add(_cardObj);//放入弃置牌堆
        Destroy(targetCard.gameObject);//销毁对象
        BattleManager.HandCount -= 1;//别忘了修改手牌数量
    }

    //回调战斗管理器攻击函数（不触发动画）
    public void Attack(int cardDamage, EnemyState enemyState)
    {
        BattleManager.Attack(cardDamage, enemyState);
    }

}
