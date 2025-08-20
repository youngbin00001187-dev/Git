using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public enum BattlePhase { PlayerTurn_CardSelection, ActionPhase, CombatEnded }
    public BattlePhase currentPhase;

    [Header("관리 대상 연결")]
    public GridManager gridManager;
    public CardManager cardManager;
    public PlayerController player;
    public TileManager tileManager;
    public EnemySpawner enemySpawner;
    public CanvasGroup inputBlocker;
    public float cancelEffectDuration = 3.0f;

    public List<EnemyController> enemies;

    [Header("시작 위치 좌표 설정")]
    public Vector2Int playerStartPosition = new Vector2Int(4, 0);

    [Header("UI 요소")]
    public Button proceedButton;
    public Transform handDisplayTransform;

    [Header("캔슬 시스템")]
    // ▼▼▼ [수정 1] 에러가 발생하는 변수 초기화 라인을 삭제합니다. ▼▼▼
    private int currentCancelCount;
    private UnitController currentActingUnit;
    public static event System.Action<int> OnCancelCountChanged;

    private bool isBonusTimeActive = false;

    private Queue<GameAction> actionQueue = new Queue<GameAction>();
    private Queue<GameAction> interruptQueue = new Queue<GameAction>();
    private bool playerActionSubmitted = false;
    private bool isProcessingQueue = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
        if (CoreEventManager.instance != null) CoreEventManager.instance.OnCombatStartRequested += HandleCombatStartRequest;
        if (EventManager.Instance != null) EventManager.Instance.OnPlayerActionCompleted += HandlePlayerActionSubmitted;
    }

    void OnDestroy()
    {
        if (CoreEventManager.instance != null) CoreEventManager.instance.OnCombatStartRequested -= HandleCombatStartRequest;
        if (EventManager.Instance != null) EventManager.Instance.OnPlayerActionCompleted -= HandlePlayerActionSubmitted;
    }

    void Start()
    {
        SetInputBlocker(false);
    }

    private void HandleCombatStartRequest()
    {
        StartCoroutine(BattleStartSequence());
    }

    private IEnumerator BattleStartSequence()
    {
        if (cardManager != null) cardManager.SetGameManager(this);
        if (proceedButton != null)
        {
            proceedButton.gameObject.SetActive(false);
            proceedButton.onClick.AddListener(OnProceedButtonClicked);
        }
        if (gridManager != null) yield return StartCoroutine(gridManager.GenerateGridCoroutine());
        InitializeBattlefield();
        StartNewRound();
    }

    void InitializeBattlefield()
    {
        if (player != null)
        {
            GameObject tile = GridManager.Instance.GetTileAtPosition(playerStartPosition);
            if (tile != null) player.MoveToTile(tile);
        }
        if (enemySpawner != null) this.enemies = enemySpawner.SpawnEnemiesFromGlobalData();
    }

    private void StartNewRound()
    {
        SetCurrentPhase(BattlePhase.PlayerTurn_CardSelection);
        if (handDisplayTransform != null) handDisplayTransform.gameObject.SetActive(true);
        foreach (var enemy in enemies)
        {
            if (enemy != null) enemy.ResetRoundState();
        }

        // ▼▼▼ [수정 2] GlobalManager에서 값을 안전하게 가져옵니다. ▼▼▼
        currentCancelCount = GlobalManager.instance.playerCancelCountPerRound;

        OnCancelCountChanged?.Invoke(currentCancelCount);
        isBonusTimeActive = false;
        if (cardManager != null) cardManager.SetupNewRound();
        ShowAllEnemyIntents();
        CheckProceedButtonState();
    }

    private void OnProceedButtonClicked()
    {
        if (currentPhase == BattlePhase.PlayerTurn_CardSelection)
        {
            SetCurrentPhase(BattlePhase.ActionPhase);
            if (handDisplayTransform != null) handDisplayTransform.gameObject.SetActive(false);
            proceedButton.gameObject.SetActive(false);
            if (EventManager.Instance != null) EventManager.Instance.RaiseActionPhaseStarted();
            if (cardManager != null) cardManager.BeginActionPhase();
            StartCoroutine(ActionPhaseCoroutine());
        }
    }

    IEnumerator ActionPhaseCoroutine()
    {
        Debug.Log("<color=orange>==== [ActionPhaseCoroutine 시작] ====</color>");

        var turnOrder = new List<UnitController>();
        turnOrder.Add(player);
        turnOrder.AddRange(enemies.Where(e => e != null && e.gameObject.activeInHierarchy));
        int turnIndex = 0;

        while (cardManager.GetActionCardCount() > 0 || enemies.Any(e => e != null && e.HasMoreActionsThisRound()))
        {
            if (CheckForCombatEnd()) yield break;

            turnOrder.RemoveAll(u => u == null || !u.gameObject.activeInHierarchy);
            if (turnOrder.Count == 0) break;

            turnIndex %= turnOrder.Count;
            var currentUnit = turnOrder[turnIndex];
            currentActingUnit = currentUnit;

            Debug.Log($"<color=lime>[턴 시작]</color> 유닛: {currentUnit.name} ({(currentUnit is PlayerController ? "플레이어" : "적")})");

            if (currentUnit is PlayerController && cardManager.GetActionCardCount() > 0)
            {
                isBonusTimeActive = false;
                Debug.Log("<color=cyan>[플레이어 턴 시작]</color> → 보너스타임 OFF");
                SetInputBlocker(false);
                playerActionSubmitted = false;
                Debug.Log("<color=cyan>플레이어 행동 대기 중...</color>");
                yield return new WaitUntil(() => playerActionSubmitted);
                Debug.Log("<color=cyan>플레이어 행동 감지됨</color>");
                isBonusTimeActive = true;
                Debug.Log("<color=cyan>[보너스타임 진입]</color>");
            }
            else if (currentUnit is EnemyController enemy && enemy.HasMoreActionsThisRound())
            {
                Debug.Log($"<color=red>[적 행동 시작]</color>: {enemy.name}");
                yield return StartCoroutine(enemy.TakeActionCoroutine());
                Debug.Log($"<color=red>[적 행동 종료]</color>: {enemy.name}");
            }

            Debug.Log("<color=yellow>[행동 큐 처리 시작]</color>");
            yield return StartCoroutine(ProcessActionQueueCoroutine());
            Debug.Log("<color=yellow>[행동 큐 처리 완료]</color>");

            if (currentUnit is PlayerController)
            {
                if (cardManager != null) cardManager.SelectNextActionCard();
            }

            yield return new WaitForSeconds(0.3f);
            turnIndex++;
        }

        Debug.Log("<color=magenta>[ActionPhaseCoroutine 종료]</color>");
        currentActingUnit = null;
        SetInputBlocker(false);
        EndRound();
    }

    private bool CheckForCombatEnd()
    {
        if (!enemies.Any(e => e != null && e.currentHealth > 0)) { OnCombatVictory(); return true; }
        if (player.currentHealth <= 0) { OnCombatDefeat(); return true; }
        return false;
    }

    private void OnCombatVictory()
    {
        if (currentPhase == BattlePhase.CombatEnded) return;
        Debug.Log("<color=yellow>===== 전투 승리! =====</color>");
        SetCurrentPhase(BattlePhase.CombatEnded);
        SetInputBlocker(true);
        if (RewardManager.Instance != null) { RewardManager.Instance.ProcessRewards(); }
        if (BattleUIManager.instance != null) { BattleUIManager.instance.ShowRewardPanel(); }
    }

    private void OnCombatDefeat()
    {
        if (currentPhase == BattlePhase.CombatEnded) return;
        Debug.Log("<color=red>===== 전투 패배... =====</color>");
        SetCurrentPhase(BattlePhase.CombatEnded);
        SetInputBlocker(true);
        if (BattleUIManager.instance != null) { BattleUIManager.instance.ShowDefeatPanel(); }
    }

    // ▼▼▼ [수정 3] 액션 시퀀스 버그를 수정한 최종 버전입니다. ▼▼▼
    IEnumerator ProcessActionQueueCoroutine()
    {
        if (actionQueue.Count == 0 && interruptQueue.Count == 0)
        {
            isProcessingQueue = false; // 큐가 모두 비었으면 상태만 false로 바꾸고 종료
            yield break;
        }

        isProcessingQueue = true;

        if (isBonusTimeActive)
        {
            SetInputBlocker(currentCancelCount <= 0);
        }
        else
        {
            SetInputBlocker(true);
        }

        while (actionQueue.Count > 0 || interruptQueue.Count > 0)
        {
            if (interruptQueue.Count > 0)
            {
                Debug.Log("<color=purple>!!! 인터럽트 발생 !!!</color> 일반 행동을 잠시 멈추고 우선 행동을 처리합니다.");

                while (interruptQueue.Count > 0)
                {
                    GameAction interruptAction = interruptQueue.Dequeue();
                    Debug.Log($"<color=purple>인터럽트 액션 실행:</color> {interruptAction.GetType().Name}");
                    yield return StartCoroutine(interruptAction.Execute());
                    yield return new WaitForSeconds(0.2f);
                }

                Debug.Log("<color=purple>우선 행동 처리가 완료되었습니다.</color> 일반 행동을 재개합니다.");

                if (isBonusTimeActive)
                {
                    SetInputBlocker(currentCancelCount <= 0);
                }
            }
            else // 인터럽트 큐가 비어있을 때만 일반 큐를 처리
            {
                GameAction currentAction = actionQueue.Dequeue();
                Debug.Log($"<color=yellow>일반 액션 실행:</color> {currentAction.GetType().Name}");
                yield return StartCoroutine(currentAction.Execute());
                yield return new WaitForSeconds(0.2f);
            }
        }
        isProcessingQueue = false;
    }

    private void EndRound()
    {
        if (CheckForCombatEnd()) return;
        if (EventManager.Instance != null) EventManager.Instance.RaiseRoundEnded();
        StartCoroutine(StartNewRoundSequence());
    }

    IEnumerator StartNewRoundSequence()
    {
        yield return new WaitForSeconds(1.5f);
        StartNewRound();
    }

    public void AddActionsToQueue(IEnumerable<GameAction> actions)
    {
        foreach (var action in actions) { actionQueue.Enqueue(action); }
    }

    public void AddActionsToInterruptQueue(IEnumerable<GameAction> actions)
    {
        foreach (var action in actions)
        {
            interruptQueue.Enqueue(action);
        }
        Debug.Log($"<color=purple>INTERRUPT QUEUED:</color> {actions.Count()} 개의 우선 행동이 예약되었습니다.");
    }

    private void HandlePlayerActionSubmitted()
    {
        playerActionSubmitted = true;
    }

    void ShowAllEnemyIntents()
    {
        if (tileManager == null) return;
        TileManager.Instance.ClearHighlight(TileManager.HighlightType.EnemyIntent);
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                enemy.UpdateIntentDisplay();
            }
        }
    }

    public BattlePhase GetCurrentPhase() { return currentPhase; }
    public void SetCurrentPhase(BattlePhase newPhase) { currentPhase = newPhase; }

    public void CheckProceedButtonState()
    {
        if (proceedButton == null || cardManager == null || player == null || GlobalManager.instance == null) return;
        bool shouldBeActive = (currentPhase == BattlePhase.PlayerTurn_CardSelection && cardManager.GetActionCardCount() >= GlobalManager.instance.playerMaxActionsPerTurn);
        proceedButton.gameObject.SetActive(shouldBeActive);
    }

    private void SetInputBlocker(bool isBlocked)
    {
        if (inputBlocker == null) return;
        Debug.Log($"[GameManager] Input Blocker {(isBlocked ? "ON" : "OFF")}");
        inputBlocker.alpha = isBlocked ? 1 : 0;
        inputBlocker.blocksRaycasts = isBlocked;
    }

    public void CleanupCombat()
    {
        // ...
    }

    public bool TryPerformCancel()
    {
        if (currentCancelCount > 0)
        {
            currentCancelCount--;
            OnCancelCountChanged?.Invoke(currentCancelCount);
            Debug.Log($"<color=cyan>캔슬 성공! 남은 횟수: {currentCancelCount}</color>");
            StartCoroutine(CancelEffectCoroutine(0.3f, cancelEffectDuration));
            if (player != null)
            {
                SandevistanGhostSpawner sandevistan = player.GetComponent<SandevistanGhostSpawner>();
                if (sandevistan != null)
                {
                    sandevistan.TriggerGhostEffect();
                }
                else
                {
                    Debug.LogWarning("[GameManager] Player 오브젝트에서 SandevistanGhostSpawner 컴포넌트를 찾을 수 없습니다!");
                }
            }
            return true;
        }
        Debug.LogWarning("캔슬 횟수가 부족합니다!");
        return false;
    }

    private IEnumerator CancelEffectCoroutine(float slowMotionScale = 0.5f, float duration = 0.8f)
    {
        if (BattleCameraController.instance != null && player != null)
        {
            BattleCameraController.instance.ZoomInForCancel(player.transform, 4.0f, 0.1f);
        }
        Time.timeScale = slowMotionScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
        if (BattleCameraController.instance != null)
        {
            BattleCameraController.instance.ResetZoom(0.1f);
        }
    }

    public UnitController GetCurrentActingUnit()
    {
        return currentActingUnit;
    }

    public bool IsBonusTimeActive()
    {
        return isBonusTimeActive;
    }
}