using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyData : MonoBehaviour
{
    //使用TextAsset方法读取文本，将目标文本拖到插槽即可
    public TextAsset enemyList;

    void Start()
    {
        
    }

    void Update()
    {
        
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
                int enemy_hp = int.Parse(rowArray[3]);
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
