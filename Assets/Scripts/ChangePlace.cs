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

    // 标记玩家是否在触发器范围内
    private bool isPlayerInTrigger = false;

    // 当玩家进入触发器范围时触发
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检测碰撞体是否是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            SetAlpha(50);
        }
    }

    // 当玩家离开触发器范围时触发
    private void OnTriggerExit2D(Collider2D collision)
    {
        // 检测碰撞体是否是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            SetAlpha(0);
        }
    }

    // 每帧检测按键（Update是Unity默认的帧更新函数）
    private void Update()
    {
        // 条件：玩家在触发器内 + 按下F键
        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.F))
        {
            //判断当前游戏模式
            if (Global_PlayerData.Instance.model == 0)
            {
                //传递地图管理器（单例）当前交互的对象
                MapManager.Instance.CurrentObject = this.gameObject;
            }
            //预先设定玩家跳转后的初始位置
            Global_PlayerData.Instance.place = place;
            //切换全局变量中玩家地图ID
            Global_PlayerData.Instance.Map = newScene;
            //执行场景切换
            SceneChanger.Instance.ChangeMainScene(oldScene, newScene);
        }
    }

    private void SetAlpha(byte alpha)
    {
        if (TryGetComponent<SpriteRenderer>(out var sr))
        {
            Color color = sr.color;
            color.a = alpha / 255f;
            sr.color = color;
        }
    }
}
