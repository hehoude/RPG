using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;//文件读写
using UnityEngine;

public class BattleReader : MonoBehaviour
{
    //使用TextAsset方法读取文本，将目标文本拖到插槽即可
    //public TextAsset battleMessage;
    public List<int> enemies; //敌人代号容器

    void Awake()
    {
        LoadEnemy();//读取本场战斗敌人
    }

    public void LoadEnemy()
    {
        enemies.Clear();//清理容器
        //读取路径
        string fullPath = Application.dataPath + "/Datas/Save/BattleMessage.csv";
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
}
