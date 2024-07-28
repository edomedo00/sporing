using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
public class FungiManager
{
    Transform playerFollowPoint;
    List<Fungi> fungis;
    NavMeshSurface navMeshSurface;

    private static FungiManager Instance;
    public Transform PlayerFollowPoint { get { return playerFollowPoint; } }
    public List<Fungi> Fungis { get { return fungis; } }
    public NavMeshSurface NavMeshSurface { get { return navMeshSurface; } }

    public static FungiManager Singleton
    {
        get
        {
            if (Instance == null)
            {
                Instance = new();

                Instance.playerFollowPoint = GameObject.FindGameObjectWithTag("Follow").transform;
                Instance.navMeshSurface = GameObject.FindGameObjectWithTag("NavmeshSurface").GetComponent<NavMeshSurface>();

                GameObject[] fungis = GameObject.FindGameObjectsWithTag("Fungi");
                Instance.fungis = new();
                foreach (GameObject fungi in fungis)
                {
                    Instance.fungis.Add(fungi.GetComponent<Fungi>());
                    Instance.ActivateFungi(Instance.fungis[^1]);
                }
                //Instance.TurnOffTestMode();
            }
            return Instance;
        }
    }

    void TurnOffTestMode()
    {
        playerFollowPoint.parent.position = new(0,1, -40);
        foreach(Fungi fungi in fungis)
            Object.Destroy(fungi.gameObject);
        Instance.fungis.Clear();
    }

    public void Follow(Transform transform)
    {
        foreach (Fungi fungi in Singleton.fungis)
            fungi.agent.SetDestination(transform.position);
    }

    bool playerTooFar = false;
    public void FollowPlayer()
    {
        if (PlayerTooFar(FindAFollowingFungi()))
        {
            if (!playerTooFar)
            {
                PlayerTooFarReaction();
                playerTooFar = true;
            }
            return;
        }
        foreach (Fungi fungi in Singleton.fungis)
            if (fungi.CompareState(Fungi.State.Following))
                fungi.agent.SetDestination(playerFollowPoint.position);

        playerTooFar = false;
    }

    void PlayerTooFarReaction()
    {
        foreach(Fungi fungi in Singleton.fungis)
            if (fungi.CompareState(Fungi.State.Following))
            {
                fungi.NoTween(5);
                fungi.agent.ResetPath();
            }
    }

    Fungi FindAFollowingFungi()
    {
        foreach (Fungi fungi in Singleton.fungis)
            if (fungi.CompareState(Fungi.State.Following))
                return fungi;

        return null;
    }

    bool PlayerTooFar(Fungi fungi)
    {
        if (fungi == null) return false;
        NavMeshPath path = new();
        if (Fungi.GetPathRemainingDistance(fungi.agent, path, playerFollowPoint.position) > fungi.MaxPathDistance)
            return true;
        return false;
    }

    public void JoinFungi(GameObject fungiObject)
    {
        foreach (Fungi fungi in Singleton.Fungis)
            if (fungiObject == fungi.gameObject) return;

        ActivateFungi(fungiObject.GetComponent<Fungi>());
        fungis.Add(fungiObject.GetComponent<Fungi>());
    }

    void ActivateFungi(Fungi fungi)
    {
        fungi.Activate();
    }

    void Seek(Vector3 location, NavMeshAgent agent)
    {
        agent.SetDestination(location);
    }

    public Fungi GetFungi(GameObject gameObject)
    {
        foreach(Fungi fungi in Singleton.fungis)
            if(fungi.gameObject == gameObject) return fungi;

        return null;
    }

    public Fungi GetFungi(RaycastHit hit)
    {
        GameObject gameObject = hit.transform.gameObject;
        foreach (Fungi fungi in Singleton.fungis)
            if (fungi.gameObject == gameObject) return fungi;

        return null;
    }

    void Pursue(NavMeshAgent agent, Transform transform)
    {
        Vector3 targetDirection = playerFollowPoint.position - transform.forward;
        float angleBetweenForwards = Vector3.Angle(transform.forward, transform.TransformVector(playerFollowPoint.transform.forward));
        float toTarget = Vector3.Angle(transform.forward, transform.TransformVector(targetDirection));

        if (toTarget > 90 && angleBetweenForwards < 20 || playerFollowPoint.GetComponent<Rigidbody>().velocity.magnitude < 0.01)
        {
            Seek(playerFollowPoint.position, agent);
            return;
        }


        float lookAhead = targetDirection.magnitude / (agent.speed + playerFollowPoint.GetComponent<Rigidbody>().velocity.magnitude);
        Seek(playerFollowPoint.transform.position + playerFollowPoint.forward * lookAhead, agent);
    }
}
