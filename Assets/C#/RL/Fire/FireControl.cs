using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public partial class FireControl : MonoBehaviour
{


    //4.21：目前性能消耗过于严重，在运行时能感受到明显的卡顿，进行优化
    //

    // 四周地块的状态(是否可以生成新的火源)
    private List<bool> _surroundingsStatus;

    // 检查地板快的顺序
    private readonly List<Vector3> _directionSequence = new() { Vector3.left/2, Vector3.right / 2, Vector3.forward / 2, Vector3.back / 2 };

    // 火焰预制体
    public GameObject firePrefab;

    // 存放临时火焰的空物体
    public GameObject tmpFireWrapper;

    // 当前环境
    public EnvControl myEnv;

    // 当前位置的火焰是否还能向四周蔓延
    private bool isPossibleToSpread = true;

    private int spreadingCountDown;

    private static readonly WaitForSeconds waitTime = new WaitForSeconds(0.1f);
    //第6帧（0.1秒后）：100个火焰同时执行检查（假设60FPS）
    //所以还是会出现火焰重叠的情况，所以采用随机时间进行扩散
    private static int activeFireCount = 0;
    private const int maxActiveFires = 500; // 最大活跃火焰数量

    private static SpatialPartition fireSpatialPartition = new SpatialPartition(1f);


    void Start() //初始化
    {
        _surroundingsStatus = new List<bool> { true, true, true, true };
       // print("火焰的初始化函数");
        this.enabled = true;    //启动脚本  
        isPossibleToSpread = true;

        // 随机化扩散计时器
        spreadingCountDown = Random.Range(50, 100);

        // 初始检查周围状态
        StartCoroutine(DelayedInitialCheck());

        // 注册到空间分区
        fireSpatialPartition.Add(transform.position, this);
    }


    private IEnumerator DelayedInitialCheck()
    {
        yield return new WaitForSeconds(Random.Range(0.05f, 0.3f));
        CheckSurroundingStatus();
    }

    // Update is called once per frame
    void FixedUpdate() //开始扩散,这样写是每一帧该火焰都会进行扩散！！！！
    {
        if (!isPossibleToSpread) return;

        if (spreadingCountDown <= 0)
        {
            StartCoroutine(TrySpreadFire());
            enabled = false; // 扩散后禁用
        }
        spreadingCountDown--;
        /*_isPossibleToSpread = IsPossibleToSpread();
        if (_isPossibleToSpread is false)
        {
            this.enabled = false;   //当不能扩散时就ban掉
            return;
        }

        if (spreadingCountDown == 0)//计数为0开始扩散
        {
            for (int Index = 0; Index < _surroundingsStatus.Count; Index++) //依次检测四周进行扩散
            {
                //要加时间，不然会直接爆炸！！！！！！！！！！！！
                if (_surroundingsStatus[Index]) //如果可以进行扩散
                {
                    Vector3 blockPosition = Vector3.zero;
                    switch (Index)
                    {
                        case 0: blockPosition = this.gameObject.transform.position + _directionSequence[0]; break;
                        case 1: blockPosition = this.gameObject.transform.position + _directionSequence[1]; break;
                        case 2: blockPosition = this.gameObject.transform.position + _directionSequence[2]; break;
                        case 3: blockPosition = this.gameObject.transform.position + _directionSequence[3]; break;
                    }

                    if (activeFireCount<=maxActiveFires) { //火焰超过一定数量之后就不再进行扩散
                        GameObject tempFire = Instantiate(firePrefab, blockPosition, Quaternion.identity);
                        tempFire.GetComponent<FireControl>().myEnv = myEnv;
                        myEnv.FireList.Add(tempFire.GetComponent<FireControl>());
                        tempFire.transform.parent = this.gameObject.transform.parent;
                        activeFireCount++;
                    }
                }
            }
            
        }
     spreadingCountDown--;
        return;*/
    }

    private void OnTriggerEnter(Collider collision)//碰撞发生，当碰撞的物体是墙时，消除该火焰，当碰撞物体是门是，销毁门
    {
        GameObject triggerObject = collision.gameObject;
        print("碰撞物体是"+collision.gameObject.tag);
       
    }
}
