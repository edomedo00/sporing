using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class FungiHeadSurface : MonoBehaviour
{
    public bool IsSomeoneOnTop { get; private set; }
    [SerializeField] LayerMask fungiMask;
    NavMeshSurface navMeshSurface;
    float maxDistance;

    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        maxDistance = Vector3.Distance(IgnoreY(transform.position), IgnoreY(transform.GetChild(0).position));
    }

    void Update()
    {
        navMeshSurface.BuildNavMesh();
        IsSomeoneOnTop = DetectSomeoneOnTop();
    }

    bool DetectSomeoneOnTop()
    {
        if(!Physics.Raycast(transform.GetChild(0).position, -transform.GetChild(0).forward, out RaycastHit hit, maxDistance, fungiMask)) return false;
        if(!hit.transform.GetComponent<Fungi>()) return false;
        Fungi fungi = hit.transform.GetComponent<Fungi>();
        if (!fungi.CompareState(Fungi.State.Waiting) && !fungi.CompareState(Fungi.State.Listening)) return false;
        return true;
    }

    private void OnDrawGizmos()
    { 
        float maxDistance = Vector3.Distance(IgnoreY(transform.position), IgnoreY(transform.GetChild(0).position));
        Gizmos.DrawRay(transform.GetChild(0).position, -transform.GetChild(0).forward * maxDistance); 
    }

    Vector2 IgnoreY(Vector3 vector3)
    {
        return new(vector3.x, vector3.z);
    }
}
