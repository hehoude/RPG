using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarMap_Manager : MonoBehaviour
{
    [Header("玩家进入地图的位置")]
    public Transform[] enterPlace;
    [Header("玩家对象")]
    public GameObject Player;
    [Header("地图资源表")]
    public GameObject[] Resource;
    //资源存在情况
    private List<int> Source;
    // Start is called before the first frame update
    void Awake()
    {
        
    }
    void Start()
    {
        //将自身赋予对话管理器
        ChatManager.Instance.CurrentMapManager = this;
        //修改玩家位置
        LoadPlayerPlace();
        //加载地图资源
        LoadMapSource();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //修改加载玩家位置
    public void LoadPlayerPlace()
    {
        int placeId = Global_PlayerData.Instance.place;
        //从全局变量中读取位置，赋予玩家进入位置
        Player.transform.position = enterPlace[placeId].position;
    }

    //加载地图资源
    public void LoadMapSource()
    {
        Source = new List<int>();
        //后续要从存档加载，现在先占位
        for (int i = 0;i< Resource.Length;i++)
        {
            Source.Add(1);//生成同等长度的表
        }
    }

    //检查当前地图资源
    public void CheckMapSource()
    {
        //遍历资源，若资源被删除，则存档数组相应修改
        for (int i = 0; i < Resource.Length; i++)
        {
            if (Resource[i] != null)
            {
                Source[i] = 1;
            }
            else
            {
                Source[i] = 0;
            }
        }
    }
}
