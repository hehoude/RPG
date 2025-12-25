using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//全局变量
public class Global_PlayerData : MonoSingleton<Global_PlayerData>
{
    public int coins;//持有的金币数
    public int maxhp;//玩家最大生命
    public int hp;//玩家生命
    public int id;//角色id
    public int seed = 100;//随机种子
    public int progress = 0;//游戏进度（已完成的关卡数）
    public bool newGame = true;//是否为新游戏

    public Vector2Int currentRoom = new Vector2Int(0, 0);//玩家所在房间的逻辑坐标（默认初始房间）
    public int CurrentId = 0;
}
