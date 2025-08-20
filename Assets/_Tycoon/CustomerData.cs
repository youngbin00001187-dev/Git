using UnityEngine;
using System.Collections.Generic;

// IngredientProperty는 별도의 파일 (IngredientProperty.cs)에 정의되어야 합니다.
// 이 파일에서는 IngredientProperty 클래스를 직접 정의하지 않습니다.

// 손님이 가진 속성 요구치를 정의하는 클래스
// IngredientProperty와 유사하지만, 여기서는 손님이 '원하는' 속성 수치
[System.Serializable]
public class CustomerPropertyRequirement
{
    public string propertyName; // 속성 이름 (예: "매콤함", "달달함")
    public int requiredValue;   // 요구 속성 수치 (이 값을 맞춰야 함)
    [Tooltip("이 속성이 요리 만족도에 미치는 중요도. 높을수록 중요.")]
    [Range(1, 10)] // 1부터 10 사이의 값으로 설정 가능
    public int importance = 5; // 중요도 추가
}

// 한 손님이 가질 수 있는 하나의 주문 패턴을 정의하는 클래스
[System.Serializable] // <<=== 이 부분이 [System.2025-07-23] Serializable]이 아닌지 꼭 확인하세요!
public class CustomerOrder
{
    [TextArea(3, 5)]
    public string orderPhrase; // 손님의 추상적인 주문 (예: "오늘은 맵고 달달한 게 당기는군.")
    public CustomerPropertyRequirement[] desiredProperties; // 이 주문이 내부적으로 요구하는 속성들
    [Tooltip("이 주문이 나타날 확률 (다른 주문들과의 상대적 비율)")]
    [Range(1, 100)] // 1부터 100 사이의 값으로 설정 가능
    public int weight = 10; // 해당 주문 패턴의 가중치 (확률)

    [Header("요리 결과 티어 기준 (충족된 속성 개수 기준)")]
    [Tooltip("요구 속성 중 몇 개 이상을 만족해야 '대만족'인가? (예: 3개 중 3개)")]
    public int greatSuccessThreshold; // 대만족을 위한 최소 충족 속성 개수
    [Tooltip("요구 속성 중 몇 개 이상을 만족해야 '만족'인가? (예: 3개 중 2개)")]
    public int successThreshold;      // 만족을 위한 최소 충족 속성 개수
    [Tooltip("요구 속성 중 몇 개 이상을 만족해야 '평범'인가? (예: 3개 중 1개)")]
    public int normalThreshold;       // 평범을 위한 최소 충족 속성 개수
    // 실패는 위 모든 조건을 만족하지 못할 경우

    [Header("티어별 보상")]
    public int greatSuccessGold;
    public int greatSuccessReputation;
    [Tooltip("대만족 시 지급할 아이템 ID (선택 사항, 없으면 지급 안 함)")]
    public string greatSuccessRewardItemId;

    public int successGold;
    public int successReputation;
    [Tooltip("만족 시 지급할 아이템 ID (선택 사항, 없으면 지급 안 함)")]
    public string successRewardItemId;

    public int normalGold;
    public int normalReputation;
    [Tooltip("평범 시 지급할 아이템 ID (선택 사항, 없으면 지급 안 함)")]
    public string normalRewardItemId;

    // 보상 데이터를 반환하는 도우미 클래스 (내부적으로만 사용)
    public class RewardData
    {
        public int gold;
        public int reputation;
        public string rewardItemId;
    }

    /// <summary>
    /// 특정 CookingTier에 해당하는 보상 데이터를 반환합니다.
    /// </summary>
    /// <param name="tier">요리 등급</param>
    /// <returns>해당 등급의 골드, 명성, 아이템 ID 보상</returns>
    public RewardData GetRewardForTier(CustomerData.CookingTier tier)
    {
        RewardData reward = new RewardData();
        switch (tier)
        {
            case CustomerData.CookingTier.GreatSuccess:
                reward.gold = greatSuccessGold;
                reward.reputation = greatSuccessReputation;
                reward.rewardItemId = greatSuccessRewardItemId;
                break;
            case CustomerData.CookingTier.Success:
                reward.gold = successGold;
                reward.reputation = successReputation;
                reward.rewardItemId = successRewardItemId;
                break;
            case CustomerData.CookingTier.Normal:
                reward.gold = normalGold;
                reward.reputation = normalReputation;
                reward.rewardItemId = normalRewardItemId;
                break;
            case CustomerData.CookingTier.Failure:
                reward.gold = 0;
                reward.reputation = 0;
                reward.rewardItemId = null;
                break;
            default:
                Debug.LogWarning($"알 수 없는 CookingTier: {tier}. 기본 보상 0으로 반환.");
                reward.gold = 0;
                reward.reputation = 0;
                reward.rewardItemId = null;
                break;
        }
        return reward;
    }

    // 티어별 손님 대사 필드
    [Header("티어별 손님 대사")]
    [TextArea(2, 3)]
    [Tooltip("대만족 시 손님이 할 대사입니다.")]
    public string greatSuccessDialogue;
    [TextArea(2, 3)]
    [Tooltip("만족 시 손님이 할 대사입니다.")]
    public string successDialogue;
    [TextArea(2, 3)]
    [Tooltip("평범 시 손님이 할 대사입니다.")]
    public string normalDialogue;
    [TextArea(2, 3)]
    [Tooltip("실패 시 손님이 할 대사입니다.")]
    public string failureDialogue;

    /// <summary>
    /// 특정 CookingTier에 해당하는 손님 대사를 반환합니다.
    /// </summary>
    /// <param name="tier">요리 등급</param>
    /// <returns>해당 등급의 손님 대사</returns>
    public string GetDialogueForTier(CustomerData.CookingTier tier)
    {
        switch (tier)
        {
            case CustomerData.CookingTier.GreatSuccess:
                return greatSuccessDialogue;
            case CustomerData.CookingTier.Success:
                return successDialogue;
            case CustomerData.CookingTier.Normal:
                return normalDialogue;
            case CustomerData.CookingTier.Failure:
                return failureDialogue;
            default:
                Debug.LogWarning($"알 수 없는 CookingTier: {tier}. 빈 문자열 반환.");
                return "";
        }
    }
}

// ScriptableObject를 만들기 위한 메뉴 항목을 추가합니다.
[CreateAssetMenu(fileName = "New Customer", menuName = "풍운객잔/Customer")]
public class CustomerData : ScriptableObject
{
    public enum CookingTier
    {
        Failure,      // 실패
        Normal,       // 평범
        Success,      // 만족
        GreatSuccess  // 대만족
    }

    [Header("기본 정보")]
    public string customerName;      // 손님 이름 (예: "공사장 손님", "당가의 연화")
    public Sprite characterPortrait; // 손님 캐릭터 초상화 (2D 일러스트)
    public GameObject characterPrefab; // 객잔에 등장할 손님 2D 모델 프리팹 (선택 사항)
    [TextArea(2, 4)]
    public string customerDescription; // 손님에 대한 간략한 설명 (퀘스트/특별 손님에게 유용)

    [Header("손님 타입")]
    public CustomerType type; // 일반, 퀘스트, 특별 손님 구분

    // 손님 타입을 구분하기 위한 열거형
    public enum CustomerType
    {
        Normal,    // 일반 손님
        Quest,     // 퀘스트를 주는 손님
        Special    // 전투를 통해 해금되고 호감도 시스템이 있는 손님
    }

    [Header("전투 정보 (퀘스트/특별 손님 전용)")]
    [Tooltip("이 손님과 연결된 전투 데이터. null이면 전투가 발생하지 않습니다.")]
    public EnemyDataSO enemyDataSO; // EnemyDataSO 타입으로 최종 확인되었습니다.

    [Header("주문 패턴")]
    public CustomerOrder[] orderPatterns; // 이 손님이 가질 수 있는 여러 주문 패턴들

    void OnValidate()
    {
        if (orderPatterns == null || orderPatterns.Length == 0)
        {
            Debug.LogWarning($"[CustomerData - {customerName}] 'Order Patterns' 배열이 비어있습니다. 손님이 주문을 하지 않을 수 있습니다.", this);
        }
        else
        {
            foreach (var order in orderPatterns)
            {
                if (order.desiredProperties == null || order.desiredProperties.Length == 0)
                {
                    Debug.LogWarning($"[CustomerData - {customerName}] 주문 패턴 '{order.orderPhrase}'에 'Desired Properties'가 정의되지 않았습니다.", this);
                    continue;
                }

                int numProps = order.desiredProperties.Length;
                if (order.greatSuccessThreshold > numProps || order.greatSuccessThreshold < 0)
                {
                    Debug.LogWarning($"[CustomerData - {customerName}] '{order.orderPhrase}'의 대만족 임계값({order.greatSuccessThreshold})이 요구 속성 개수({numProps}) 범위를 벗어납니다.", this);
                }
                if (order.successThreshold > numProps || order.successThreshold < 0)
                {
                    Debug.LogWarning($"[CustomerData - {customerName}] '{order.successThreshold}'의 만족 임계값({order.successThreshold})이 요구 속성 개수({numProps}) 범위를 벗어납니다.", this);
                }
                if (order.normalThreshold > numProps || order.normalThreshold < 0)
                {
                    Debug.LogWarning($"[CustomerData - {customerName}] '{order.orderPhrase}'의 평범 임계값({order.normalThreshold})이 요구 속성 개수({numProps}) 범위를 벗어납니다.", this);
                }
                if (order.greatSuccessThreshold < order.successThreshold ||
                    order.successThreshold < order.normalThreshold)
                {
                    Debug.LogWarning($"[CustomerData - {customerName}] '{order.orderPhrase}'의 요리 티어 임계값 순서가 잘못되었습니다 (대만족 < 만족 < 평범).", this);
                }
            }
        }
    }
}