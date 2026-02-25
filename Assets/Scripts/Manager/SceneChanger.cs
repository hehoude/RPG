using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoSingleton<SceneChanger>
{
    private int CleanMap = 999;//待卸载的场景编号
    //****************************从临时场景切换回主场景的方法*************************
    public void GetMajorCity(int ClearMap)
    {
        //预约卸载场景
        CleanMap = ClearMap;
        //继续原有逻辑
        GetMajorCity();
    }
    public void GetMajorCity()
    {
        //获取当前地图ID
        int CurrentMap = Global_PlayerData.Instance.Map;
        //SceneManager.LoadScene(0);//打开序号为0的场景（默认销毁其它场景）
        LoadSceneAdditive(CurrentMap);//加载地图
        ResumeTargetScene(CurrentMap);//解除场景暂停
        //卸载不用的场景
        if (CleanMap < 999)
        {
            UnloadScene(CleanMap);
            CleanMap = 999;
        }
        //卸载部分管理器（否则进入相同场景时，这些管理器无法执行Start方法）
        var battleManager = BattleManager.FindInstance();
        if (battleManager != null)
        {
            battleManager.UnloadSingleton();
            Debug.Log("战斗管理器单例已卸载");
        }
        var boxManager = BoxManager.FindInstance();
        if (boxManager != null)
        {
            boxManager.UnloadSingleton();
            Debug.Log("宝箱管理器单例已卸载");
        }
        var shopManager = ShopManager.FindInstance();
        if (shopManager != null)
        {
            shopManager.UnloadSingleton();
            Debug.Log("商店管理器单例已卸载");
        }
        var chatManager = ChatManager.FindInstance();
        if (chatManager != null)
        {
            chatManager.UnloadSingleton();
            Debug.Log("对话管理器单例已卸载");
        }

    }
    //****************************临时场景打开方法********************************
    public void GetBattle()
    {
        PauseTargetScene(Global_PlayerData.Instance.Map);//暂停地图场景
        LoadSceneAdditive(2);//打开序号为2的场景
    }
    public void GetFire()
    {
        PauseTargetScene(Global_PlayerData.Instance.Map);
        LoadSceneAdditive(3);
    }
    public void GetDelete()
    {
        PauseTargetScene(Global_PlayerData.Instance.Map);
        LoadSceneAdditive(4);
    }
    public void GetShop()
    {
        PauseTargetScene(Global_PlayerData.Instance.Map);
        LoadSceneAdditive(5);
    }
    public void GetBag()
    {
        PauseTargetScene(Global_PlayerData.Instance.Map);
        LoadSceneAdditive(6);
    }
    public void GetChoose()
    {
        PauseTargetScene(Global_PlayerData.Instance.Map);
        LoadSceneAdditive(7);
    }

    public void GetBox()
    {
        PauseTargetScene(Global_PlayerData.Instance.Map);
        LoadSceneAdditive(8);
    }

    //*****************************主场景切换方法*****************************
    public void ChangeMainScene(int oldScene, int newScene)
    {
        // 启动协程处理场景切换，保证时序正确
        StartCoroutine(ChangeMainSceneCoroutine(oldScene, newScene));
    }

    // 用协程处理场景切换
    private IEnumerator ChangeMainSceneCoroutine(int oldScene, int newScene)
    {
        //叠加加载新场景
        Scene targetScene = SceneManager.GetSceneByBuildIndex(newScene);
        if (!targetScene.isLoaded)
        {
            // 加载新场景（叠加模式）
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
            // 等待加载完成
            yield return loadOp;
        }

        //激活新场景
        targetScene = SceneManager.GetSceneByBuildIndex(newScene);
        if (targetScene.isLoaded)
        {
            SceneManager.SetActiveScene(targetScene);
        }
        else
        {
            Debug.LogError($"新场景 {newScene} 加载失败，无法切换");
            yield break;
        }

        //异步卸载老场景
        Scene sceneToUnload = SceneManager.GetSceneByBuildIndex(oldScene);
        if (sceneToUnload.isLoaded)
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload);
            // 等待卸载完成
            yield return unloadOp;
            //Debug.Log($"老场景 {oldScene} 已成功卸载");
        }
    }

    public void BackManage()//返回主菜单
    {
        //GameObject.Find("DataManager").GetComponent<PlayerData>().SavePlayerData();//保存所有数据
        SceneManager.LoadScene(0);//退出到主菜单
        MapManager.Instance?.UnloadSingleton();//卸载地图管理器单例
        PlayerData.Instance?.UnloadSingleton();//卸载数据管理器单例
        //CardStore没有Start方法，因此无需卸载
        //Debug.Log("成功退出");
    }


    //***********************************执行函数******************************************


    // 叠加加载场景（如目标场景存在，则直接打开，不重新加载）
    private void LoadSceneAdditive(int sceneIndex)
    {
        // 1. 检查场景是否已加载（避免重复加载）
        Scene targetScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
        if (!targetScene.isLoaded)
        {
            // 叠加式加载场景（保留当前所有已加载场景）
            SceneManager.LoadScene(sceneIndex, LoadSceneMode.Additive);
            // 等待场景加载完成后再激活（避免加载中切换导致的异常）
            StartCoroutine(ActivateSceneAfterLoad(sceneIndex));
        }
        else
        {
            // 场景已加载，直接激活显示
            SceneManager.SetActiveScene(targetScene);
        }
    }

    // 协程：等待场景加载完成后激活
    private IEnumerator ActivateSceneAfterLoad(int sceneIndex)
    {
        // 等待场景加载完成
        while (!SceneManager.GetSceneByBuildIndex(sceneIndex).isLoaded)
        {
            yield return null;
        }
        // 激活目标场景（使其成为显示/交互的场景）
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sceneIndex));
    }

    //卸载指定场景
    public void UnloadScene(int sceneIndex)
    {
        Scene sceneToUnload = SceneManager.GetSceneByBuildIndex(sceneIndex);
        if (sceneToUnload.isLoaded)
        {
            SceneManager.UnloadSceneAsync(sceneToUnload);
        }
    }

    // 暂停指定场景（仅修改标记，不影响全局时间）
    public void PauseTargetScene(int sceneIndex)
    {
        //1号场景（主地图）存在时才会暂停，主城（调试用）不会
        Scene scene = SceneManager.GetSceneByBuildIndex(sceneIndex);
        if (!scene.isLoaded) return;

        // 找到该场景的暂停标记
        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            int task = 0;
            SceneTimeMarker marker = SceneTimeMarker.Instance;
            if (marker != null)
            {
                marker.isPaused = true; // 标记该场景暂停
                //Debug.Log("场景已暂停");
                task++;
            }
            // 处理根对象下的Canvas（直接是根对象的Canvas）
            if (rootObj.GetComponent<Canvas>() != null)
            {
                rootObj.SetActive(false);
                task++;
            }
            //如果所有任务完成则提前结束
            if (task >= 2)
            {
                break;
            }
        }
    }

    // 恢复指定场景
    private void ResumeTargetScene(int sceneIndex)
    {
        Scene scene = SceneManager.GetSceneByBuildIndex(sceneIndex);
        if (!scene.isLoaded) return;

        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            int task = 0;
            SceneTimeMarker marker = SceneTimeMarker.Instance;
            if (marker != null)
            {
                marker.isPaused = false; // 标记该场景恢复
                task++;
            }
            // 处理根对象下的Canvas（直接是根对象的Canvas）
            if (rootObj.GetComponent<Canvas>() != null)
            {
                rootObj.SetActive(true);
                task++;
            }
            //如果所有任务完成则提前结束
            if (task >= 2)
            {
                break;
            }
        }
    }

}
