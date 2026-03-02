using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 全局UI管理器（单例模式）
/// 直接挂载在Canvas根对象上，保证Canvas及其子对象单例+跨场景保留
/// </summary>
[RequireComponent(typeof(Canvas))] // 强制要求挂载对象有Canvas组件
public class UIManager : MonoSingleton<UIManager>
{
    // 【核心配置】通过单例基类的变量控制是否跨场景保留（默认true）
    // 可在Inspector面板直接修改useDontDestroyOnLoad开关

    // 缓存当前挂载的Canvas组件（无需手动赋值，自动获取）
    private Canvas mainCanvas;

    protected override void Awake()
    {
        // 第一步：执行单例基类逻辑（关键！保证单例+跨场景保留）
        base.Awake();

        // 第二步：自动获取挂载对象的Canvas组件
        GetCanvasComponent();

        // 第三步：初始化Canvas的基础配置（确保UI正常显示）
        InitCanvasConfig();
    }

    /// <summary>
    /// 自动获取挂载对象的Canvas组件
    /// </summary>
    private void GetCanvasComponent()
    {
        mainCanvas = GetComponent<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("UIManager挂载的对象没有Canvas组件！已自动添加", gameObject);
            mainCanvas = gameObject.AddComponent<Canvas>();
        }
    }

    /// <summary>
    /// 初始化Canvas的基础配置（保证UI正常渲染和交互）
    /// </summary>
    private void InitCanvasConfig()
    {
        // 设置默认渲染模式（可根据需求修改）
        if (mainCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // 自动添加缺失的核心组件（保证UI缩放和交互）
        if (!gameObject.TryGetComponent(out CanvasScaler canvasScaler))
        {
            canvasScaler = gameObject.AddComponent<CanvasScaler>();
            // 设置默认缩放模式
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
        }

        if (!gameObject.TryGetComponent(out GraphicRaycaster graphicRaycaster))
        {
            graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
        }

        //Debug.Log($"全局Canvas初始化完成：{gameObject.name}，渲染模式：{mainCanvas.renderMode}", gameObject);
    }

    /// <summary>
    /// 【扩展方法】外部调用初始化子UI（如需动态创建子UI时使用）
    /// </summary>
    /// <param name="uiPrefab">要创建的UI预制体</param>
    /// <param name="parentTrans">父节点（默认挂在Canvas下）</param>
    /// <returns>创建的UI实例</returns>
    public GameObject CreateUI(GameObject uiPrefab, Transform parentTrans = null)
    {
        if (uiPrefab == null)
        {
            Debug.LogError("创建UI失败：预制体为空！");
            return null;
        }

        // 默认父节点为Canvas
        Transform targetParent = parentTrans ?? mainCanvas.transform;
        GameObject uiInstance = Instantiate(uiPrefab, targetParent);
        uiInstance.name = uiPrefab.name; // 去掉克隆后缀，便于识别

        Debug.Log($"成功创建UI：{uiInstance.name}，父节点：{targetParent.name}", gameObject);
        return uiInstance;
    }
}