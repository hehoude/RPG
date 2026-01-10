using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComboList
{
    //连携数组列表
    public static int[] GetComboArray(int comboNum)
    {
        // 根据编号返回对应的数组，所有配置都在这里，一目了然
        switch (comboNum)
        {
            case 0: return new int[] { 1, 2, 3 };  // 重装战士连携 - 3个元素
            case 1: return new int[] { 1, 2, 3 };  // 刺客连携 - 3个元素
            case 2: return new int[] { 1, 2, 3 };  // 元素使连携 - 3个元素
            case 10: return new int[] { 1, 1, 1 };  // 火龙战士连携 -3个元素
            case 11: return new int[] { 2, 2, 2 };  // 巫毒猎手连携 -3个元素
            case 12: return new int[] { 3, 3, 3 };  // 雷电守卫连携 -3个元素


            // 默认值：防止传了未知编号导致空指针
            default:
                Debug.LogError("未知连携！");
                return new int[] { 1, 1, 1 };
        }
    }

    //连携介绍文本
    public static string GetText(int comboNum)
    {
        string _text = "";
        switch (comboNum)
        {
            case 0:
                _text = "信念之盾\n获得6点格挡";
                break;
            case 1:
                _text = "猎杀之血\n获得1点力量";
                break;
            case 2:
                _text = "元素之力\n获得1点能量";
                break;
            case 10:
                _text = "炼狱审判\n获得2点火焰附加";
                break;
            case 11:
                _text = "移形换影\n抽2张牌";
                break;
            case 12:
                _text = "雷霆守护\n获得1点坚固";
                break;
        }
        return _text;
    }
}
