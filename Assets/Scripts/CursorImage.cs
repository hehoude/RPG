using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorImage : MonoBehaviour
{
    public Image cursorImage;
    // 鼠标样式配置
    public Sprite defaultCursorSprite;   // 默认鼠标图片
    public Sprite swordCursorSprite;     // 悬浮敌人时的剑图片
    [Tooltip("优先使用手动指定的相机，未指定则自动查找MainCamera/2D正交相机")]
    public Camera mainCamera;            // 可选：手动指定相机（非必需）
    public string enemyTagName = "Enemy"; // 敌人的标签名称

    // 记录原始状态，避免重复赋值
    private bool isOverEnemy = false;

    void Start()
    {
        // 核心修改1：解除鼠标锁定，让系统鼠标可以自由移出窗口且不消失
        Cursor.lockState = CursorLockMode.None; // 不锁定鼠标位置
        Cursor.visible = true; // 先让系统鼠标可见（后续根据逻辑切换）

        // 核心优化：自动查找主相机
        AutoFindMainCamera();

        // 初始化鼠标图片为默认样式
        if (defaultCursorSprite != null)
        {
            cursorImage.sprite = defaultCursorSprite;
        }

        // 安全检查：如果还是没找到相机，给出提示
        if (mainCamera == null)
        {
            Debug.LogWarning("未找到主相机！请确保场景中有Tag为MainCamera的相机，或手动指定相机。", this);
        }
    }

    void Update()
    {
        // 保留原有逻辑：UI鼠标跟随
        transform.position = Input.mousePosition;
        // 保留原有逻辑：鼠标点击交互
        HandleMouseClick();

        // 检测鼠标是否在游戏窗口内
        bool isMouseInGameWindow = IsMouseInWindow();

        // 核心修改2：根据鼠标是否在窗口内，切换系统鼠标/UI鼠标的显示
        if (isMouseInGameWindow)
        {
            // 鼠标在游戏窗口内：隐藏系统鼠标，显示自定义UI鼠标
            Cursor.visible = false;
            cursorImage.enabled = true;

            // 检测敌人并切换UI鼠标图片
            if (mainCamera != null)
            {
                CheckMouseOverEnemyByTag();
                SwitchCursorSprite();
            }
        }
        else
        {
            // 鼠标移出游戏窗口：显示系统原生鼠标，隐藏自定义UI鼠标
            Cursor.visible = true;
            cursorImage.enabled = false;
            isOverEnemy = false; // 重置状态
        }
    }

    /// <summary>
    /// 判断鼠标是否在游戏窗口内
    /// </summary>
    /// <returns>是否在窗口内</returns>
    private bool IsMouseInWindow()
    {
        Vector2 mousePos = Input.mousePosition;
        // 检查鼠标坐标是否在游戏窗口的可视范围内
        return mousePos.x >= 0 && mousePos.x <= Screen.width
            && mousePos.y >= 0 && mousePos.y <= Screen.height;
    }

    /// <summary>
    /// 自动查找2D场景的主相机
    /// </summary>
    private void AutoFindMainCamera()
    {
        // 1. 如果手动指定了相机，直接使用
        if (mainCamera != null)
        {
            return;
        }

        // 2. 查找Tag为"MainCamera"的相机
        Camera tagCamera = Camera.main;
        if (tagCamera != null)
        {
            mainCamera = tagCamera;
            return;
        }

        // 3. 查找场景中第一个正交相机
        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam.orthographic)
            {
                mainCamera = cam;
                return;
            }
        }

        // 4. 最后尝试找任意相机
        if (allCameras.Length > 0)
        {
            mainCamera = allCameras[0];
        }
    }

    //鼠标点击时变色
    private void HandleMouseClick()
    {
        // 只有鼠标在窗口内时，才响应点击
        if (!IsMouseInWindow()) return;

        if (Input.GetMouseButtonDown(0))
        {
            cursorImage.color = Color.red;
            transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            cursorImage.color = Color.white;
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    /// <summary>
    /// 通过标签检测敌人
    /// </summary>
    private void CheckMouseOverEnemyByTag()
    {
        if (mainCamera == null)
        {
            isOverEnemy = false;
            return;
        }

        // 1. 转换坐标（加Z轴修正）
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = mainCamera.nearClipPlane + 1f;
        Vector2 worldMousePos = mainCamera.ScreenToWorldPoint(screenPos);

        // 2. 使用OverlapPoint检测
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(worldMousePos);

        // 3. 遍历所有碰撞体，判断是否有Enemy标签
        isOverEnemy = false;
        foreach (Collider2D coll in hitColliders)
        {
            if (coll.CompareTag(enemyTagName))
            {
                isOverEnemy = true;
                break;
            }
        }
    }

    /// <summary>
    /// 切换鼠标图片
    /// </summary>
    private void SwitchCursorSprite()
    {
        if (isOverEnemy)
        {
            if (cursorImage.sprite != swordCursorSprite && swordCursorSprite != null)
            {
                cursorImage.sprite = swordCursorSprite;
            }
        }
        else
        {
            if (cursorImage.sprite != defaultCursorSprite && defaultCursorSprite != null)
            {
                cursorImage.sprite = defaultCursorSprite;
            }
        }
    }

    // 退出时恢复系统鼠标
    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}