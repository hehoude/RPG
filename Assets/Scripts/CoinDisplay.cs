using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinDisplay : MonoBehaviour
{
    public Text Text;
    private int coins;
    private Global_PlayerData Global_PlayerData;

    void Awake()
    {
        Global_PlayerData = Global_PlayerData.Instance;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (coins != Global_PlayerData.coins)
        {
            coins = Global_PlayerData.coins;
            RefreshCoin();
        }
    }

    //显示金币数量
    public void RefreshCoin()
    {
        Text.text = coins.ToString();
    }
}
