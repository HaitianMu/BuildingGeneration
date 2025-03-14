using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobotControl : MonoBehaviour
{
    public string robotCommand;
    public List<HumanControl> myDirectFollowers;
    public int robotFollowerCounter;
    public RobotBrain myAgent;
    // bot��NavMeshAgent���
    private NavMeshAgent _botNavMeshAgent;
    public bool isRunning;//�������Ƿ��ڹ���״̬
    // Start is called before the first frame update
    public void Start()
    {
        this.gameObject.SetActive(true);
        isRunning = false;//������Ĭ��Ϊ������
        myDirectFollowers = new List<HumanControl>();
        _botNavMeshAgent = GetComponent<NavMeshAgent>();
    }

}
