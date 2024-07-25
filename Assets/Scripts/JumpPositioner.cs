using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class JumpPositioner : MonoBehaviour
{
    [SerializeField] LayerMask layerMask;
    [Range(1, 360)]
    [SerializeField] int subdivisions = 1;
    [Range(1, 360)]
    [SerializeField] int circleDegrees = 360;
    [SerializeField] float maxDistance = 2;
    NavMeshLink navMeshLink;
    Vector3 offset;
    float startAngle;

    void Start()
    {
        navMeshLink = GetComponent<NavMeshLink>();
        navMeshLink.activated = false;
        offset =  navMeshLink.startPoint;
        startAngle = transform.localEulerAngles.y;
    }
    void LateUpdate()
    {
        SetStartPoint();
    }

    void SetStartPoint()
    {
        List<Vector3> positions = new();
        Ray[] rays = Rays(offset, subdivisions, transform, out Vector3[] directions, startAngle, circleDegrees);
        for (int i = 0; i < rays.Length; i++)
        {
            if (!Physics.Raycast(rays[i], out RaycastHit hit, maxDistance, layerMask)) continue;
            Vector3 point = directions[i] * new Vector3(offset.x, 0, offset.z).magnitude;
            float height = hit.point.y - transform.parent.position.y;
            positions.Add(new(point.x, height, point.z));
        }
        if (positions.Count == 0)
        {
            navMeshLink.startPoint = offset;
            return;
        }
        int index = Mathf.CeilToInt(positions.Count / 2);
        navMeshLink.activated = true;
        navMeshLink.startPoint = positions[index];
        transform.localEulerAngles = (-transform.parent.eulerAngles.y) * Vector3.up;
    }

    public static Ray[] Rays(Vector3 offset, int numberOfDirections, Transform transform, out Vector3[] directions, float startAngle = 0, int circleDegrees = 360)
    {
        float heightOffset = offset.y;
        float radius = offset.z;

        directions = new Vector3[numberOfDirections];
        Quaternion rotation = Quaternion.Euler(0, startAngle, 0);
        for (int i = 0; i < directions.Length; i++)
        {
            directions[i] = rotation * Vector3.forward;
            rotation.eulerAngles += Vector3.up * ((float)circleDegrees / directions.Length);
        }

        Ray[] rays = new Ray[directions.Length];
        for(int i = 0; i < rays.Length; i++)
        {
            Vector3 position = transform.position + directions[i] * radius;
            position += Vector3.up * heightOffset;
            rays[i] = new(position, Vector3.down);
        }
        return rays;
    }

    private void OnDrawGizmos()
    {
        navMeshLink = GetComponent<NavMeshLink>();
        Gizmos.color = Color.red;
        foreach (Ray ray in Rays(offset, subdivisions, transform, out _, startAngle, circleDegrees))
            Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);
    }
}
