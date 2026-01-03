using UnityEngine;
using UnityEngine.Tilemaps;

public class Tilemap_Tree : MonoBehaviour
{
    [Header("瓦片设置")]
    public Tilemap targetTilemap; // 目标Tilemap
    public TileBase[] tilePrefabs; // 随机选择的Tile数组
    public int mapWidth; // 地图宽度（单元格数）
    public int mapHeight; // 地图高度（单元格数）
    //public Vector2Int startPos = new Vector2Int(-10, -10); // 地图起始位置
    private Vector2Int startPos; //地图起始位置

    [Header("树木生成概率%")]
    public int ran;

    [Header("噪声设置（可选）")]
    public float noiseScale = 0.1f;//越大噪声变化越急促

    private void Awake()
    {
        //startPos = new Vector2Int(-mapWidth/2, -mapHeight/2);
    }

    public void GenerateRandomMap(int _seed, int _cenx, int _ceny)
    {
        if (targetTilemap == null || tilePrefabs.Length == 0) return;
        //targetTilemap.ClearAllTiles();// 清空原有瓦片
        // 设置随机数
        Random.InitState(_seed + _cenx * 100 + _ceny * 1000);
        //确定起始位置
        startPos = new Vector2Int(_cenx - mapWidth / 2, _ceny - mapHeight / 2);
        // 遍历地图区域随机填充（边缘不放置）
        for (int x = 2; x < mapWidth-2; x++)
        {
            for (int y = 2; y < mapHeight-2; y++)
            {
                //设置概率门槛
                if (ran < Random.Range(1, 101))
                {
                    continue;
                }
                //设置瓦片位置
                Vector3Int tilePos = new Vector3Int(startPos.x + x, startPos.y + y, 0);
                //随机选择一个瓦片
                TileBase randomTile = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
                //放置瓦片
                targetTilemap.SetTile(tilePos, randomTile);
            }
        }
    }

    //生成噪声地图（可供其它脚本调用）
    public void GenerateNoiseMap(int _seed, int _cenx, int _ceny)
    {
        //确定起始位置
        startPos = new Vector2Int(_cenx - mapWidth / 2, _ceny - mapHeight / 2);
        Random.InitState(_seed+200);

        for (int x = 2; x < mapWidth - 2; x++)
        {
            for (int y = 2; y < mapHeight - 2; y++)
            {
                Vector3Int tilePos = new Vector3Int(startPos.x + x, startPos.y + y, 0);
                int seedOffset = _seed % 10000;//避免种子过大导致的浮点数精度问题
                //计算噪声值
                float noiseValue = Mathf.PerlinNoise(
                    (startPos.x + x + seedOffset * 1000) * noiseScale,
                    (startPos.y + y + seedOffset * 1000) * noiseScale
                );
                //根据噪声值选择瓦片
                TileBase selectedTile;
                if (noiseValue < 0.3f)
                {
                    selectedTile = tilePrefabs[0]; // 水域
                    targetTilemap.SetTile(tilePos, selectedTile);
                }
            }
        }
    }
}