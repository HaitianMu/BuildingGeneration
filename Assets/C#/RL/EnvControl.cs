using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.MLAgents;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public partial class EnvControl : MonoBehaviour
{
   
    //�����б�
    public List<HumanControl> personList = new();
    //�������б�
    public List<RobotControl> RobotList = new();
    // �����˴����б�
    public List<RobotBrain> BrainList = new();
    //�����еĳ���
    public List<GameObject> Exits=new();
    //�洢������ŵ�λ����Ϣ
    public List<Vector3> cachedRoomPositions;
    public List<Vector3> cachedDoorPositions;
    /*���ɳ����͵���ʱ�õ������ !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
    public ComplexityControl complexityControl;//���ɻ����õ������
    public NavMeshSurface surface;//���ɵ��������
    public GameObject HumanPrefab;//���������õ������
    public GameObject RobotPrefab;//��ӻ������õ������
    public GameObject BrainPerfab;//�����˴���Ԥ����

    public float TotalSize;//�����ܴ�С
    public int RoomNum;//������Ŀ
    // �Ƿ���ѵ��
    public bool isTraining;
    // �Ƿ�ʹ�û�����
    public bool useRobot;


    /*չʾDemoʹ�ã����ڳ�������!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
    public int currentFloorhuman=0;

    public int StepCount;//������
    public int MaxStep;//�����


    private void Start()
    {

        //Debug.Log("Env��start����");

        if (currentFloorhuman == 0)
        {
            CleanTheScene();
            int number2 = UnityEngine.Random.Range(8, 15); // ���ֵķ�������
            TotalSize = 900;
            RoomNum = number2;
            complexityControl.BeginGenerationBinary(TotalSize, RoomNum);
            surface.BuildNavMesh();//���ɵ���
                                   //AddRobot();
            AddPerson(10);
            AddRobot();//��ӻ�����
            foreach (HumanControl human in personList)//ͳ�Ƶ�ǰ¥�������
            {
                if (human.isActiveAndEnabled)
                {
                    currentFloorhuman++;
                }
                //Debug.Log(currentFloorhuman);
            }
            AddExits();//��ӳ��ڣ��Ա��ں��������˵���ʹ��
            AddRobotBrain();//��ӻ����˴���

            StepCount = 0;//ѵ��������Ŀ
            MaxStep = 5000;
        }
    }

    private void FixedUpdate()
    {
        StepCount++;
        if(StepCount > MaxStep)
        {
            print("������������");
            BrainList[0].EpisodeInterrupted();//����������������ǰ�غ�
            StepCount = 0;
        }
    }


    /*nt number1 = 900;
    int number2 = UnityEngine.Random.Range(8, 15); // ���ֵķ�������
    timer = 0f;
    complexityControl.BeginGenerationBinary(number1, number2);*/





    /*  private void Update()
      {

          // ÿ֡���¼�ʱ��,��һ�����������ɳ���չʾ
          *//* timer += Time.deltaTime;

           // �����ʱ���ﵽ 1 ��
           if (timer >= interval)
           {

               int number2 = UnityEngine.Random.Range(25, 35); // ���ֵķ�������
               complexityControl.BeginGenerationBinary(4900, number2);
               surface.BuildNavMesh();//���ɵ���
               timer = 0f;
           }*//*

          // ������������������������������������������������������������������������������ ��һ����������������ʾ
          if (isTraining != true)
          {
              if (currentFloorhuman == 0)
              {
                  CleanTheScene();
                  int number2 = UnityEngine.Random.Range(20, 30); // ���ֵķ�������
                  complexityControl.BeginGenerationBinary(3600, number2);
                  surface.BuildNavMesh();//���ɵ���
                                         //AddRobot();
                  AddPerson(10);
                  AddRobot(1);//��ӻ�����
                  foreach (HumanControl human in personList)//ͳ�Ƶ�ǰ¥�������
                  {
                      if (human.isActiveAndEnabled)
                      {
                          currentFloorhuman++;
                      }
                      //Debug.Log(currentFloorhuman);
                  }
                  AddExits();//��ӳ��ڣ��Ա��ں��������˵���ʹ��
                  AddRobotBrain(1);//��ӻ����˴���

              }
          }
          //������������������������������������������������������������������������������   


      }*/
    public void CleanTheScene()
    {
       // print("ִ��CleanScene����");

        ResetAgentandClearList();
    }
    /// <summary>
    /// �������л����˿���������
    /// </summary>
    private void ResetAgentandClearList()
    {
        if (!useRobot)
            return;

        // �������
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

        // ���������
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

        // ��������壬ֻ����������б��ɣ�ʵ�岻�����
        if (BrainList.Count > 0)
        {
            BrainList.Clear();
        }

        // �������
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
    }
    public void AddRobot()  //�����ﶯ̬��ӻ����ˣ���ȷ�����������ڻ�������֮��������ȥ�ģ���ȷ�������˵���������ʹ��
    {
        // �ڳ���������num�������ˣ��������Ǽ��뵽List��
            GameObject RobotParent = GameObject.Find("RobotList");
            Vector3 spawnPosition = Vector3.zero;
            // �����ҵ�һ��û����ײ��λ��
            // �������λ��
            float randomX = UnityEngine.Random.Range(1f, complexityControl.buildingGeneration.totalWidth);
            float randomZ = UnityEngine.Random.Range(1f, complexityControl.buildingGeneration.totalHeight);
            spawnPosition = new Vector3(randomX, 0.5f, randomZ);
            // ʵ����������
            GameObject Robot = Instantiate(RobotPrefab, spawnPosition, Quaternion.identity);//ʵ���������˵�λ��
            RobotList.Add(Robot.GetComponent<RobotControl>()); //�������˼����б�
            Robot.transform.parent = RobotParent.transform;    //�������˷��ڳ�����RobotList������
    }

    public void AddRobotBrain()  //��������ӻ����˴���. ��������װ  �����˵��������ٶ���ȷ�������ֱ���ڳ����н������
    {
          GameObject RobotBrain = GameObject.Find("RobotBrain1");
          RobotBrain robotBrain = RobotBrain.GetComponent<RobotBrain>();
          BrainList.Add(robotBrain);
               /*  ��������л����˺ʹ��ԵĽű���ʼ������������������������������������������������������������������������������������*/
              RobotControl robot = RobotList[0];
            robot.myAgent=robotBrain;
            robotBrain.robot = robot.gameObject;
            robotBrain.robotNavMeshAgent=robot.GetComponent<NavMeshAgent>();
            robotBrain.robotInfo = robot;
            robotBrain.robotRigidbody=robot.GetComponent<Rigidbody>();
    }
    public void AddPerson(int num)
    {
        // �ڳ���������num�����࣬�������Ǽ��뵽personList��
        GameObject humanParent = GameObject.Find("HumanList");

        for (int i = 0; i < num; i++)
        {
            bool positionFound = false;
            Vector3 spawnPosition = Vector3.zero;
            int attempts = 0;

            // �����ҵ�һ��û����ײ��λ��
            while (!positionFound && attempts < 100) // ��ೢ��100��
            {
                // �������λ��
                float randomX = UnityEngine.Random.Range(1f, complexityControl.buildingGeneration.totalWidth);
                float randomZ = UnityEngine.Random.Range(1f, complexityControl.buildingGeneration.totalHeight);
                spawnPosition = new Vector3(randomX, 0.5f, randomZ);

                // ����Ƿ�������������ײ
                float radius = 0.5f; // �������ײ�뾶
                Collider[] colliders = Physics.OverlapSphere(spawnPosition, radius);

                // ���û����ײ�����ҵ�����λ��,����ѭ��
                if (colliders.Length == 0)
                {
                    positionFound = true;
                    break;
                }

                attempts++;
            }

            if (positionFound)
            {
                // ʵ��������
                GameObject Person = Instantiate(HumanPrefab, spawnPosition, Quaternion.identity);
                personList.Add(Person.GetComponent<HumanControl>());
                Person.GetComponent<HumanControl>().Start();//��ʼ������ĸ�������
                Person.transform.parent = humanParent.transform;
                Person.GetComponent<HumanControl>().myEnv = this;
            }
            else
            {
                Debug.LogWarning($"�޷�Ϊ�� {i + 1} �������ҵ����ʵ�λ�ã�");
            }
        }

       // Debug.Log("ִ���������ຯ��");
    }
    public void AddExits()
    {
         GameObject Exit = GameObject.Find("Exit");
        if (Exit != null)
        {
            Exits.Add(Exit);
         //   Debug.Log("ִ����ӳ��ں�������ֵ");
        }
        else { Debug.Log("û���ҵ����ڵ���Ϸ����"); }
       
    }

    public List<Vector3> GetAllRoomPositions()
    {
        print("ִ���˷���λ�û��溯��");
        List<Vector3> positions = new List<Vector3>();
        foreach (GameObject roomObj in GameObject.FindGameObjectsWithTag("Floor"))
        {
            positions.Add(roomObj.transform.position);
            //print("����λ��Ϊ��"+ roomObj.transform.position);
        }
        return positions;
    }

    public List<Vector3> GetAllDoorPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (GameObject roomObj in GameObject.FindGameObjectsWithTag("Door"))
        {
            positions.Add(roomObj.transform.position);
        }
        return positions;
    }
}
