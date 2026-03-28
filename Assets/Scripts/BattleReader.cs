using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;//文件读写
using UnityEngine;

public class BattleReader : MonoBehaviour
{
    //使用TextAsset方法读取文本，将目标文本拖到插槽即可
    public TextAsset enemyList;//敌人数据列表
    //public TextAsset battleMessage;
    public List<int> enemies; //敌人代号临时容器
    private string LoadSet = "Save";//数据加载文件夹位置（储存了本场战斗的敌人的数量与id）

    void Awake()
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
        LoadEnemy();//读取本场战斗敌人
    }

    void Start()
    {
        
    }

    public void LoadEnemy()
    {
        enemies.Clear();//清理容器
        //读取路径
        string fullPath = Application.dataPath + "/Datas/" + LoadSet +"/BattleMessage.csv";
        //转换为字符串
        string fileContent = File.ReadAllText(fullPath, System.Text.Encoding.UTF8);
        //拆分
        string[] dataRow = fileContent.Split('\n');
        //string[] datarow = fileContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var row in dataRow)//遍历元素
        {
            string[] rowArray = row.Split(',');//再创建字符串数组，指定逗号为分隔符
            if (rowArray[0] == "#")//第一个为#忽略
            {
                continue;
            }
            else if (string.IsNullOrEmpty(rowArray[0]))
            {
                break; // 终止循环，不再继续读取后续行
            }
            else if (rowArray[0] == "enemy")
            {
                int enemy_id = int.Parse(rowArray[1]);
                enemies.Add(enemy_id);//将敌人ID加入容器中
            }
        }
        //Debug.Log("所有敌人ID预加载完成");
    }

    //加载指定ID的敌人信息
    public EnemyType LoadEnemyMessage(int enemyID)
    {
        string[] datarow = enemyList.text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var row in datarow)//遍历元素
        {
            string[] rowArray = row.Split(',');//再创建字符串数组，指定逗号为分隔符
            if (rowArray[1] == enemyID.ToString())//如果找到符合ID的行
            {
                //读取表格内容创建类
                int enemy_id = enemyID;
                string enemy_name = rowArray[2];
                int enemy_maxhp = int.Parse(rowArray[3]);
                int enemy_hp = enemy_maxhp;
                int enemy_attack = int.Parse(rowArray[4]);
                int enemy_defense = int.Parse(rowArray[5]);
                int enemy_build = int.Parse(rowArray[6]);
                int enemy_negative = int.Parse(rowArray[7]);
                int enemy_special1 = int.Parse(rowArray[8]);
                int enemy_special2 = int.Parse(rowArray[9]);
                int enemy_special3 = int.Parse(rowArray[10]);
                int start = int.Parse(rowArray[11]);
                EnemyType enemyType =
                    new EnemyType(enemy_id, enemy_name, enemy_maxhp, enemy_hp,
                    enemy_attack, enemy_defense, enemy_build, enemy_negative, enemy_special1,
                    enemy_special2, enemy_special3, start);
                return enemyType;
            }
            else
            {

            }
        }
        //未知ID则默认返回恶魔
        EnemyType emo = new EnemyType(0, "恶魔", 50, 50, 10, 10, 1, 0, 0, 0, 0, 0);
        return emo;
    }
}
