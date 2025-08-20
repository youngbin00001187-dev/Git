using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyController : UnitController
{
    [Header("적 전용 데이터")]
    public EnemyDataSO enemyData;

    private int currentActionIndex = 0;
    private int actionsTakenThisRound = 0;

    private CardDataSO nextReferenceCard;
    private GameObject nextTargetTile;

    // 이 변수는 이제 '이전에 내가 뭘 그렸는지' 기억하는 용도로 사용됩니다.
    private List<GameObject> _currentIntentTiles = new List<GameObject>();

    /// <summary>
    /// [신규] 내부 계산용으로, 다음 행동과 목표 타일을 결정하고 예측 타일 목록을 반환합니다.
    /// </summary>
    private List<GameObject> CalculateIntentTiles()
    {
        if (enemyData == null || !HasMoreActionsThisRound()) return new List<GameObject>();

        EnemyAction enemyActionData = enemyData.actionPattern[currentActionIndex];
        nextReferenceCard = enemyActionData.referenceCard;
        if (nextReferenceCard == null || !nextReferenceCard.actionSequence.Any()) return new List<GameObject>();

        // --- '??' 연산자 오류 수정 ---
        GameAction targetingAction = nextReferenceCard.actionSequence.OfType<MoveAction>().FirstOrDefault();
        if (targetingAction == null)
        {
            targetingAction = nextReferenceCard.actionSequence.OfType<AttackAction>().FirstOrDefault();
        }
        // --- '??' 연산자 오류 수정 ---

        if (targetingAction == null) return new List<GameObject>();

        List<GameObject> targetableTiles = targetingAction.GetTargetableTiles(this);
        if (!targetableTiles.Any()) return new List<GameObject>();

        if (enemyActionData.movementType == E_MoveType.Fixed)
        {
            nextTargetTile = targetableTiles[Random.Range(0, targetableTiles.Count)];
        }
        else if (enemyActionData.movementType == E_MoveType.ChasePlayer)
        {
            nextTargetTile = GetChaseTargetTile(targetableTiles);
        }
        if (nextTargetTile == null) return new List<GameObject>();

        if (targetingAction is MoveAction)
        {
            Tile targetTileComponent = nextTargetTile.GetComponent<Tile>();
            if (targetTileComponent == null)
            {
                return new List<GameObject>();
            }
        }

        var previewVectors = nextReferenceCard.intentPredictionRange;
        if (!previewVectors.Any()) return new List<GameObject>();

        List<GameObject> finalIntentTiles = new List<GameObject>();
        Vector2Int originPos = nextTargetTile.GetComponent<Tile>().gridPosition;
        Vector2Int attackerPos = this.GetGridPosition();
        Vector2Int direction = originPos - attackerPos;
        if (direction == Vector2Int.zero) direction = Vector2Int.up;

        foreach (var vector in previewVectors)
        {
            Vector2Int rotatedVector = TileManager.Instance.RotateVector(vector, direction);
            GameObject tile = GridManager.Instance.GetTileAtPosition(originPos + rotatedVector);
            if (tile != null) finalIntentTiles.Add(tile);
        }
        return finalIntentTiles;
    }
    protected override void Die()
    {
        // 1. 보상 등록 먼저
        if (enemyData != null && RewardManager.Instance != null)
        {
            RewardManager.Instance.RegisterDefeatedEnemy(enemyData);
        }

        // 2. 기본 사망 처리
        base.Die();
    }

    /// <summary>
    /// 자신의 행동 예측을 계산하고 'IntentManager'를 통해 즉시 업데이트합니다.
    /// </summary>
    public void UpdateIntentDisplay()
    {
        // ▼▼▼ [핵심 수정] TileManager 대신 IntentManager를 사용합니다. ▼▼▼
        if (IntentManager.Instance == null) return;

        // 1. 이전에 내가 그렸던 하이라이트를 지워달라고 요청합니다.
        IntentManager.Instance.RemoveIntentHighlight(_currentIntentTiles);

        // 2. 이번에 새로 표시할 타일 목록을 계산합니다.
        List<GameObject> newIntentTiles = CalculateIntentTiles();

        // 3. 새로 계산된 타일을 그려달라고 요청합니다.
        IntentManager.Instance.AddIntentHighlight(newIntentTiles);

        // 4. 다음 업데이트를 위해, '새로운 타일'을 '이전 타일'로 기억합니다.
        _currentIntentTiles = newIntentTiles;
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    }

    /// <summary>
    /// 넉백 등 위치가 강제로 변경되었을 때,
    /// 자신의 다음 행동(타겟, 공격 범위 등)을 다시 계산하고 표시합니다.
    /// </summary>
    public void RecalculateIntent()
    {
        Debug.Log($"[EnemyController] {this.name}가 넉백되어 행동을 재계산합니다.");
        UpdateIntentDisplay();
    }

    private GameObject GetChaseTargetTile(List<GameObject> targetableTiles)
    {
        if (GameManager.Instance == null || GameManager.Instance.player == null) return null;

        Vector2Int playerPos = GameManager.Instance.player.GetGridPosition();
        GameObject bestTile = null;
        float minDistance = float.MaxValue;

        foreach (var tile in targetableTiles)
        {
            Tile tileComponent = tile.GetComponent<Tile>();
            if (tileComponent == null) continue;

            Vector2Int tilePos = tileComponent.gridPosition;
            float distance = Vector2Int.Distance(tilePos, playerPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                bestTile = tile;
            }
        }
        return bestTile;
    }

    public IEnumerator TakeActionCoroutine()
    {
        isActing = true;

        if (IntentManager.Instance != null)
        {
            IntentManager.Instance.RemoveIntentHighlight(_currentIntentTiles);
        }

        bool actionPreparedAndQueued = false;

        if (nextReferenceCard != null && nextTargetTile != null)
        {
            GameAction firstActionInSequence = nextReferenceCard.actionSequence.FirstOrDefault();
            bool isValidTarget = true;
            if (firstActionInSequence is MoveAction)
            {
                Tile targetTileComponent = nextTargetTile.GetComponent<Tile>();
                if (targetTileComponent == null || !GridManager.Instance.IsTileWalkable(targetTileComponent.gridPosition))
                {
                    isValidTarget = false;

                    // ▼▼▼ [신규] 이동 포기 시 리코일 실행 로직 ▼▼▼
                    StartCoroutine(RecoilCoroutine(nextTargetTile.transform.position));
                    Debug.Log($"<color=yellow>[EnemyController] {name}이(가) 이동 경로가 막혀 리코일합니다.</color>");
                }
            }

            if (isValidTarget)
            {
                foreach (var action in nextReferenceCard.actionSequence)
                {
                    action.Prepare(this, nextTargetTile);
                }
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddActionsToQueue(nextReferenceCard.actionSequence);
                }
                actionPreparedAndQueued = true;
            }
        }

        actionsTakenThisRound++;
        currentActionIndex = (currentActionIndex + 1) % enemyData.actionPattern.Count;
        isActing = false;

        if (actionPreparedAndQueued)
        {
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            // 이동이 막혀 리코일만 한 경우, 약간의 딜레이를 주어 연출을 보여줍니다.
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void ResetRoundState()
    {
        actionsTakenThisRound = 0;
    }

    public bool HasMoreActionsThisRound()
    {
        if (enemyData == null) return false;
        return actionsTakenThisRound < enemyData.actionsPerTurn;
    }
}