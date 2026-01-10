using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
//实例化时机：抽卡商店被创建时
//public class CardStore : MonoBehaviour
public class CardStore : MonoSingleton<CardStore>
{
    //使用TextAsset方法读取文本，将目标文本拖到插槽即可（只读文档尽量可以使用这种方法）
    public TextAsset cardData;//卡牌文档
    public TextAsset mateData;//队友文档
    //创建最终存放卡牌的容器
    public List<Card> cardList = new List<Card>();
    //创建各稀有度卡牌的id容器
    public List<int> White_Cards;
    public List<int> Blue_Cards;
    public List<int> Gold_Cards;
    //存放所有队友的卡组容器的容器
    public List<List<int>> MateLists;
    // Start is called before the first frame update
    void Start()
    {
        //LoadCardData();   //不加载了，由PlayerData里面执行加载
        //TestLoad();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //这里会读取表格中每一张卡
    public void LoadCardData()
    {
        //创建一个字符串数组，读取cardData，指定分隔符为换行
        //string[] datarow = cardData.text.Split('\n');
        string[] datarow = cardData.text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var row in datarow)//遍历元素
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
            else
            {
                string CardType = rowArray[0];
                int type = 0;
                if (CardType == "attack") { type = 0; }
                else if (CardType == "skill") { type = 1; }
                else if (CardType == "ability") { type = 2; }
                int id = int.Parse(rowArray[1]);
                int ra = int.Parse(rowArray[2]);
                string name = rowArray[3];
                int spend = int.Parse(rowArray[4]);
                int target = int.Parse(rowArray[5]);
                int attack = int.Parse(rowArray[6]);
                int defense = int.Parse(rowArray[7]);
                int keep = int.Parse(rowArray[8]);
                int consume = int.Parse(rowArray[9]);
                int element = int.Parse(rowArray[10]);
                int fire = int.Parse(rowArray[11]);
                int toxin = int.Parse(rowArray[12]);
                int electricity = int.Parse(rowArray[13]);
                int other = int.Parse(rowArray[14]);
                int front = int.Parse(rowArray[15]);
                Card card = new Card(id, type, ra, name, spend, target, attack, 
                    defense, keep, consume, element, fire, toxin, electricity, other, front);
                cardList.Add(card);//将类加入容器中
                //Debug.Log("读取到卡牌：" + card.cardName);

                //根据稀有度把id加入稀有度容器中
                if (ra == 1)//白卡
                {
                    White_Cards.Add(id);
                }
                else if (ra == 2)//蓝卡
                {
                    Blue_Cards.Add(id);
                }
                else if (ra == 3)//金卡
                {
                    Gold_Cards.Add(id);
                }
            }
        }
        Debug.Log("所有卡牌预加载完成");
    }

    //加载队友卡组数据表
    public void LoadMateList()
    {
        //初始化容器（之所以没有放到Awake，因为这个函数会在PlayerData的Awake中调用，可能出现次序问题）
        MateLists = new List<List<int>>();
        string[] datarow = mateData.text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var row in datarow)//遍历元素
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
            else if (rowArray[0] == "teammate")
            {
                if (rowArray.Length < 3)
                {
                    Debug.LogWarning($"行数据不完整，跳过此行：{row}");
                    continue;
                }

                try
                {
                    List<int> currentRowCards = new List<int>();
                    for (int i = 2; i < rowArray.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(rowArray[i]))
                            continue;
                        currentRowCards.Add(int.Parse(rowArray[i]));
                    }

                    MateLists.Add(currentRowCards);
                }
                catch (FormatException ex)
                {
                    // 错误级别日志（红色）
                    Debug.LogError($"行数据格式错误，跳过此行：{row}，错误：{ex.Message}");
                }
            }
        }
        Debug.Log("所有队友预加载完成");
    }

    public void TestLoad()
    {
        foreach (var item in cardList)
        {
            //Debug.Log("读取到卡牌：" + item.id.ToString() + item.cardName);
        }
    }

    public Card RandomCard()
    {
        Card card = cardList[UnityEngine.Random.Range(0, cardList.Count)];
        return card;
    }

    //根据id复制一张卡返回
    public Card CopyCard(int _id)
    {
        //获取目标id卡牌引用
        Card ca = cardList[_id];
        //构建复制品
        Card cardCopy = new Card(ca.id, ca.type, ca.rarity, ca.cardName, ca.spend,
        ca.target, ca.attack, ca.defense, ca.keep, ca.consume, ca.element, ca.fire, 
        ca.toxin, ca.electricity, ca.other, ca.front);

        return cardCopy;
    }
}

