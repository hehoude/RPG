using UnityEngine;

//别忘了把玩家对象的标签改为Player

public class CameraController : MonoBehaviour
{
    Transform target;
    Vector3 velocity = Vector3.zero;

    [Range(0, 1)]
    public float smoothTime;//相机跟随延迟时间

    public Vector3 positionOffset;

    [Header("Axis Limitation")]
    public Vector2 xLimit;//相机位置限制
    public Vector2 yLimit;

    private void Awake()
    {
        //target = GameObject.Find("Player").transform;
        FindPlayer();
    }
    private void LateUpdate()
    {
        // 核心修复：先检查target是否有效，无效则重新查找
        if (!IsTargetValid())
        {
            FindPlayer();
            // 重新查找后仍无效，直接返回，避免报错
            if (!IsTargetValid())
            {
                return;
            }
        }

        //这里增加了偏移量positionOffset后，可以在外部把偏移量的Z坐标调整为负值
        Vector3 targetPosition = target.position + positionOffset;
        //限制相机位置
        //这里的-10为Z轴限制，可随意调整
        //外部可设置xLimit.x和xLimit.y，这实际上是最小值与最大值
        targetPosition = new Vector3(Mathf.Clamp(targetPosition.x,xLimit.x,xLimit.y),Mathf.Clamp(targetPosition.y, yLimit.x, yLimit.y),-10);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    /// <summary>
    /// 查找Player并赋值target（封装成方法，方便重复调用）
    /// </summary>
    private void FindPlayer()
    {
        // 优先用Tag查找（更稳定，不受名字修改影响）
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
        else
        {
            target = null;
            Debug.LogWarning("未找到标签为Player的对象！请检查Player的标签是否正确");
        }
    }

    /// <summary>
    /// 检查target是否有效（未被销毁、不为空）
    /// </summary>
    private bool IsTargetValid()
    {
        // Unity特有的判断：销毁的对象引用不会自动置空，需用ReferenceEquals
        if (target == null) return false;
        if (Object.ReferenceEquals(target, null)) return false;
        return true;
    }
}