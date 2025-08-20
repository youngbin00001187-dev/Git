using System.Collections;
using UnityEngine;
using System; // 'Action' (콜백)을 사용하기 위해 필요합니다.

/// <summary>
/// GameEffect가 액션의 어느 시점에 발동될지를 정의합니다.
/// </summary>
public enum EffectTiming
{
    OnActionStart,
    OnTargetImpact,
    AfterMove,        // [신규] 이동 애니메이션 완료 후
    OnArrival,
    OnActionEnd
}

/// <summary>
/// 게임에 존재하는 모든 GameEffect 클래스의 종류를 정의합니다.
/// </summary>
public enum EffectType
{
    None,
    Knockback,
    Stun,
    // --- 향후 추가될 효과 예시 ---
    // LeavePoisonMist, 
    // GainArmorBuff,
}

/// <summary>
/// GameAction에 부착될 효과의 모든 데이터를 정의합니다.
/// </summary>
[System.Serializable]
public struct ActionEffect
{
    public EffectType effectType;
    public EffectTiming timing;
    [Tooltip("체크: 이 효과의 애니메이션이 끝날 때까지 액션이 기다립니다. (예: 넉백)\n해제: 효과를 발동만 시키고 즉시 다음 행동으로 넘어갑니다. (예: 독안개)")]
    public bool waitForCompletion;

    // ▼▼▼ [핵심 수정] C# 구버전 호환성을 위한 생성자 추가 ▼▼▼
    // 이 생성자 덕분에, 인스펙터에서 새로 추가된 ActionEffect는
    // waitForCompletion 값이 자동으로 true(체크된 상태)가 됩니다.
    public ActionEffect(bool wait = true) // 기본값을 true로 설정
    {
        this.effectType = EffectType.None;
        this.timing = EffectTiming.OnTargetImpact;
        this.waitForCompletion = wait;
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
}

/// <summary>
/// 모든 게임 내 부가 효과가 상속받는 추상 클래스입니다.
/// </summary>
[System.Serializable]
public abstract class GameEffect
{
    /// <summary>
    /// [핵심 수정] 이제 'onComplete' 라는 '내선 전화'를 받을 수 있도록 수정합니다.
    /// </summary>
    public abstract IEnumerator Apply(UnitController user, GameObject targetTile, Action onComplete);
}