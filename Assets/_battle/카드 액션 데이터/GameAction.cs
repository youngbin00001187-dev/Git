using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// --- VFX를 위한 Enum과 구조체(설계도)를 여기에 정의합니다 ---

/// <summary>
/// VFX 생성 방식을 정의합니다.
/// </summary>
public enum E_VfxSpawnType
{
    PerTarget,  // 적중한 대상마다 이펙트 생성
    AtCaster    // 사용자(시전자) 위치에 이펙트 1회 생성
}

/// <summary>
/// VFX의 발동 타이밍과 방식을 정의하는 구조체입니다.
/// </summary>
[System.Serializable]
public struct VFXEffect
{
    [Tooltip("VFXManager에 등록된 이펙트의 ID")]
    public int vfxId;
    [Tooltip("이 VFX가 발동될 타이밍")]
    public EffectTiming timing;
    [Tooltip("VFX 생성 방식 (타겟마다/시전자에게)")]
    public E_VfxSpawnType spawnType;
}


[System.Serializable]
public abstract class GameAction
{
    protected UnitController actionUser;
    protected GameObject actionTargetTile;

    // ▼▼▼ 두 개의 독립적인 리스트를 사용합니다 ▼▼▼
    [Header("부가 효과 (게임 로직)")]
    [Tooltip("이 액션에 부착될 게임 효과 목록입니다. (예: 넉백, 기절)")]
    public List<ActionEffect> attachedEffects = new List<ActionEffect>();

    [Header("시각 효과 (VFX)")]
    [Tooltip("이 액션이 실행될 때, 지정된 타이밍에 발동할 VFX 목록입니다.")]
    public List<VFXEffect> vfxEffects = new List<VFXEffect>();
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    /// <summary>
    /// [공통 함수] 특정 타이밍의 게임 효과(attachedEffects)를 실행합니다.
    /// </summary>
    protected IEnumerator ExecuteEffectsByTiming(EffectTiming timing)
    {
        if (attachedEffects == null) yield break;
        foreach (var effect in attachedEffects)
        {
            if (effect.timing == timing)
            {
                yield return ExecuteEffectAndWait(effect);
            }
        }
    }

    /// <summary>
    /// [공통 함수] 단일 게임 효과를 실행하고 필요 시 완료될 때까지 기다립니다.
    /// </summary>
    protected IEnumerator ExecuteEffectAndWait(ActionEffect effect)
    {
        if (effect.effectType == EffectType.None)
        {
            yield break;
        }

        if (!effect.waitForCompletion)
        {
            actionUser.StartCoroutine(
                EffectManager.Instance.ExecuteEffect(effect.effectType, actionUser, actionTargetTile, null)
            );
            yield break;
        }

        bool isEffectFinished = false;
        Action callback = () => { isEffectFinished = true; };
        actionUser.StartCoroutine(
            EffectManager.Instance.ExecuteEffect(effect.effectType, actionUser, actionTargetTile, callback)
        );
        yield return new WaitUntil(() => isEffectFinished);
    }

    /// <summary>
    /// [신규 공통 함수] 특정 타이밍의 시각 효과(vfxEffects)를 실행합니다.
    /// </summary>
    protected void ExecuteVFXByTiming(EffectTiming timing, UnitController primaryTarget = null)
    {
        if (vfxEffects == null || vfxEffects.Count == 0 || VFXManager.Instance == null) return;

        foreach (var vfx in vfxEffects)
        {
            if (vfx.timing == timing)
            {
                Vector3 spawnPos = Vector3.zero;
                if (vfx.spawnType == E_VfxSpawnType.AtCaster)
                {
                    spawnPos = actionUser.transform.position;
                }
                else if (vfx.spawnType == E_VfxSpawnType.PerTarget)
                {
                    if (primaryTarget != null)
                        spawnPos = primaryTarget.transform.position;
                    else if (actionTargetTile != null)
                        spawnPos = actionTargetTile.transform.position;
                }

                VFXManager.Instance.PlayHitEffect(spawnPos, vfx.vfxId);
            }
        }
    }

    public virtual void Prepare(UnitController user, GameObject target)
    {
        this.actionUser = user;
        this.actionTargetTile = target;
    }

    public IEnumerator Execute()
    {
        if (actionUser.currentState == UnitState.Stun)
        {
            actionUser.SetState(UnitState.Normal);
            yield break;
        }
        yield return InternalExecute();
    }

    protected abstract IEnumerator InternalExecute();
    public abstract List<GameObject> GetTargetableTiles(UnitController user);
    public abstract List<GameObject> GetActionImpactTiles(UnitController user, GameObject targetTile);
}