using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.MLAgents;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public partial class EnvControl : MonoBehaviour
{
   
    //人类列表
    public List<HumanControl> personList = new();
    //机器人列表
    public List<RobotControl> RobotList = new();
    // 机器人大脑列表
    public List<RobotBrain> BrainList = new();

    public List<FireControl>FireList = new();

    //环境中的出口
    public List<GameObject> Exits=new();
    //存储房间的位置信息
    public List<Vector3> cachedRoomPositions;
 
    /*生成场景和导航时用到的组件 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
    public ComplexityControl complexityControl;//生成环境用到的组件
    public NavMeshSurface surface;//生成导航的组件
    public GameObject HumanPrefab;//生成人类用到的组件
    public GameObject RobotPrefab;//添加机器人用到的组件
    public GameObject BrainPerfab;//机器人大脑预制体
    public GameObject FirePerfab; //火焰预制体

    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public GameObject RobotParent;//机器人的父物体，减少性能消耗
    public GameObject FireParent;//机器人的父物体，减少性能消耗
    public GameObject RobotBrainParent;//机器人大脑的父物体，减少性能消耗，但目前这个用不到
    public GameObject humanParent;//机器人的父物体，减少性能消耗

    public float TotalSize;//区域总大小
    public int RoomNum;//房间数目
    // 是否在训练
    public bool isTraining;
    // 是否使用机器人
    public bool useRobot;
    //是否使用火焰智能体
    public bool useFireAgent;
   

    /*展示Demo使用，用于场景重置!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
    public int currentFloorhuman=0;
    public int FireStep;//计数器，用来生成火焰
    public int StepCount;//计数器，用来步数过多时重置场景
    public int MaxStep;//最大步数
    public int layoutNum;

   //引入火焰机制4.20 ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
    //手动添加火焰时的火焰位置缓存
    public List<Vector3> FirePosition;
    //添加的火焰数量
    public int FireNum;


    private void Start()
    {
        //预览模式，机器人使用贪心算法，人类使用自由移动，火焰人为控制生成地点!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        if (!isTraining)
        {
            Debug.Log("Env的start函数");

/*            if (currentFloorhuman == 0)//第二次场景开始条件，检测场景中的人类数量
            {*/
                CleanTheScene();
                string filename = "layout_900_15";
                string[] name = { "layout1", "layout2", "layout3", "layout4",
                "apartment", "family_house", "clinic", "elementary_school", "hospital_er","tech_office", "shopping_mall" };
                //string layoutname = name[layoutNum];

                string layoutname = "shopping_mall";
                //print("读取的布局名称为：" + layoutname);

                //!!!!!!!!!!!!!!!!!!!!!!!!场景实现
                complexityControl.BeginGenerationJsonLoad(filename, layoutname); //在这里指定加载数据的文件
                AddExits();//添加出口，以便于后续机器人导航使用
                surface.BuildNavMesh();//生成导航
                                       //!!!!!!!!!!!!!!!!!!!!!!!!场景实现

                AddPerson(10);
                AddRobot();//添加机器人

                //计算场景中有几个人类，对场景运行没有影响
                foreach (HumanControl human in personList)//统计当前楼层的人数
                {
                    if (human.isActiveAndEnabled)
                    {
                        currentFloorhuman++;
                    }
                    //Debug.Log(currentFloorhuman);
                }

                AddRobotBrain();//添加机器人大脑
                /*  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
                FireStep = 0;//训练决策数目
                MaxStep = 5000;

                /*  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
                //手动添加的火焰位置
                AddFirePosition();
                FireNum = 0;
            }
        /*}*/
        //预览模式，机器人使用贪心算法，人类使用自由移动，火焰人为控制生成地点!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    }

    private void FixedUpdate()
    {
        //预览模式，机器人使用贪心算法，人类使用自由移动，火焰人为控制生成地点!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        if (!isTraining)
        {
            FireStep++;
            if (FireStep % 200 == 0)
            {
                //(32,0,44) 右上角房间
                //(3,0,44) 左上角房间
                if (FireNum < FirePosition.Count)
                {
                    AddFire(FirePosition[FireNum]);
                    FireNum++;
                    FireStep++;
                }
                else { FireStep++; }
            }
            if (FireStep % 100 == 0)//每过一定帧更新导航地图
            {
                var surface = FindObjectOfType<NavMeshSurface>();
                if (surface != null)
                {
                    surface.UpdateNavMesh(surface.navMeshData);
                }
                FireStep++;
            }
        }
        else if (isTraining)
        {
            if (currentFloorhuman == 0)//当场景中的人类数量为0时，重新构建新一轮的训练场景
            {
                string filename = "layout_900_15";
                string[] name = { "layout1", "layout2", "layout3", "layout4",
                "apartment", "family_house", "clinic", "elementary_school", "hospital_er","tech_office", "shopping_mall" };
                //string layoutname = name[layoutNum];

                string layoutname = "shopping_mall";
                //print("读取的布局名称为：" + layoutname);
                CleanTheScene();
                //UnityEngine.Random.Range(8, 15); // 划分的房间数量
                complexityControl.BeginGenerationJsonLoad(filename, layoutname); //在这里指定加载数据的文件
                
                RecordRoomPosition(complexityControl.buildingGeneration.roomList);//记录房间的位置，用于后续的归一化训练


                AddExits();//添加出口，以便于后续机器人导航使用
                surface.BuildNavMesh();//生成导航

                AddPerson(10);

                foreach (HumanControl human in personList)//初始化统计当前楼层的人数，
                                                          //后续通过控制currentFloorhuman的增减，来决定是否开始新的回合，避免每一帧进行查找，影响性能
                {
                    if (human.isActiveAndEnabled)
                    {
                        currentFloorhuman++;
                    }
                    //Debug.Log(currentFloorhuman);
                }

                AddRobot();//添加机器人
                AddRobotBrain();//添加机器人大脑
                AddFirePosition();
                FireNum = 0;
                FireStep = 0;
                StepCount = 0;
                MaxStep = 2000;
                //myEnv.cachedDoorPositions=myEnv.GetAllDoorPositions();//添加门的位置信息
                //myEnv.cachedRoomPositions = myEnv.GetAllRoomPositions();//添加房间的位置信息
                BrainList[0].isInitialized = true;//初始化已完成，可以执行后续函数
            }


            // 添加火焰！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
            FireStep++;
            if (FireStep % 100 == 0)
            {
                //(32,0,44) 右上角房间
                //(3,0,44) 左上角房间
                if (FireNum < FirePosition.Count)
                {
                    AddFire(FirePosition[FireNum]);
                    FireNum++;
                    FireStep++;
                }
                else { FireStep++; }
            }
            if (FireStep % 100 == 0)//每过一定帧更新导航地图
            {
                var surface = FindObjectOfType<NavMeshSurface>();
                if (surface != null)
                {
                    surface.UpdateNavMesh(surface.navMeshData);
                }
                FireStep++;
            }
            // ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！

            // 步数超时重置环境！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
            StepCount++;
            if (StepCount > MaxStep)
            {
                BrainList[0].EpisodeInterrupted();//机器人终止该回合
                currentFloorhuman = 0;//重置当前场景人数，相当于重新开始训练
            }
            
            
            BrainList[0].RequestDecision(); //机器人智能体每一帧都要获取动作，来决定自己下一帧朝哪个方向移动
        }
    }

    public void CleanTheScene()
    {
        // print("执行CleanScene函数");
        // 1. 先停止所有火焰的传播
        foreach (var fire in FindObjectsOfType<FireControl>())
        {
            fire.StopAllCoroutines();
        }
        // 2. 执行常规清理
        ResetAgentandClearList();

        // 3. 强制垃圾回收（可选）
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
    }
    /// <summary>
    /// 重置所有机器人控制智能体
    /// </summary>
    private void ResetAgentandClearList()
    {
        if (!useRobot)
            return;

        // 清除人类
        if (personList.Count > 0)
        {
            foreach (HumanControl person in personList)
            {
                if (person != null && person.gameObject != null)
                {
                    Destroy(person.gameObject);
                }
            }
            personList.Clear();
        }

        // 清除机器人
        if (RobotList.Count > 0)
        {
            foreach (RobotControl robot in RobotList)
            {
                if (robot != null && robot.gameObject != null)
                {
                    Destroy(robot.gameObject);
                }
            }
            RobotList.Clear();
        }

        // 清除智能体，只清空智能体列表即可，实体不用清除
        if (BrainList.Count > 0)
        {
            BrainList.Clear();
        }

        // 清除出口
        if (Exits.Count > 0)
        {
            foreach (var exit in Exits)
            {
                if (exit != null && exit.gameObject != null)
                {
                    Destroy(exit.gameObject);
                }
            }
            Exits.Clear();
        }
        cachedRoomPositions.Clear();

        if (FireList.Count > 0)  //清除三个火源
        {
            foreach (FireControl fire in FireList)
            {
                if (fire != null && fire.gameObject != null)
                {
                    // 先执行火焰的自定义清理逻辑
                    fire.StopAllCoroutines();

                    // 直接从场景销毁，不返回对象池（因为整个场景要重置）
                    Destroy(fire.gameObject);
                }
            }
            FireList.Clear();
        }
        if (FirePoolManager.Instance != null)
        {
            FirePoolManager.Instance.ClearPool(); // 需要实现这个方法
        }
    }
    public void AddRobot()  //在这里动态添加机器人，以确保机器人是在环境生成之后才添加上去的，以确保机器人导航的正常使用
    {
        // 在场景中生成num个机器人，并把他们加入到List中
            Vector3 spawnPosition = Vector3.zero;
        // 尝试找到一个没有碰撞的位置
        // 随机生成位置
        //标注掉的只适用于矩形布局
        /*float randomX = UnityEngine.Random.Range(1f, complexityControl.buildingGeneration.totalWidth);
        float randomZ = UnityEngine.Random.Range(1f, complexityControl.buildingGeneration.totalHeight);*/

          spawnPosition = GetRandomPosInLayout();
            // 实例化机器人
            GameObject Robot = Instantiate(RobotPrefab, spawnPosition, Quaternion.identity);//实例化机器人的位置
            RobotList.Add(Robot.GetComponent<RobotControl>()); //将机器人加入列表
            Robot.transform.parent = RobotParent.transform;    //将机器人放在场景的RobotList物体下
    }

    public void AddRobotBrain()  //在这里添加机器人大脑. 并进行组装  机器人的数量较少而且确定，因此直接在场景中进行添加
    {
          GameObject RobotBrain = GameObject.Find("RobotBrain1");
          RobotBrain robotBrain = RobotBrain.GetComponent<RobotBrain>();
          BrainList.Add(robotBrain);
               /*  在这里进行机器人和大脑的脚本初始化工作！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！*/
              RobotControl robot = RobotList[0];
            robot.myAgent=robotBrain;
            robotBrain.robot = robot.gameObject;
            robotBrain.robotNavMeshAgent=robot.GetComponent<NavMeshAgent>();
            robotBrain.robotInfo = robot;
            robotBrain.robotRigidbody=robot.GetComponent<Rigidbody>();
    }
    public void AddPerson(int num)
    {
        print("添加人类函数");
        // 在场景中生成num个人类，并把他们加入到personList中
        for (int i = 0; i < num; i++)
        {
  
            Vector3 spawnPosition = Vector3.zero;
      
            // 尝试找到一个没有碰撞的位置
                spawnPosition = GetRandomPosInLayout();

                // 实例化人类
                GameObject Person = Instantiate(HumanPrefab, spawnPosition, Quaternion.identity);
                personList.Add(Person.GetComponent<HumanControl>());
                Person.transform.parent = humanParent.transform;
                Person.GetComponent<HumanControl>().myEnv = this;
                Person.GetComponent<HumanControl>().Start();//初始化人类的各个变量  

        }

       // Debug.Log("执行生成人类函数");
    }
    public void AddExits() //将出口添加到出口列表当中
    {
         GameObject Exit = GameObject.Find("Exit");
        if (Exit != null)
        {
            Exits.Add(Exit);
         //   Debug.Log("执行添加出口函数，赋值");
        }
        else { Debug.Log("没有找到出口的游戏物体"); }
       
    }

    public void AddFire(Vector3 FirePosition) //添加火焰，只需传入x，z坐标即可
    {
        //随机生成火焰位置
        /*Vector3 spawnPosition = Vector3.zero;
        spawnPosition = GetRandomPosInLayout();
        spawnPosition.x = Mathf.Round(spawnPosition.x);
        spawnPosition.z = Mathf.Round(spawnPosition.z);//四舍五入取整*/
        // 检查是否达到最大火焰数量


        // 使用对象池安全获取
        GameObject fire = FirePoolManager.Instance.GetFire(FirePosition + new Vector3(0, 0.5f, 0), Quaternion.identity, this);

        if (fire != null)
        {
            var fireControl = fire.GetComponent<FireControl>();
            if (fireControl != null)
            {
                FireList.Add(fireControl);
            }
            else
            {
                Debug.LogError("生成的火焰缺少FireControl组件");
                FirePoolManager.Instance.ReturnFire(fire);
            }
        }

        }

    /// <summary>
    /// 获取房间内无碰撞的随机位置
    /// 加权随机分布,较大概率生成在大房间内部，但又保证房间的随机性
    /// </summary>
    public Vector3 GetRandomPosInLayout() {
        //随机选取一个房间，在随机房间内选取随机点
        List<Room> rooms = complexityControl.buildingGeneration.roomList;
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogError("房间列表为空！");
            return Vector3.zero;
        }
        System.Random _random = new System.Random();
        // _random.Next(maxValue)
        //作用：返回 0 到 maxValue-1 之间的随机整数
        //NextDouble() 生成的是一个范围在 [0, 1) 的浮动值
        Room room = rooms[_random.Next(rooms.Count)];
        // 计算有效范围
        float xMin = room.xzPosition.x + room.width/4;
        float xMax = room.xzPosition.x + room.width *3/4; // 正确右边界
        float zMin = room.xzPosition.z + room.height / 4;
        float zMax = room.xzPosition.z + room.height * 3/4; // 正确上边界

        float x = xMin + (float)_random.NextDouble() * (xMax - xMin);
        float z = zMin + (float)_random.NextDouble() * (zMax - zMin);

        Vector3 RandomPos= new Vector3(x,0.5f,z);

        return RandomPos;
    }



    public void AddFirePosition()//找到场景中的左上，右上，右下三个房间，将他们的中心作为三个火源的位置
    {
        if (complexityControl.buildingGeneration.roomList == null || complexityControl.buildingGeneration.roomList.Count == 0)
        {
            Debug.LogError("房间列表为空！");
            return;
        }

        // 初始化关键变量（单次遍历完成所有计算）
        Room topLeftRoom = null;
        Room topRightRoom = null;
        Room bottomRightRoom = null;
        float minX = float.MaxValue;
        float maxX = float.MinValue;

        // 单次遍历处理所有逻辑
        foreach (Room room in complexityControl.buildingGeneration.roomList)
        {
            float x = room.xzPosition.x;
            float z = room.xzPosition.z;

            // 更新左上房间逻辑
            if (x < minX || (x == minX && z > topLeftRoom?.xzPosition.z))
            {
                minX = x;
                topLeftRoom = room;
            }
            else if (x == minX && z > topLeftRoom.xzPosition.z)
            {
                topLeftRoom = room;
            }

            // 更新右上和右下房间逻辑
            if (x > maxX)
            {
                maxX = x;
                topRightRoom = room;
                bottomRightRoom = room;
            }
            else if (x == maxX)
            {
                // 更新右上（取Z最大）
                if (z > topRightRoom.xzPosition.z)
                {
                    topRightRoom = room;
                }
                // 更新右下（取Z最小）
                if (z < bottomRightRoom.xzPosition.z)
                {
                    bottomRightRoom = room;
                }
            }
        }

        // 计算中心点
        Vector3 topLeftCenter = CalculateRoomCenter(topLeftRoom);
        Vector3 topRightCenter = CalculateRoomCenter(topRightRoom);
        Vector3 bottomRightCenter = CalculateRoomCenter(bottomRightRoom);

        FirePosition.Add(topLeftCenter);
        FirePosition.Add(topRightCenter);
        FirePosition.Add(bottomRightCenter);
    }

    // 保持原有中心计算方法
    private Vector3 CalculateRoomCenter(Room room) =>
        new Vector3(
            room.xzPosition.x + room.width / 2f,
            room.xzPosition.y,
            room.xzPosition.z + room.height / 2f
        );

    private void RecordRoomPosition(List<Room> roomList)
    {
        foreach (Room room in roomList)
        {
            cachedRoomPositions.Add(new Vector3(
                room.xzPosition.x+room.width/2,
                0,
                room.xzPosition.z+room.height/2));
        }
    }
}
