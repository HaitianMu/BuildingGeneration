using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class DoorControl : MonoBehaviour
{
    public string doorDirection; // ÃÅµÄ³¯Ïò

   
    public void AddNavMeshLink()
    {
        NavMeshLink Link = this.AddComponent<Unity.AI.Navigation.NavMeshLink>();
        if (doorDirection == "Vertical")
        {
            Link.startPoint = new Vector3(0, -1.5f, 0.4f);
            Link.endPoint = new Vector3(0, -1.5f, -0.4f);
        }
        else if (doorDirection == "Horizontal")
        {
            Link.startPoint = new Vector3(0.4f, -1.5f, 0);
            Link.endPoint = new Vector3(-0.4f, -1.5f, 0);
        }

    }
}
