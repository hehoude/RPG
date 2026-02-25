using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Unity.VisualScripting;

[System.Serializable]
public class DialogueEntry
{
    public string speakerName; // 说话者名字
    public string message;     // 对话内容
}

[System.Serializable]
public class DialogueData
{
    public List<DialogueEntry> dialogues; // 对话列表
}

public class ChatManager : MonoSingleton<ChatManager>
{
    public GameObject ChatWindow; // 对话框
    public Text Name; // 名字文本
    public Text Message; // 对话文本
    public GameObject Player; //玩家对象

    private DialogueData currentDialogueData; // 当前加载的对话数据
    private int currentDialogueIndex = 0; // 当前对话索引

    void Start()
    {
        // 初始化：添加空引用检查，避免启动时报错
        if (ChatWindow != null)
        {
            ChatWindow.SetActive(false);
        }
        else
        {
            Debug.LogWarning("请在Inspector面板绑定ChatWindow对象！");
        }
    }

    void Update()
    {
        // 只有对话框显示时，才响应鼠标点击推进对话
        if (ChatWindow != null && ChatWindow.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                AdvanceDialogue();
            }
        }
    }

    /// <summary>
    /// 外部调用入口：根据对话序号启动对应对话
    /// </summary>
    /// <param name="chatId">对话文件序号（0=chat0.json，1=chat1.json...）</param>
    public void StartChat(int chatId)
    {
        if (Player != null)
        {
            //使用局部暂停方法暂停玩家动作
            Player.GetComponent<Player>().playerStop = true;
        }

        // 重置对话索引
        currentDialogueIndex = 0;

        // 加载对应序号的对话文件
        if (LoadDialogueFromJson(chatId))
        {
            // 加载成功，显示对话框并显示第一条对话
            if (ChatWindow != null)
            {
                ChatWindow.SetActive(true);
                ShowCurrentDialogue();
            }
        }
        else
        {
            Debug.LogError("启动对话失败，无法加载chat" + chatId + ".json");
        }
    }

    /// <summary>
    /// 动态加载指定序号的JSON对话文件
    /// </summary>
    /// <param name="chatId">对话文件序号</param>
    /// <returns>是否加载成功</returns>
    private bool LoadDialogueFromJson(int chatId)
    {
        // 拼接完整文件路径（Assets/Chats/chatX.json）
        string jsonFilePath = (Application.dataPath + "/Chats/" + "chat" + chatId + ".json");

        try
        {
            // 检查文件是否存在
            if (!File.Exists(jsonFilePath))
            {
                Debug.LogError("找不到JSON文件：" + jsonFilePath);
                return false;
            }

            // 读取JSON文件（指定UTF8编码避免中文乱码）
            string jsonContent = File.ReadAllText(jsonFilePath, System.Text.Encoding.UTF8);

            // 解析JSON数据
            currentDialogueData = JsonUtility.FromJson<DialogueData>(jsonContent);

            // 验证数据有效性
            if (currentDialogueData == null || currentDialogueData.dialogues == null || currentDialogueData.dialogues.Count == 0)
            {
                Debug.LogError("chat" + chatId + ".json 数据为空或格式错误");
                return false;
            }

            Debug.Log("成功加载chat" + chatId + ".json，共" + currentDialogueData.dialogues.Count + "条对话");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("加载JSON失败：" + e.Message);
            return false;
        }
    }

    /// <summary>
    /// 推进对话逻辑
    /// </summary>
    private void AdvanceDialogue()
    {
        // 检查对话数据和UI是否有效
        if (currentDialogueData == null || ChatWindow == null || Name == null || Message == null)
        {
            return;
        }

        // 索引自增，准备下一条对话
        currentDialogueIndex++;

        // 检查是否还有后续对话
        if (currentDialogueIndex < currentDialogueData.dialogues.Count)
        {
            // 显示当前对话
            ShowCurrentDialogue();
        }
        else
        {
            // 对话结束：隐藏对话框，重置数据
            ChatWindow.SetActive(false);
            currentDialogueIndex = 0;
            currentDialogueData = null; // 清空当前对话数据
            //Debug.Log("对话结束，已自动关闭对话框");
            if (Player != null)
            {
                Player.GetComponent<Player>().playerStop = false;//释放玩家动作
            }
        }
    }

    /// <summary>
    /// 显示当前索引的对话内容
    /// </summary>
    private void ShowCurrentDialogue()
    {
        if (currentDialogueData == null || currentDialogueIndex >= currentDialogueData.dialogues.Count)
        {
            return;
        }

        // 获取当前对话条目并更新UI
        DialogueEntry currentEntry = currentDialogueData.dialogues[currentDialogueIndex];
        Name.text = currentEntry.speakerName;
        Message.text = currentEntry.message;
    }
}