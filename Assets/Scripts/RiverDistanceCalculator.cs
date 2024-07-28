using UnityEngine;
using UnityEngine.Events;

public class RiverDistanceCalculator : MonoBehaviour
{
    public Transform object1;
    public Transform object2;

    public UnityEvent<float> OnDistanceChanged = new UnityEvent<float>();

    private Collider collider1;
    private Collider collider2;

    void Start()
    {
        collider1 = object1.GetComponent<Collider>();
        collider2 = object2.GetComponent<Collider>();
    }

    void Update()
    {
        if (object1 != null && object2 != null && collider1 != null && collider2 != null)
        {
            Vector3 closestPoint1 = collider1.ClosestPoint(object2.position);
            Vector3 closestPoint2 = collider2.ClosestPoint(object1.position);

            float distance = Vector3.Distance(closestPoint1, closestPoint2);

            float normalizedDistance;
            if (distance < 60)
            {
                normalizedDistance = distance / 60f;
            }
            else
            {
                normalizedDistance = 1f;
            }

            OnDistanceChanged.Invoke(normalizedDistance);
        }
    }
}
