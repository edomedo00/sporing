using DG.Tweening;
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
        base.NoTween(times, speed);
        // Aquí puedes poner el sonido de NO
    }

    public override Sequence JumpTween(int jumpNumber = 1)
    {
        return base.JumpTween(jumpNumber);
        // Aquí puedes poner el sonido de SALTO
    }

    public override Sequence Talk()
    {
        //Aquí puedes poner el sonido que van a hacer al HABLAR
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
