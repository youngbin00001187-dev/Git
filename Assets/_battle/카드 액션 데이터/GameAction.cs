
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; // Action (콜백)을 사용하기 위해 필요합니다.

[System.Serializable]
public abstract class GameAction
{
    protected UnitController actionUser;
    protected GameObject actionTargetTile;

    /// <summary>
    /// [공통 함수] 특정 효과를 실행하고, 'waitForCompletion' 설정에 따라 완료될 때까지 기다립니다.
    /// 이것이 바로 '팀장'이 '실무자'의 보고를 기다리는 핵심 기능입니다.
    /// </summary>
    /// <param name="effect">실행할 효과와 설정이 담긴 ActionEffect 구조체</param>
    protected IEnumerator ExecuteEffectAndWait(ActionEffect effect)
    {
        // 효과가 없으면 즉시 종료합니다.
        if (effect.effectType == EffectType.None)
        {
            yield break;
        }

        // '기다릴 필요가 없는' 효과 (예: 독안개)의 경우
        if (!effect.waitForCompletion)
        {
            // 실행만 하고 기다리지 않습니다. (내선 번호 없이 호출)
            actionUser.StartCoroutine(
                EffectManager.Instance.ExecuteEffect(effect.effectType, actionUser, actionTargetTile, null)
            );
            yield break; // 즉시 다음으로 넘어감
        }

        // '기다려야 하는' 효과 (예: 넉백)의 경우
        bool isEffectFinished = false; // '전화 수신' 램프
        Action callback = () => { isEffectFinished = true; }; // 내선 번호

        // 실무자에게 '내선 번호'를 알려주며 실행을 요청합니다.
        actionUser.StartCoroutine(
            EffectManager.Instance.ExecuteEffect(effect.effectType, actionUser, actionTargetTile, callback)
        );

        // '내선 전화'가 올 때까지 기다립니다.
        yield return new WaitUntil(() => isEffectFinished);
    }

    public virtual void Prepare(UnitController user, GameObject target)
    {
        this.actionUser = user;
        this.actionTargetTile = target;
    }

    /// <summary>
    /// [공개 실행 함수] 액션을 실행하기 위한 진입점입니다.
    /// 공통 선행 조건(스턴 체크 등)을 여기서 검사합니다.
    /// </summary>
    public IEnumerator Execute()
    {
        // 행동을 시작하기 전, 사용자의 상태를 가장 먼저 확인합니다.
        if (actionUser.currentState == UnitState.Stun)
        {
            Debug.Log($"<color=orange>ACTION CANCELLED: {actionUser.name} is stunned.</color>");

            // ▼▼▼ [최종 수정] 행동을 취소하는 대신, 상태를 정상으로 되돌립니다. ▼▼▼
            actionUser.SetState(UnitState.Normal);
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            // 스턴 상태일 경우, 아무 일도 하지 않고 코루틴을 즉시 종료합니다.
            yield break;
        }

        // 선행 조건을 통과했다면, 자식 클래스에 구현된 실제 행동 로직을 실행합니다.
        yield return InternalExecute();
    }

    /// <summary>
    /// [내부 실행 함수] 각 자식 액션이 반드시 구현해야 할 실제 행동 로직입니다.
    /// </summary>
    protected abstract IEnumerator InternalExecute();

    public abstract List<GameObject> GetTargetableTiles(UnitController user);

    public abstract List<GameObject> GetActionImpactTiles(UnitController user, GameObject targetTile);
}