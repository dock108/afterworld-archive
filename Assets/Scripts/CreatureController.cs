using System.Collections;
using Afterworld.Systems.MiniGames;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Drives simple creature behavior with proximity-based state changes.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class CreatureController : MonoBehaviour
{
    public enum CreatureState
    {
        HiddenWatching,
        Peeking,
        CuriousApproach,
        HesitantNearPlayer,
        Engaged
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform hidePoint;
    [SerializeField] private Transform peekPoint;
    [SerializeField] private ScanTarget scanTarget;
    [SerializeField] private ScannableObject scannable;
    [SerializeField] private ArchiveManager archiveManager;
    [SerializeField] private CreatureEncounter encounter;
    [SerializeField] private InstinctPredictionGame instinctPredictionGame;
    [SerializeField] private PatternMemoryMiniGame patternMemoryMiniGame;

    [Header("Distances")]
    [SerializeField] private float watchRadius = 8f;
    [SerializeField] private float approachRadius = 5f;
    [SerializeField] private float hesitantRadius = 2.2f;
    [SerializeField] private float retreatRadius = 3.2f;
    [SerializeField] private float returnBuffer = 1.5f;

    [Header("Idle Loop")]
    [SerializeField] private Vector2 idlePauseRange = new Vector2(1.2f, 2.4f);
    [SerializeField] private float lookAtSpeed = 4f;

    [Header("Encounter")]
    [SerializeField] private float calmDuration = 1.6f;

    private NavMeshAgent agent;
    private CreatureState currentState = CreatureState.HiddenWatching;
    private float idleTimer;
    private float idleDuration;
    private bool isIdling;
    private float calmTimer;
    private bool hasBeenScanned;
    private bool hasTriggeredEncounter;
    private bool isResolvingInstinct;
    private Coroutine instinctRoutine;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        idleDuration = Random.Range(idlePauseRange.x, idlePauseRange.y);
        if (scanTarget == null)
        {
            scanTarget = GetComponentInChildren<ScanTarget>(true);
        }

        if (scannable == null)
        {
            scannable = GetComponentInChildren<ScannableObject>(true);
        }

        if (encounter == null)
        {
            encounter = GetComponent<CreatureEncounter>();
        }

        if (instinctPredictionGame == null)
        {
            instinctPredictionGame = GetComponent<InstinctPredictionGame>();
        }

        if (patternMemoryMiniGame == null)
        {
            patternMemoryMiniGame = GetComponent<PatternMemoryMiniGame>();
        }

        if (instinctPredictionGame == null && patternMemoryMiniGame == null)
        {
            instinctPredictionGame = gameObject.AddComponent<InstinctPredictionGame>();
        }

        if (archiveManager == null)
        {
            archiveManager = FindObjectOfType<ArchiveManager>();
        }

        SetScanAvailable(false);
    }

    private void OnEnable()
    {
        if (scannable != null)
        {
            scannable.OnScanComplete.AddListener(HandleScanComplete);
        }
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject == null)
            {
                playerObject = GameObject.Find("Player");
            }

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    private void OnDisable()
    {
        if (scannable != null)
        {
            scannable.OnScanComplete.RemoveListener(HandleScanComplete);
        }
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        if (isResolvingInstinct)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case CreatureState.HiddenWatching:
                HandleHiddenWatching(distanceToPlayer);
                break;
            case CreatureState.Peeking:
                HandlePeeking(distanceToPlayer);
                break;
            case CreatureState.CuriousApproach:
                HandleCuriousApproach(distanceToPlayer);
                break;
            case CreatureState.HesitantNearPlayer:
                HandleHesitantNearPlayer(distanceToPlayer);
                break;
            case CreatureState.Engaged:
                HandleEngaged(distanceToPlayer);
                break;
        }
    }

    private void HandleHiddenWatching(float distanceToPlayer)
    {
        MoveToPoint(hidePoint);
        UpdateIdleLoop();
        FacePlayerWhenIdle();

        if (distanceToPlayer <= watchRadius)
        {
            TransitionTo(CreatureState.Peeking);
        }
    }

    private void HandlePeeking(float distanceToPlayer)
    {
        MoveToPoint(peekPoint != null ? peekPoint : hidePoint);
        UpdateIdleLoop();
        FacePlayerWhenIdle();

        if (distanceToPlayer <= approachRadius)
        {
            TransitionTo(CreatureState.CuriousApproach);
        }
        else if (distanceToPlayer > watchRadius + returnBuffer)
        {
            TransitionTo(CreatureState.HiddenWatching);
        }
    }

    private void HandleCuriousApproach(float distanceToPlayer)
    {
        isIdling = false;
        agent.isStopped = false;
        if (agent.isOnNavMesh)
        {
            agent.stoppingDistance = hesitantRadius;
            agent.SetDestination(player.position);
        }

        if (distanceToPlayer <= hesitantRadius)
        {
            TransitionTo(CreatureState.HesitantNearPlayer);
        }
        else if (distanceToPlayer > watchRadius + returnBuffer)
        {
            TransitionTo(CreatureState.Peeking);
        }
    }

    private void HandleHesitantNearPlayer(float distanceToPlayer)
    {
        UpdateIdleLoop();
        Vector3 awayDirection = (transform.position - player.position).normalized;
        if (awayDirection.sqrMagnitude < 0.01f)
        {
            awayDirection = -player.forward;
        }

        if (!isIdling && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.stoppingDistance = 0f;
            Vector3 retreatTarget = player.position + awayDirection * retreatRadius;
            agent.SetDestination(retreatTarget);
        }

        FacePlayerWhenIdle();

        if (distanceToPlayer > approachRadius + returnBuffer)
        {
            TransitionTo(CreatureState.CuriousApproach);
            return;
        }

        if (distanceToPlayer <= hesitantRadius && isIdling)
        {
            calmTimer += Time.deltaTime;
            if (calmTimer >= calmDuration)
            {
                TransitionTo(CreatureState.Engaged);
            }
        }
        else
        {
            calmTimer = 0f;
        }
    }

    private void HandleEngaged(float distanceToPlayer)
    {
        agent.isStopped = true;
        isIdling = true;
        FacePlayerWhenIdle();

        if (distanceToPlayer > approachRadius + returnBuffer)
        {
            TransitionTo(CreatureState.CuriousApproach);
        }
    }

    private void MoveToPoint(Transform target)
    {
        if (target == null)
        {
            agent.isStopped = true;
            return;
        }

        if (!agent.isOnNavMesh)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > agent.stoppingDistance + 0.1f)
        {
            agent.isStopped = false;
            agent.stoppingDistance = 0f;
            agent.SetDestination(target.position);
        }
        else
        {
            agent.isStopped = true;
        }
    }

    private void UpdateIdleLoop()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleDuration)
        {
            idleTimer = 0f;
            idleDuration = Random.Range(idlePauseRange.x, idlePauseRange.y);
            isIdling = !isIdling;
        }

        agent.isStopped = isIdling;
    }

    private void FacePlayerWhenIdle()
    {
        if (!isIdling)
        {
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookAtSpeed * Time.deltaTime);
    }

    private void TransitionTo(CreatureState nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        currentState = nextState;
        idleTimer = 0f;
        idleDuration = Random.Range(idlePauseRange.x, idlePauseRange.y);
        isIdling = false;
        calmTimer = 0f;
        if (!hasBeenScanned)
        {
            SetScanAvailable(nextState == CreatureState.Engaged);
        }

        if (nextState == CreatureState.Engaged && !hasTriggeredEncounter)
        {
            TriggerEncounterFlow();
        }
    }

    private void SetScanAvailable(bool available)
    {
        if (scanTarget != null)
        {
            scanTarget.enabled = available;
        }
    }

    private void HandleScanComplete()
    {
        hasBeenScanned = true;
        SetScanAvailable(true);
        encounter?.RegisterEncounter();
        if (archiveManager != null)
        {
            archiveManager.HandleScanCompleted();
        }
    }

    private void TriggerEncounterFlow()
    {
        hasTriggeredEncounter = true;
        encounter?.RegisterEncounter();
        if (encounter != null)
        {
            EncounterUI.ShowEncounter(encounter.Creature);
        }

        if (instinctPredictionGame != null)
        {
            if (patternMemoryMiniGame != null)
            {
                patternMemoryMiniGame.BeginMemoryChallenge(encounter != null ? encounter.Creature : null);
            }
            else
            {
                instinctPredictionGame.BeginPrediction(encounter != null ? encounter.Creature : null);
            }
        }
    }

    public void PlayInstinctBehavior(CreatureInstinct instinct, float duration)
    {
        if (instinctRoutine != null)
        {
            StopCoroutine(instinctRoutine);
        }

        instinctRoutine = StartCoroutine(InstinctBehaviorRoutine(instinct, duration));
    }

    public void DisengageCalmly()
    {
        if (instinctRoutine != null)
        {
            StopCoroutine(instinctRoutine);
            instinctRoutine = null;
        }

        isResolvingInstinct = false;
        TransitionTo(CreatureState.HiddenWatching);
        hasTriggeredEncounter = false;
        SetScanAvailable(false);
    }

    private IEnumerator InstinctBehaviorRoutine(CreatureInstinct instinct, float duration)
    {
        isResolvingInstinct = true;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

        switch (instinct)
        {
            case CreatureInstinct.Flee:
                MoveAwayFromPlayer();
                break;
            case CreatureInstinct.Freeze:
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }
                break;
            case CreatureInstinct.Climb:
                MoveToPoint(peekPoint != null ? peekPoint : hidePoint);
                break;
            case CreatureInstinct.Investigate:
                if (agent.isOnNavMesh && player != null)
                {
                    agent.stoppingDistance = hesitantRadius;
                    agent.SetDestination(player.position);
                }
                break;
        }

        if (duration > 0f)
        {
            yield return new WaitForSeconds(duration);
        }

        isResolvingInstinct = false;
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
        instinctRoutine = null;
    }

    private void MoveAwayFromPlayer()
    {
        if (player == null || !agent.isOnNavMesh)
        {
            return;
        }

        Vector3 awayDirection = (transform.position - player.position).normalized;
        if (awayDirection.sqrMagnitude < 0.01f)
        {
            awayDirection = -player.forward;
        }

        Vector3 retreatTarget = transform.position + awayDirection * retreatRadius;
        agent.stoppingDistance = 0f;
        agent.SetDestination(retreatTarget);
    }
}
