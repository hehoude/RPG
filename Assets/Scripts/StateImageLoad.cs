using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

// 继承你已有的单例基类
public class StateImageLoad : MonoSingleton<StateImageLoad>
{
    // 缓存所有状态的精灵（key=状态ID，value=对应的Sprite）
    private Dictionary<int, Sprite> _stateSpriteCache = new Dictionary<int, Sprite>();

    // 可选：配置状态ID范围和图片根路径（在Inspector调整，无需改代码）
    [Header("状态图片配置")]
    [SerializeField] private int _maxStateId = 5; // 需要预加载的状态数量
    [SerializeField] private string _imageRootPath = "/Image/State/"; // 相对DataPath的路径

    protected override void Awake()
    {
        base.Awake();
        // 游戏启动时提前加载所有状态图片到缓存（仅执行一次）
        PreloadAllStateSprites();
    }

    // 核心：提前加载所有本地图片到缓存（仅启动时执行一次）
    private void PreloadAllStateSprites()
    {
        // 拼接完整的图片根路径（和你原有的路径逻辑一致）
        string rootPath = Application.dataPath + "/Image/State/";

        // 遍历所有状态ID，提前加载并缓存精灵
        for (int stateId = 0; stateId < _maxStateId; stateId++)
        {
            string imagePath = $"{rootPath}/{stateId}.png";
            // 提前加载并缓存精灵（复用你原有的图片加载逻辑）
            Sprite sprite = LoadSpriteFromLocalFile(imagePath);
            if (sprite != null)
            {
                _stateSpriteCache[stateId] = sprite;
                //Debug.Log($"提前加载状态{stateId}的图片成功：{imagePath}");
            }
            else
            {
                Debug.LogWarning($"状态{stateId}的图片不存在/加载失败：{imagePath}");
            }
        }
    }

    // 从本地文件加载Sprite
    private Sprite LoadSpriteFromLocalFile(string imagePath)
    {
        // 检查文件是否存在
        if (!File.Exists(imagePath))
        {
            return null;
        }

        try
        {
            // 读取图片字节（仅提前加载时执行一次，而非每次创建对象）
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(imageBytes))
            {
                // 创建精灵并返回
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
                return sprite;
            }
            Destroy(texture); // 加载失败释放纹理
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载本地图片异常：{e.Message}");
        }
        return null;
    }

    // 对外提供：获取指定状态的精灵（直接从缓存取，无耗时）
    public Sprite GetStateSprite(int stateId)
    {
        // 优先从缓存取（核心优化：无文件IO、无纹理创建）
        if (_stateSpriteCache.TryGetValue(stateId, out Sprite sprite))
        {
            return sprite;
        }

        // 缓存未命中时，临时加载（应对偶发的未提前配置的ID）
        string imagePath = $"{Application.dataPath}{_imageRootPath}/{stateId}.png";
        sprite = LoadSpriteFromLocalFile(imagePath);
        if (sprite != null)
        {
            _stateSpriteCache[stateId] = sprite; // 加入缓存，避免下次再加载
        }
        return sprite;
    }
}