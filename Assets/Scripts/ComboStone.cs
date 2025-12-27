using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboStone : MonoBehaviour
{
    public Image Stone;//指示器石头颜色
    public GameObject Fire;//指示器火焰

    void Start()
    {
        Fire.SetActive(false);
    }
}
