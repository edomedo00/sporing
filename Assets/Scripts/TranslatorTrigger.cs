using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslatorTrigger : MonoBehaviour
{
    FungiTranslator fungi;

    private void Awake()
    {
        fungi = transform.parent.GetComponent<FungiTranslator>();
    }

    private void OnTriggerStay(Collider other)
    {
        StartCoroutine(fungi.JoinAnotherFungi(other));
    }
}
