using UnityEngine;
using System; // Action 이벤트를 위해 필요
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요

public class DayManager : MonoBehaviour
{
    public static DayManager instance;

    // === 이벤트 선언 ===
    public static event Action OnDayStarted; // 새로운 날이 시작될 때 발행
    public static event Action OnCustomerVisitPhaseStarted; // 손님 방문 단계 시작
    public static event Action OnRewardProcessingPhaseStarted; // 보상 처리 단계 시작
    public static event Action<CustomerData.CookingTier> OnCustomerDeparturePhaseStarted; // 손님 퇴장 단계 시작 (CookingTier 인자 추가)

    public static event Action OnNightPhaseEntered; // 밤 페이즈로 전환될 때 발행
    public static event Action OnDayEnded; // 하루가 완전히 끝날 때 발행 (다음 날로 넘어가기 전)

    public static event Action OnRequestSpawnCustomer; // 낮 페이즈 중 손님 스폰 요청 시 발행
    public static event Action<CustomerData.CookingTier, CustomerOrder> OnCookingFinishedAndEvaluate; // 요리 완료 후 평가 요청 (CookingManager에서 호출)
    public static event Action<CustomerData> OnRequestBattleSceneLoad; // 전투 씬 전환 요청 시 발행 (어떤 손님과 전투할지 정보 전달)
    public static event Action OnRequestNextDay; // 밤 활동 후 다음 날 전환 요청 시 발행
    // ==================

    [Header("현재 게임 상태")]
    public int currentDay = 0;
    public DayPhase currentDayPhase = DayPhase.None;

    public UIMode currentUIMode = UIMode.None;

    public enum DayPhase
    {
        None,
        CustomerVisit,
        RewardProcessing,
        CustomerDeparture
    }

    public enum UIMode { None, Day, Night }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[DayManager] Awake 호출됨. 인스턴스 설정 완료.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("[DayManager] Start 호출됨. 새로운 날 시작 시도.");
        currentDay = 0;
        StartNewDay();
    }

    /// <summary>
    /// 새로운 날을 시작하고 관련 이벤트를 발행합니다.
    /// </summary>
    public void StartNewDay()
    {
        currentDay++;
        Debug.Log($"[DayManager] --- 새로운 날 시작: {currentDay}일차 ---");

        currentDayPhase = DayPhase.CustomerVisit;
        currentUIMode = UIMode.Day;

        OnDayStarted?.Invoke();
        OnCustomerVisitPhaseStarted?.Invoke();

        // 하루 시작 시 첫 손님 스폰을 직접 요청 (확실한 테스트를 위해)
        RequestSpawnCustomer();
    }

    /// <summary>
    /// 현재 손님과의 상호작용이 끝나고 다음 단계로 넘어갈 때 호출됩니다.
    /// (예: 요리가 제공되었을 때, TycoonUIManager.AnimateDishToCustomerRoutine 완료 후)
    /// </summary>
    public void ProceedToRewardProcessingPhase(CustomerData.CookingTier tier, CustomerOrder order)
    {
        // 현재 단계가 CustomerVisit이 아니라면 진입 거부
        if (currentDayPhase != DayPhase.CustomerVisit)
        {
            Debug.LogWarning($"[DayManager] RewardProcessing 단계로 진입할 수 없습니다. 현재 단계({currentDayPhase})가 CustomerVisit 단계가 아니거나 이미 넘어갔습니다.");
            return;
        }

        currentDayPhase = DayPhase.RewardProcessing;
        OnRewardProcessingPhaseStarted?.Invoke();
        Debug.Log("[DayManager] --- 낮 페이즈: 보상 처리 단계 진입 ---");

        // 이 이벤트는 TycoonUIManager가 구독하여 보상 처리를 시작할 것입니다.
        OnCookingFinishedAndEvaluate?.Invoke(tier, order);
    }

    /// <summary>
    /// 보상 처리가 끝나고 손님 퇴장 단계로 넘어갈 때 호출됩니다.
    /// (TycoonManager.HandleCookedDishResult에서 호출될 예정)
    /// </summary>
    public void ProceedToCustomerDeparturePhase(CustomerData.CookingTier tier, CustomerData currentCustomerData, CustomerOrder currentCustomerOrder)
    {
        // 현재 단계가 RewardProcessing이 아니라면 진입 거부
        if (currentDayPhase != DayPhase.RewardProcessing)
        {
            Debug.LogWarning($"[DayManager] CustomerDeparture 단계로 진입할 수 없습니다. 현재 단계({currentDayPhase})가 RewardProcessing이 아닙니다.");
            return;
        }

        currentDayPhase = DayPhase.CustomerDeparture;
        // 손님 퇴장 단계 시작 이벤트를 발행하며 CookingTier 정보를 함께 전달
        OnCustomerDeparturePhaseStarted?.Invoke(tier);
        Debug.Log("[DayManager] --- 낮 페이즈: 손님 퇴장 단계 진입 ---");
    }

    /// <summary>
    /// 낮 활동이 끝나고 밤 페이즈로 전환합니다. (모든 손님 응대 완료 또는 '마감' 버튼 클릭 시)
    /// </summary>
    public void EnterNightPhase()
    {
        if (!currentUIMode.Equals(UIMode.Day)) return;

        Debug.Log("[DayManager] --- 밤 페이즈 진입 ---");
        currentUIMode = UIMode.Night;

        OnNightPhaseEntered?.Invoke();
    }

    /// <summary>
    /// 하루의 모든 활동을 마감하고 다음 날로 넘어갈 준비를 합니다. (취침 또는 야행 1회 완료 시)
    /// </summary>
    public void EndDay()
    {
        Debug.Log($"[DayManager] --- {currentDay}일차 종료 ---");

        OnDayEnded?.Invoke();

        // 필요하다면 여기서 저장 로직 호출
        // SaveManager.instance.SaveGame();

        // 다음 날 시작 (플레이어의 '취침' 또는 '야행 완료' 버튼에 연결)
        // StartNewDay(); // 이 부분은 플레이어의 명시적인 행동(취침/야행 완료)에 따라 호출되도록 분리
    }

    /// <summary>
    /// 낮 페이즈 동안 새로운 손님 스폰을 요청합니다.
    /// (예: 하루 시작 시, 이전 손님 퇴장 후)
    /// </summary>
    public void RequestSpawnCustomer()
    {
        if (currentUIMode.Equals(UIMode.Day) && currentDayPhase.Equals(DayPhase.CustomerVisit))
        {
            Debug.Log("[DayManager] OnRequestSpawnCustomer 이벤트 발행 시도.");
            OnRequestSpawnCustomer?.Invoke();
        }
        else
        {
            Debug.LogWarning("[DayManager] 손님 스폰 요청 불가: 현재 낮 페이즈의 손님 방문 단계가 아닙니다.");
        }
    }

    /// <summary>
    /// CookingManager에서 요리 완료 후 평가 결과를 DayManager에게 전달합니다.
    /// (CookingManager.FinalizeCooking()에서 호출될 예정)
    /// </summary>
    public void NotifyCookingFinishedAndEvaluate(CustomerData.CookingTier tier, CustomerOrder order)
    {
        OnCookingFinishedAndEvaluate?.Invoke(tier, order);
    }

    /// <summary>
    /// 전투 씬으로 전환을 요청합니다.
    /// (TycoonManager.StartBattleWithCurrentCustomer()에서 호출될 예정)
    /// </summary>
    /// <param name="customerData">전투할 손님의 데이터</param>
    public void RequestBattleSceneLoad(CustomerData customerData)
    {
        OnRequestBattleSceneLoad?.Invoke(customerData);
    }

    /// <summary>
    /// 밤 활동 후 다음 날 전환을 요청합니다. (취침 또는 야행 1회 완료 시 호출)
    /// </summary>
    public void RequestNextDay()
    {
        if (currentUIMode.Equals(UIMode.Night))
        {
            EndDay();
            StartNewDay();
        }
        else
        {
            Debug.LogWarning("[DayManager] 다음 날로 전환 요청 불가: 현재 밤 페이즈가 아닙니다.");
        }
    }
}