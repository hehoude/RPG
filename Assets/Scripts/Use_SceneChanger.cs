using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Use_SceneChanger : MonoBehaviour
{
    //由于场景切换器单例不可被卸载，所以不能由按钮直接引用，故借这个脚本访问
    public void OpenBag()
    {
        SceneChanger.Instance.GetBag();
    }

    public void BackManage()
    {
        SceneChanger.Instance.BackManage();
    }
}
