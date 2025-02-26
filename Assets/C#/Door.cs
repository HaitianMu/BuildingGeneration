using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    // 门的朝向
    public string doorDirection;

    public void Start()
    {
        // 通过检查水平方向上两侧是否有东西，来确定们的朝向
        Vector3 myPosition = transform.position;
        if (Physics.CheckSphere(myPosition + Vector3.left * 1.5f, 0.49f) && Physics.CheckSphere(myPosition + Vector3.right * 1.5f, 0.1f))//在门的左右1.5米的位置放一个小球看是否有东西来决定门的朝向
            doorDirection = "Horizontal";
        else
            doorDirection = "Vertical";
    }
}
