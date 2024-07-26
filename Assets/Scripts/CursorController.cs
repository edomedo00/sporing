using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class CursorController : MonoBehaviour
{
    [SerializeField] GameObject cursorPrefab;
    [SerializeField] LayerMask layerMask;
    [SerializeField] LayerMask fungiMask;
    [SerializeField] float maxDistance = 50;
    CinemachineFreeLook cinemachine;
    Fungi commandedFungi;
    readonly List<Transform> waypoints = new();
    MeshRenderer meshRenderer;
    PlayerInput playerInput;
    RaycastHit cursorHit;

    enum State { MovingCamera, SetPoint, Default }
    StateChanger state;

    // Start is called before the first frame update
    void Start()
    {
        cinemachine = FindAnyObjectByType<CinemachineFreeLook>();
        playerInput = GetComponent<PlayerInput>();
        state = new(State.Default);
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        CameraControl();
        HoldInteraction();

        state = state.UpdateState();
        CursorUpdate();
    }

    bool priorityActionPerformed = false;
    void HoldInteraction()
    {
        if (playerInput.actions["Follow"].phase == InputActionPhase.Performed)
        {
            priorityActionPerformed = true;
            Follow();
            return;
        }
        if (!playerInput.actions["Follow"].WasReleasedThisFrame()) return;

        if (priorityActionPerformed)
        {
            priorityActionPerformed = false;
            return;
        }

        priorityActionPerformed = false;
        playerInput.actions["Follow"].Reset();
        SetWaypoints();
        CommandAgent();
    }

    Sequence sequence;
    public void CommandAgent()
    {
        if (state.IsExit()) return;
        if (state.CompareState(State.MovingCamera)) return;
        if (sequence.IsActive()) return;

        Ray ray = Camera.main.ScreenPointToRay(playerInput.actions["Cursor"].ReadValue<Vector2>());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, fungiMask)) return;
        ResetWaypoints();
        Fungi fungi = hit.transform.parent.GetComponent<Fungi>();
        commandedFungi = fungi;
        if (commandedFungi == null) return;
        if (commandedFungi.CompareState(Fungi.State.Walking)) return;

        commandedFungi.ChangeState(Fungi.State.Listening);
        state.ChangeState(State.SetPoint);
        sequence = commandedFungi.JumpTween();
    }

    public void Follow()
    {
        if (sequence.IsActive()) return;
        if (state.CompareState(State.MovingCamera)) return;

        Ray ray = Camera.main.ScreenPointToRay(playerInput.actions["Cursor"].ReadValue<Vector2>());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, fungiMask)) return;

        Fungi fungi = hit.transform.parent.GetComponent<Fungi>();
        if (fungi.CompareState(Fungi.State.Walking) || 
            fungi.CompareState(Fungi.State.Following))  return;
        state.ChangeState(State.Default);
        ResetWaypoints();
        fungi.FollowPlayer();
    }

    void ResetWaypoints()
    {
        meshRenderer.enabled = false;
        foreach (var waypoint in waypoints)
            Destroy(waypoint.gameObject);
        waypoints.Clear();
    }

    StateChanger lastState;
    public void CameraControl()
    {
        if (!state.CompareState(State.MovingCamera)) return;
        Vector2 input = playerInput.actions["MoveCamera"].ReadValue<Vector2>();
        cinemachine.m_XAxis.m_InputAxisValue = input.x;
        cinemachine.m_YAxis.m_InputAxisValue = input.y;
    }

    public void ActivateCamera(CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            meshRenderer.enabled = false;
            lastState = new(state.GetState());
            state.ChangeState(State.MovingCamera);
            return;
        }
        if (!callbackContext.canceled) return;
        cinemachine.m_XAxis.m_InputAxisValue = 0f;
        cinemachine.m_YAxis.m_InputAxisValue = 0f;
        state = lastState;
        meshRenderer.enabled = state.CompareState(State.SetPoint);
    }

    public void SetWaypoints()
    {
        if (sequence.IsActive()) return;
        if (state.IsExit()) return;
        if (!state.CompareState(State.SetPoint)) return;
        if (cursorHit.transform.gameObject.layer == 6) return;
        if(cursorHit.distance > maxDistance)
        {
            commandedFungi.NoTween();
            return;
        }

        GameObject cursor = Instantiate(cursorPrefab, transform);
        waypoints.Add(cursor.transform);
        cursor.transform.parent = cursorHit.transform;

        state.ChangeState(State.Default);
        commandedFungi.FollowPath(waypoints);
        meshRenderer.enabled = false;
        return;
    }

    void CursorUpdate()
    {
        if (sequence.IsActive()) return;
        Ray ray = Camera.main.ScreenPointToRay(playerInput.actions["Cursor"].ReadValue<Vector2>());
        if (!Physics.Raycast(ray, out cursorHit, Mathf.Infinity, layerMask)) return;
        if (cursorHit.transform.gameObject.layer == 6)
        {
            meshRenderer.enabled = false;
            return;
        }
        if (cursorHit.distance > maxDistance)
            meshRenderer.material.color = Color.black;
        else meshRenderer.material.color = Color.white;

        transform.forward = cursorHit.normal;
        transform.position = cursorHit.point;

        if (!state.CompareState(State.SetPoint))
        {
            meshRenderer.enabled = false;
            if (!state.CompareState(State.MovingCamera)) waypoints.Clear();
            return;
        }

        meshRenderer.enabled = true;
    }

    private void OnDrawGizmos()
    {
        float maxDistance;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, this.maxDistance, layerMask))
            maxDistance = this.maxDistance;
        else maxDistance = hit.distance;

        Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);
    }
}

public class StateChanger
{
    private int state;
    private int nextState;
    public enum Phase { Enter, Update, Exit }

    private Phase phase;
    public StateChanger(object state)
    {
        phase = Phase.Enter;
        this.state = (int)state;
    }

    public virtual void Enter() { phase = Phase.Update; }
    public virtual void Update() { phase = Phase.Update; }
    public virtual void Exit() { phase = Phase.Exit; }

    public StateChanger UpdateState()
    {
        if (phase == Phase.Enter) Enter();
        if (phase == Phase.Update) Update();
        if (phase == Phase.Exit)
        {
            Exit();
            return new(nextState);
        }
        return this;
    }

    public int GetState()
    {
        return state;
    }

    public Phase GetPhase() { return phase; }

    public bool CompareState(object state)
    {
        return this.state == (int)state;
    }

    public virtual void ChangeState(object newState)
    {
        nextState = (int)newState;
        phase = Phase.Exit;
    }

    public virtual void ChangeStateImmediate(object newState)
    {
        state = (int)newState;
        phase = Phase.Enter;
    }

    public bool IsEnter() { return phase == Phase.Enter; }
    public bool IsUpdate() { return phase == Phase.Update; }
    public bool IsExit() { return phase == Phase.Exit; }
}
