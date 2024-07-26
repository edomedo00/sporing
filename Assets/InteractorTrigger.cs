using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractorTrigger : MonoBehaviour
{
    InteractorFungi fungi;

    private void Awake()
    {
        fungi = transform.parent.GetComponent<InteractorFungi>();
    }

    private void OnTriggerStay(Collider other)
    {
        StartCoroutine(fungi.PressButton(other));
    }
}
