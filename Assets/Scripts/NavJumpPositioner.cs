using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class NavJumpPositioner : MonoBehaviour
{
    [SerializeField] LayerMask layerMask;
    NavMeshLink navMeshLink;
    Fungi fungi;

    // Start is called before the first frame update
    void Start()
    {
        navMeshLink = GetComponent<NavMeshLink>();
        fungi = transform.parent.GetComponent<Fungi>();
        navMeshLink.activated = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(fungi.agent.remainingDistance > fungi.agent.stoppingDistance)
        {
            navMeshLink.activated = false;
            return;
        }
        navMeshLink.activated = true;
        SetStartPoint();
    }

    void SetStartPoint()
    {
        Ray ray = Ray(out float maxDistance);
        if(!Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask)) return;
        float height = hit.point.y - transform.parent.position.y;
        navMeshLink.startPoint = new(navMeshLink.startPoint.x, height, navMeshLink.startPoint.z);
    }

    Ray Ray(out float maxDistance)
    {
        maxDistance = 2;
        float heightOffset = 0.5f;
        float jumpDistance = navMeshLink.startPoint.z;
        Vector3 position = transform.position + transform.forward * jumpDistance;
        position += Vector3.up * heightOffset;
        return new Ray(position, Vector3.down);
    }

    private void OnDrawGizmos()
    {
        navMeshLink = GetComponent<NavMeshLink>();
        Ray ray = Ray(out float maxDistance);
        Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);
    }
}
