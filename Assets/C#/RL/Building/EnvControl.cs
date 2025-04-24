using System.Collections.Generic;
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
    //存储房间和门的位置信息
    public List<Vector3> cachedRoomPositions;
    public List<Vector3> cachedDoorPositions;
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

    public int StepCount;//计数器
    public int MaxStep;//最大步数

    public int layoutNum;

    private int fireSpawnInterval = 100; // 可配置的生成间隔

    private Transform fireParent; // 缓存父物体

   //引入火焰机制4.20 ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
    //手动添加火焰时的火焰位置缓存
    public List<Vector3> FirePosition;
    //添加的火焰数量
    public int FireNum;


    private void Start()
    {
        layoutNum = 10; //5有bug
        Debug.Log("Env的start函数");

        if (currentFloorhuman == 0)
        {
            CleanTheScene();
            int number2 = 15; // 划分的房间数量
            TotalSize = 900;
            RoomNum = number2;
            string filename = "layout_900_15";
            string[] name = { "layout1", "layout2", "layout3", "layout4",
                "apartment", "family_house", "clinic", "elementary_school", "hospital_er","tech_office", "shopping_mall" };
            //string layoutname = name[layoutNum];
            string layoutname = "shopping_mall";
            print("读取的布局名称为：" + layoutname);

            //!!!!!!!!!!!!!!!!!!!!!!!!场景实现
            complexityControl.BeginGenerationJsonLoad(filename,layoutname); //在这里指定加载数据的文件
            AddExits();//添加出口，以便于后续机器人导航使用
            surface.BuildNavMesh();//生成导航
            //!!!!!!!!!!!!!!!!!!!!!!!!场景实现


            AddPerson(10);
            AddRobot();//添加机器人
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
            StepCount = 0;//训练决策数目
            MaxStep = 5000;

            /*  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
            //手动添加的火焰位置
            FirePosition.Add(new Vector3(3, 0, 44));
            FirePosition.Add(new Vector3(32, 0, 44));
            FireNum = 0;
        }
    }

    private void FixedUpdate()
    {
        StepCount++;
        if (StepCount % 200==0) 
        {
            //(32,0,44) 右上角房间
            //(3,0,44) 左上角房间
            if (FireNum<FirePosition.Count)
            {
                AddFire(FirePosition[FireNum]);
                FireNum++;
                StepCount++;
            }
            else { StepCount++; }
        }
        if (StepCount % 100 == 0)//每过一定帧更新导航地图
        {
            var surface = FindObjectOfType<NavMeshSurface>();
            if (surface != null)
            {
                surface.UpdateNavMesh(surface.navMeshData);
            }
            StepCount++;
        }
        /*  Debug.Log("Env的start函数");

          if (currentFloorhuman == 0)
          {
              CleanTheScene();
              int number2 = 15; // 划分的房间数量
              TotalSize = 900;
              RoomNum = number2;
              string filename = "layout_900_15";
              string[] name = { "layout1", "layout2", "layout3", "layout4",
                  "apartment", "family_house", "clinic", "elementary_school", "hospital_er","tech_office", "shopping_mall", "general_hospital" };
              string layoutname = name[layoutNum];//5、7、8、9、10、11布局有重叠
              print("读取的布局名称为：" + layoutname);
              complexityControl.BeginGenerationJsonLoad(filename, layoutname); //在这里指定加载数据的文件


              surface.BuildNavMesh();//生成导航
                                     //AddRobot();
              AddPerson(10);
              AddRobot();//添加机器人
              foreach (HumanControl human in personList)//统计当前楼层的人数
              {
                  if (human.isActiveAndEnabled)
                  {
                      currentFloorhuman++;
                  }
                  //Debug.Log(currentFloorhuman);
              }
              AddExits();//添加出口，以便于后续机器人导航使用
              AddRobotBrain();//添加机器人大脑

              StepCount = 0;//训练决策数目
              MaxStep = 5000;
              layoutNum++;
          }*/
        /* StepCount++;
         if(StepCount > MaxStep)
         {
             print("超出次数限制");
             BrainList[0].EpisodeInterrupted();//超出次数，结束当前回合
             StepCount = 0;
         }*/
    }


    /*nt number1 = 900;
    int number2 = UnityEngine.Random.Range(8, 15); // 划分的房间数量
    timer = 0f;
    complexityControl.BeginGenerationBinary(number1, number2);*/





    /*  private void Update()
      {

          // 每帧更新计时器,这一部分用于生成场景展示
          *//* timer += Time.deltaTime;

           // 如果计时器达到 1 秒
           if (timer >= interval)
           {

               int number2 = UnityEngine.Random.Range(25, 35); // 划分的房间数量
               complexityControl.BeginGenerationBinary(4900, number2);
               surface.BuildNavMesh();//生成导航
               timer = 0f;
           }*//*

          // ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！ 这一部分是整个流程演示
          if (isTraining != true)
          {
              if (currentFloorhuman == 0)
              {
                  CleanTheScene();
                  int number2 = UnityEngine.Random.Range(20, 30); // 划分的房间数量
                  complexityControl.BeginGenerationBinary(3600, number2);
                  surface.BuildNavMesh();//生成导航
                                         //AddRobot();
                  AddPerson(10);
                  AddRobot(1);//添加机器人
                  foreach (HumanControl human in personList)//统计当前楼层的人数
                  {
                      if (human.isActiveAndEnabled)
                      {
                          currentFloorhuman++;
                      }
                      //Debug.Log(currentFloorhuman);
                  }
                  AddExits();//添加出口，以便于后续机器人导航使用
                  AddRobotBrain(1);//添加机器人大脑

              }
          }
          //！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！   


      }*/
    public void CleanTheScene()
    {
       // print("执行CleanScene函数");

        ResetAgentandClearList();
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
        cachedDoorPositions.Clear();
        cachedRoomPositions.Clear();

        if (FireList.Count > 0)
        {
            foreach (var Fire in FireList)
            {
                if (Fire != null && Fire.gameObject != null)
                {
                    Destroy(Fire.gameObject);
                }
            }
            FireList.Clear();
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
            bool positionFound = false;
            Vector3 spawnPosition = Vector3.zero;
            int attempts = 0;

            // 尝试找到一个没有碰撞的位置
            while (!positionFound && attempts < 100) // 最多尝试100次
            {
                /*// 随机生成位置.布局外围是固定矩形
                float randomX = UnityEngine.Random.Range(1f, complexityControl.buildingGeneration.totalWidth);
                float randomZ = UnityEngine.Random.Range(1f, complexityControl.buildingGeneration.totalWidth);
                spawnPosition = new Vector3(randomX, 0.5f, randomZ);*/


                spawnPosition = GetRandomPosInLayout();

                // 检测是否与其他对象碰撞
                float radius = 0.5f; // 人类的碰撞半径
                Collider[] colliders = Physics.OverlapSphere(spawnPosition, radius);

                // 如果没有碰撞，则找到合适位置,跳出循环
                if (colliders.Length == 0)
                {
                    positionFound = true;
                    break;
                }

                attempts++;
            }

            if (positionFound)
            {
                // 实例化人类
                GameObject Person = Instantiate(HumanPrefab, spawnPosition, Quaternion.identity);
                personList.Add(Person.GetComponent<HumanControl>());
                Person.transform.parent = humanParent.transform;
                Person.GetComponent<HumanControl>().myEnv = this;
                Person.GetComponent<HumanControl>().Start();//初始化人类的各个变量  
            }
            else
            {
                Debug.LogWarning($"无法为第 {i + 1} 个人类找到合适的位置！");
            }
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

        GameObject Fire = Instantiate(FirePerfab, FirePosition+ new Vector3(0,0.5f,0), Quaternion.identity);
        FireList.Add(Fire.GetComponent<FireControl>());
        Fire.transform.parent = FireParent.transform;
        Fire.GetComponent<FireControl>().myEnv = this;
       
    }

    public Vector3 GetRandomPosInLayout() {
        //随机选取一个房间，在随机房间内选取随机点
        List<Room> rooms = complexityControl.buildingGeneration.roomList;
        System.Random _random = new System.Random();
   
        // _random.Next(maxValue)
        //作用：返回 0 到 maxValue-1 之间的随机整数
       Room room = rooms[_random.Next(rooms.Count)];
        // 计算房间边界
        float xMin = room.xzPosition.x+2f;
        float zMin = room.xzPosition.z+2f;
        float xMax = xMin + room.width-2f;
        float zMax = zMin + room.height-2f;
        // 生成随机点
        float x = (float)(xMin + _random.NextDouble() * (xMax - xMin));
        float z = (float)(zMin + _random.NextDouble() * (zMax - zMin));
        Vector3 RandomPos = Vector3.zero;
        RandomPos=new Vector3(x,0,z);
        return RandomPos;
    }
}
