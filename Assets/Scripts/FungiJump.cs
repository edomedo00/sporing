using DG.Tweening;
using extOSC;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class FungiJump : Fungi
{
    FungiHeadSurface headSurface;
    NavMeshLink[] navMeshLinks; 

    public override void Awake()
    {
        base.Awake();
        headSurface = transform.GetComponentInChildren<FungiHeadSurface>();
        navMeshLinks = GetComponentsInChildren<NavMeshLink>();
    }


    public override void NoTween(int times = 3, float speed = 0.2F)
    {
        Transmitter.Send(new OSCMessage("/trampolineNo"));
        base.NoTween(times, speed);
    }

    public override Sequence JumpTween(int jumpNumber = 1)
    {
        var transpose = -100;
        var message = new OSCMessage("/fungiJump", OSCValue.Float(transpose), OSCValue.Int(jumpNumber));
        Transmitter.Send(message);
        return base.JumpTween(jumpNumber);
    }

    public override Sequence Talk()
    {
        Transmitter.Send(new OSCMessage("/trampolineTalk"));
        Transmitter.Send(new OSCMessage("/ampBass"));
        return base.JumpTween();
    }


    private void LateUpdate()
    {
        if (agent.velocity.sqrMagnitude > 0.001f ||
            CompareState(State.Walking) ||
            headSurface.IsSomeoneOnTop)
        {
            foreach (var link in navMeshLinks)
                link.enabled = false;
            return;
        }
        foreach (var link in navMeshLinks)
            link.enabled = true;
    }

    public override void Activate()
    {
        base.Activate();
        DestroyImmediate(GetComponent<Rigidbody>());
    }
}
