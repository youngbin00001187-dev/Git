using System.Collections;
using System.Collections.Generic;
using System.Linq; // .Any()를 사용하기 위해 추가
using UnityEngine;

[System.Serializable]
public class MoveAction : GameAction
{
    [Header("Targeting Settings")]
    public List<Vector2Int> movementRange = new List<Vector2Int>();

    [Header("Settings")]
    public float moveSpeed = 8f;
    public int motionID = 1;

    [Header("Attached Effects")]
    public List<ActionEffect> attachedEffects = new List<ActionEffect>();

    public override List<GameObject> GetTargetableTiles(UnitController user)
    {
        List<GameObject> tiles = new List<GameObject>();
        Vector2Int userPos = user.GetGridPosition();
        foreach (var vector in movementRange)
        {
            GameObject tile = GridManager.Instance.GetTileAtPosition(userPos + vector);
            if (tile != null)
            {
                tiles.Add(tile);
            }
        }
        return tiles;
    }

    public override List<GameObject> GetActionImpactTiles(UnitController user, GameObject targetTile)
    {
        return new List<GameObject> { targetTile };
    }

    protected override IEnumerator InternalExecute()
    {
        Debug.Log($"<color=green>ACTION: {actionUser.name}이(가) {actionTargetTile.name}(으)로 이동/충돌을 시도합니다.</color>");

        yield return ExecuteEffectsByTiming(EffectTiming.OnActionStart);

        Vector2Int targetPos = GridManager.Instance.GetGridPositionFromTileObject(actionTargetTile);
        UnitController unitOnTarget = GridManager.Instance.GetUnitAtPosition(targetPos);
        bool knockbackDone = true;

        if (unitOnTarget != null && unitOnTarget != actionUser)
        {
            // ▼▼▼ [핵심 버그 수정] OnTargetImpact 효과 유무에 따라 분기 처리 ▼▼▼
            bool hasImpactEffect = attachedEffects.Any(effect => effect.timing == EffectTiming.OnTargetImpact);

            if (hasImpactEffect)
            {
                // [효과가 있을 경우] 기존 방식대로 비동기 처리 (밀어내는 건 정상 작동)
                knockbackDone = false;
                ExecuteEffectsByTiming_NoWait(EffectTiming.OnTargetImpact, () =>
                {
                    // 참고: 현재 구조에서는 넉백 후 리코일이 함께 실행됩니다.
                    actionUser.StartCoroutine(
                        actionUser.RecoilCoroutine(actionTargetTile.transform.position)
                    );
                    knockbackDone = true;
                });
            }
            else
            {
                // [효과가 없을 경우] 직접 리코일 실행하여 버그 해결
                actionUser.StartCoroutine(actionUser.RecoilCoroutine(actionTargetTile.transform.position));
            }
        }

        // --- 핵심 이동 로직 ---
        // 재확인 로직 추가: OnTargetImpact 효과로 인해 타겟이 이동했을 수 있음
        UnitController unitOnTarget_AfterEffect = GridManager.Instance.GetUnitAtPosition(targetPos);
        if (unitOnTarget_AfterEffect == null && GridManager.Instance.IsTileWalkable(targetPos))
        {
            Animator animator = actionUser.GetComponent<Animator>();
            if (animator != null) animator.SetInteger("motionID", this.motionID);

            yield return actionUser.StartCoroutine(actionUser.MoveCoroutine(actionTargetTile, moveSpeed));

            if (animator != null) animator.SetInteger("motionID", 0);
        }

        // --- 이동이 끝난 후 넉백 완료 대기 ---
        yield return new WaitUntil(() => knockbackDone);

        yield return ExecuteEffectsByTiming(EffectTiming.AfterMove);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionEnd);

        Debug.Log($"<color=green>ACTION: {actionUser.name}의 이동/충돌 액션이 완료되었습니다.</color>");
    }

    private IEnumerator ExecuteEffectsByTiming(EffectTiming timing)
    {
        if (attachedEffects != null)
        {
            foreach (var effect in attachedEffects)
            {
                if (effect.timing == timing)
                {
                    yield return ExecuteEffectAndWait(effect);
                }
            }
        }
    }

    private void ExecuteEffectsByTiming_NoWait(EffectTiming timing, System.Action onComplete)
    {
        if (attachedEffects != null)
        {
            foreach (var effect in attachedEffects)
            {
                if (effect.timing == timing)
                {
                    actionUser.StartCoroutine(
                        EffectManager.Instance.ExecuteEffect(effect.effectType, actionUser, actionTargetTile, onComplete)
                    );
                }
            }
        }
    }
}