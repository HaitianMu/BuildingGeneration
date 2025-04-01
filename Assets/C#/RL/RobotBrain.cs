using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using Unity.Barracuda;
using TMPro;

public class RobotBrain : Agent
{
    //环境变量
    public EnvControl myEnv;
    // 机器人本体
    public GameObject robot;

    // 机器人的移动目的地
    public Vector3 robotDestinationCache;

    // 机器人的NavMeshAgent组件
    [HideInInspector] public NavMeshAgent robotNavMeshAgent;

    // 机器人的刚体组件
    [HideInInspector] public Rigidbody robotRigidbody;

    // 机器人的脚本类
    [HideInInspector] public RobotControl robotInfo;
 
    // 当前所在楼层
    public int currentFloor;

    // 机器人卡死计数器
    public int stuckCounter;

    //当前楼层人数
    public int floor_human;

    //用于奖励计算
    private float RewardDelayRate = 0.001f;
   



    private void FixedUpdate()
    {
      
        int currentFloorhuman = 0;

        foreach (HumanControl human in myEnv.personList)//统计当前楼层的人数
        {
            if (human.isActiveAndEnabled)
            {
                currentFloorhuman++;
            }
        }
        floor_human = currentFloorhuman;
        //print("当前楼层人数为:" + floor_human); 
        Vector3 robotPosition = robot.transform.position;
        robotPosition.y = 0.5f;

        // 如果不是训练模式，机器人就自己进行移动，暂时不使用训练收集的数据

        //Debug.Log("每一帧更新");
        // 每个时间步都要求决策,决策后才会收集信息以及执行操作,后续函数执行的前置条件

       
        AddReward(-RewardDelayRate * floor_human);
        LogReward("持续时间惩罚", -RewardDelayRate * floor_human );
        RequestDecision();

        if (myEnv.isTraining is false)
        {
            //print("当前楼层人数为:" + floor_human);
            int lonelyHumanLeaderCounter = (from human in myEnv.personList
                                            let humanPosition = human.transform.position - new Vector3(0, 0.5f, 0)
                                            where human.isActiveAndEnabled && Mathf.Abs(humanPosition.y - robotPosition.y) < 0.5f
                                            select human).Count(human => human.myBehaviourMode is "Leader" && human.transform.position.z > 0);

            //print("孤独人类领导者的数量为："+lonelyHumanLeaderCounter);

            if (lonelyHumanLeaderCounter <= 10)
            {//人类领导者数量（lonelyHumanLeaderCounter）小于等于4，并且机器人跟随者数量（robotInfo.robotFollowerCounter）等于0时，条件1为真;人类领导者数量（lonelyHumanLeaderCounter）等于0时，条件2为真
                robot.GetComponent<RobotControl>().isRunning = true;//机器人开始工作,人类开始跟随机器人
                GMoveAgent();
                return;
            }
        }
    }//定帧更新
public override void OnEpisodeBegin()
    {
        Debug.Log("一次训练开始了");
        myEnv.CleanTheScene();
        int number2 = 15;
            //UnityEngine.Random.Range(8, 15); // 划分的房间数量
        myEnv.complexityControl.BeginGenerationBinary(900, number2);
        myEnv.surface.BuildNavMesh();//生成导航
        myEnv.AddPerson(10);
        myEnv.AddRobot();//添加机器人
        myEnv.AddExits();//添加出口，以便于后续机器人导航使用
        myEnv.AddRobotBrain();//添加机器人大脑
        //myEnv.cachedDoorPositions=myEnv.GetAllDoorPositions();//添加门的位置信息
        myEnv.cachedRoomPositions=myEnv.GetAllRoomPositions();//添加房间的位置信息
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //建议先固定观测值数量并确保严格归一化到[-1, 1]范围，这是PPO算法稳定训练的前提条件

        //在RequestDecision函数执行后，会先执行该函数来收集环境观测值
        //观测值需要添加：
        //每一个人类的位置，来学习人类的移动逻辑
        //每一个机器人的位置，来学习其他机器人的移动逻辑，但目前只有一个机器人
        //总区域的面积，房间的数量/位置，每一个门的位置， 来学习建筑的生成逻辑
        // 计算环境边界（与Human观测保持一致）
        float envWidth = myEnv.complexityControl.buildingGeneration.totalWidth;
        float envHeight = myEnv.complexityControl.buildingGeneration.totalHeight;
        Vector3 envCenter = new Vector3(envWidth * 0.5f, 0, envHeight * 0.5f);
        float envMaxSize = Mathf.Max(envWidth, envHeight);

        if (myEnv.useRobot is false)
            return;

        //Debug.Log("CollectObservations called."); 
        if (myEnv == null || myEnv.useRobot is false)
        {
            Debug.Log("myEnv is null or useRobot is false.");
            return;
        }
        

        // 添加 Agent 观测值
        /*sensor.AddObservation(robotInfo.myDirectFollowers.Count/10);  //1
        Debug.Log("机器人跟随者的数量为"+ robotInfo.myDirectFollowers.Count / 10);*/

        // 归一化 Agent 位置 ，           3
        foreach (RobotBrain agent in myEnv.BrainList)
        {
            Vector3 normalizedPos = (agent.robot.transform.position ) /
                                   Mathf.Max(myEnv.complexityControl.buildingGeneration.totalWidth, myEnv.complexityControl.buildingGeneration.totalHeight);
            sensor.AddObservation(normalizedPos.x);
            sensor.AddObservation(normalizedPos.z);
           //  Debug.Log("机器人的位置为" + normalizedPos);
        }

        // 归一化 Human 位置，            30个
        foreach (HumanControl human in myEnv.personList)
        {
            Vector3 normalizedPos = (human.transform.position) /
                                   Mathf.Max(myEnv.complexityControl.buildingGeneration.totalWidth, myEnv.complexityControl.buildingGeneration.totalHeight);
            sensor.AddObservation(normalizedPos.x);
            sensor.AddObservation(normalizedPos.z);
            // Debug.Log("人类的位置为" + normalizedPos);
        }

        // 添加房间位置（相对Agent） 最少24个，最多45个
        foreach (Vector3 roomPos in myEnv.cachedRoomPositions)
        {
            // 位置归一化（相对于环境中心）
            Vector3 normalizedPos = (roomPos) / envMaxSize;
            sensor.AddObservation(normalizedPos.x); // X坐标 [-1, 1]
            sensor.AddObservation(normalizedPos.z); // Z坐标 [-1, 1]
         //  Debug.Log("房间的位置为" + normalizedPos);
        }

        //添加出口位置   3个          39+[24,45]=[63,84]
        sensor.AddObservation((myEnv.Exits[0].transform.position.x) / Mathf.Max(myEnv.complexityControl.buildingGeneration.totalWidth, myEnv.complexityControl.buildingGeneration.totalHeight));
        sensor.AddObservation((myEnv.Exits[0].transform.position.z) / Mathf.Max(myEnv.complexityControl.buildingGeneration.totalWidth, myEnv.complexityControl.buildingGeneration.totalHeight));
        // Debug.Log("出口的位置为" + (myEnv.Exits[0].transform.position) / Mathf.Max(myEnv.complexityControl.buildingGeneration.totalWidth, myEnv.complexityControl.buildingGeneration.totalHeight));

        //添加区域面积和房间数量,记得进行归一化处理

        /*sensor.AddObservation(myEnv.TotalSize);
        sensor.AddObservation(myEnv.RoomNum);*/
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (myEnv.useRobot is false)
            return;

        MoveAgent(actions);  // 移动Agent
        if (floor_human == 0)
        {
            print("所有人类逃生");
            AddReward(500);
            LogReward("人类全部逃生奖励", 500);
            EndEpisode();
            return;
        }
    }//奖励函数V2.0

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (myEnv.useRobot is false)
            return;

        if (currentFloor != 3)
            return;

        Vector3 robotPosition = robot.transform.position;
        robotPosition.y = 0.5f + 4 * (currentFloor - 1);
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActions[0] = 1;
            Vector3 targetPosition = robotPosition + new Vector3(0, 0, 1);
            continuousActions[0] = targetPosition.x / 18.0f;
            continuousActions[1] = targetPosition.z / 18.0f;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActions[0] = 1;
            Vector3 targetPosition = robotPosition + new Vector3(0, 0, -1);
            continuousActions[0] = targetPosition.x / 18.0f;
            continuousActions[1] = targetPosition.z / 18.0f;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActions[0] = 1;
            Vector3 targetPosition = robotPosition + new Vector3(-1, 0, 0);
            continuousActions[0] = targetPosition.x / 18.0f;
            continuousActions[1] = targetPosition.z / 18.0f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActions[0] = 1;
            Vector3 targetPosition = robotPosition + new Vector3(1, 0, 0);
            continuousActions[0] = targetPosition.x / 18.0f;
            continuousActions[1] = targetPosition.z / 18.0f;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActions[0] = 2;
            continuousActions[0] = robotPosition.x / 18.0f;
            continuousActions[1] = robotPosition.z / 18.0f;
        }
        else if (Input.GetKey(KeyCode.B))
        {
            discreteActions[0] = 3;
            continuousActions[0] = robotPosition.x / 18.0f;
            continuousActions[1] = robotPosition.z / 18.0f;
        }
    }


    /// <summary>
    /// 利用模型决策结果移动机器人本体
    /// </summary>
    /// <param name="actions"></param>
    private void MoveAgent(ActionBuffers actions)
    {
        ActionSegment<float> continuousActions = actions.ContinuousActions;
        Vector3 robotPosition = robot.transform.position;
        robotPosition.y = 0.5f + 4 * (currentFloor - 1);
        Vector3 positionExit = GetCrossDoorDestination(myEnv.Exits[0].gameObject);

        //将目标位置映射在整个房间区域内部
       // Debug.Log("区域的宽度为"+myEnv.complexityControl.buildingGeneration.totalWidth);
        //Debug.Log("区域的高度为" + myEnv.complexityControl.buildingGeneration.totalHeight);
        Debug.Log($"Actions: [{continuousActions[0]}, {continuousActions[1]}]");
        float targetX = Mathf.Clamp(continuousActions[0], -1, 1) * (myEnv.complexityControl.buildingGeneration.totalWidth / 2f) + (myEnv.complexityControl.buildingGeneration.totalWidth / 2f);
        float targetZ = Mathf.Clamp(continuousActions[1], -1, 1) * (myEnv.complexityControl.buildingGeneration.totalHeight / 2f) + (myEnv.complexityControl.buildingGeneration.totalHeight / 2f);
        Vector3 targetPosition = new(targetX, 0.5f, targetZ);

        if (Vector3.Distance(targetPosition, positionExit) < 6 && robotInfo.robotFollowerCounter>0)
            targetPosition = positionExit;
        //Debug.Log("这一帧的目的地是："+targetPosition);
        if (myEnv.isTraining is false)
        {
            if (robotInfo.robotFollowerCounter > 0)
            {

                if (currentFloor == 1 && robotDestinationCache == positionExit && Vector3.Distance(targetPosition, positionExit) < 40 && robotInfo.robotCommand is "LightOn")
                    return;
            }
            else if (robotInfo.robotFollowerCounter <= 0)
            {

                if (targetPosition == positionExit)
                    return;
            }
        }
        // 如果目标可达则前往目标，不可达则继续上一步动作
        stuckCounter = 0;
        //
        if (robotDestinationCache == targetPosition)
        {
            GMoveAgent();
        }
        else
        {
            robotDestinationCache = targetPosition;
            robotNavMeshAgent.SetDestination(robotDestinationCache);
        }

    }
    private void GMoveAgent()
    {
        Vector3 targetPosition = new();
        Vector3 robotPosition = robot.transform.position;
        //print("机器人的跟随者数量为：" + robotInfo.myDirectFollowers.Count);
        if (robotInfo.robotFollowerCounter > 0)//如果当前机器人当前跟随者大于0个，前往出口
        {
            //随机一个出口，将人送到出口
            //print("2机器人检测到的出口数量为："+myEnv.Exits.Count);
            targetPosition = GetCrossDoorDestination(myEnv.Exits[0].gameObject);
        }
        else
        {
            //找到离机器人最近的人类，并朝其进行移动
            float minDist = int.MaxValue;
            //float minDist = 18f;
            foreach (HumanControl human in myEnv.personList)
            {
                Vector3 humanPosition = human.transform.position - new Vector3(0, 0.5f, 0);
                if (human.isActiveAndEnabled is false || Mathf.Abs(humanPosition.y - robotPosition.y) > 1 || humanPosition.x < -20 || humanPosition.z > 20)
                    continue;
                if (Vector3.Distance(humanPosition, robotPosition) < minDist)
                {
                    minDist = Vector3.Distance(humanPosition, robotPosition);
                    targetPosition = humanPosition + human.transform.forward ;
                }
            }
            robotDestinationCache = targetPosition;
            robotNavMeshAgent.SetDestination(robotDestinationCache);
            return;
        }

        if (targetPosition != Vector3.zero)
        {
            robotDestinationCache = targetPosition;
            robotNavMeshAgent.SetDestination(robotDestinationCache);
            return;
        }

    }//贪心算法移动机器人

    private Vector3 GetCrossDoorDestination(GameObject targetDoor)//去到门前的位置，该函数是给机器人使用，放置发生拥堵
    {
        Vector3 myPosition = transform.position;

        if (targetDoor.CompareTag("Door") || targetDoor.CompareTag("Exit"))
        {
            string doorDirection = targetDoor.GetComponent<DoorControl>().doorDirection;
            Vector3 doorPosition = targetDoor.transform.position + new Vector3(0, -1.5f, 0);
            switch (doorDirection)
            {
                case "Horizontal": //水平
                    if (myPosition.z < doorPosition.z)
                        return doorPosition + new Vector3(0, 0, -1);
                    return doorPosition - new Vector3(0, 0, -1);
                case "Vertical"://垂直
                    if (myPosition.x < doorPosition.x)
                        return doorPosition + new Vector3(-1, 0, 0);
                    return doorPosition - new Vector3(-1, 0, 0);
                default:
                    return myPosition;
            }
        }
        else
        {
            return myPosition;
        }
    }




    // 奖励统计可视化
    private Dictionary<string, float> rewardLog = new Dictionary<string, float>();

   public void LogReward(string type, float value)
    {
        rewardLog.TryGetValue(type, out float current);
        rewardLog[type] = current + value;
    }

    void OnDestroy()
    {
        foreach (var kv in rewardLog)
            Debug.Log($"{kv.Key}: {kv.Value}");
    }
}
