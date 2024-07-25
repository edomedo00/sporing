using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class HeadFungi : Fungi
{
    FungiHeadSurface headSurface;
    NavMeshLink[] navMeshLinks; 

    public override void Awake()
    {
        base.Awake();
        headSurface = transform.GetComponentInChildren<FungiHeadSurface>();
        navMeshLinks = GetComponentsInChildren<NavMeshLink>();
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
            link.enabled = false;
    }

    public override void JoinFungi()
    {
        base.JoinFungi();
        DestroyImmediate(GetComponent<Rigidbody>());
    }

    public override void Activate()
    {
        base.Activate();
        DestroyImmediate(GetComponent<Rigidbody>());
    }
}
