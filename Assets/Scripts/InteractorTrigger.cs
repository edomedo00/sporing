using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractorTrigger : MonoBehaviour
{
    FungiInteractor fungi;

    private void Awake()
    {
        fungi = transform.parent.GetComponent<FungiInteractor>();
    }

    private void OnTriggerStay(Collider other)
    {
        StartCoroutine(fungi.PressButton(other));
    }
}
