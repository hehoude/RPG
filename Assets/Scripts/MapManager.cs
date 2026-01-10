using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

// 房间开口状态类
public class RoomOpenState
{
    public bool top;
    public bool bottom;
    public bool left;
    public bool right;

    public RoomOpenState()
    {
        top = false;
        bottom = false;
        left = false;
        right = false;
    }
}

public class MapManager : MonoSingleton<MapManager>//单例后更容易访问
{
    [Header("围墙生成器引用")]
    public GameObject WallMap; // 围墙地图对象
    private Tilemap_Wall wallGenerator; // 引用围墙生成脚本

    [Header("地面生成器引用")]
    public GameObject GroundMap; // 地面地图对象
    private Tilemap_Ground groundGenerator; // 引用地面生成脚本

    [Header("树木生成器引用")]
    public GameObject TreeMap; // 树木地图对象
    private Tilemap_Tree treeGenerator; // 引用树木生成脚本

    [Header("房间引用")]
    public GameObject RoomPrefab; // 房间预制体引用
    public Transform Rooms; // 房间父物体引用

    [Header("地图设置")]
    public int baseRoomCount = 6; // 基础直线房间数
    public int roomInterval = 24; // 房间之间的间隔坐标
    public int extendRange = 1; // 延伸的格子数（固定为1）

    [Header("额外房间数量限制")]
    public int minExtraRooms = 2; // 额外房间下限
    public int maxExtraRooms = 4; // 额外房间上限

    [Header("其它管理器")]
    //public GameObject DataManager;//从数据管理器获取玩家数据
    private PlayerData PlayerData;
    private Global_PlayerData Global_PlayerData;

    public GameObject Player;//玩家对象

    public Text SeedCount;//种子编号显示文本

    public GameObject CurrentObject;//当前交互对象

    private Dictionary<Vector2Int, Vector2Int> roomCenterPositions; // 所有房间的中心点物理坐标（键为逻辑坐标、内容为物理坐标）
    private List<Vector2Int> allRooms; // 所有房间的逻辑坐标
    private Dictionary<Vector2Int, RoomOpenState> roomOpenStates; // 房间开口状态容器（键为逻辑坐标、内容为开口状态类）
    private List<Vector2Int> candidateRooms; // 候选延伸的基础房间（x=1~4）
    private Dictionary<Vector2Int, List<int>> roomSourceData;//存储每个房间的emptySource数据（键为房间坐标）
    private Dictionary<Vector2Int, GameObject> roomObjects;//存储所有房间的引用（使用逻辑坐标）

    public int seed;//随机种子

    protected override void Awake()
    {
        Global_PlayerData = Global_PlayerData.Instance;
        base.Awake();
        // 初始化数据结构
        roomCenterPositions = new Dictionary<Vector2Int, Vector2Int>();
        allRooms = new List<Vector2Int>();
        roomOpenStates = new Dictionary<Vector2Int, RoomOpenState>();
        candidateRooms = new List<Vector2Int>();
        roomSourceData = new Dictionary<Vector2Int, List<int>>();
        roomObjects = new Dictionary<Vector2Int, GameObject>();
        //获取地图生成器
        GetGrid();
        //获取玩家数据脚本
        PlayerData = PlayerData.Instance;
    }

    void Start()
    {
        GameStart(); //执行一次游戏初始化
    }

    //void OnEnable()
    //{
    //    //当脚本PlayerData的DataLoaded事件被触发后，执行OnScriptADataLoaded
    //    PlayerData.PlayerLoaded += OnDataLoaded;
    //}

    //void OnDataLoaded()
    //{
    //    GameStart(); //执行一次游戏初始化
    //    PlayerData.PlayerLoaded -= OnDataLoaded;//取消事件订阅释放内存
    //}

    //游戏初始化
    public void GameStart()
    {
        LoadPlayerPlace();//初始化玩家位置
        //判断是新游戏还是读取存档
        if (Global_PlayerData.newGame)
        {
            //随机新的种子（修改全局种子）
            Global_PlayerData.seed = Random.Range(int.MinValue, int.MaxValue);
            CreateNewRandomMap();//新游戏生成新地图
            PlayerData.SaveBaseData();//让PlayerData保存种子
        }
        else
        {
            //读取存档使用的种子（种子已经被PlayerData加载）
            seed = Global_PlayerData.seed + Global_PlayerData.floor * 1000;
            LoadMap();//读取存档加载旧地图
        }
        SeedCount.text = "种子编号: " + Global_PlayerData.seed.ToString();//显示种子编号
        Debug.Log("地图初始化完成，种子编号为"+Global_PlayerData.seed);
    }

    //获取地图生成器
    public void GetGrid()
    {
        // 获取围墙生成脚本引用
        if (WallMap != null)
        {
            wallGenerator = WallMap.GetComponent<Tilemap_Wall>();
        }
        else
        {
            Debug.LogError("未设置WallMap对象引用！");
        }
        // 获取地面生成脚本引用
        if (GroundMap != null)
        {
            groundGenerator = GroundMap.GetComponent<Tilemap_Ground>();
        }
        else
        {
            Debug.LogError("未设置WallMap对象引用！");
        }
        // 获取树木生成脚本引用
        if (TreeMap != null)
        {
            treeGenerator = TreeMap.GetComponent<Tilemap_Tree>();
        }
        else
        {
            Debug.LogError("未设置TreeMap对象引用！");
        }
    }

    /// <summary>
    /// 创建新的随机地图（主函数）
    /// </summary>
    /// <param name="seed">随机种子</param>
    public void CreateNewRandomMap()
    {
        //种子关联层级
        seed = Global_PlayerData.seed + Global_PlayerData.floor * 1000;
        // 设置随机种子
        Random.InitState(seed);

        // 生成基础直线房间
        GenerateBaseLineRooms();
        // 生成随机延伸房间（满足数量限制）
        GenerateRandomExtendRooms(seed);
        // 计算房间中心点坐标
        CalculateRoomCenterPositions();
        // 处理房间之间的开口状态
        ProcessRoomOpenStates();

        // 生成每个房间的资源数据（获取roomSourceData）
        PreGenerateAllRoomSourceData();

        // 先保存资源数据到文件
        SaveSourceData();

        // 在以上资源都准备妥当后，才能一次性生成所有房间的内容
        GenerateAllRoomWallsOnce(seed);
    }

    //加载旧地图
    public void LoadMap()
    {
        // 设置随机种子
        Random.InitState(seed);

        // 生成基础直线房间
        GenerateBaseLineRooms();
        // 生成随机延伸房间（满足数量限制）
        GenerateRandomExtendRooms(seed);
        // 计算房间中心点坐标
        CalculateRoomCenterPositions();
        // 处理房间之间的开口状态
        ProcessRoomOpenStates();

        // 加载资源数据到字典
        LoadSourceData();
        // 在以上资源都准备妥当后，才能一次性生成所有房间的内容
        GenerateAllRoomWallsOnce(seed);
    }

    /// <summary>
    /// 生成基础直线房间（最左为起点(0,0)，最右为终点(5,0)）
    /// </summary>
    private void GenerateBaseLineRooms()
    {
        for (int x = 0; x < baseRoomCount; x++)
        {
            Vector2Int roomPos = new Vector2Int(x, 0);
            allRooms.Add(roomPos);// 添加基础房间
            roomOpenStates[roomPos] = new RoomOpenState();//将开口状态类加入字典
            if (x >= 1 && x <= baseRoomCount - 2) // 如果是中间房间
            {
                candidateRooms.Add(roomPos);// 添加到延伸候选列表
            }
        }
    }

    /// <summary>
    /// 为中间房间生成随机上下延伸（满足数量限制）
    /// </summary>
    /// <param name="seed">随机种子</param>
    private void GenerateRandomExtendRooms(int seed)
    {
        //Random.InitState(seed);
        // 使用System.Random创建局部随机数生成器，避免全局干扰
        System.Random random = new System.Random(seed);
        List<Vector2Int> remainingCandidates = new List<Vector2Int>(candidateRooms); // 剩余候选房间
        List<Vector2Int> extendedRooms = new List<Vector2Int>(); // 已生成的额外房间

        // 第一步：随机生成初始延伸房间（不考虑数量限制）
        // 遍历候选房间列表，尝试随机延伸
        for (int i = remainingCandidates.Count - 1; i >= 0; i--)
        {
            Vector2Int baseRoom = remainingCandidates[i];
            int extendDir = Random.Range(-1, 2); // -1:下，0:不延伸，1:上
            if (extendDir != 0)//随机成功
            {
                Vector2Int extendRoom = new Vector2Int(baseRoom.x, extendDir * extendRange);
                extendedRooms.Add(extendRoom);
                remainingCandidates.RemoveAt(i);
            }
        }

        // 第二步：调整数量到下限
        while (extendedRooms.Count < minExtraRooms && remainingCandidates.Count > 0)
        {
            int randIdx = random.Next(0, remainingCandidates.Count); // 固定调用
            Vector2Int baseRoom = remainingCandidates[randIdx];
            int extendDir = random.Next(-1, 2); // 固定调用
            if (extendDir == 0) extendDir = random.Next(0, 2) * 2 - 1; // 若为0，再调用一次（固定逻辑）
            Vector2Int extendRoom = new Vector2Int(baseRoom.x, extendDir * extendRange);
            extendedRooms.Add(extendRoom);
            remainingCandidates.RemoveAt(randIdx);
        }

        // 第三步：裁剪数量到上限
        if (extendedRooms.Count > maxExtraRooms)
        {
            int removeCount = extendedRooms.Count - maxExtraRooms;
            // 为了固定随机数调用次数，先预先生成所有要移除的索引（排序后从后往前删，避免顺序影响）
            List<int> removeIdxs = new List<int>();
            for (int i = 0; i < removeCount; i++)
            {
                removeIdxs.Add(random.Next(0, extendedRooms.Count - i)); // 固定调用
            }
            // 排序索引（从大到小删，避免索引偏移）
            removeIdxs.Sort((a, b) => b.CompareTo(a));
            foreach (int idx in removeIdxs)
            {
                if (idx < extendedRooms.Count)
                {
                    extendedRooms.RemoveAt(idx);
                }
            }
        }

        // 第四步：添加最终的延伸房间到总列表
        foreach (Vector2Int extendRoom in extendedRooms)
        {
            if (!allRooms.Contains(extendRoom))//避免重复添加
            {
                allRooms.Add(extendRoom);//添加延伸房间
                roomOpenStates[extendRoom] = new RoomOpenState();
            }
            Vector2Int baseRoom = new Vector2Int(extendRoom.x, 0);
            //在字典中查找到对应基础房间，设置开口状态
            if (extendRoom.y > 0) roomOpenStates[baseRoom].top = true;
            else roomOpenStates[baseRoom].bottom = true;
        }
    }

    /// <summary>
    /// 计算所有房间的中心点坐标
    /// </summary>
    private void CalculateRoomCenterPositions()
    {
        foreach (Vector2Int room in allRooms)
        {
            int centerX = room.x * roomInterval;
            int centerY = room.y * roomInterval;
            roomCenterPositions[room] = new Vector2Int(centerX, centerY);
        }
    }

    /// <summary>
    /// 处理房间之间的开口状态
    /// </summary>
    private void ProcessRoomOpenStates()
    {
        // 处理基础直线房间的左右开口
        for (int x = 0; x < baseRoomCount; x++)
        {
            Vector2Int room = new Vector2Int(x, 0);
            if (x > 0) // 不是最左房间，左开口为true
            {
                roomOpenStates[room].left = true;
                Vector2Int leftRoom = new Vector2Int(x - 1, 0);
                roomOpenStates[leftRoom].right = true;
            }
            if (x < baseRoomCount - 1) // 不是最右房间，右开口为true
            {
                roomOpenStates[room].right = true;
                Vector2Int rightRoom = new Vector2Int(x + 1, 0);
                roomOpenStates[rightRoom].left = true;
            }
        }

        // 处理延伸房间的上下开口
        foreach (Vector2Int room in allRooms)
        {
            if (room.y != 0) // 是延伸房间
            {
                Vector2Int baseRoom = new Vector2Int(room.x, 0);
                if (room.y > 0) // 向上延伸
                {
                    roomOpenStates[room].bottom = true;
                }
                else // 向下延伸
                {
                    roomOpenStates[room].top = true;
                }
            }
        }
    }

    /// <summary>
    /// 一次性生成所有房间的内容
    /// </summary>
    private void GenerateAllRoomWallsOnce(int seed)
    {
        if (wallGenerator == null)
        {
            Debug.LogError("未引用Tilemap_Wall脚本！");
            return;
        }

        // 清空原有地图
        wallGenerator.targetTilemap.ClearAllTiles();
        groundGenerator.targetTilemap.ClearAllTiles();
        treeGenerator.targetTilemap.ClearAllTiles();
        // 先销毁父物体下已有的房间预制体（避免重复生成）
        if (Rooms != null)
        {
            for (int i = Rooms.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(Rooms.GetChild(i).gameObject);
            }
        }
        int roomIdCounter = 0;//房间ID计数器
        foreach (Vector2Int room in allRooms)//遍历逻辑坐标
        {
            //Random.InitState(seed);// 初始化Unity随机数种子
            //尝试获取中心点物理坐标
            if (roomCenterPositions.TryGetValue(room, out Vector2Int center))
            {
                //获取房间开口状态
                RoomOpenState openState = roomOpenStates[room];
                //生成围墙
                wallGenerator.GenerateWall(
                    center.x,
                    center.y,
                    openState.top,
                    openState.bottom,
                    openState.left,
                    openState.right
                );
                //生成地面
                groundGenerator.GenerateNoiseMap(
                    _seed: seed,
                    _cenx: center.x,
                    _ceny: center.y
                );
                //生成树木
                treeGenerator.GenerateNoiseMap(
                    _seed: seed,
                    _cenx: center.x,
                    _ceny: center.y
                );
                //获取房间的资源数据
                List<int> emptyList = roomSourceData[room];
                //创建房间数据实例（前六个是主房间）
                Room newRoom_type = new Room(roomIdCounter, room, (roomIdCounter < 6), emptyList);
                //实例化房间预制体（对象、坐标、旋转、父对象）
                GameObject newRoom = Instantiate(RoomPrefab, new Vector3(center.x, center.y, 0), Quaternion.identity, Rooms);
                //立刻赋值房间数据
                newRoom.GetComponent<RoomDisplay>().room = newRoom_type;
                //存储到房间列表中（方便后续寻址）
                roomObjects[room] = newRoom;
            }
            roomIdCounter++;
        }
    }

    /// /// <summary>
    /// 生成房间的source数据（按房间坐标为键）
    /// </summary>
    public void PreGenerateAllRoomSourceData()
    {
        foreach (Vector2Int room in allRooms)//遍历逻辑坐标
        {
            int roomSeed = seed + 100 * room.x + room.y + 10 * Global_PlayerData.floor; // 房间专属种子
            List<int> roomSource;
            if (room.x == 0 && room.y == 0)//如果是起始房间（第一个主房间）
            {
                roomSource = new List<int>() { 0, 0, 0, 0, 0, 5, 0, 0, 0 };//右边为三选一
            }
            else if (room.x == 3 && room.y == 0)//如果是商店房间（第四个主房间）
            {
                roomSource = RandomShopSource(roomSeed);
            }
            else if (room.x == 5 && room.y == 0)//如果是BOSS房间（第六个主房间）
            {
                roomSource = RandomBossSource(roomSeed);
            }
            else
            {
                roomSource = RandomSourceData(roomSeed);//生成资源数据
            }
            roomSourceData[room] = roomSource;//放入字典
        }
    }

    // 生成随机的资源数据
    private List<int> RandomSourceData(int seed)
    {
        // 初始化默认列表
        List<int> sourcePlace = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        // 设置随机种子（可选，若需按房间定制种子）
        Random.InitState(seed);
        // 随机选索引并修改值
        //放置资源
        int randomIndex1 = Random.Range(0, sourcePlace.Count);
        while (randomIndex1 == 5)//5号位是怪物的位置
        {
            randomIndex1 = Random.Range(0, sourcePlace.Count);
        }
        int randomValue1 = Random.Range(1, 4);//1火堆、2删卡、3宝箱（这里不加商店）
        sourcePlace[randomIndex1] = randomValue1;
        //放置怪物
        int randomIndex2 = 5;
        int randomValue2 = RandomEnemy();
        sourcePlace[randomIndex2] = randomValue2;
        return sourcePlace;
    }
    //商店房间加载

    private List<int> RandomShopSource(int seed)
    {
        // 设置随机种子（可选，若需按房间定制种子）
        Random.InitState(seed+100);
        int randomValue = RandomEnemy();
        List<int> source = new List<int>() { 0, 0, 0, 0, 4, randomValue, 0, 0, 0 };//中间为商店
        return source;
    }

    //boss房间加载
    private List<int> RandomBossSource(int seed)
    {
        // 设置随机种子（可选，若需按房间定制种子）
        Random.InitState(seed + 200);
        int randomValue = RandomBoss();
        List<int> source = new List<int>() { 0, 0, 0, 0, 6, randomValue, 0, 0, 0 };//中间为传送门
        return source;
    }

    //保存资源数据到csv表格
    public void SaveSourceData()
    {
        string path = Application.dataPath + "/Datas/MapSource.csv";
        //预存到datas里
        List<string> datas = new List<string>();
        //遍历每个房间的source数据
        foreach (var kvp in roomSourceData)
        {
            Vector2Int roomPos = kvp.Key;//键值为逻辑坐标
            List<int> source = kvp.Value;//内容为source数据
            //构建CSV行
            string line = $"{roomPos.x},{roomPos.y}";
            foreach (int val in source)
            {
                line += $",{val}";
            }
            datas.Add(line);
        }
        //保存数据
        File.WriteAllLines(path, datas);
    }

    //加载资源数据
    public void LoadSourceData()
    {
        string fullPath = Application.dataPath + "/Datas/MapSource.csv";
        //清空字典
        roomSourceData.Clear();
        //读取所有行
        string fileContent = File.ReadAllText(fullPath, System.Text.Encoding.UTF8);//转换为字符串
        string[] dataRow = fileContent.Split('\n');//拆分
        foreach (var row in dataRow)
        {
            string cleanRow = row.Trim();// 去除首尾的空格、\r、\n、制表符等
            string[] rowArray = cleanRow.Split(',');
            if (rowArray.Length < 3)
            {
                continue;//跳过无效行
            }
            //解析逻辑坐标
            int x = int.Parse(rowArray[0]);
            int y = int.Parse(rowArray[1]);
            Vector2Int roomPos = new Vector2Int(x, y);
            //解析source数据
            List<int> source = new List<int>();
            for (int i = 2; i < rowArray.Length; i++)
            {
                source.Add(int.Parse(rowArray[i]));
            }
            //存入字典
            roomSourceData[roomPos] = source;
        }

    }

    //删除指定房间的指定资源点
    public void DeleteRoomSource(Vector2Int roomPos, int sourceIndex)
    {
        //根据逻辑坐标访问字典中的资源数据
        if (roomSourceData.TryGetValue(roomPos, out List<int> source))
        {
            if (sourceIndex > 0)//如果不为空格
            {
                foreach (int val in source)//遍历资源点
                {
                    if (val == sourceIndex)//找到对应资源点
                    {
                        int idx = source.IndexOf(val);//获取索引
                        source[idx] = 0; // 将指定资源点设为0（空）
                        break;
                    }
                }
                roomSourceData[roomPos] = source; // 更新字典
                SaveSourceData(); // 保存更新后的数据
            }
        }
        else
        {
            Debug.LogWarning("未找到指定房间的资源数据！");
        }
    }

    //根据全局逻辑坐标加载玩家位置
    public void LoadPlayerPlace()
    {
        Player.transform.position = 
            new Vector3(Global_PlayerData.currentRoom.x * roomInterval, 
            Global_PlayerData.currentRoom.y * roomInterval, 0);
        //Debug.Log("地图加载完成，玩家坐标更改为x:"+ Global_PlayerData.currentRoom.x * roomInterval + ",y:" + Global_PlayerData.currentRoom.y * roomInterval);
    }

    //删除当前交互对象（SourceId为资源代号）
    public void DeleteCurrentObject(int SourceId)
    {
        //显示层删除
        if (CurrentObject != null)
        {
            Destroy(CurrentObject);
            CurrentObject = null;
        }
        //赋值给RoomDisplay的数据没必要删除，因为这个数据只会在地图生成时使用，只需要把roomSourceData中的删除就行
        //数据层删除
        Vector2Int playerRoom = Global_PlayerData.currentRoom;//获取玩家所在房间逻辑坐标
        List<int> newSource = roomSourceData[playerRoom];//以逻辑坐标获取当前房间资源列表
        //找到符合SourceId的元素将其变为0 
        for (int i = 0; i < newSource.Count; i++)
        {
            if (newSource[i] == SourceId)
            {
                newSource[i] = 0;
                break;
            }
        }
        roomSourceData[playerRoom] = newSource;//存回字典
        SaveSourceData();//保存到表格
    }

    //击杀怪物后
    public void RoomClean()
    {
        GameObject room = roomObjects[Global_PlayerData.currentRoom];//获取当前房间对象
        //查找该房间是否还有敌人
        bool Clean = true;
        List<int> newSource = roomSourceData[Global_PlayerData.currentRoom];
        for (int i = 0; i < newSource.Count; i++)
        {
            if (newSource[i] >= 100)
            {
                Clean = false;
                break;
            }
        }
        if (Clean)
        {
            //判断是否为主线房间
            if (room.GetComponent<RoomDisplay>().room.MainRoom)
            {
                //Debug.Log("主线房间已清空");
                ProgressGame();//主线房间被清空则推进游戏进程
                if (room.GetComponent<RoomDisplay>().room.roomPos.x == 5)
                {
                    room.GetComponent<RoomDisplay>().SourceSetActive(true);//最终房间显示传送门
                }
            }
            else
            {
                //Debug.Log("支线房间已清空");
                room.GetComponent<RoomDisplay>().SourceSetActive(true);//显示支线房间内资源
            }
        }
        
    }

    //游戏推进函数（在拾取初始武器、招募同伴、打败看门敌人时，都会调用这个推进函数）
    public void ProgressGame()
    {
        Global_PlayerData.progress += 1;//进度加1
        //Debug.Log("游戏进程推进，当前进程为："+ Global_PlayerData.progress);
        //更新每个房间的门状态
        foreach (GameObject room in roomObjects.Values)//遍历字典的值
        {
            room.GetComponent<RoomDisplay>().RefreshDoorState();//更新它们的门状态
        }
        PlayerData.SavePlayerData();//保存所有数据
    }

    //层级推进
    public void NextFloor()
    {
        Global_PlayerData.progress = 0;//进度清零
        Global_PlayerData.floor += 1;//层级+1
        //房间数据重新初始化
        roomCenterPositions = new Dictionary<Vector2Int, Vector2Int>();
        allRooms = new List<Vector2Int>();
        roomOpenStates = new Dictionary<Vector2Int, RoomOpenState>();
        candidateRooms = new List<Vector2Int>();
        roomSourceData = new Dictionary<Vector2Int, List<int>>();
        roomObjects = new Dictionary<Vector2Int, GameObject>();
        //重置地图
        CreateNewRandomMap();
        //重置玩家位置
        Global_PlayerData.currentRoom.x = 0;
        Global_PlayerData.currentRoom.y = 0;
        LoadPlayerPlace();//初始化玩家位置
        //保存数据
        PlayerData.SaveBaseData();
    }

    //根据层级获取随机的普通敌人
    public int RandomEnemy()
    {
        int _enemy = 100;//默认为恶魔
        switch (Global_PlayerData.floor)
        {
            case 0:
            case 1:
                _enemy = Random.Range(100, 107);
                break;
        }
        return _enemy;
    }

    //随机BOSS
    public int RandomBoss()
    {
        int _boss = 1000;//默认为
        switch (Global_PlayerData.floor)
        {
            case 0:
            case 1:
                _boss = Random.Range(1000, 1001);
                break;
        }
        return _boss;
    }

}