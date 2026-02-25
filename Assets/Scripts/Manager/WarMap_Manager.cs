using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarMap_Manager : MonoBehaviour
{
    [Header("玩家进入地图的位置")]
    public Transform[] enterPlace;
    [Header("玩家对象")]
    public GameObject Player;
    // Start is called before the first frame update
    void Awake()
    {
        
    }
    void Start()
    {
        //修改玩家位置
        LoadPlayerPlace();
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
}
