using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement; // 新增：引入场景管理命名空间
using UnityEngine.UI;

[System.Serializable]//加了这个属性才能正常解析
public class DialogueEntry
{
    public string speakerName; // 说话者名字
    public string message;     // 对话内容
}

[System.Serializable]
public class Jump
{
    public int target;//目标对话
    public string btn;//按钮内容
}

[System.Serializable]
public class DialogueData
{
    public List<DialogueEntry> dialogues; // 对话列表
    public List<Jump> jumps;// 后续跳转对话（可选）
}

public class ChatManager : MonoSingleton<ChatManager>
{
    public GameObject ChatWindow; // 对话框
    public Text Name; // 名字文本
    public Text Message; // 对话文本
    [Tooltip("优先使用手动绑定的Player对象，若未绑定则自动查找tag为Player的对象")]
    private GameObject Player; //玩家对象

    private DialogueData currentDialogueData; // 当前加载的对话数据
    private int currentDialogueIndex = 0; // 当前对话索引
    private bool isPlayerFound = false; // 标记是否已找到Player对象
    public int chatActionType = 0;//对话完成后执行的内容类型 
    private bool ChatLock = false;//打开后阻止推进对话

    //选择按钮及其文本
    public GameObject Button1;
    public GameObject Button2;
    public GameObject Button3;
    public Text BtnText1;
    public Text BtnText2;
    public Text BtnText3;
    private Button Btn1;
    private Button Btn2;
    private Button Btn3;

    //当前对象（后续删除对象或操作其参数用）
    public GameObject CurrentTarget;

    //当前地图管理器（每个管理器Start方法时，将自身传入这个参数中）
    public WarMap_Manager CurrentMapManager;

    //
    protected override void Awake()
    {
        useDontDestroyOnLoad = false;
        base.Awake();
    }

    void Start()
    {
        // 初始化：添加空引用检查，避免启动时报错
        if (ChatWindow != null)
        {
            GetButton();
            ChatWindow.SetActive(false);
        }
        else
        {
            Debug.LogWarning("请在Inspector面板绑定ChatWindow对象！");
        }

        // 启动时先尝试查找Player
        FindPlayerObject();
    }

    void OnEnable()
    {
        // 注册场景加载事件：场景切换完成后自动重置Player查找状态
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 注销事件，避免内存泄漏
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        // 如果还没找到Player，每帧尝试查找
        if (!isPlayerFound)
        {
            FindPlayerObject();
        }

        // 只有对话框显示且不在按钮选择时，才响应鼠标点击推进对话
        if (ChatWindow != null && ChatWindow.activeSelf && !ChatLock)
        {
            if (Input.GetMouseButtonDown(0))
            {
                AdvanceDialogue();
            }
        }
    }

    // 场景加载完成时的回调函数
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 场景切换后重置Player查找状态，重新查找
        ResetPlayerStatus();
        //Debug.Log($"场景{scene.name}加载完成，已重置Player查找状态");
    }

    // 重置Player相关状态（场景切换时调用）
    public void ResetPlayerStatus()
    {
        Player = null; // 清空旧的Player引用
        isPlayerFound = false; // 标记为未找到
    }

    // 由于Player不是单例，所以需要持续查找并绑定Player对象（tag为Player）
    private void FindPlayerObject()
    {
        // 如果已经手动绑定了Player，直接标记为找到
        if (Player != null)
        {
            isPlayerFound = true;
            return;
        }

        // 查找场景中tag为Player的第一个对象
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
        if (foundPlayer != null)
        {
            Player = foundPlayer;
            isPlayerFound = true;
            //Debug.Log("成功自动找到Player对象：" + foundPlayer.name);
        }
    }

    //外部调用入口：根据对话序号启动对应对话
    public void StartChat(int chatId)
    {
        // 确保Player已找到后再暂停
        if (isPlayerFound && Player != null)
        {
            //使用局部暂停方法暂停玩家动作
            Player.GetComponent<Player>().playerStop = true;
        }
        else
        {
            Debug.LogWarning("启动对话时未找到Player对象，无法暂停玩家动作！");
        }

        //隐藏选择按钮
        Button1.SetActive(false);
        Button2.SetActive(false);
        Button3.SetActive(false);

        //检查是否特殊功能
        if (999 == chatId)
        {
            //找到场景切换器，切换至战斗场景（战斗信息由NPC负责传入）
            SceneChanger.Instance.GetBattle();
            //隐藏对话窗口
            ChatWindow.SetActive(false);
            return;
        }
        if (995 == chatId)
        {
            //找到场景切换器，切换至选择场景
            SceneChanger.Instance.GetChoose();
            //隐藏对话窗口
            ChatWindow.SetActive(false);
            return;
        }
        if (990 == chatId)
        {
            //释放玩家动作
            if (Player != null)
            {
                //使用局部暂停方法暂停玩家动作
                Player.GetComponent<Player>().playerStop = false;
            }
            //隐藏对话窗口
            ChatWindow.SetActive(false);
            return;
        }
        if (970<=chatId && 990>chatId)
        {
            //推进状态的特殊对话
            //Debug.Log("检测到推进状态的对话");
            SceneOver(chatId - 970);
            return;
        }

        // 重置对话索引
        currentDialogueIndex = 0;

        // 加载对应序号的对话文件
        if (LoadDialogueFromJson(chatId))
        {
            // 加载成功，显示对话框并显示第一条对话
            if (ChatWindow != null)
            {
                //显示对话框
                ChatWindow.SetActive(true);
                //生成第一条对话
                ShowCurrentDialogue();
                //解除对话锁
                ChatLock = false;
            }
        }
        else
        {
            Debug.LogError("启动对话失败，无法加载chat" + chatId + ".json");
        }
    }

    //加载指定序号的JSON对话文件
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

            // 验证数据有效性（只验证主对话内容，不验证选配项目）
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

    // 推进对话逻辑
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
            //结束本段对话
            //重置对话索引
            currentDialogueIndex = 0;
            //检查是否有跳转对话的按钮
            if (currentDialogueData.jumps != null && currentDialogueData.jumps.Count > 0)
            {
                ChatLock = true;//暂时锁住对话
                showButton();
                return;
            }
            //结束对话窗
            OverChat();
        }
    }

    // 显示当前索引的对话内容
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

    //显示选择按钮
    public void showButton()
    {
        //第一个按钮
        BtnText1.text = currentDialogueData.jumps[0].btn;
        Button1.SetActive(true);
        //第二个按钮
        if (currentDialogueData.jumps.Count >= 2)
        {
            BtnText2.text = currentDialogueData.jumps[1].btn;
            Button2.SetActive(true);
        }
        //第三个按钮
        if (currentDialogueData.jumps.Count >= 3)
        {
            BtnText3.text = currentDialogueData.jumps[2].btn;
            Button3.SetActive(true);
        }
    }

    //按钮调用：按钮按下后执行的内容
    public void JumpChat(int _id)
    {
        int _target = currentDialogueData.jumps[_id].target;
        //清空内容
        currentDialogueData = null;
        //开启新的对话（目前默认为普通对话）
        Debug.Log("前往对话："+ _target);
        StartChat(_target);
    }

    //获取按钮脚本并绑定
    public void GetButton()
    {
        Btn1 = Button1.GetComponent<Button>();
        Btn2 = Button2.GetComponent<Button>();
        Btn3 = Button3.GetComponent<Button>();
        Btn1.onClick.AddListener(() => JumpChat(0));
        Btn2.onClick.AddListener(() => JumpChat(1));
        Btn3.onClick.AddListener(() => JumpChat(2));
    }

    //=====================================NPC状态更新函数=======================================
    //当每次NPC事件结束后，都要调用这个函数更新NPC的状态

    //由子场景事件完成后调用的函数（完成场景result = true，未完成场景result = false）
    public void SceneOver(bool result)
    {
        if (result)
        {
            if (CurrentTarget != null)
            {
                //获取当前NPC脚本
                NPC_Chat npc_chat = CurrentTarget.GetComponent<NPC_Chat>();
                if (npc_chat != null)
                {
                    npc_chat.StatePush(0);//推进当前NPC状态，设置子状态为0
                }
            }
            else
            {
                Debug.LogWarning("找不到当前NPC对象，无法推进");
            }
        }
        //如果当前窗口没有完成，则不推进NPC的状态
        //释放玩家动作
        if (Player != null)
        {
            //使用局部暂停方法暂停玩家动作
            Player.GetComponent<Player>().playerStop = false;
        }
    }
    //由对话完成调用的
    public void SceneOver(int result)
    {
        if (CurrentTarget != null)
        {
            //获取当前NPC脚本
            NPC_Chat npc_chat = CurrentTarget.GetComponent<NPC_Chat>();
            if (npc_chat != null)
            {
                //Debug.Log("推进NPC的对话");
                npc_chat.StatePush(result);//推进当前NPC状态，设置子状态为0
            }
        }
        else
        {
            Debug.LogWarning("找不到当前NPC对象，无法推进");
        }
        //如果当前窗口没有完成，则不推进NPC的状态
        //结束对话窗
        OverChat();
    }

    //由对话结束后触发的更新状态函数


    //通知地图管理器更新资源表
    public void RefreshMapSource()
    {
        CurrentMapManager.CheckMapSource();
    }

    //删除NPC（由NPC调用）
    public void DeleteNPC(GameObject _npc)
    {
        Destroy(_npc);
        //延时刷新地图资源
        Invoke(nameof(RefreshMapSource), 0.1f);
    }

    //结束对话界面并释放玩家控制权
    public void OverChat()
    {
        //清空内容
        currentDialogueData = null;
        //隐藏对话框
        ChatWindow.SetActive(false);


        // 确保Player已找到后再恢复
        if (isPlayerFound && Player != null)
        {
            Player.GetComponent<Player>().playerStop = false;//释放玩家动作
        }
    }

}