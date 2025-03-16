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
    public List<RobotBrain> BrianList = new();
    //环境中的出口
    public List<GameObject> Exits=new();


    /*生成场景和导航时用到的组件 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
    public ComplexityControl complexityControl;//生成环境用到的组件
    public NavMeshSurface surface;//生成导航的组件
    public GameObject HumanPrefab;//生成人类用到的组件
    public GameObject RobotPrefab;//添加机器人用到的组件
    public GameObject BrainPerfab;//机器人大脑预制体
    // 是否在训练
    public bool isTraining;
   /* // 是否使用火源生成器
    public bool useFireAgent;*/
    // 是否使用强化学习机器人
    public bool useAgent;
    // 是否使用机器人
    public bool useRobot;




    /*展示Demo使用，用于场景重置!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
    public int currentFloorhuman=0;
    private float timer = 0f; // 计时器
    private const float interval = 1f; // 间隔时间（1 秒）


    private void Start()
    {



            /*int number1 = 900;
            int number2 = UnityEngine.Random.Range(8, 15); // 划分的房间数量
            timer = 0f;
            complexityControl.BeginGenerationBinary(number1, number2);
      */

        /* //print("Env环境初始化脚本开始了");
         //生成环境，环境的父物体是Env的子物体：Buildin。生成的环境包括地板的NavMeshModified和门的NavMeshLink

         int number1 = UnityEngine.Random.Range(100, 1000);
         int number2 = UnityEngine.Random.Range(5, 10); // 划分的房间数量

         int number1 = 900;
         int number2 = 10; // 划分的房间数量
         complexityControl.BeginGenerationFangTree(number1, number2);//方形树图发生成房间布局

         complexityControl.BeginGenerationBinary(number1, number2);
         surface.BuildNavMesh();//生成导航
                                //AddRobot();
         AddPerson(10);

         foreach (HumanControl human in personList)//统计当前楼层的人数
         {
             if (human.isActiveAndEnabled)
             {
                 currentFloorhuman++;
             }
             Debug.Log(currentFloorhuman);
         }

         AddExits();
         CleanTheScene();*/

    }


    private void Update()
    {

        // 每帧更新计时器,这一部分用于生成场景展示
        /*timer += Time.deltaTime;

        // 如果计时器达到 1 秒
        if (timer >= interval)
        {
            int number1 = 900;
            int number2 = UnityEngine.Random.Range(8, 15); // 划分的房间数量
            complexityControl.BeginGenerationBinary(number1, number2);
            surface.BuildNavMesh();//生成导航
            timer = 0f;
        }*/

        // ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！ 这一部分是整个流程演示
        if (isTraining != true)
        {
            if (currentFloorhuman == 0)
            {
                CleanTheScene();
                int number2 = UnityEngine.Random.Range(8, 15); // 划分的房间数量
                complexityControl.BeginGenerationBinary(900, number2);
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


    }
    /// <summary>
    /// 获取随机出生位置（）
    /// </summary>
    /// <param name="floor"></param>
    /// <returns></returns>
    private static Vector3 GetSpawnBlockPosition(int floor)//！！！！！！！！奶奶的，坐标的问题也要注意一下,找没有父物体的查看世界坐标
    {
        Vector3 spawnBlockPosition = new();
        for (int tryCounter = 80000; tryCounter > 0; tryCounter--)
        {
            // Generate random X and Z coordinates within the (0, 10) range.
            float randomX = Random.Range(1, 9) + 0.5f;
            float randomZ = Random.Range(1, 9) + 0.5f;

            // 跳过该区域
            if (floor == 1 && randomX is > 5 and < 8 && randomZ is > 2 and < 5)
                continue;

            // Set the spawn position at the appropriate floor level
            spawnBlockPosition.Set(randomX, (floor - 1) * 4, randomZ);

            // Check if the spawn position is valid (no collision)
            if (Physics.CheckBox(spawnBlockPosition + Vector3.up, new Vector3(0.49f, 0.49f, 0.49f)) is false)

                return spawnBlockPosition;
        }
        print("生成的随机位置是" + spawnBlockPosition);
        // Return a default value if no valid position is found
        return new Vector3();
    }

    private void CleanTheScene()
    {
        print("执行CleanScene函数");

        ResetAllAgents();
    }
    /// <summary>
    /// 重置所有机器人控制智能体
    /// </summary>
    private void ResetAllAgents()
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

        // 清除智能体
        if (BrianList.Count > 0)
        {
            foreach (RobotBrain agent in BrianList)
            {
                if (agent != null && agent.gameObject != null)
                {
                    Destroy(agent.gameObject);
                }
            }
            BrianList.Clear();
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
    }
    /// <summary>
    /// 重置所有人类
    /// </summary>
    public void ResetAllHumans()
    {
        Debug.Log("进入了重置人类的函数");
        foreach (HumanControl human in personList)
        {
            Debug.Log("重置时找到了列表中的人类" + human);
            int randomFloor = 1;
            Vector3 spawnPosition = GetSpawnBlockPosition(randomFloor) + new Vector3(0, 1, 0);

            if (spawnPosition == Vector3.zero)
                continue;
            if (!human.gameObject.activeSelf)
            {
                human.gameObject.SetActive(true);
                human.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            }  // 激活人物}
            human.Start();
        }
    }

    private void AddRobot(int num)  //在这里添加机器人，及其大脑.机器人的数量较少而且确定，是否有必要控制其数量？
    {
        // 在场景中生成num个机器人，并把他们加入到List中

        GameObject RobotParent = GameObject.Find("RobotList");

        for (int i = 0; i < num; i++)
        {
            Vector3 spawnPosition = Vector3.zero;
            // 尝试找到一个没有碰撞的位置
            // 随机生成位置
            float randomX = UnityEngine.Random.Range(1f, complexityControl.buildingGeneration.totalWidth);
            float randomZ = UnityEngine.Random.Range(1f, complexityControl.buildingGeneration.totalHeight);
            spawnPosition = new Vector3(randomX, 0.5f, randomZ);
            // 实例化机器人
            GameObject Robot = Instantiate(RobotPrefab, spawnPosition, Quaternion.identity);//实例化机器人的位置
            RobotList.Add(Robot.GetComponent<RobotControl>()); //将机器人加入列表
            Robot.transform.parent = RobotParent.transform;    //将机器人放在场景的RobotList物体下
        }
    }

    private void AddRobotBrain(int num)  //在这里添加机器人大脑. 并进行组装  机器人的数量较少而且确定，是否有必要控制其数量？
    {
        GameObject BrainParent = GameObject.Find("RobotBrain");
        Debug.Log("我在添加机器人的大脑");
        for(int i = 0;i < num;i++)
        {

            GameObject RobotBrain = Instantiate(BrainPerfab);//实例化机器人的位置
            BrianList.Add(RobotBrain.GetComponent<RobotBrain>());//加入Brain列表中
            RobotBrain robotBrain = RobotBrain.GetComponent<RobotBrain>();
            RobotBrain.transform.parent = BrainParent.transform;//设置父物体

            /*  在这里进行机器人和大脑的脚本初始化工作！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！*/
            RobotControl robot = RobotList[i];
            robot.myAgent=robotBrain;

            robotBrain.myEnv = this;
            robotBrain.robot = robot.gameObject;
            robotBrain.robotNavMeshAgent=robot.GetComponent<NavMeshAgent>();
            robotBrain.robotInfo = robot;
            robotBrain.robotRigidbody=robot.GetComponent<Rigidbody>();
        }
    }
    private void AddPerson(int num)
    {
        // 在场景中生成num个人类，并把他们加入到personList中
        GameObject humanParent = GameObject.Find("HumanList");

        for (int i = 0; i < num; i++)
        {
            bool positionFound = false;
            Vector3 spawnPosition = Vector3.zero;
            int attempts = 0;

            // 尝试找到一个没有碰撞的位置
            while (!positionFound && attempts < 100) // 最多尝试100次
            {
                // 随机生成位置
                float randomX = UnityEngine.Random.Range(1f, complexityControl.buildingGeneration.totalWidth);
                float randomZ = UnityEngine.Random.Range(1f, complexityControl.buildingGeneration.totalHeight);
                spawnPosition = new Vector3(randomX, 0.5f, randomZ);

                // 检测是否与其他对象碰撞
                float radius = 0.5f; // 人类的碰撞半径
                Collider[] colliders = Physics.OverlapSphere(spawnPosition, radius);

                // 如果没有碰撞，则找到合适位置
                if (colliders.Length == 0)
                {
                    positionFound = true;
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
            }
            else
            {
                Debug.LogWarning($"无法为第 {i + 1} 个人类找到合适的位置！");
            }
        }

        Debug.Log("执行生成人类函数");
    }
    private void AddExits()
    {
         GameObject Exit = GameObject.Find("Exit");
            Exits.Add(Exit);
            Debug.Log("执行添加出口函数，赋值");
       
    }
}
