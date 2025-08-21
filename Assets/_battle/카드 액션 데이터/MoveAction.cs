
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

        // --- 액션 시작 시점 ---
        ExecuteVFXByTiming(EffectTiming.OnActionStart);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionStart);

        Vector2Int targetPos = GridManager.Instance.GetGridPositionFromTileObject(actionTargetTile);
        UnitController unitOnTarget = GridManager.Instance.GetUnitAtPosition(targetPos);
        bool knockbackDone = true;

        if (unitOnTarget != null && unitOnTarget != actionUser)
        {
            // --- 충돌 시점 ---
            // ▼▼▼ 이 부분이 지적에 따라 수정되었습니다 ▼▼▼
            ExecuteVFXByTiming(EffectTiming.OnTargetImpact); // 더 이상 unitOnTarget을 넘기지 않습니다.
                                                             // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            bool hasImpactEffect = attachedEffects.Any(effect => effect.timing == EffectTiming.OnTargetImpact);

            if (hasImpactEffect)
            {
                knockbackDone = false;
                ExecuteEffectsByTiming_NoWait(EffectTiming.OnTargetImpact, () =>
                {
                    actionUser.StartCoroutine(
                        actionUser.RecoilCoroutine(actionTargetTile.transform.position)
                    );
                    knockbackDone = true;
                });
            }
            else
            {
                actionUser.StartCoroutine(actionUser.RecoilCoroutine(actionTargetTile.transform.position));
            }
        }

        // --- 핵심 이동 로직 ---
        UnitController unitOnTarget_AfterEffect = GridManager.Instance.GetUnitAtPosition(targetPos);
        if (unitOnTarget_AfterEffect == null && GridManager.Instance.IsTileWalkable(targetPos))
        {
            Animator animator = actionUser.GetComponent<Animator>();
            if (animator != null) animator.SetInteger("motionID", this.motionID);

            yield return actionUser.StartCoroutine(actionUser.MoveCoroutine(actionTargetTile, moveSpeed));

            if (animator != null) animator.SetInteger("motionID", 0);
        }

        yield return new WaitUntil(() => knockbackDone);

        // --- 이동 완료 시점 ---
        ExecuteVFXByTiming(EffectTiming.AfterMove);
        yield return ExecuteEffectsByTiming(EffectTiming.AfterMove);

        // --- 액션 종료 시점 ---
        ExecuteVFXByTiming(EffectTiming.OnActionEnd);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionEnd);

        Debug.Log($"<color=green>ACTION: {actionUser.name}의 이동/충돌 액션이 완료되었습니다.</color>");
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