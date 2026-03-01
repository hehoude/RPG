using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public Transform libraryPanel;//卡牌/连携/遗物展示容器（已挂载GridLayoutGroup）

    // 三类内容预制体
    public GameObject cardPrefab;//卡组卡
    public GameObject comboPrefab;//连携
    public GameObject equipPrefab;//遗物

    private PlayerData PlayerData;
    public ScrollRect cardScrollRect;//滚动条

    // 布局配置：三类内容的单元格大小（可在Inspector面板调整）
    [Header("布局配置")]
    private Vector2 cardCellSize = new Vector2(300, 400);    // 卡牌单元格大小
    private Vector2 comboCellSize = new Vector2(300, 60);     // 连携单元格大小
    private Vector2 equipCellSize = new Vector2(120, 80);    // 遗物单元格大小

    // 缓存Grid布局组件
    private GridLayoutGroup gridLayout;

    void Awake()
    {
        // 获取玩家数据
        PlayerData = PlayerData.Instance;

        // 获取已挂载的GridLayoutGroup组件
        gridLayout = libraryPanel.GetComponent<GridLayoutGroup>();
    }

    void Start()
    {
        // 初始默认显示卡牌
        ShowCards();
    }

    #region 显示切换方法（绑定到按钮）
    /// <summary>
    /// 显示卡牌（绑定到「卡牌」按钮）
    /// </summary>
    public void ShowCards()
    {
        // 1. 切换为卡牌布局
        UpdateGridCellSize(cardCellSize);
        // 2. 清空容器
        ClearAllContentInLibrary();
        // 3. 生成卡牌
        for (int i = 0; i < PlayerData.PlayerCardList.Count; i++)
        {
            CreateCard(PlayerData.PlayerCardList[i]);
        }
        // 4. 滚动条归位
        ResetScrollPosition();
    }

    /// <summary>
    /// 显示连携（绑定到「连携」按钮）
    /// </summary>
    public void ShowCombos()
    {
        // 1. 切换为连携布局
        UpdateGridCellSize(comboCellSize);
        // 2. 清空容器
        ClearAllContentInLibrary();
        // 3. 生成连携（需替换为你的连携列表逻辑）
        for (int i = 0; i < PlayerData.ComboList.Count; i++)
        {
            CreateCombo(PlayerData.ComboList[i]);
        }
        // 4. 滚动条归位
        ResetScrollPosition();
    }

    /// <summary>
    /// 显示遗物（绑定到「遗物」按钮）
    /// </summary>
    public void ShowEquips()
    {
        // 1. 切换为遗物布局
        UpdateGridCellSize(equipCellSize);
        // 2. 清空容器
        ClearAllContentInLibrary();
        // 3. 生成遗物（需替换为你的遗物列表逻辑）
        for (int i = 0; i < PlayerData.EquipList.Count; i++)
        {
            CreateEquip(PlayerData.EquipList[i]);
        }
        // 4. 滚动条归位
        ResetScrollPosition();
    }
    #endregion

    #region 内容创建方法
    /// <summary>
    /// 创建卡牌
    /// </summary>
    public void CreateCard(Card card)
    {
        GameObject newCard = Instantiate(cardPrefab, libraryPanel);
        newCard.GetComponent<CardDisplay>().card = card;
        // 确保UI适配Grid单元格大小
        SetUIElementStretch(newCard);
    }

    /// <summary>
    /// 创建连携
    /// </summary>
    public void CreateCombo(int combo)
    {
        GameObject newCombo = Instantiate(comboPrefab, libraryPanel);
        // 替换为你的连携显示逻辑
        newCombo.GetComponent<ComboBar>().ComboType = combo;
        // 确保UI适配Grid单元格大小
        SetUIElementStretch(newCombo);
    }

    /// <summary>
    /// 创建遗物
    /// </summary>
    public void CreateEquip(int equip)
    {
        //GameObject newEquip = Instantiate(equipPrefab, libraryPanel);
        //// 替换为你的遗物显示逻辑
        //newEquip.GetComponent<EquipmentDisplay>().equipment = equip;
        //// 确保UI适配Grid单元格大小
        //SetUIElementStretch(newEquip);
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 动态更新Grid单元格大小
    /// </summary>
    private void UpdateGridCellSize(Vector2 targetSize)
    {
        if (gridLayout != null)
        {
            gridLayout.cellSize = targetSize;
        }
    }

    /// <summary>
    /// 清空容器内所有内容（优化版：更高效）
    /// </summary>
    public void ClearAllContentInLibrary()
    {
        // 倒序遍历删除，避免索引错乱
        for (int i = libraryPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(libraryPanel.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 重置滚动条到顶部
    /// </summary>
    private void ResetScrollPosition()
    {
        if (cardScrollRect != null)
        {
            cardScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    /// <summary>
    /// 设置UI元素拉伸，适配Grid单元格大小
    /// </summary>
    private void SetUIElementStretch(GameObject uiObj)
    {
        RectTransform rt = uiObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }
    }
    #endregion

    // 退出按钮
    public void Exit()
    {
        SceneChanger.Instance.GetMajorCity(6);
    }
}