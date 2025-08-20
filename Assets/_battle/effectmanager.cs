using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��� GameEffect �� ���� �� ��ȣ�ۿ��� �����ϴ� �߾� ������(�̱���)�Դϴ�.
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
        Debug.Log($"[EffectManager] {_effectPrototypes.Count}���� ���� ����Ʈ�� �ʱ�ȭ�ϰ� ����߽��ϴ�.");
    }

    /// <summary>
    /// [���� ����] EffectType ID�� �޾� �ش��ϴ� ȿ���� �����ϰ�, �� ȿ���� ���� ������ '�����ϰ�' ��ٸ��ϴ�.
    /// </summary>
    public IEnumerator ExecuteEffect(EffectType effectType, UnitController user, GameObject targetTile, Action onComplete)
    {
        if (_effectPrototypes.TryGetValue(effectType, out GameEffect effectToApply))
        {
            // ���� [�ٽ� ����] StartCoroutine���� ������ �ʰ�, Apply�� ��ȯ�� IEnumerator�� ���� ��ȯ(yield return)�մϴ�. ����
            // �̰��� �ٷ� '�ǹ���(Effect)�� ���� ���� ������ ��ȯ��(Manager)�� �Բ� ��ٸ���' ������ ���� ü���Դϴ�.
            yield return effectToApply.Apply(user, targetTile, onComplete);
            // �������������������������������������
        }
        else
        {
            Debug.LogWarning($"[EffectManager] '{effectType}'�� �ش��ϴ� ȿ���� ��ϵǾ� ���� �ʽ��ϴ�!");
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// �� ������ ���ÿ� �����̴� ���� ���� ���Դϴ�.
    /// </summary>
    public IEnumerator ExecuteSimultaneousMoveCoroutine(UnitController unitA, UnitController unitB, Vector3 destA, Vector3 destB, float moveSpeed)
    {
        Debug.Log($"[EffectManager] Simultaneous Move ����: {unitA.name}, {unitB.name}");

        GridManager.Instance.UnregisterUnitPosition(unitA, unitA.GetGridPosition());
        GridManager.Instance.UnregisterUnitPosition(unitB, unitB.GetGridPosition());

        Sequence sequence = DOTween.Sequence();
        float duration = Vector3.Distance(unitA.transform.position, destA) / moveSpeed;
        sequence.Append(unitA.transform.DOMove(destA, duration).SetEase(Ease.OutQuad));
        sequence.Join(unitB.transform.DOMove(destB, duration).SetEase(Ease.OutQuad));

        yield return sequence.WaitForCompletion();

        // �̵��� ���� ��, �� ������ ���� ��ġ �����͸� ��Ȯ�ϰ� �����մϴ�.
        unitA.MoveToTile(GridManager.Instance.GetTileAtPosition(GridManager.Instance.GetGridPositionFromTileObject(unitA.gameObject)));
        unitB.MoveToTile(GridManager.Instance.GetTileAtPosition(GridManager.Instance.GetGridPositionFromTileObject(unitB.gameObject)));
        Debug.Log($"[EffectManager] Simultaneous Move �Ϸ� �� ������ ���� �Ϸ�.");
    }
}