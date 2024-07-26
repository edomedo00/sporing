using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslatorTrigger : MonoBehaviour
{
    TranslatorFungi fungi;

    private void Awake()
    {
        fungi = transform.parent.GetComponent<TranslatorFungi>();
    }

    private void OnTriggerStay(Collider other)
    {
        StartCoroutine(fungi.JoinAnotherFungi(other));
    }
}
