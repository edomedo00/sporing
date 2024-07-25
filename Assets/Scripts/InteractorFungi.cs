using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class InteractorFungi : Fungi
{
    private void OnTriggerStay(Collider other)
    {
        StartCoroutine(PressButton(other));
    }

    IEnumerator PressButton(Collider other)
    {
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
        float lastStoppingDistance = agent.stoppingDistance;
        agent.stoppingDistance = 0.3f;

        while (state != State.Waiting) yield return null;
        state = State.Walking;

        yield return StartCoroutine(other.GetComponent<ButtonActionLinker>().PressButton());
        agent.SetDestination(originalPos);
        while (agent.remainingDistance > agent.stoppingDistance) yield return null;

        state = State.Waiting;
        agent.stoppingDistance = lastStoppingDistance;
        other.gameObject.layer = 0;
        interacting = false;
    }

    IEnumerator Interact(Collider other)
    {
        if(state != State.Walking) yield break;
        if (waypoints.Count == 0) yield break;
        if(interacting) yield break;
        interacting = true;

        yield return StartCoroutine(RepositionInFrontOf(other.transform, 0.3f));

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
