using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    // �ŵĳ���
    public string doorDirection;

    public void Start()
    {
        // ͨ�����ˮƽ�����������Ƿ��ж�������ȷ���ǵĳ���
        Vector3 myPosition = transform.position;
        if (Physics.CheckSphere(myPosition + Vector3.left * 1.5f, 0.49f) && Physics.CheckSphere(myPosition + Vector3.right * 1.5f, 0.1f))//���ŵ�����1.5�׵�λ�÷�һ��С���Ƿ��ж����������ŵĳ���
            doorDirection = "Horizontal";
        else
            doorDirection = "Vertical";
    }
}
