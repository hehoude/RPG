using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;//文件读写

public class NPC_Chat : MonoBehaviour
{
    [Header("提示箭头")]
    public GameObject Tip;
    [Header("激活对话id（999代表立即战斗）")]
    public int chatId;
    [Header("是否可能战斗")]
    public bool battle;
    [Header("战斗中的敌人编号集")]
    public int[] enemies;

    private string LoadSet = "Save";//数据加载文件夹位置

    //标记玩家是否在触发器范围内
    private bool isPlayerInTrigger = false;

    void Start()
    {
        //根据游戏模式决定加载目录
        switch (Global_PlayerData.Instance.model)
        {
            case 0://经典模式
                LoadSet = "Save";
                break;
            case 1://战役模式
                LoadSet = "War_Save";
                break;
        }
    }

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
            //提前传入战斗相关参数
            if(battle)
            {
                SavePlayerData();
            }
            //将自身传入ChatManager
            ChatManager.Instance.CurrnetTarget = gameObject;
            //对话管理器触发
            ChatManager.Instance.StartChat(chatId, 0);
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

    //传入战斗相关参数
    public void SavePlayerData()
    {
        // 定义保存路径
        string folderPath = Application.dataPath + "/Datas";
        string filePath = folderPath + "/" + LoadSet + "/BattleMessage.csv";

        // 如果文件夹不存在，创建文件夹
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("Datas 文件夹不存在，已创建：" + folderPath);
        }

        // 如果 CSV 文件不存在，先创建一个空文件（也可以不创建，File.WriteAllLines 会自动创建）
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose(); // 创建文件后释放资源
            Debug.Log("BattleMessage.csv 文件不存在，已创建：" + filePath);
        }

        // 准备写入内容
        List<string> datas = new List<string>();
        datas.Add("#,敌人编号");
        for (int i = 0; i < enemies.Length; i++)
        {
            datas.Add("enemy," + enemies[i].ToString());
        }

        // 写入 CSV
        File.WriteAllLines(filePath, datas);
        //Debug.Log("敌人信息写入完成：" + filePath);
    }
}
