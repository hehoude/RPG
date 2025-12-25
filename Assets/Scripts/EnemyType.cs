public class EnemyType
{
    public int id;//编号
    public string name;//卡名
    public int maxhp;//最大血量
    public int hp;//血量
    public int attack;//攻击意图
    public int defense;//格挡意图
    public int build;//强化意图
    public int negative;//负面意图
    public int special1;//特殊意图1
    public int special2;//特殊意图2
    public int special3;//特殊意图3
    public int start;//指定初始意图
    public EnemyType(int _id, string _name, int _maxhp, int _hp, 
        int _attack, int _defense, int _build, int _negative, int _special1,
        int _special2, int _special3, int   _start)
    {
        this.id = _id;
        this.name = _name;
        this.maxhp = _maxhp;
        this.hp = _hp;
        this.attack = _attack;
        this.defense = _defense;
        this.build = _build;
        this.negative = _negative;
        this.special1 = _special1;
        this.special2 = _special2;
        this.special3 = _special3;
        this.start = _start;
    }
}
