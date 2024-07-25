using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslatorFungi : Fungi
{
    [SerializeField] float talkMargin = 2;
    Transform player;
    // Start is called before the first frame update

    public override void Awake()
    {
        base.Awake();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(JoinMyself());
    }

    IEnumerator JoinMyself()
    {
        yield return sequence.WaitForKill();
        while (Vector3.Distance(transform.position, player.position) > talkMargin)
            yield return null;

        sequence = JumpTween();
        yield return sequence.WaitForKill();

        JoinFungi();
    }

    IEnumerator JoinAnotherFungi(Collider other)
    {
        if (waypoints.Count == 0) yield break;
        if(interacting) yield break;
        interacting = true;
        Fungi fungi = other.gameObject.GetComponent<Fungi>();
        
        yield return StartCoroutine(RepositionInFrontOf(other.transform, 1f));

        sequence = JumpTween();

        yield return sequence.WaitForKill();
        fungi.sequence = fungi.JumpTween();
        yield return fungi.sequence.WaitForKill();

        for(int i = 0; i < 3; i++)
        {
            sequence = JumpTween();
            fungi.sequence = fungi.JumpTween();
            yield return fungi.sequence.WaitForKill();
        }

        fungi.JoinFungi();
        FollowPlayer();
        interacting = false;
    }

    private void OnTriggerStay(Collider other)
    {
        StartCoroutine(JoinAnotherFungi(other));
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, talkMargin);
    }
}
