using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public interface IServable
{
    void Serve(bool hasServedFood);
}

public class Character : MonoBehaviour, IServable
{
    private NavMeshAgent agent;
    private Chair _currentChair;
    private CharacterManager _characterManager;
    private Vector3 exitPosition;

    public Chair CurrentChair => _currentChair;

    private bool hasServedFood = false;
    [SerializeField] private float foodServingWaitTime = 5f;
    [SerializeField] private float timeToEat = 2f;

    public CharacterState State { get; private set; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }


    public void Initialize(Chair chair, Vector3 exit, CharacterManager characterManager)
    {
        this._characterManager = characterManager;
        _currentChair = chair;
        exitPosition = exit;
        State = CharacterState.MovingToChair;
    }

    public void StartJourney()
    {
        Debug.Log("Character has entered the restaurant");
        StartCoroutine(RunStateMachine());
    }

    public void Serve(bool hasServedFood)
    {
        this.hasServedFood = hasServedFood;
    }

    private IEnumerator RunStateMachine()
    {
        while (State != CharacterState.Exited)
        {
            switch (State)
            {
                case CharacterState.MovingToChair:
                    if (_currentChair == null)
                    {
                        Debug.LogError("Character is moving to a chair but the chair is null");
                        yield break;
                    }
                    _currentChair.ChangeStatus(Chair.SlotStatus.Occupied);
                    yield return StartCoroutine(MoveToPosition(_currentChair.transform.position));
                    State = CharacterState.WaitforFood;
                    break;
                case CharacterState.WaitforFood:
                    yield return StartCoroutine(WaitForFood());
                    break;
                case CharacterState.Eating:
                    yield return new WaitForSeconds(foodServingWaitTime);
                    State = CharacterState.MovingToExit;
                    break;
                // ----------------------------------------------------------------
                case CharacterState.MovingToExit:
                    _currentChair.ChangeStatus(Chair.SlotStatus.Empty);
                    yield return StartCoroutine(MoveToExit());
                    State = CharacterState.Exited;
                    break;
            }
        }

        _characterManager.RemoveCharacter(this);
    }


    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        agent.SetDestination(targetPosition);
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }
        agent.isStopped = true;
    }

    private IEnumerator WaitForFood()
    {
        float waitTime = 0f;
        while (!hasServedFood && waitTime < foodServingWaitTime)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        if (hasServedFood)
        {
            State = CharacterState.Eating;
        }
        else
        {
            State = CharacterState.MovingToExit;
        }
    }

    private IEnumerator JumpToChair()
    {
        transform.position = _currentChair.transform.position;
        yield return new WaitForSeconds(0.5f); // Simulating jump animation time
    }

    private IEnumerator MoveToExit()
    {
        agent.enabled = false;
        Vector3 safePosition = FindSafePosition();
        while (Vector3.Distance(transform.position, safePosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, safePosition, agent.speed * Time.deltaTime);
            yield return null;
        }
        agent.enabled = true;
        yield return StartCoroutine(MoveToPosition(exitPosition));
    }

    private Vector3 FindSafePosition()
    {
        Vector3 safePosition = transform.position;
        NavMeshHit hit;
        float searchRadius = 5f;

        for (int i = 0; i < 8; i++)
        {
            Vector3 direction = Quaternion.Euler(0, i * 45, 0) * Vector3.forward;
            if (NavMesh.SamplePosition(transform.position + direction * searchRadius, out hit, searchRadius, NavMesh.AllAreas))
            {
                safePosition = hit.position;
                break;
            }
        }

        return safePosition;
    }

    public void ResetForPooling()
    {
        // agent.ResetPath();
        State = CharacterState.MovingToChair;
        _currentChair = null;
        StopAllCoroutines();
    }
}

public enum CharacterState
{
    MovingToChair,
    JumpingToChair,
    WaitforFood,
    Eating,
    MovingToExit,
    Exited
}