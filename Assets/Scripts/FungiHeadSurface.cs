using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class FungiHeadSurface : MonoBehaviour
{
    public bool IsSomeoneOnTop { get; private set; }
    [SerializeField] LayerMask fungiMask;
    NavMeshSurface navMeshSurface;
    float halfHeight;

    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        halfHeight = GetComponent<MeshRenderer>().bounds.size.y / 2f;
    }

    void Update()
    {
        navMeshSurface.BuildNavMesh();
        IsSomeoneOnTop = DetectSomeoneOnTop();
    }

    bool DetectSomeoneOnTop()
    {
        float maxDistance = halfHeight + 1.5f;
        if(!Physics.Raycast(transform.position, Vector3.up, out RaycastHit hit, maxDistance, fungiMask)) return false;
        Fungi fungi = hit.transform.GetComponent<Fungi>();
        if (!fungi.CompareState(Fungi.State.Waiting) && !fungi.CompareState(Fungi.State.Listening)) return false;
        return true;
    }

    private void OnDrawGizmos()
    {
        float maxDistance = GetComponent<MeshRenderer>().bounds.size.y / 2f + 1;
        Gizmos.DrawRay(transform.position, Vector3.up * maxDistance); 
    }
}
