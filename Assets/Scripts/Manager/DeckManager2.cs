using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager2 : MonoBehaviour
{
    public Transform libraryPanel;//卡牌放置区
    public GameObject Place;//卡牌展示区
    public GameObject cardPrefab;//卡组卡

    private BattleManager battleManager;

    public ScrollRect cardScrollRect;//滚动条

    // Start is called before the first frame update
    void Start()
    {
        //从游戏对象DataManager处获取这两个脚本
        battleManager = BattleManager.Instance;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //显示指定卡组中的卡（0抽牌堆、1弃牌堆、2消耗牌堆）
    public void UpdateDeck(int _pile)
    {
        Place.SetActive(true);
        ClearAllCardsInLibrary();
        switch (_pile)
        {
            case 0:
                for (int i = 0; i < battleManager.DrawCardList.Count; i++)
                {
                    CreatCard(battleManager.DrawCardList[i]);
                }
                break;
            case 1:
                for (int i = 0; i < battleManager.DisCardList.Count; i++)
                {
                    CreatCard(battleManager.DisCardList[i]);
                }
                break;
            case 2:
                for (int i = 0; i < battleManager.ConsumeList.Count; i++)
                {
                    CreatCard(battleManager.ConsumeList[i]);
                }
                break;
        }
        cardScrollRect.verticalNormalizedPosition = 1f;//滚动条到顶部
        
    }

    private void ClearAllCardsInLibrary()
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

    //在指定容器内创建卡片
    public void CreatCard(Card card)
    {
        //在指定容器上创建对象
        GameObject newCard = Instantiate(cardPrefab, libraryPanel);
        //导入类card（后续会用CardDisplay的ShowCard将卡牌信息呈现）
        newCard.GetComponent<CardDisplay>().card = card;
    }
}
