using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Event
{
    /*标准写法
    //出牌事件
    public static event Action SendCard;
    public static void CallSendCard()
    {
        SendCard?.Invoke();
    }
    */

    /*使用方法
    事件发起方法：
    Event.CallSendCard();//调用这里的静态函数发起事件
    将函数注册（挂载）到这个事件上：
    Event.SendCard += hanshu();//这样做了就可以在事件触发时，附带执行这个函数
    别忘了不用的时候卸载
    Event.SendCard -= hanshu();
    */

    //战斗管理器发出的出牌属性信号
    public static event Action<int> SendCard;
    public static void CallSendCard(int type)
    {
        SendCard?.Invoke(type);//将参数type传递给所有注册的函数
    }

    //回合结束事件
    public static event Action TurnEnd;
    public static void CallTurnEnd()
    {
        TurnEnd?.Invoke();
    }

}
