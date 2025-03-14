using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class NavitionControl : MonoBehaviour
{
    public NavMeshSurface surface;
    // Start is called before the first frame update
    void Start()
    {
        BakeNavMesh();
    }

    // Update is called once per frame
    public void BakeNavMesh()
    {
        surface.BuildNavMesh();
    }
}
