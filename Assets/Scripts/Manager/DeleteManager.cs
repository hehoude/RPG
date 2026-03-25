using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeleteManager : MonoBehaviour
{
    //卡槽
    public GameObject CardBlock;
    public GameObject BigBlock;
    //卡牌
    public GameObject DeCard_Prefab;
    public GameObject BigCard_Prefab;
    //数据管理器
    //public GameObject DataManager;
    private PlayerData PlayerData;
    private CardStore CardStore;
    //卡牌类暂存
    private Card card;
    //动画
    public Animator anim1;

    void Awake()
    {
        PlayerData = PlayerData.Instance;
        CardStore = CardStore.Instance;
    }

    //在两个卡槽中创建指定卡牌
    public void CreateCard(Card _card)
    {
        card = _card;
        //清除原有卡牌
        Destroy(CardBlock.GetComponent<Block>().obj);
        //找到容器生成自己的复制品
        GameObject newCard = Instantiate(DeCard_Prefab, CardBlock.transform);
        //给复制品赋予card类实例
        newCard.GetComponent<CardDisplay>().card = card;
        //卡牌与卡槽关联
        CardBlock.GetComponent<Block>().obj = newCard;
    }

    //执行删除
    public void DoDeleteCard()
    {
        for (int i = 0; i < PlayerData.PlayerCardList.Count; i++)
        {
            //找到卡组中符合id的卡牌
            if (PlayerData.PlayerCardList[i] == card)
            {
                PlayerData.PlayerCardList.RemoveAt(i);//删除
                break;
            }
        }
        //展示被删除的卡牌（将放大版的卡牌呈现在中间）
        //生成展示卡牌
        GameObject bigCard = Instantiate(BigCard_Prefab, BigBlock.transform);
        //给展示卡牌赋予card类实例
        bigCard.GetComponent<CardDisplay>().card = CardBlock.GetComponent<Block>().obj.GetComponent<CardDisplay>().card;
        //播放删除动画
        anim1.SetBool("Action", true);
        //延迟1秒后退出
        Invoke("FinishUp", 1f);

    }

    public void FinishUp()
    {
        //保存数据
        PlayerData.SavePlayerData();
        //判断当前游戏模式
        if (Global_PlayerData.Instance.model == 0)
        {
            //通知地图管理器删除自身
            MapManager.Instance.DeleteCurrentObject(2);//2代表删卡
        }
        else if (Global_PlayerData.Instance.model == 1)//战役模式结算
        {
            ChatManager.Instance.SceneOver(true);//删除对象
        }
        //返回主城
        SceneChanger.Instance.GetMajorCity(4);
    }

    //退出按钮
    public void Exit()
    {
        if (Global_PlayerData.Instance.model == 1)
        {
            ChatManager.Instance.SceneOver(false);//不删除对象
        }
        SceneChanger.Instance.GetMajorCity(4);
    }


}
