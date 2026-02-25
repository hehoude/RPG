using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Chat : MonoBehaviour
{
    [Header("提示箭头")]
    public GameObject Tip;
    [Header("激活对话")]
    public int chatId;

    //标记玩家是否在触发器范围内
    private bool isPlayerInTrigger = false;

    //当玩家进入触发器范围时触发
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检测碰撞体是否是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            Tip.SetActive(true);
        }
    }

    //当玩家离开触发器范围时触发
    private void OnTriggerExit2D(Collider2D collision)
    {
        //检测碰撞体是否是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            Tip.SetActive(false);
        }
    }

    //每帧检测按键（Update是Unity默认的帧更新函数）
    private void Update()
    {
        // 条件：玩家在触发器内 + 按下F键
        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.F))
        {
            //触发逻辑
            //对话管理器触发
            ChatManager.Instance.StartChat(chatId);
            //防止重复触发
            isPlayerInTrigger = false;
        }
    }

    //场景切换时清理状态（防止异常）
    private void OnDestroy()
    {
        isPlayerInTrigger = false;
        Tip.SetActive(false);
    }
}
