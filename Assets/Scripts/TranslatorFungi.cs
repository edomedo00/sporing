using DG.Tweening;
using extOSC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class TranslatorFungi : Fungi
{
    [SerializeField] float talkMargin = 2;
    Transform player;
    public OSCTransmitter Transmitter;
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

        var transpose = 100;
        var jumps = 3;
        var message = new OSCMessage("/fungiJump", OSCValue.Float(transpose), OSCValue.Int(jumps));
        Transmitter.Send(message);
        sequence = JumpTween();
        yield return sequence.WaitForKill();

        Transmitter.Send(new OSCMessage("/ampPiano"));
        JoinFungi();
    }

    public IEnumerator JoinAnotherFungi(Collider other)
    {
        if (!IsInTheSameHeight(other.transform.parent, 1)) yield break;

        if (waypoints.Count == 0) yield break;
        if(interacting) yield break;
        interacting = true;
        Fungi otherFungi = other.transform.parent.gameObject.GetComponent<Fungi>();
        
        yield return StartCoroutine(RepositionInFrontOf(other.transform.parent));

        sequence = JumpTween();
        state = State.Walking;
        yield return sequence.WaitForKill();
        otherFungi.sequence = otherFungi.JumpTween();
        yield return otherFungi.sequence.WaitForKill();

        for(int i = 0; i < 3; i++)
        {
            sequence = JumpTween();
            otherFungi.sequence = otherFungi.JumpTween();
            yield return otherFungi.sequence.WaitForKill();
        }

        otherFungi.JoinFungi();
        FollowPlayer();
        interacting = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, talkMargin);
    }
}
