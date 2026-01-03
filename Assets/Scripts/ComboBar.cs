using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BattleManager;

public class ComboBar : MonoBehaviour
{
    public GameObject[] ComboStone;//指示器
    private ComboStone[] ComboStoneList;//指示器脚本
    public List<int> ComboList;//连携数组（需要创建连携条时赋值进来）
    private int ComboSch;//连携进度（目前仅支持每回合一次连携，后续需要再修改）

    void Awake()
    {
        ComboSch = 0;
        //初始化指示器脚本数组
        ComboStoneList = new ComboStone[3];
        //获取三个指示器的脚本
        for (int i = 0; i < 3; i++)
        {
            ComboStoneList[i] = ComboStone[i].GetComponent<ComboStone>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //根据连携数组设置三颗指示宝石的颜色
        //获取三个指示器的脚本
        for (int i = 0; i < ComboList.Count; i++)
        {
            switch (ComboList[i])
            {
                case 1://火
                    ComboStoneList[i].Stone.color = Color.red;
                    break;
                case 2://毒
                    ComboStoneList[i].Stone.color = Color.green;
                    break;
                case 3://电
                    ComboStoneList[i].Stone.color = Color.yellow;
                    break;
                default:
                    ComboStoneList[i].Stone.color = Color.white;
                    break;
            }
        }
        Event.SendCard += SendCard1;//订阅出牌事件
        Event.TurnEnd += Clear;//订阅回合结束事件
    }

    //当收到出牌事件时
    public void SendCard1(int type)
    {
        //Debug.Log("收到信号："+ type);
        //如果连携没有走完
        if (ComboSch < 3)
        {
            //则比对是否符合连携数组
            if (type == ComboList[ComboSch] || type == 10)
            {
                if (ComboStoneList[ComboSch] != null)
                {
                    //推进连携进度
                    ComboStoneList[ComboSch].Fire.SetActive(true);
                    ComboSch++;
                } 
            }
            else
            {
                Clear();//比对失败则清空连携进度
                //清空后再尝试对比第一个
                if (type == ComboList[ComboSch])
                {
                    if (ComboStoneList[ComboSch] != null)
                    {
                        //推进连携进度
                        ComboStoneList[ComboSch].Fire.SetActive(true);
                        ComboSch++;
                    }
                }
            }
        }
    }

    //回合结束清除连携指示器
    public void Clear()
    {
        for(int i = 0;i < 3;i++)
        {
            if (ComboStoneList[i] != null)
            {
                ComboStoneList[i].Fire.SetActive(false);
            }
        }
        ComboSch = 0;
    }

    
}
