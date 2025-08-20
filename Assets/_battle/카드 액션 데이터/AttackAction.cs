using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // 'Action' (콜백)을 사용하기 위해 추가

// 공격 패턴의 종류를 정의하는 enum
public enum E_AttackPatternType
{
    Rotatable,
    FixedHorizontalSpread,
    FixedVerticalSpread
}

[System.Serializable]
public class AttackAction : GameAction
{
    [Header("타겟팅 설정")]
    public List<Vector2Int> attackRange = new List<Vector2Int>();

    [Header("설정값")]
    public int damage;
    public List<Vector2Int> areaOfEffect;
    public int motionID = 2;
    public string animationStateName = "Attack";
    private readonly float defaultWaitTime = 0.5f;

    [Header("공격 패턴 설정")]
    public E_AttackPatternType attackPatternType = E_AttackPatternType.Rotatable;

    [Header("Attached Effects")]
    [Tooltip("이 공격 액션에 부착될 효과와 발동 타이밍 목록입니다.")]
    public List<ActionEffect> attachedEffects = new List<ActionEffect>();

    public override List<GameObject> GetTargetableTiles(UnitController user)
    {
        List<GameObject> tiles = new List<GameObject>();
        Vector2Int userPos = user.GetGridPosition();
        foreach (var vector in attackRange)
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
        List<GameObject> tiles = new List<GameObject>();
        Vector2Int originPos = GridManager.Instance.GetGridPositionFromTileObject(targetTile);
        Vector2Int attackerPos = user.GetGridPosition();
        Vector2Int attackDirection = originPos - attackerPos;
        if (attackDirection == Vector2Int.zero) attackDirection = Vector2Int.up;

        foreach (var baseVector in areaOfEffect)
        {
            Vector2Int finalVector = baseVector;
            if (attackPatternType == E_AttackPatternType.Rotatable)
            {
                finalVector = TileManager.Instance.RotateVector(baseVector, attackDirection);
            }
            Vector2Int hitPos = originPos + finalVector;
            GameObject hitPosTile = GridManager.Instance.GetTileAtPosition(hitPos);
            if (hitPosTile != null) tiles.Add(hitPosTile);
        }
        return tiles;
    }

    // ▼▼▼ [핵심 수정] 함수의 이름과 접근 제한자를 변경했습니다. ▼▼▼
    protected override IEnumerator InternalExecute()
    {
        Debug.Log($"<color=red>ACTION: {actionUser.name} starts attacking {actionTargetTile.name}.</color>");

        // --- 타임라인 1: 액션 시작 즉시 (OnActionStart) ---
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionStart);

        Animator animator = actionUser.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("motionID", this.motionID);
            yield return new WaitForSeconds(defaultWaitTime);
        }

        // --- 핵심 데미지 처리 ---
        if (TileManager.Instance != null)
        {
            TileManager.Instance.ApplyDamageToArea(this.actionTargetTile, this.areaOfEffect, this.damage, this.actionUser);
        }

        // --- 타임라인 2: 대상 적중 시 (OnTargetImpact) ---
        yield return ExecuteEffectsByTiming(EffectTiming.OnTargetImpact);

        if (animator != null) { animator.SetInteger("motionID", 0); }

        // --- 타임라인 3: 액션 종료 후 (OnActionEnd) ---
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionEnd);

        Debug.Log($"<color=red>ACTION: {actionUser.name} attack completed.</color>");
    }

    /// <summary>
    /// [최종 수정] 특정 타이밍에 해당하는 모든 효과를, 부모의 '보고 대기' 함수를 통해 실행합니다.
    /// </summary>
    private IEnumerator ExecuteEffectsByTiming(EffectTiming timing)
    {
        if (attachedEffects != null)
        {
            foreach (var effect in attachedEffects)
            {
                if (effect.timing == timing)
                {
                    // 부모(GameAction)에게 물려받은 '공통 대기 함수'를 호출합니다.
                    yield return ExecuteEffectAndWait(effect);
                }
            }
        }
    }
}