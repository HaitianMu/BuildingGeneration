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
    public List<RobotBrain> BrianList = new();
    //�����еĳ���
    public List<GameObject> Exits=new();


    /*���ɳ����͵���ʱ�õ������ !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
    public ComplexityControl complexityControl;//���ɻ����õ������
    public NavMeshSurface surface;//���ɵ��������
    public GameObject HumanPrefab;//���������õ������
    public GameObject RobotPrefab;//��ӻ������õ������
    public GameObject BrainPerfab;//�����˴���Ԥ����
    // �Ƿ���ѵ��
    public bool isTraining;
   /* // �Ƿ�ʹ�û�Դ������
    public bool useFireAgent;*/
    // �Ƿ�ʹ��ǿ��ѧϰ������
    public bool useAgent;
    // �Ƿ�ʹ�û�����
    public bool useRobot;




    /*չʾDemoʹ�ã����ڳ�������!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
    public int currentFloorhuman=0;
    private float timer = 0f; // ��ʱ��
    private const float interval = 1f; // ���ʱ�䣨1 �룩


    private void Start()
    {



            /*int number1 = 900;
            int number2 = UnityEngine.Random.Range(8, 15); // ���ֵķ�������
            timer = 0f;
            complexityControl.BeginGenerationBinary(number1, number2);
      */

        /* //print("Env������ʼ���ű���ʼ��");
         //���ɻ����������ĸ�������Env�������壺Buildin�����ɵĻ��������ذ��NavMeshModified���ŵ�NavMeshLink

         int number1 = UnityEngine.Random.Range(100, 1000);
         int number2 = UnityEngine.Random.Range(5, 10); // ���ֵķ�������

         int number1 = 900;
         int number2 = 10; // ���ֵķ�������
         complexityControl.BeginGenerationFangTree(number1, number2);//������ͼ�����ɷ��䲼��

         complexityControl.BeginGenerationBinary(number1, number2);
         surface.BuildNavMesh();//���ɵ���
                                //AddRobot();
         AddPerson(10);

         foreach (HumanControl human in personList)//ͳ�Ƶ�ǰ¥�������
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

        // ÿ֡���¼�ʱ��,��һ�����������ɳ���չʾ
        /*timer += Time.deltaTime;

        // �����ʱ���ﵽ 1 ��
        if (timer >= interval)
        {
            int number1 = 900;
            int number2 = UnityEngine.Random.Range(8, 15); // ���ֵķ�������
            complexityControl.BeginGenerationBinary(number1, number2);
            surface.BuildNavMesh();//���ɵ���
            timer = 0f;
        }*/

        // ������������������������������������������������������������������������������ ��һ����������������ʾ
        if (isTraining != true)
        {
            if (currentFloorhuman == 0)
            {
                CleanTheScene();
                int number2 = UnityEngine.Random.Range(8, 15); // ���ֵķ�������
                complexityControl.BeginGenerationBinary(900, number2);
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


    }
    /// <summary>
    /// ��ȡ�������λ�ã���
    /// </summary>
    /// <param name="floor"></param>
    /// <returns></returns>
    private static Vector3 GetSpawnBlockPosition(int floor)//�������������������̵ģ����������ҲҪע��һ��,��û�и�����Ĳ鿴��������
    {
        Vector3 spawnBlockPosition = new();
        for (int tryCounter = 80000; tryCounter > 0; tryCounter--)
        {
            // Generate random X and Z coordinates within the (0, 10) range.
            float randomX = Random.Range(1, 9) + 0.5f;
            float randomZ = Random.Range(1, 9) + 0.5f;

            // ����������
            if (floor == 1 && randomX is > 5 and < 8 && randomZ is > 2 and < 5)
                continue;

            // Set the spawn position at the appropriate floor level
            spawnBlockPosition.Set(randomX, (floor - 1) * 4, randomZ);

            // Check if the spawn position is valid (no collision)
            if (Physics.CheckBox(spawnBlockPosition + Vector3.up, new Vector3(0.49f, 0.49f, 0.49f)) is false)

                return spawnBlockPosition;
        }
        print("���ɵ����λ����" + spawnBlockPosition);
        // Return a default value if no valid position is found
        return new Vector3();
    }

    private void CleanTheScene()
    {
        print("ִ��CleanScene����");

        ResetAllAgents();
    }
    /// <summary>
    /// �������л����˿���������
    /// </summary>
    private void ResetAllAgents()
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

        // ���������
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
    }
    /// <summary>
    /// ������������
    /// </summary>
    public void ResetAllHumans()
    {
        Debug.Log("��������������ĺ���");
        foreach (HumanControl human in personList)
        {
            Debug.Log("����ʱ�ҵ����б��е�����" + human);
            int randomFloor = 1;
            Vector3 spawnPosition = GetSpawnBlockPosition(randomFloor) + new Vector3(0, 1, 0);

            if (spawnPosition == Vector3.zero)
                continue;
            if (!human.gameObject.activeSelf)
            {
                human.gameObject.SetActive(true);
                human.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            }  // ��������}
            human.Start();
        }
    }

    private void AddRobot(int num)  //��������ӻ����ˣ��������.�����˵��������ٶ���ȷ�����Ƿ��б�Ҫ������������
    {
        // �ڳ���������num�������ˣ��������Ǽ��뵽List��

        GameObject RobotParent = GameObject.Find("RobotList");

        for (int i = 0; i < num; i++)
        {
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
    }

    private void AddRobotBrain(int num)  //��������ӻ����˴���. ��������װ  �����˵��������ٶ���ȷ�����Ƿ��б�Ҫ������������
    {
        GameObject BrainParent = GameObject.Find("RobotBrain");
        Debug.Log("������ӻ����˵Ĵ���");
        for(int i = 0;i < num;i++)
        {

            GameObject RobotBrain = Instantiate(BrainPerfab);//ʵ���������˵�λ��
            BrianList.Add(RobotBrain.GetComponent<RobotBrain>());//����Brain�б���
            RobotBrain robotBrain = RobotBrain.GetComponent<RobotBrain>();
            RobotBrain.transform.parent = BrainParent.transform;//���ø�����

            /*  ��������л����˺ʹ��ԵĽű���ʼ������������������������������������������������������������������������������������*/
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

                // ���û����ײ�����ҵ�����λ��
                if (colliders.Length == 0)
                {
                    positionFound = true;
                }

                attempts++;
            }

            if (positionFound)
            {
                // ʵ��������
                GameObject Person = Instantiate(HumanPrefab, spawnPosition, Quaternion.identity);
                personList.Add(Person.GetComponent<HumanControl>());
                Person.transform.parent = humanParent.transform;
                Person.GetComponent<HumanControl>().myEnv = this;
            }
            else
            {
                Debug.LogWarning($"�޷�Ϊ�� {i + 1} �������ҵ����ʵ�λ�ã�");
            }
        }

        Debug.Log("ִ���������ຯ��");
    }
    private void AddExits()
    {
         GameObject Exit = GameObject.Find("Exit");
            Exits.Add(Exit);
            Debug.Log("ִ����ӳ��ں�������ֵ");
       
    }
}
