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
    // bot的NavMeshAgent组件
    private NavMeshAgent _botNavMeshAgent;
    public bool isRunning;//机器人是否处于工作状态
    // Start is called before the first frame update
    public void Start()
    {
        this.gameObject.SetActive(true);
        isRunning = true;//机器人默认工作
        myDirectFollowers = new List<HumanControl>();
        _botNavMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void Update()
    {
        robotFollowerCounter = myDirectFollowers.Count;

    }
}
