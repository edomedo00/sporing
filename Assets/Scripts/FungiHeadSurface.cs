using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class FungiHeadSurface : MonoBehaviour
{
    public bool IsSomeoneOnTop { get; private set; }
    [SerializeField] LayerMask fungiMask;
    NavMeshSurface navMeshSurface;
    [SerializeField] float maxDistance = 1;

    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
    }

    void Update()
    {
        navMeshSurface.BuildNavMesh();
        IsSomeoneOnTop = DetectSomeoneOnTop();
    }

    bool DetectSomeoneOnTop()
    {
        if(!Physics.Raycast(transform.GetChild(0).position, Vector3.up, out RaycastHit hit, maxDistance, fungiMask)) return false;
        if(!hit.transform.parent.GetComponent<Fungi>()) return false;
        Fungi fungi = hit.transform.parent.GetComponent<Fungi>();
        if (!fungi.CompareState(Fungi.State.Waiting) && !fungi.CompareState(Fungi.State.Listening)) return false;
        return true;
    }

    private void OnDrawGizmos()
    { 
        Gizmos.DrawRay(transform.GetChild(0).position, Vector3.up * maxDistance); 
    }

    Vector2 IgnoreY(Vector3 vector3)
    {
        return new(vector3.x, vector3.z);
    }
}
