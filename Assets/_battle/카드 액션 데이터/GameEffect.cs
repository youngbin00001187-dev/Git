using System.Collections;
using UnityEngine;
using System; // 'Action' (�ݹ�)�� ����ϱ� ���� �ʿ��մϴ�.

/// <summary>
/// GameEffect�� �׼��� ��� ������ �ߵ������� �����մϴ�.
/// </summary>
public enum EffectTiming
{
    OnActionStart,
    OnTargetImpact,
    AfterMove,        // [�ű�] �̵� �ִϸ��̼� �Ϸ� ��
    OnArrival,
    OnActionEnd
}

/// <summary>
/// ���ӿ� �����ϴ� ��� GameEffect Ŭ������ ������ �����մϴ�.
/// </summary>
public enum EffectType
{
    None,
    Knockback,
    Stun,// ���� ���⿡ ���ο� ����Ʈ Ÿ�� �߰� ����
    // --- ���� �߰��� ȿ�� ���� ---
    // LeavePoisonMist, 
    // GainArmorBuff,
}

/// <summary>
/// GameAction�� ������ ȿ���� ��� �����͸� �����մϴ�.
/// </summary>
[System.Serializable]
public struct ActionEffect
{
    public EffectType effectType;
    public EffectTiming timing;
    [Tooltip("üũ: �� ȿ���� �ִϸ��̼��� ���� ������ �׼��� ��ٸ��ϴ�. (��: �˹�)\n����: ȿ���� �ߵ��� ��Ű�� ��� ���� �ൿ���� �Ѿ�ϴ�. (��: ���Ȱ�)")]
    public bool waitForCompletion;

    public ActionEffect(bool wait = true)
    {
        this.effectType = EffectType.None;
        this.timing = EffectTiming.OnTargetImpact;
        this.waitForCompletion = wait;
    }
}

/// <summary>
/// ��� ���� �� �ΰ� ȿ���� ��ӹ޴� �߻� Ŭ�����Դϴ�.
/// </summary>
[System.Serializable]
public abstract class GameEffect
{
    public abstract IEnumerator Apply(UnitController user, GameObject targetTile, Action onComplete);
}