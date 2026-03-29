using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;//文件读写

public class NPC_Chat : MonoBehaviour
{
    //事件类
    [System.Serializable]
    public class StateSolve
    {
        public int main;
        public int son;
        public int function;
        public int number;
    }

    [Header("提示箭头")]
    public GameObject Tip;
    //[Header("激活对话id（999战斗、995选职业、990关闭对话）")]
    //public int chatId;
    [Header("状态事件集")]
    public List<StateSolve> State_Event;
    [Header("是否可能战斗")]
    public bool battle;
    [Header("战斗中的敌人编号集")]
    public int[] enemies;
    [Header("当前NPC的主状态字（初始为0）")]
    public int main_state;
    [Header("当前NPC的子状态字（初始为0）")]
    public int son_state;
    [Header("图片（需要更换图片才需要配置）")]
    public Sprite targetSprite;

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
            ChatManager.Instance.CurrentTarget = gameObject;
            //执行当前NPC的效果
            CheckCurrentState(true);
            //防止重复触发
            isPlayerInTrigger = false;
        }
    }

    //场景切换时清理状态（防止异常）
    private void OnDestroy()
    {
        isPlayerInTrigger = false;
        if (Tip != null)
        {
            Tip.SetActive(false);
        }
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
        Debug.Log("敌人信息写入完成：" + filePath);
    }

    //NPC更新状态
    public void StatePush(int _son)
    {
        //推进主状态
        main_state++;
        //更新子状态
        son_state = _son;
        CheckCurrentState(false);//查看当前状态是否有对应功能
            
    }

    //查看当前状态功能函数
    public void CheckCurrentState(bool _do)
    {
        //寻找符合的事件
        foreach (var state in State_Event)
        {
            // 匹配：主状态 == 当前主状态 && 子状态 == 当前子状态
            if (state.main == main_state && state.son == son_state)
            {
                //是否真的执行此功能（功能10且以上的立即执行）
                if(_do || state.function>=10)
                {
                    //执行函数
                    DoCurrentState(state.function, state.number);
                }
                //找到就退出，不继续遍历
                return;
            }
        }
        //没找到相应功能，说明这个NPC没作用了，让ChatManager删除自身
        ChatManager.Instance.DeleteNPC(gameObject);
    }

    //执行当前状态功能函数
    public void DoCurrentState(int function, int num)
    {
        switch(function)
        {
            case 0://激活对话
                //对话管理器触发
                ChatManager.Instance.StartChat(num);
                break;
            case 10://更换图片
                ChangeImage();
                StatePush(num);//推进
                break;
            default:
                break;
        }
    }

    //更换图片（既可以自身调用，也可以外部调用）
    public void ChangeImage()
    {
        //获取当前对象的图片插槽
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning("当前物体没有找到 SpriteRenderer！", gameObject);
            return;
        }
        if (targetSprite == null)
        {
            Debug.LogWarning("未设置目标图片 targetSprite！", gameObject);
            return;
        }
        sr.sprite = targetSprite;
    }

}
