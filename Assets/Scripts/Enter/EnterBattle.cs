using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;//文件读写

public class EnterBattle : MonoBehaviour
{
    public GameObject SpriteObject;//2D精灵对象（挂载SpriteRenderer的对象）
    public int ImageId = 0;//敌人图片ID
    public int[] enemies;//敌人编号集
    public int currentId = 0;//当前对象数据层的ID

    void Start()
    {
        LoadImage(ImageId);//加载图片
    }

    //当玩家走进触发器范围时
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //Debug.Log("OnTriggerEnter2D Fired!");
            //使用敌人编号集合改写战斗信息文件
            SavePlayerData();
            if (MapManager.Instance != null)
            {
                //传递地图管理器（单例）当前交互的对象
                MapManager.Instance.CurrentObject = this.gameObject;
            }
            //找到场景切换器，切换至战斗场景
            SceneChanger.Instance.GetBattle();
            //告知全局数据当前交互对象的ID
            Global_PlayerData.Instance.CurrentId = currentId;
            //摧毁自身（防止重复触发）
            Destroy(gameObject);
        }
    }

    public void SavePlayerData()
    {
        // 定义保存路径
        string folderPath = Application.dataPath + "/Datas";
        string filePath = folderPath + "/BattleMessage.csv";

        // 如果文件夹不存在，创建文件夹
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("Datas 文件夹不存在，已创建：" + folderPath);
        }

        // 如果 CSV 文件不存在，先创建一个空文件（也可以不创建，File.WriteAllLines 会自动创建）
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose(); // 创建文件后释放资源
            Debug.Log("BattleMessage.csv 文件不存在，已创建：" + filePath);
        }

        // 准备写入内容
        List<string> datas = new List<string>();
        datas.Add("#,敌人编号");
        for (int i = 0; i < enemies.Length; i++)
        {
            datas.Add("enemy," + enemies[i].ToString());
        }

        // 写入 CSV
        File.WriteAllLines(filePath, datas);
        //Debug.Log("敌人信息写入完成：" + filePath);
    }

    //图片加载函数
    public void LoadImage(int _id)
    {
        string imageEnemy = Application.dataPath + "/Image/Enemy/" + _id.ToString() + ".png";
        ChooseImage(SpriteObject, imageEnemy);
    }

    //替换图片
    public void ChooseImage(GameObject targetObject, string imagePath)
    {
        SpriteRenderer spriteRenderer = targetObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"{targetObject.name} 上未挂载SpriteRenderer组件！");
            return;
        }
        // 检查文件是否存在
        if (!File.Exists(imagePath))
        {
            Debug.LogError($"图片文件不存在：{imagePath}");
            return;
        }
        byte[] imageBytes = File.ReadAllBytes(imagePath);// 读取图片文件字节数据
        // 创建纹理并加载图片数据
        Texture2D texture = new Texture2D(2, 2); // 初始尺寸任意，LoadImage会自动调整
        if (texture.LoadImage(imageBytes))
        {
            // 将纹理转换为精灵
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height), // 精灵矩形区域（整个纹理）
                new Vector2(0.5f, 0.5f), // 精灵中心点（中心位置）
                16f
            );

            // 设置Image组件的精灵
            spriteRenderer.sprite = sprite;
        }
        else
        {
            Debug.LogError($"加载图片数据失败：{imagePath}");
            Destroy(texture); // 释放无效纹理
        }
    }
}

