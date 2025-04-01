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
    //��������
    public EnvControl myEnv;
    // �����˱���
    public GameObject robot;

    // �����˵��ƶ�Ŀ�ĵ�
    public Vector3 robotDestinationCache;

    // �����˵�NavMeshAgent���
    [HideInInspector] public NavMeshAgent robotNavMeshAgent;

    // �����˵ĸ������
    [HideInInspector] public Rigidbody robotRigidbody;

    // �����˵Ľű���
    [HideInInspector] public RobotControl robotInfo;
 
    // ��ǰ����¥��
    public int currentFloor;

    // �����˿���������
    public int stuckCounter;

    //��ǰ¥������
    public int floor_human;

    //���ڽ�������
    private float RewardDelayRate = 0.001f;
   



    private void FixedUpdate()
    {
      
        int currentFloorhuman = 0;

        foreach (HumanControl human in myEnv.personList)//ͳ�Ƶ�ǰ¥�������
        {
            if (human.isActiveAndEnabled)
            {
                currentFloorhuman++;
            }
        }
        floor_human = currentFloorhuman;
        //print("��ǰ¥������Ϊ:" + floor_human); 
        Vector3 robotPosition = robot.transform.position;
        robotPosition.y = 0.5f;

        // �������ѵ��ģʽ�������˾��Լ������ƶ�����ʱ��ʹ��ѵ���ռ�������

        //Debug.Log("ÿһ֡����");
        // ÿ��ʱ�䲽��Ҫ�����,���ߺ�Ż��ռ���Ϣ�Լ�ִ�в���,��������ִ�е�ǰ������

       
        AddReward(-RewardDelayRate * floor_human);
        LogReward("����ʱ��ͷ�", -RewardDelayRate * floor_human );
        RequestDecision();

        if (myEnv.isTraining is false)
        {
            //print("��ǰ¥������Ϊ:" + floor_human);
            int lonelyHumanLeaderCounter = (from human in myEnv.personList
                                            let humanPosition = human.transform.position - new Vector3(0, 0.5f, 0)
                                            where human.isActiveAndEnabled && Mathf.Abs(humanPosition.y - robotPosition.y) < 0.5f
                                            select human).Count(human => human.myBehaviourMode is "Leader" && human.transform.position.z > 0);

            //print("�¶������쵼�ߵ�����Ϊ��"+lonelyHumanLeaderCounter);

            if (lonelyHumanLeaderCounter <= 10)
            {//�����쵼��������lonelyHumanLeaderCounter��С�ڵ���4�����һ����˸�����������robotInfo.robotFollowerCounter������0ʱ������1Ϊ��;�����쵼��������lonelyHumanLeaderCounter������0ʱ������2Ϊ��
                robot.GetComponent<RobotControl>().isRunning = true;//�����˿�ʼ����,���࿪ʼ���������
                GMoveAgent();
                return;
            }
        }
    }//��֡����
public override void OnEpisodeBegin()
    {
        Debug.Log("һ��ѵ����ʼ��");
        myEnv.CleanTheScene();
        int number2 = 15;
            //UnityEngine.Random.Range(8, 15); // ���ֵķ�������
        myEnv.complexityControl.BeginGenerationBinary(900, number2);
        myEnv.surface.BuildNavMesh();//���ɵ���
        myEnv.AddPerson(10);
        myEnv.AddRobot();//��ӻ�����
        myEnv.AddExits();//��ӳ��ڣ��Ա��ں��������˵���ʹ��
        myEnv.AddRobotBrain();//��ӻ����˴���
        //myEnv.cachedDoorPositions=myEnv.GetAllDoorPositions();//����ŵ�λ����Ϣ
        myEnv.cachedRoomPositions=myEnv.GetAllRoomPositions();//��ӷ����λ����Ϣ
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //�����ȹ̶��۲�ֵ������ȷ���ϸ��һ����[-1, 1]��Χ������PPO�㷨�ȶ�ѵ����ǰ������

        //��RequestDecision����ִ�к󣬻���ִ�иú������ռ������۲�ֵ
        //�۲�ֵ��Ҫ��ӣ�
        //ÿһ�������λ�ã���ѧϰ������ƶ��߼�
        //ÿһ�������˵�λ�ã���ѧϰ���������˵��ƶ��߼�����Ŀǰֻ��һ��������
        //���������������������/λ�ã�ÿһ���ŵ�λ�ã� ��ѧϰ�����������߼�
        // ���㻷���߽磨��Human�۲Ᵽ��һ�£�
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
        

        // ��� Agent �۲�ֵ
        /*sensor.AddObservation(robotInfo.myDirectFollowers.Count/10);  //1
        Debug.Log("�����˸����ߵ�����Ϊ"+ robotInfo.myDirectFollowers.Count / 10);*/

        // ��һ�� Agent λ�� ��           3
        foreach (RobotBrain agent in myEnv.BrainList)
        {
            Vector3 normalizedPos = (agent.robot.transform.position ) /
                                   Mathf.Max(myEnv.complexityControl.buildingGeneration.totalWidth, myEnv.complexityControl.buildingGeneration.totalHeight);
            sensor.AddObservation(normalizedPos.x);
            sensor.AddObservation(normalizedPos.z);
           //  Debug.Log("�����˵�λ��Ϊ" + normalizedPos);
        }

        // ��һ�� Human λ�ã�            30��
        foreach (HumanControl human in myEnv.personList)
        {
            Vector3 normalizedPos = (human.transform.position) /
                                   Mathf.Max(myEnv.complexityControl.buildingGeneration.totalWidth, myEnv.complexityControl.buildingGeneration.totalHeight);
            sensor.AddObservation(normalizedPos.x);
            sensor.AddObservation(normalizedPos.z);
            // Debug.Log("�����λ��Ϊ" + normalizedPos);
        }

        // ��ӷ���λ�ã����Agent�� ����24�������45��
        foreach (Vector3 roomPos in myEnv.cachedRoomPositions)
        {
            // λ�ù�һ��������ڻ������ģ�
            Vector3 normalizedPos = (roomPos) / envMaxSize;
            sensor.AddObservation(normalizedPos.x); // X���� [-1, 1]
            sensor.AddObservation(normalizedPos.z); // Z���� [-1, 1]
         //  Debug.Log("�����λ��Ϊ" + normalizedPos);
        }

        //��ӳ���λ��   3��          39+[24,45]=[63,84]
        sensor.AddObservation((myEnv.Exits[0].transform.position.x) / Mathf.Max(myEnv.complexityControl.buildingGeneration.totalWidth, myEnv.complexityControl.buildingGeneration.totalHeight));
        sensor.AddObservation((myEnv.Exits[0].transform.position.z) / Mathf.Max(myEnv.complexityControl.buildingGeneration.totalWidth, myEnv.complexityControl.buildingGeneration.totalHeight));
        // Debug.Log("���ڵ�λ��Ϊ" + (myEnv.Exits[0].transform.position) / Mathf.Max(myEnv.complexityControl.buildingGeneration.totalWidth, myEnv.complexityControl.buildingGeneration.totalHeight));

        //�����������ͷ�������,�ǵý��й�һ������

        /*sensor.AddObservation(myEnv.TotalSize);
        sensor.AddObservation(myEnv.RoomNum);*/
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (myEnv.useRobot is false)
            return;

        MoveAgent(actions);  // �ƶ�Agent
        if (floor_human == 0)
        {
            print("������������");
            AddReward(500);
            LogReward("����ȫ����������", 500);
            EndEpisode();
            return;
        }
    }//��������V2.0

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
    /// ����ģ�;��߽���ƶ������˱���
    /// </summary>
    /// <param name="actions"></param>
    private void MoveAgent(ActionBuffers actions)
    {
        ActionSegment<float> continuousActions = actions.ContinuousActions;
        Vector3 robotPosition = robot.transform.position;
        robotPosition.y = 0.5f + 4 * (currentFloor - 1);
        Vector3 positionExit = GetCrossDoorDestination(myEnv.Exits[0].gameObject);

        //��Ŀ��λ��ӳ�����������������ڲ�
       // Debug.Log("����Ŀ��Ϊ"+myEnv.complexityControl.buildingGeneration.totalWidth);
        //Debug.Log("����ĸ߶�Ϊ" + myEnv.complexityControl.buildingGeneration.totalHeight);
        Debug.Log($"Actions: [{continuousActions[0]}, {continuousActions[1]}]");
        float targetX = Mathf.Clamp(continuousActions[0], -1, 1) * (myEnv.complexityControl.buildingGeneration.totalWidth / 2f) + (myEnv.complexityControl.buildingGeneration.totalWidth / 2f);
        float targetZ = Mathf.Clamp(continuousActions[1], -1, 1) * (myEnv.complexityControl.buildingGeneration.totalHeight / 2f) + (myEnv.complexityControl.buildingGeneration.totalHeight / 2f);
        Vector3 targetPosition = new(targetX, 0.5f, targetZ);

        if (Vector3.Distance(targetPosition, positionExit) < 6 && robotInfo.robotFollowerCounter>0)
            targetPosition = positionExit;
        //Debug.Log("��һ֡��Ŀ�ĵ��ǣ�"+targetPosition);
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
        // ���Ŀ��ɴ���ǰ��Ŀ�꣬���ɴ��������һ������
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
        //print("�����˵ĸ���������Ϊ��" + robotInfo.myDirectFollowers.Count);
        if (robotInfo.robotFollowerCounter > 0)//�����ǰ�����˵�ǰ�����ߴ���0����ǰ������
        {
            //���һ�����ڣ������͵�����
            //print("2�����˼�⵽�ĳ�������Ϊ��"+myEnv.Exits.Count);
            targetPosition = GetCrossDoorDestination(myEnv.Exits[0].gameObject);
        }
        else
        {
            //�ҵ����������������࣬����������ƶ�
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

    }//̰���㷨�ƶ�������

    private Vector3 GetCrossDoorDestination(GameObject targetDoor)//ȥ����ǰ��λ�ã��ú����Ǹ�������ʹ�ã����÷���ӵ��
    {
        Vector3 myPosition = transform.position;

        if (targetDoor.CompareTag("Door") || targetDoor.CompareTag("Exit"))
        {
            string doorDirection = targetDoor.GetComponent<DoorControl>().doorDirection;
            Vector3 doorPosition = targetDoor.transform.position + new Vector3(0, -1.5f, 0);
            switch (doorDirection)
            {
                case "Horizontal": //ˮƽ
                    if (myPosition.z < doorPosition.z)
                        return doorPosition + new Vector3(0, 0, -1);
                    return doorPosition - new Vector3(0, 0, -1);
                case "Vertical"://��ֱ
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




    // ����ͳ�ƿ��ӻ�
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
