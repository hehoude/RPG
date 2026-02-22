using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public Transform libraryPanel;//卡牌展示容器

    public GameObject cardPrefab;//卡组卡

    //public GameObject DataManager;
    private PlayerData PlayerData;

    public ScrollRect cardScrollRect;//滚动条
    public GameObject Button1;
    public GameObject Button2;
    public GameObject Button3;
    public GameObject Button4;

    void Awake()
    {
        //从游戏对象DataManager处获取脚本
        PlayerData = PlayerData.Instance;
        //这里获取脚本一定要趁早，如果放到Start中，会等到OnDataLoaded()生效后还没获取到脚本
    }

    void Start()
    {
        UpdateDeck(0);
        HideButton();
    }

    void Update()
    {
        
    }

    //显示卡组中的卡
    public void UpdateDeck(int _pile)
    {
        ClearAllCardsInLibrary();//清空界面的卡牌
        switch (_pile)
        {
            case 0://玩家卡组
                for (int i = 0; i < PlayerData.PlayerCardList.Count; i++)
                {
                    CreatCard(PlayerData.PlayerCardList[i]);
                }
                break;
            case 1://队友0卡组
                for (int i = 0; i < PlayerData.MateCardList0.Count; i++)
                {
                    CreatCard(PlayerData.MateCardList0[i]);
                }
                break;
            case 2://队友1卡组
                for (int i = 0; i < PlayerData.MateCardList1.Count; i++)
                {
                    CreatCard(PlayerData.MateCardList1[i]);
                }
                break;
            case 3://队友2卡组
                for (int i = 0; i < PlayerData.MateCardList2.Count; i++)
                {
                    CreatCard(PlayerData.MateCardList2[i]);
                }
                break;
            case 4://队友3卡组
                for (int i = 0; i < PlayerData.MateCardList3.Count; i++)
                {
                    CreatCard(PlayerData.MateCardList3[i]);
                }
                break;
        }
        cardScrollRect.verticalNormalizedPosition = 1f;//滚动条到顶部
    }

    //在指定容器内创建卡片
    public void CreatCard(Card card)
    {
        //在指定容器上创建对象
        GameObject newCard = Instantiate(cardPrefab, libraryPanel);
        //导入类card（后续会用CardDisplay的ShowCard将卡牌信息呈现）
        newCard.GetComponent<CardDisplay>().card = card;
    }

    //清除libraryPanel的所有卡牌
    public void ClearAllCardsInLibrary()
    {
        // 遍历所有子物体
        Transform[] allCardTransforms = libraryPanel.GetComponentsInChildren<Transform>();
        foreach (Transform cardTransform in allCardTransforms)
        {
            // 排除libraryPanel自身（只删除子物体）
            if (cardTransform != libraryPanel.transform)
            {
                // 销毁卡牌对象
                Destroy(cardTransform.gameObject);
            }
        }
    }

    //隐藏不可使用的按钮
    public void HideButton()
    {
        //如果没有对应队友，则隐藏按钮
        if (PlayerData.MateList.Count < 1)
        {
            if (Button1 != null)
                Button1.SetActive(false);
        }
        if (PlayerData.MateList.Count < 2)
        {
            if (Button2 != null)
                Button2.SetActive(false);
        }
        if (PlayerData.MateList.Count < 3)
        {
            if (Button3 != null)
                Button3.SetActive(false);
        }
        if (PlayerData.MateList.Count < 4)
        {
            if (Button4 != null)
                Button4.SetActive(false);
        }
    }

    //退出按钮
    public void Exit()
    {
        SceneChanger.Instance.GetMajorCity(6);
    }


}
