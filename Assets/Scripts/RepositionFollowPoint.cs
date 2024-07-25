using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepositionFollowPoint : MonoBehaviour
{
    [SerializeField] LayerMask layerMask;
    [SerializeField] float maxDistance = 2;
    Vector3 startPoint;

    // Start is called before the first frame update
    void Start()
    {
        startPoint = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        SetPoint();
    }

    void SetPoint()
    {
        if (!Physics.Raycast(Ray(), out RaycastHit hit, maxDistance, layerMask)) return;

        float height = hit.point.y;
        transform.position = new(transform.position.x, height, transform.position.z);
    }

    Ray Ray()
    {
        float heightOffset = startPoint.y;
        float distance = startPoint.z;

        Vector3 position = transform.parent.position + transform.forward * distance;
        position += Vector3.up * heightOffset;
        Ray ray = new(position, Vector3.down);
        return ray;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Ray ray = Ray();
        Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);
    }
}
