using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 카드 하나의 모든 데이터를 정의하는 ScriptableObject입니다.
/// 카드의 기본 정보, 능력치 요구량, 그리고 실제 행동(Action)과 부가 효과(Effect)의
/// 순차적인 조합(시퀀스)을 이곳에서 설정합니다.
/// </summary>
[CreateAssetMenu(fileName = "New Card", menuName = "무림에는 외상이 없다/Card Data", order = 1)]
public class CardDataSO : ScriptableObject
{
    [Header("카드 기본 정보")]
    public string cardID;
    public string cardName;
    public Sprite cardImage;
    [TextArea(3, 5)]
    public string description;

    [Header("의도 표시 설정")]
    [Tooltip("이 카드를 적이 사용하거나 플레이어가 호버할 때 표시될 예측 범위입니다.")]
    public List<Vector2Int> intentPredictionRange = new List<Vector2Int>();

    [Header("무공 정보 (연계 시스템용)")]
    [Tooltip("이 카드가 속한 무공의 종류를 나타내는 ID입니다. (예: 태극권=1, 소림권=2)")]
    public int martialArtID = 0;

    [Header("심법 요구 능력치")]
    [Tooltip("이 카드를 덱에 포함시키기 위해 필요한 최소 오행 능력치입니다.")]
    public FiveElementsStats requiredStats;

    [Header("카드 액션 시퀀스")]
    [Tooltip("이 카드를 사용했을 때 순서대로 발동할 '주요 행동(Action)' 목록입니다. (예: 이동, 공격)")]
    [SerializeReference] // 자식 클래스(MoveAction, AttackAction)를 인스펙터에 표시하기 위해 필수
    public List<GameAction> actionSequence = new List<GameAction>();

    // ▼▼▼ [추가] 카드에 부가 효과(GameEffect)를 직접 추가할 수 있는 리스트 ▼▼▼
    [Header("카드 부가 효과")]
    [Tooltip("이 카드의 '주요 행동'이 모두 끝난 후, 추가로 발동할 '부가 효과(Effect)' 목록입니다. (예: 넉백, 기절)")]
    [SerializeReference] // 자식 클래스(KnockbackEffect 등)를 인스펙터에 표시하기 위해 필수
    public List<GameEffect> appliedEffects = new List<GameEffect>();
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    // =================================================================================
    // 인스펙터 편의 기능 (우클릭 메뉴)
    // =================================================================================

    [ContextMenu("액션 시퀀스/Move Action 추가")]
    private void AddMoveAction()
    {
        actionSequence.Add(new MoveAction());
    }

    [ContextMenu("액션 시퀀스/Attack Action 추가")]
    private void AddAttackAction()
    {
        actionSequence.Add(new AttackAction());
    }

    // ▼▼▼ [추가] 넉백 이펙트를 쉽게 추가할 수 있는 우클릭 메뉴 ▼▼▼
    [ContextMenu("부가 효과/Knockback Effect 추가")]
    private void AddKnockbackEffect()
    {
        appliedEffects.Add(new KnockbackEffect());
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
}
