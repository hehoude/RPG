using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//全局变量
public class Global_PlayerData : MonoSingleton<Global_PlayerData>
{
    //************************通用全局变量*************************

    public int coins;//持有的金币数
    public int maxhp;//玩家最大生命
    public int hp;//玩家生命
    public int id;//角色id
    public int seed = 100;//随机种子
    public bool newGame = true;//是否为新游戏
    public int Map = 1;//玩家当前所在地图

    //*************************经典模式变量************************

    public int floor = 0;//玩家层级
    public int progress = 0;//游戏进度（已完成的关卡数）
    public Vector2Int currentRoom = new Vector2Int(0, 0);//玩家所在房间的逻辑坐标（默认初始房间）
    public int CurrentId = 0;

    //*************************战役模式变量************************
    public int place = 0;
}
