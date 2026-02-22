using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UpManager : MonoBehaviour
{
    //卡槽
    public GameObject CardBlock;
    public GameObject UpBlock;
    public GameObject BigBlock;
    //卡牌
    public GameObject UpCard_Prefab;
    public GameObject BigCard_Prefab;
    //数据管理器
    //public GameObject DataManager;
    private PlayerData PlayerData;
    private CardStore CardStore;
    //卡牌ID暂存
    private int id;
    //动画
    public Animator anim1;
    public Animator anim2;
    
    public GameObject ChuiZi;//锤子
    public GameObject FireLight;//火光

    void Awake()
    {
        PlayerData = PlayerData.Instance;
        CardStore = CardStore.Instance;
    }

    //在两个卡槽中创建指定卡牌
    public void CreateCard(int _id)
    {
        id = _id;
        //清除原有卡牌
        Destroy(CardBlock.GetComponent<Block>().obj);
        //找到容器生成自己的复制品
        GameObject newCard = Instantiate(UpCard_Prefab, CardBlock.transform);
        //给复制品赋予card类实例
        newCard.GetComponent<CardDisplay>().card = CardStore.CopyCard(id);
        //卡牌与卡槽关联
        CardBlock.GetComponent<Block>().obj = newCard;

        //清除原有卡牌
        Destroy(UpBlock.GetComponent<Block>().obj);
        //找到升级后容器生成复制品
        GameObject newUpCard = Instantiate(UpCard_Prefab, UpBlock.transform);
        //给复制品赋予升级后的card类实例
        newUpCard.GetComponent<CardDisplay>().card = PlayerData.Upgrade(CardStore.CopyCard(id));
        //卡牌与卡槽关联
        UpBlock.GetComponent<Block>().obj = newUpCard;
    }

    //执行升级
    public void DoUpCard()
    {
        for (int i = 0; i < PlayerData.PlayerCardList.Count; i++)
        {
            //找到卡组中符合id的卡牌
            if (PlayerData.PlayerCardList[i].id == id)
            {
                if(!PlayerData.PlayerCardList[i].upgrade)
                {
                    //PlayerData.PlayerCardList[i].upgrade = true;这种升级不完全，弃用
                    PlayerData.PlayerCardList[i] = PlayerData.Upgrade(PlayerData.PlayerCardList[i]);//升级
                    Debug.Log("升级成功！");
                    break;
                }
            }
        }
        //展示升级后的卡牌（将放大版的卡牌呈现在中间）
        //生成展示卡牌
        GameObject bigCard = Instantiate(BigCard_Prefab, BigBlock.transform);
        //给展示卡牌赋予card类实例（使用升级后的卡牌）
        bigCard.GetComponent<CardDisplay>().card = UpBlock.GetComponent<Block>().obj.GetComponent<CardDisplay>().card;
        //显示锤子
        ChuiZi.SetActive(true);
        //播放升级动画
        anim1.SetBool("Action", true);
        //延迟生成火光
        Invoke("CreateFireStar", 0.4f);
        
    }

    //这个由动画事件调用
    public void CreateFireStar()
    {
        FireLight.SetActive(true);
        //生成火光
        anim2.SetBool("Action", true);
        //延迟0.5秒后退出
        Invoke("FinishUp", 0.5f);
    }

    public void FinishUp()
    {
        //保存数据
        PlayerData.SavePlayerData();
        //通知地图管理器删除当前火堆
        if (MapManager.Instance != null)
        {
            //如果是主城就不删了，游戏地图上才删除
            MapManager.Instance.DeleteCurrentObject(1);//1代表火堆
        }
        //返回主城
        Exit();
    }

    //退出按钮
    public void Exit()
    {
        SceneChanger.Instance.GetMajorCity(3);
    }


}
