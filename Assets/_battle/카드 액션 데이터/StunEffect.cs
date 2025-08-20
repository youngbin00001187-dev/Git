using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class StunEffect : GameEffect
{
    [Header("스턴 설정")]
    [Tooltip("스턴이 지속될 턴 수입니다. (현재는 1턴만 적용)")]
    public int stunDuration = 1;

    public override IEnumerator Apply(UnitController user, GameObject targetTile, Action onComplete)
    {
        // 1. 목표 타일 위에 있는 유닛을 찾습니다.
        Tile targetTileComponent = targetTile.GetComponent<Tile>();
        if (targetTileComponent == null)
        {
            Debug.LogWarning("[StunEffect] 대상 타일에 Tile 컴포넌트가 없습니다.");
            onComplete?.Invoke();
            yield break;
        }

        UnitController victim = GridManager.Instance.GetUnitAtPosition(targetTileComponent.gridPosition);

        // 2. 타겟이 없거나, 자기 자신일 경우 효과를 발동하지 않습니다.
        if (victim == null || victim == user)
        {
            onComplete?.Invoke();
            yield break;
        }

        // 3. 대상 유닛의 상태를 Stun으로 변경합니다.
        victim.SetState(UnitState.Stun);
        Debug.Log($"<color=yellow>EFFECT: {victim.name} is now STUNNED for {stunDuration} turn(s).</color>");

        // (선택) 여기에 스턴 시각 효과(VFX)를 재생하는 코드를 추가할 수 있습니다.
        // yield return new WaitForSeconds(0.2f); // 예: 시각 효과 재생 시간만큼 대기

        // 4. 효과 적용이 끝났음을 알립니다.
        onComplete?.Invoke();
    }
}