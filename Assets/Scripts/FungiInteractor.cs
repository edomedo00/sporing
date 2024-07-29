using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class FungiInteractor : Fungi
{

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


    private void OnTriggerStay(Collider other)
    {
        StartCoroutine(PressButton(other));
    }

    public IEnumerator PressButton(Collider other)
    {
        if (!IsInTheSameHeight(other.transform, 1)) yield break;
        if (state != State.Walking) yield break;
        if (waypoints.Count == 0) yield break;
        if (interacting) yield break;
        interacting = true;
        Vector3 target = other.transform.position;
        target += (other.GetComponent<MeshRenderer>().bounds.size.y / 2f) * Vector3.up;
        target += (agent.height / 2f) * Vector3.up;
        Vector3 originalPos = transform.position;

        waypoints[^1].position = target;
        waypoints[^1].GetComponent<MeshRenderer>().enabled = false;
        agent.SetDestination(target);

        while (state != State.Waiting) yield return null;
        state = State.Walking;
        yield return StartCoroutine(other.GetComponent<ButtonActionLinker>().PressButton());
        agent.SetDestination(originalPos);
        while (agent.remainingDistance > agent.stoppingDistance) yield return null;

        state = State.Waiting;
        other.gameObject.layer = 0;
        interacting = false;

        if (IsPlayerCloseEnough(10, 2)) FollowPlayer();
    }

    IEnumerator Interact(Collider other)
    {
        if(state != State.Walking) yield break;
        if (waypoints.Count == 0) yield break;
        if(interacting) yield break;
        interacting = true;

        yield return StartCoroutine(RepositionInFrontOf(other.transform));

        Vector3 point = other.transform.parent.GetChild(0).position;
        float velocity = Vector3.Distance(other.transform.position, point) / 5;

        NavMeshObstacle[] obstacles = other.transform.parent.GetComponentsInChildren<NavMeshObstacle>();
        foreach(NavMeshObstacle obstacle in obstacles)
            obstacle.enabled = false;
        yield return null;
        Vector3 offset = other.transform.parent.position - transform.position;
        agent.SetDestination(point);
        yield return null;
        while(agent.remainingDistance > agent.stoppingDistance)
        {
            other.transform.parent.position = transform.position + offset;
            yield return null;
        }
        
        agent.ResetPath();
        ChangeState(State.Waiting);
        other.gameObject.layer = 0;
        interacting = false;
        foreach (NavMeshObstacle obstacle in obstacles)
            obstacle.enabled = true;
    }
}
