using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class StunEffect : GameEffect
{
    [Header("���� ����")]
    [Tooltip("������ ���ӵ� �� ���Դϴ�. (����� 1�ϸ� ����)")]
    public int stunDuration = 1;

    public override IEnumerator Apply(UnitController user, GameObject targetTile, Action onComplete)
    {
        // 1. ��ǥ Ÿ�� ���� �ִ� ������ ã���ϴ�.
        Tile targetTileComponent = targetTile.GetComponent<Tile>();
        if (targetTileComponent == null)
        {
            Debug.LogWarning("[StunEffect] ��� Ÿ�Ͽ� Tile ������Ʈ�� �����ϴ�.");
            onComplete?.Invoke();
            yield break;
        }

        UnitController victim = GridManager.Instance.GetUnitAtPosition(targetTileComponent.gridPosition);

        // 2. Ÿ���� ���ų�, �ڱ� �ڽ��� ��� ȿ���� �ߵ����� �ʽ��ϴ�.
        if (victim == null || victim == user)
        {
            onComplete?.Invoke();
            yield break;
        }

        // 3. ��� ������ ���¸� Stun���� �����մϴ�.
        victim.SetState(UnitState.Stun);
        Debug.Log($"<color=yellow>EFFECT: {victim.name} is now STUNNED for {stunDuration} turn(s).</color>");

        // (����) ���⿡ ���� �ð� ȿ��(VFX)�� ����ϴ� �ڵ带 �߰��� �� �ֽ��ϴ�.
        // yield return new WaitForSeconds(0.2f); // ��: �ð� ȿ�� ��� �ð���ŭ ���

        // 4. ȿ�� ������ �������� �˸��ϴ�.
        onComplete?.Invoke();
    }
}