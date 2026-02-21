using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameCanvas : MonoBehaviour
{
    //public GameObject DataManager;//从数据管理器获取玩家数据
    //private PlayerData PlayerData;

    void Awake()
    {
        //PlayerData = DataManager.GetComponent<PlayerData>();
    }

    //重新开始经典模式
    public void StartGameButton()
    {
        //执行数据重置函数（不必调用PlayerData）
        ResetPlayerData();
        Global_PlayerData.Instance.newGame = true;
        SceneManager.LoadScene(1);//打开序号为1的场景
    }
    public void QuitGameButton()
    {
        Application.Quit();
    }
    //继续经典模式
    public void LoadGameButton()
    {
        Global_PlayerData.Instance.newGame = false;
        SceneManager.LoadScene(1);
    }

    //开启战役模式
    public void StartWar()
    {
        SceneManager.LoadScene(9);//打开序号为9的场景
    }


    //一键重置玩家数据
    public void ResetPlayerData()
    {
        string originalPath;
        string backupPath;
        //重置PlayerData.csv
        originalPath = Application.dataPath + "/Datas/Save/PlayerData.csv";
        backupPath = Application.dataPath + "/Datas/Normal/PlayerData.csv";
        ResetPlayerData(originalPath, backupPath);
        //清空玩家卡组
        ClearCsvFile(Application.dataPath + "/Datas/Save/PlayerCard.csv");
        //清空同伴卡组
        ClearCsvFile(Application.dataPath + "/Datas/Save/MateCard0.csv");
        ClearCsvFile(Application.dataPath + "/Datas/Save/MateCard1.csv");
        ClearCsvFile(Application.dataPath + "/Datas/Save/MateCard2.csv");
        ClearCsvFile(Application.dataPath + "/Datas/Save/MateCard3.csv");
        Debug.Log("玩家数据已重置为默认状态。");
    }

    //从备份中重置指定文件
    public void ResetPlayerData(string originalPath, string backupPath)
    {
        try
        {
            // 1. 检查备份文件是否存在
            if (!File.Exists(backupPath))
            {
                Debug.LogError($"备份文件不存在：{backupPath}");
                return;
            }

            // 2. 若原有文件存在，先删除（避免复制时因文件占用报错）
            if (File.Exists(originalPath))
            {
                // 尝试释放文件占用（若有），再删除
                File.SetAttributes(originalPath, FileAttributes.Normal);
                File.Delete(originalPath);
                //Debug.Log($"已删除原有文件：{originalPath}");
            }

            // 3. 复制备份文件到原有文件路径
            File.Copy(backupPath, originalPath, true); // true表示覆盖（此处实际已删除，仅做冗余处理）
            //Debug.Log($"备份文件已成功复制并替换原有文件：\n备份源：{backupPath}\n目标：{originalPath}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"文件操作失败（IO异常）：{ex.Message}");
        }
        catch (System.UnauthorizedAccessException ex)
        {
            Debug.LogError($"文件操作失败（权限不足）：{ex.Message}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"文件操作失败（未知错误）：{ex.Message}");
        }
    }

    //清空指定CSV文件内容
    public void ClearCsvFile(string csvPath)
    {
        try
        {
            // 检查文件是否存在，不存在则创建空文件（可选）
            if (File.Exists(csvPath))
            {
                // 写入空字符串清空内容
                File.WriteAllText(csvPath, string.Empty, System.Text.Encoding.UTF8);
                //Debug.Log($"CSV文件已清空：{csvPath}");
            }
            else
            {
                // 若文件不存在，创建空文件（根据需求选择是否执行）
                File.Create(csvPath).Close(); // 需手动关闭文件流，避免占用
                Debug.Log($"CSV文件不存在，已创建空文件：{csvPath}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"清空CSV文件失败：{ex.Message}");
        }
    }
}
