using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Character : MonoBehaviour
{

    /// <summary>
    ///  Reference 
    /// </summary>
    private CharacterManager characterManager;
    private NavMeshAgent agent;

    /// <summary>
    /// Slot reference
    /// </summary>
    private Chair slot;
    private Vector3 jumpStartPosition; // position before jump into chair
    private Vector3 exitPosition; // position to exit

    /// <summary>
    /// Settings 
    /// Settings
    private float foodServingWaitTime = 5f; // time to eat in chair
    private float timeToEat = 5f;

    /// <summary>
    /// Character status
    /// </summary>
    private CharacterStatus preCharacterStatus = CharacterStatus.MovingToSlot;
    private CharacterStatus characterStatus = CharacterStatus.MovingToSlot;

    private void Awake()
    {
        InitComponent();
    }

    private void InitComponent()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public void SetInfo(Chair slot, CharacterManager characterManager, Vector3 exitPosition)
    {
        this.slot = slot;
        this.characterManager = characterManager;
        this.exitPosition = exitPosition;

    }

    public void Go()
    {
        Debug.Log("Bảnh đã đi vào nhà hàng");
        StartCoroutine(StartCircleLoop());
    }

    public IEnumerator StartCircleLoop()
    {
        if (slot == null)
        {
            Debug.LogError("<color=red> Charactor Script - Slot is null");
            yield break;
        }

        // Check init
        if (agent == null)
        {
            Debug.LogError("<color=red> Charactor Script - Agent is null waiting for init");
            yield return null;
        }

        characterStatus = CharacterStatus.MovingToSlot;
        Debug.Log("Di chuyển đến ghế: " + slot.transform.position);
        yield return StartCoroutine(MoveToPos(slot.Trans.position));

        // Jump to chair and eat
        Debug.Log("Nhảy lên ghế");
        characterStatus = CharacterStatus.InChair;
        yield return StartCoroutine(JumpToChair());
        yield return new WaitForSeconds(foodServingWaitTime);

        // Move to exit
        Debug.Log("Di chuyển đến lối ra");
        characterStatus = CharacterStatus.MovingToExit;
        slot.ChangeStatus(Chair.SlotStatus.Empty);
        yield return StartCoroutine(MoveToExit());

        // Destroy character
        if (characterManager == null)
        {
            Debug.LogError("<color=red> Charactor Script - CharacterManager is null");
            yield break;
        }
        // Check is characterManager contain this character
        if (characterManager.ListCurrentCharacter.Contains(this))
            characterManager.RemoveCharacter(this);
        else
            Debug.LogError("<color=red> Charactor Script - CharacterManager not contain this character");
        Destroy(gameObject);
    }

    private IEnumerator MoveToPos(Vector3 pos)
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogError("NavMeshAgent không ở trên NavMesh.");
            yield break;
        }

        agent.SetDestination(pos);

        // Đợi cho đến khi agent bắt đầu di chuyển
        yield return new WaitUntil(() => agent.hasPath);

        while (agent.remainingDistance > agent.stoppingDistance)
        {
            // Kiểm tra xem agent có đang di chuyển không
            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                // Agent đang di chuyển, tiếp tục đợi
                yield return null;
            }
            else
            {
                // Agent không di chuyển, có thể đã bị kẹt
                Debug.LogWarning("Agent có vẻ bị kẹt. Đang thử điều chỉnh vị trí.");
                agent.SetDestination(pos); // Thử đặt lại đích đến
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Đảm bảo agent đã dừng hoàn toàn
        agent.isStopped = true;
        Debug.Log("Đã đến vị trí: " + pos);
    }

    public IEnumerator JumpToChair()
    {
        // Thêm logic nhảy lên ghế ở đây
        transform.position = slot.Trans.position;
        yield return new WaitForSeconds(0.5f); // Thời gian để hoàn thành animation nhảy
    }

    private IEnumerator MoveToExit()
    {
        // Tắt NavMeshAgent
        agent.enabled = false;

        // Di chuyển ra khỏi vùng không đi được
        Vector3 safePosition = FindSafePosition();
        while (Vector3.Distance(transform.position, safePosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, safePosition, agent.speed * Time.deltaTime);
            yield return null;
        }

        // Bật lại NavMeshAgent
        agent.enabled = true;

        // Di chuyển đến lối ra
        yield return StartCoroutine(MoveToPos(exitPosition));
        Debug.Log("Chim cút khỏi nhà hàng");
    }

    private Vector3 FindSafePosition()
    {
        Vector3 safePosition = transform.position;
        NavMeshHit hit;
        float searchRadius = 5f;

        for (int i = 0; i < 8; i++) // Tìm kiếm 8 hướng
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
}

public enum CharacterStatus
{
    MovingToSlot,
    InChair,
    MovingToExit,
}