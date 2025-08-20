using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// EnemyGameAction.cs
public abstract class EnemyGameAction : ScriptableObject // ScriptableObject를 상속
{
    [Header("기본 액션 설정")]
    public int motionID = 0; // 적 애니메이션 ID
    public string animationStateName = ""; // 적 애니메이션 상태 이름

    // 실행 시점에 주입될 정보 (액션마다 다를 수 있으므로 Prepare에서 처리)
    protected UnitController actionUser;
    protected GameObject actionTargetTile;

    // GameAction의 Prepare 메서드 시그니처를 UnitController와 GameObject를 받도록 변경
    public virtual void Prepare(UnitController user, GameObject target)
    {
        this.actionUser = user;
        this.actionTargetTile = target;
    }

    // Execute 메서드 시그니처를 인자를 받지 않도록 변경
    // Prepare에서 받은 actionUser와 actionTargetTile을 사용합니다.
    public abstract IEnumerator Execute();

    // ⭐ 새로 추가: 특정 행동 시각화를 위한 함수들 ⭐
    // 이 함수들은 TileManager를 직접 호출하여 의도를 표시/제거합니다.
    public abstract List<GameObject> GetTargetableTiles(UnitController user); // 추가
    public abstract void ShowIntentVisual(UnitController user, GameObject targetTile);
    public abstract void ClearIntentVisual();
}