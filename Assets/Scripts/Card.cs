public class Card
{
    public int id;//编号
    public int type;//种类（0攻击牌、1技能牌、2能力牌）
    public int rarity;//稀有度
    public string cardName;//卡名
    public int spend;//费用
    public int target;//是否需要选择目标（1敌人、2全体、3手牌）
    public int attack;//伤害数值
    public int defense;//防御数值
    public int keep;//虚无1/保留2/固有3
    public int consume;//消耗1
    public bool upgrade;//是否升级
    public int element;//元素属性（影响连携，10为万能元素）
    public int fire;//火焰
    public int toxin;//毒素
    public int electricity;//电
    public int other;//1抽到效果,2丢弃效果,3回合结束效果
    public int imprint;//印记（攻击牌获得印记会增加相应伤害）
    public int front;//1有需要提前执行的效果
    public Card(int _id, int _type, int _rarity, string _cardName, int _spend, 
        int _target, int _attack, int _defense, int _keep, int _consume, 
        int _element, int _fire, int _toxin, int _electricity, int _other, int _front)
    {
        this.id = _id;
        this.type = _type;
        this.rarity = _rarity;
        this.cardName = _cardName;
        this.spend = _spend;
        this.target = _target;
        this.attack = _attack;
        this.defense = _defense;
        this.keep = _keep;
        this.consume = _consume;
        this.upgrade = false;//默认没有升级
        this.element = _element;
        this.fire = _fire;
        this.toxin = _toxin;
        this.electricity = _electricity;
        this.other = _other;
        this.imprint = 0;
        this.front = _front;
    }
}
/*
public class AttackCard: Card
{
    public AttackCard(int _id, string _cardName, int _spend, int _rarity, int _attack, int _healthPointMax) : 
        base( _id, _cardName, _spend, _rarity)//调用父类构造函数
    {
        
    }
}

public class SkillCard : Card
{
    public SkillCard(int  _id, string _cardName, int _spend, int _rarity, string _effect) : 
        base(_id, _cardName, _spend, _rarity)
    {
        
    }
}
*/