using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomDisplay : MonoBehaviour
{
    public Room room;//接受一个房间数据实例
    [Header("资源预制体")]
    public Transform[] sourceIcons;//资源点图标数组
    public GameObject FirePrefab;//火堆预制体
    public GameObject DeletePrefab;//删卡预制体
    public GameObject BoxPrefab;//宝箱预制体
    public GameObject ShopPrefab;//商人预制体
    public GameObject ChoosePrefab;//三选一预制体
    public GameObject DoorPrefab;//门预制体
    public GameObject EnemyPrefab;//敌人预制体
    [Header("资源接口")]
    private GameObject Door;//门对象
    private GameObject[] Source;//资源
    private Global_PlayerData Global_PlayerData;

    void Awake()
    {
        Global_PlayerData = Global_PlayerData.Instance;
        //初始化资源表
        Source = new GameObject[9];
    }

    void Start()
    {
        if (room == null)
        {
            Debug.LogError("房间数据未设置，无法生成资源点");
        }
        ShowRoom();//生成房间资源（游戏开始只能执行一次，后续不可再重复执行）
        //判断房间是否为支线房间，支线房间如有怪物需要隐藏资源
        if (!room.MainRoom && !room.Clear)
        {
            SourceSetActive(false);
        }
        RefreshDoorState();//更新门状态
    }

    
    void Update()
    {
        
    }

    // 当玩家进入触发器范围时触发
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检测碰撞体是否是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            //修改全局玩家数据中的当前房间坐标，变为此房间的逻辑坐标
            Global_PlayerData.currentRoom = room.roomPos;
            SavePlayerPos();
        }
    }

    public void ShowRoom()
    {
        //在九个资源点上生成资源
        for (int i = 0; i < 9; i++)
        {
            switch (room.Source[i])
            {
                case 1://火堆
                    Source[i] = Instantiate(FirePrefab, sourceIcons[i]);
                    //Debug.Log("生成火堆");
                    break;
                case 2://删卡
                    Source[i] = Instantiate(DeletePrefab, sourceIcons[i]);
                    //Debug.Log("生成删卡");
                    break;
                case 3://宝箱
                    Source[i] = Instantiate(BoxPrefab, sourceIcons[i]);
                    //Debug.Log("生成宝箱");
                    break;
                case 4://商店
                    Source[i] = Instantiate(ShopPrefab, sourceIcons[i]);
                    //Debug.Log("生成商店");
                    break;
                case 5://三选一
                    Source[i] = Instantiate(ChoosePrefab, sourceIcons[i]);
                    break;
                default:
                    break;
            }
            if (room.Source[i]>=100)
            {
                //生成怪物（为方便后续迁移，另起一个函数）
                CreateEnemy(room.Source[i] - 100, sourceIcons[i]);//ID偏移100，从0开始计算
            }
        }
        //Debug.Log("房间"+room.id+"资源点显示完成");
        //Debug.Log("房间坐标为x:"+room.centerPos.x+",y:"+ room.centerPos.y);
    }

    //隐藏/显示所有资源（不会隐藏怪物）
    public void SourceSetActive(bool active)
    {
        foreach(var _source in Source)
        {
            if (_source != null)
            {
                _source.SetActive(active);
            }
        }
    }


    //创建房间门（这里固定房间大小为24）
    public void CreateDoor()
    {
        Vector3 vector3 = new Vector3(room.roomPos.x * 24 + 12, room.roomPos.y * 24, 0);
        Door = Instantiate(DoorPrefab, vector3, Quaternion.Euler(0, 0, 90));
    }

    //删除房间门
    public void DeleteDoor()
    {
        if (Door != null) 
        { Destroy(Door); }
        else
        { Debug.Log("没有找到房间门"); }
    }

    //找到玩家数据保存玩家当前位置
    public void SavePlayerPos()
    {
        //保存玩家基础数据
        GameObject.Find("DataManager").GetComponent<PlayerData>().SaveBaseData();
    }

    //更新门状态
    public void RefreshDoorState()
    {
        if (room.MainRoom && room.roomPos.x != 5)//最后一个房间不生成门
        {
            int progress = Global_PlayerData.progress;
            int roomNumber = room.roomPos.x;
            if (progress - 1 == roomNumber)//判断游戏进度决定是否需要开门
            {
                //Debug.Log("当前房间号为："+ roomNumber+",进度为"+ progress+",故开门");
                DeleteDoor();
            }
            else
            {
                //Debug.Log("当前房间号为：" + roomNumber + ",进度为" + progress + ",故关门");
                if (Door == null)
                {
                    CreateDoor();//没有门则创建门
                }
            }
        }
    }

    //创建敌人
    public void CreateEnemy(int number, Transform place)
    {
        int[] _enemies = new int[] { 0 };
        int _id = 0;
        switch (number)
        {
            case 0://恶魔1
                _enemies = new int[] { 0 };
                _id = 0;
                break;
            case 1://幽灵1
                _enemies = new int[] { 1 };
                _id = 1;
                break;
            case 2://小鸡战士1
                _enemies = new int[] { 2 };
                _id = 2;
                break;
            default:
                Debug.Log("未知敌人，默认创建单个恶魔");
                break;
        }
        //参数配置完毕后创建敌人
        GameObject Enemy = Instantiate(EnemyPrefab, place);
        Enemy.GetComponent<EnterBattle>().ImageId = _id;
        Enemy.GetComponent<EnterBattle>().enemies = _enemies;
        Enemy.GetComponent<EnterBattle>().currentId = number + 100;//传递对象的ID
        //有敌人被创建时，强制将房间的clean状态变为false
        room.Clear = false;
    }

}
