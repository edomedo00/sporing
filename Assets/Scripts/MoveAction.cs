using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveAction : Action
{
    Transform target;

    private void Start()
    {
        target = transform.GetChild(0);
    }

    public override void ExcecuteAction()
    {
        base.ExcecuteAction();
        StartCoroutine(PushProcess());
    }

    IEnumerator PushProcess()
    {
        excecuting = true;
        float duration = CalculateDuration(transform.position, target.position, 5);
        yield return transform.DOMove(target.position, duration).SetEase(Ease.InOutSine).WaitForKill();
        excecuting = false;
    }

    float CalculateDuration(Vector3 from, Vector3 to, float velocity)
    {
        float distance = Vector3.Distance(from, to);
        return distance / velocity;
    }
}
