using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateManager : MonoBehaviour
{
    public GameObject State_Prefab;//状态图标预制体

    //添加状态主要图标，接收状态id与添加位置，返回图标对象
    public GameObject AddState(int _state, Transform _stateLab)
    {
        GameObject NewState = Instantiate(State_Prefab, _stateLab);//添加状态图标
        StateDisplay stateDisplay = NewState.GetComponent<StateDisplay>();//获取脚本
        stateDisplay.id = _state;//赋予状态id
        return NewState;
    }

}
