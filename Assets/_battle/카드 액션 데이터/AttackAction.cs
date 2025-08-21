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

    // attachedEffects 리스트는 GameAction에 이미 있으므로 여기서 선언할 필요가 없습니다.
    // public List<ActionEffect> attachedEffects = new List<ActionEffect>();

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

    protected override IEnumerator InternalExecute()
    {
        Debug.Log($"<color=red>ACTION: {actionUser.name} starts attacking {actionTargetTile.name}.</color>");

        // --- 타임라인 1: 액션 시작 즉시 (OnActionStart) ---
        ExecuteVFXByTiming(EffectTiming.OnActionStart);
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
            // 실제 데미지 처리 직전에 'OnTargetImpact' VFX를 재생합니다.
            ExecuteVFXByTiming(EffectTiming.OnTargetImpact);
            TileManager.Instance.ApplyDamageToArea(this.actionTargetTile, this.areaOfEffect, this.damage, this.actionUser);
        }

        // --- 타임라인 2: 대상 적중 시 (OnTargetImpact) ---
        yield return ExecuteEffectsByTiming(EffectTiming.OnTargetImpact);

        if (animator != null) { animator.SetInteger("motionID", 0); }

        // --- 타임라인 3: 액션 종료 후 (OnActionEnd) ---
        ExecuteVFXByTiming(EffectTiming.OnActionEnd);
        yield return ExecuteEffectsByTiming(EffectTiming.OnActionEnd);

        Debug.Log($"<color=red>ACTION: {actionUser.name} attack completed.</color>");
    }
}