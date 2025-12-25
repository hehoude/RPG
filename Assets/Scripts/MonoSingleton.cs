using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // 私有静态实例（双重校验锁，线程安全，Unity主线程下可简化）
    private static T _instance;
    // 公共只读实例属性
    public static T Instance
    {
        get
        {
            // 若实例为空，主动查找
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                // 若仍为空，自动创建一个GameObject挂载实例（可选，增强容错）
                if (_instance == null)
                {
                    GameObject singletonObj = new GameObject(typeof(T).Name + " (Singleton)");
                    _instance = singletonObj.AddComponent<T>();
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
            // 首次初始化：赋值实例+标记为DontDestroyOnLoad（跨场景保留）
            _instance = this as T;
            DontDestroyOnLoad(gameObject);//创建一个特殊的全局场景
        }
        else
        {
            // 重复实例：直接销毁
            if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    // 可选：防止在编辑器模式下因脚本重载导致实例丢失（Unity特有）
    protected virtual void OnApplicationQuit()
    {
        _instance = null;
    }

    // 新增：仅查找实例，不创建（用于卸载/检查）
    public static T FindInstance()
    {
        if (_instance != null) return _instance;
        // 只查找场景中已存在的实例，不新建
        _instance = FindObjectOfType<T>();
        return _instance;
    }

    // 手动卸载单例的核心函数
    public void UnloadSingleton()
    {
        if (_instance == this)
        {
            // 解除跨场景保留 + 销毁对象 + 清空静态实例
            transform.SetParent(null); // 取消DontDestroyOnLoad关联
            Destroy(gameObject);
            _instance = null;
            //Debug.Log($"单例 {typeof(T).Name} 已卸载");
        }
    }
}