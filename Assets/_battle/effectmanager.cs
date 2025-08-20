using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 GameEffect 및 유닛 간 상호작용을 관리하는 중앙 관리자(싱글턴)입니다.
/// </summary>
public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    private Dictionary<EffectType, GameEffect> _effectPrototypes;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeEffects();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeEffects()
    {
        _effectPrototypes = new Dictionary<EffectType, GameEffect>();
        _effectPrototypes.Add(EffectType.Knockback, new KnockbackEffect());
        _effectPrototypes.Add(EffectType.Stun, new StunEffect());
        Debug.Log($"[EffectManager] {_effectPrototypes.Count}개의 게임 이펙트를 초기화하고 등록했습니다.");
    }

    /// <summary>
    /// [최종 수정] EffectType ID를 받아 해당하는 효과를 실행하고, 그 효과가 끝날 때까지 '정직하게' 기다립니다.
    /// </summary>
    public IEnumerator ExecuteEffect(EffectType effectType, UnitController user, GameObject targetTile, Action onComplete)
    {
        if (_effectPrototypes.TryGetValue(effectType, out GameEffect effectToApply))
        {
            // ▼▼▼ [핵심 수정] StartCoroutine으로 감싸지 않고, Apply가 반환한 IEnumerator를 직접 반환(yield return)합니다. ▼▼▼
            // 이것이 바로 '실무자(Effect)의 일이 끝날 때까지 교환원(Manager)도 함께 기다리는' 정직한 보고 체계입니다.
            yield return effectToApply.Apply(user, targetTile, onComplete);
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
        }
        else
        {
            Debug.LogWarning($"[EffectManager] '{effectType}'에 해당하는 효과가 등록되어 있지 않습니다!");
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// 두 유닛이 동시에 움직이는 범용 연출 툴입니다.
    /// </summary>
    public IEnumerator ExecuteSimultaneousMoveCoroutine(UnitController unitA, UnitController unitB, Vector3 destA, Vector3 destB, float moveSpeed)
    {
        Debug.Log($"[EffectManager] Simultaneous Move 시작: {unitA.name}, {unitB.name}");

        GridManager.Instance.UnregisterUnitPosition(unitA, unitA.GetGridPosition());
        GridManager.Instance.UnregisterUnitPosition(unitB, unitB.GetGridPosition());

        Sequence sequence = DOTween.Sequence();
        float duration = Vector3.Distance(unitA.transform.position, destA) / moveSpeed;
        sequence.Append(unitA.transform.DOMove(destA, duration).SetEase(Ease.OutQuad));
        sequence.Join(unitB.transform.DOMove(destB, duration).SetEase(Ease.OutQuad));

        yield return sequence.WaitForCompletion();

        // 이동이 끝난 후, 각 유닛의 최종 위치 데이터를 정확하게 갱신합니다.
        unitA.MoveToTile(GridManager.Instance.GetTileAtPosition(GridManager.Instance.GetGridPositionFromTileObject(unitA.gameObject)));
        unitB.MoveToTile(GridManager.Instance.GetTileAtPosition(GridManager.Instance.GetGridPositionFromTileObject(unitB.gameObject)));
        Debug.Log($"[EffectManager] Simultaneous Move 완료 및 데이터 갱신 완료.");
    }
}