using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class Fungi : MonoBehaviour
{
    public NavMeshAgent agent;
    public List<Transform> waypoints;
    [SerializeField] float interactionDistance = 1;
    [SerializeField] protected float maxPathDistance = 70;
    public float MaxPathDistance { get { return maxPathDistance; } }
    public enum State { Listening, Walking, Following, Waiting }
    public Sequence sequence;
    public AgentLinkMover agentLinkMover;
    protected State state;
    protected bool interacting;
    protected new Collider collider;

    public virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agentLinkMover = GetComponent<AgentLinkMover>();
        collider = transform.GetChild(0).GetComponent<Collider>();
        state = State.Following;
        sequence = DOTween.Sequence();
        interacting = false;
        if (!CompareTag("Fungi")) StartCoroutine(RepositionAgent());
        maxPathDistance = 150;
    }

    IEnumerator RepositionAgent()
    {
        GetComponent<NavMeshObstacle>().enabled = false;
        agent.enabled = true;
        yield return new WaitForEndOfFrame();
        agent.enabled = false;
        GetComponent<NavMeshObstacle>().enabled = true;
    }

    public virtual void Interact()
    {

    }

    public virtual void Activate()
    {
        collider.gameObject.layer = 6;
        gameObject.tag = "Fungi";
        DestroyImmediate(GetComponent<NavMeshObstacle>());
        DestroyImmediate(GetComponent<NavMeshModifier>());
        agent.enabled = true;
        transform.SetParent(null);
    }

    public void FollowPath(List<Transform> points) => StartCoroutine(FollowPath_(points));
    IEnumerator FollowPath_(List<Transform> points)
    {
        float lastStoppingDistance = agent.stoppingDistance;
        agent.stoppingDistance = 0.3f;
        waypoints = new(points);

        foreach (Transform point in waypoints)
        {
            if (DetectMyself(point)) { yield return sequence.WaitForKill(); continue; }
            yield return StartCoroutine(SecureAPath(point));
        }

        ChangeState(State.Waiting);
        foreach (Transform point in waypoints)
            Destroy(point.gameObject);
        agent.ResetPath();
        waypoints = new();

        agent.stoppingDistance = lastStoppingDistance;
    }

    public void FollowPlayer() => StartCoroutine(FollowPlayer_());
    IEnumerator FollowPlayer_()
    {
        yield return StartCoroutine(SecureAPath(FungiManager.Singleton.PlayerFollowPoint, 2, State.Following));
        if (state != State.Waiting) ChangeState(State.Following);
        else agent.ResetPath();
    }

    int iteration = 0;
    IEnumerator SecureAPath(Transform point, float targetMargin = 0, State newState = State.Walking)
    {
        iteration = 0;
        ChangeState(State.Walking);
        yield return null;
        while (true)
        {
            agent.SetDestination(point.position);
            yield return null;

            yield return StartCoroutine(NoPath(point, newState, targetMargin));
            if (finalized) break;
            iteration++;
            yield return StartCoroutine(CheckPath(point, false));
            if (thereIsAPath) break;
            iteration++;
        }
        iteration = 0;
    }

    IEnumerator NoPath(Transform point, State newState = State.Walking, float targetTolerance = 0)
    {
        yield return StartCoroutine(CheckPath(point, true, newState, targetTolerance));
        if (thereIsAPath) yield break;

        while (agentLinkMover.IsJumping) yield return null;
        ChangeState(State.Waiting);
        NoTween();
        if (sequence.IsActive()) yield return sequence.WaitForKill();
        finalized = true;
    }

    bool thereIsAPath = false;
    bool finalized = false;
    IEnumerator CheckPath(Transform point, bool findingPath, State newState = State.Walking, float targetTolerance = 0)
    {
        finalized = false;
        //if (findingPath) print(gameObject.name + ": Buscando camino");
        //else print(gameObject.name + ": Conservando camino");

        NavMeshPath path = new();
        agent.CalculatePath(point.position, path);
        if (path.corners.Length != 0) agent.SetDestination(path.corners[^1]);

        yield return null;
        float lastSpeed = 0;
        float minVelocity = 0.001f;
        float speedLimit = agent.speed * agent.speed * 0.1f;
        float velocity = new Vector2(agent.velocity.x, agent.velocity.z).magnitude;

        float time = 0;
        if (iteration == 0) //Último cambio
        {
            if (GetPathRemainingDistance(agent, path, point.position) > maxPathDistance)
            {
                thereIsAPath = false;
                yield break;
            }
            if(point.parent != null)
                if(point.parent.gameObject.layer == 8)
                    if(path.status == NavMeshPathStatus.PathPartial ||
                        path.status == NavMeshPathStatus.PathInvalid ||
                        path.corners.Length == 0)
                    {
                        thereIsAPath = false;
                        yield break;
                    }
            while (velocity <= minVelocity && time < 0.2f)
            {
                time += Time.deltaTime;
                velocity = new Vector2(agent.velocity.x, agent.velocity.z).magnitude;
                yield return null;
            }
        }
        while (velocity > minVelocity || agentLinkMover.IsJumping)
        {
            if (!findingPath) ChangeState(newState);
            if (velocity <= minVelocity)
                while (agentLinkMover.IsJumping || lastSpeed > speedLimit)
                {
                    lastSpeed = velocity;
                    yield return null;
                }
            lastSpeed = velocity;

            velocity = new Vector2(agent.velocity.x, agent.velocity.z).magnitude;

            path = new();
            agent.CalculatePath(point.position, path);
            if (path.corners.Length != 0) agent.SetDestination(path.corners[^1]);

            bool comparison = (path.status == NavMeshPathStatus.PathComplete) && !agent.isPathStale;
            if (!findingPath) comparison = !comparison;

            if (comparison)
            {
                //if (findingPath) print(gameObject.name + ": Camino encontrado");
                //else print(gameObject.name + ": Camino perdido");

                thereIsAPath = findingPath;
                yield break;
            }

            yield return null;
        }
        thereIsAPath = !findingPath;
        if (findingPath)
        {
            float distance = Vector3.Distance(transform.position, point.position);
            if (distance < agent.stoppingDistance + targetTolerance)
            {
                //print(gameObject.name + ": Sea como sea pero llegué");
                ChangeState(newState);
                finalized = true;
                thereIsAPath = true;
            }
            //else print(gameObject.name + ": No encontré un camino");
        }
        //else print(gameObject.name + ": Camino finalizado");
    }

    bool DetectMyself(Transform point)
    {
        if (point.parent.parent != null)
            if (point.parent.parent.gameObject.Equals(gameObject))
            {
                NoTween();
                return true;
            }
        return false;
    }

    public static float GetPathRemainingDistance(NavMeshAgent agent, NavMeshPath path, Vector3 point)
    {
        if (path.status == NavMeshPathStatus.PathInvalid || path.corners.Length == 0)
        {
            NavMesh.SamplePosition(point, out NavMeshHit hit, 5, NavMesh.AllAreas);
            path = new();
            if (!hit.hit) return -1;
            agent.CalculatePath(hit.position, path);
        }

        float distance = 0.0f;
        for (int i = 0; i < path.corners.Length - 1; ++i)
            distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        return distance;
    }

    public virtual void JoinFungi()
    {
        FungiManager.Singleton.JoinFungi(gameObject);
        FollowPlayer();
    }

    public IEnumerator RepositionInFrontOf(Transform other)
    {
        float size = 0;
        if (other.GetComponent<MeshRenderer>() != null) size = other.transform.localScale.z / 2f;
        size += transform.localScale.z / 2f;
        float distance = size + interactionDistance;
        waypoints[^1].position = other.position + other.forward * distance;
        waypoints[^1].GetComponent<MeshRenderer>().enabled = false;

        while (state != State.Waiting) yield return null;

        while (agent.velocity.magnitude != 0 || agentLinkMover.IsJumping) yield return null;
        ChangeState(State.Walking);
        sequence.Kill();
        sequence = DOTween.Sequence();
        sequence = LookAtTween(other.position, 0.5f);
        yield return sequence.WaitForKill();
    }

    public void NoTween(int times = 3, float speed = 0.2f)
    {
        sequence.Kill();
        int sign;
        for (int i = 0; i < times; i++)
        {
            sign = 1;
            if (i % 2 == 0) sign = -1;
            sequence = DOTween.Sequence();
            sequence.Insert(i * speed, transform.DOLocalRotate
                                 (transform.eulerAngles + 30 * sign * Vector3.up, speed));
        }
        sequence.Insert(times * speed, transform.DOLocalRotate(transform.eulerAngles, speed));
    }

    public Sequence JumpTween()
    {
        Sequence sequence = DOTween.Sequence();
        return sequence.Insert(0, transform.DOJump(transform.position, 1, 1, 0.4f, false));
    }
    protected Sequence LookAtTween(Vector3 target, float speed = 1)
    {
        Vector3 direction = Vector3.forward;
        Vector3 targetDirection = target - transform.position;
        float dotProduct = Vector3.Dot(direction, targetDirection);
        float clockwise = 1;
        if (Cross(direction, targetDirection).y < 0) clockwise = -1;
        float targetAngle = Mathf.Acos(dotProduct / (targetDirection.magnitude * direction.magnitude)) * Mathf.Rad2Deg;
        targetAngle *= -clockwise;
        float time = speed / (180 / Mathf.Abs(Mathf.DeltaAngle(targetAngle, transform.localEulerAngles.y)));
        return sequence.Insert(0, transform.DOLocalRotate(Vector3.up * targetAngle, time));
    }

    protected bool IsInTheSameHeight(Transform other, float tolerance)
    {
        return Mathf.Abs(transform.position.y - other.position.y) < tolerance;
    }

    Vector3 Cross(Vector3 v, Vector3 w)
    {
        float x = v.y * w.z - v.z * w.y;
        float y = v.x * w.z - v.z * w.x;
        float z = v.x * w.y - v.y * w.x;
        return new(x, y, z);
    }

    protected bool IsPlayerCloseEnough(float farnessLimit, float heightLimit)
    {
        Vector3 player = FungiManager.Singleton.PlayerFollowPoint.position;
        Vector2 playerPos = new(player.x, player.z);
        Vector2 fugniPos = new(transform.position.x, transform.position.z);
        if (Mathf.Abs(player.y - transform.position.y) > heightLimit) return false;
        if (Vector2.Distance(playerPos, fugniPos) > farnessLimit) return false;
        return true;
    }

    public static List<GameObject> corners = new();
    List<GameObject> DebugPath(NavMeshPath path)
    {
        print(gameObject.name + ": " + path.status +
                                "   Has path: " + agent.hasPath +
                                "   Path stale: " + agent.isPathStale);

        if (path.corners.Length == 0) return null;
        for (int i = 0; i < path.corners.Length; i++)
        {
            Vector3 corner = path.corners[i];
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = corner;
            DestroyImmediate(sphere.GetComponent<Collider>());
            if (i < path.corners.Length - 1) sphere.transform.localScale = Vector3.one * 0.3f;
            corners.Add(sphere);
        }
        return corners;
    }

    [ContextMenu("Clear path")]
    void ClearCorners()
    {
        foreach (GameObject coner in corners)
            Destroy(coner);
        corners.Clear();
    }

    public virtual void ChangeState(State state)
    {
        if (state == State.Listening || state == State.Waiting) agent.ResetPath();
        this.state = state;
    }

    public bool CompareState(State state)
    {
        return this.state == state;
    }

    public State GetState()
    {
        return state;
    }
}
