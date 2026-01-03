using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterTransDoor : MonoBehaviour
{
    public GameObject Tip;//提示对象

    // 标记玩家是否在触发器范围内
    private bool isPlayerInTrigger = false;

    // 当玩家进入触发器范围时触发
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检测碰撞体是否是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            Tip.SetActive(true);
        }
    }

    // 当玩家离开触发器范围时触发
    private void OnTriggerExit2D(Collider2D collision)
    {
        // 检测碰撞体是否是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            Tip.SetActive(false);
        }
    }

    // 每帧检测按键（Update是Unity默认的帧更新函数）
    private void Update()
    {
        // 条件：玩家在触发器内 + 按下F键
        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.F))
        {
            if (MapManager.Instance != null)
            {
                //传递地图管理器（单例）到下一层级
                MapManager.Instance.NextFloor();
            }
        }
    }

    // 可选：场景切换时清理状态（防止异常）
    private void OnDestroy()
    {
        isPlayerInTrigger = false;
        Tip.SetActive(false);
    }
}

