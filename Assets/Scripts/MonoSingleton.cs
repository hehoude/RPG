using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // 【核心配置】可配置是否启用跨场景保留（子类可在Inspector面板设置）
    [Header("单例配置")]
    [Tooltip("是否启用DontDestroyOnLoad（跨场景保留）")]
    public bool useDontDestroyOnLoad = true;

    // 私有静态实例
    private static T _instance;
    // 公共只读实例属性
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();

                if (_instance == null)
                {
                    GameObject singletonObj = new GameObject(typeof(T).Name + " (Singleton)");
                    _instance = singletonObj.AddComponent<T>();

                    // 【修复核心】通过类型转换，让编译器识别 MonoSingleton<T> 的字段
                    if (_instance is MonoSingleton<T> monoSingleton)
                    {
                        monoSingleton.useDontDestroyOnLoad = true;
                    }
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        // 单例唯一性校验
        if (_instance == null)
        {
            _instance = this as T;

            // 根据配置决定是否启用跨场景保留
            if (useDontDestroyOnLoad)
            {
                if (transform.parent != null)
                {
                    transform.SetParent(null);
                }
                DontDestroyOnLoad(gameObject);
                //Debug.Log($"单例 {typeof(T).Name} 已启用跨场景保留");
            }
            else
            {
                //Debug.Log($"单例 {typeof(T).Name} 未启用跨场景保留，将随场景销毁");
            }
        }
        else
        {
            if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _instance = null;
    }

    public static T FindInstance()
    {
        if (_instance != null) return _instance;
        _instance = FindObjectOfType<T>();
        return _instance;
    }

    public void UnloadSingleton()
    {
        if (_instance == this)
        {
            transform.SetParent(null);
            Destroy(gameObject);
            _instance = null;
        }
    }
}