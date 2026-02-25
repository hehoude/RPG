using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePlace : MonoBehaviour
{
    [Header("此场景代号")]
    public int oldScene;
    [Header("目标场景代号")]
    public int newScene;
    [Header("初始位置")]
    public int place;

    // 当玩家进入触发器范围时触发
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检测碰撞体是否是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            //预先设定玩家跳转后的初始位置
            Global_PlayerData.Instance.place = place;
            //切换全局变量中玩家地图ID
            Global_PlayerData.Instance.Map = newScene;
            //执行场景切换（这里很奇怪，场景中不用拖入SceneChanger单例也能正常执行）
            SceneChanger.Instance.ChangeMainScene(oldScene, newScene);
        }
    }
}
