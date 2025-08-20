using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager를 사용하기 위해 추가
using System.Collections.Generic; // List를 사용하기 위해 추가
using System; // Action 이벤트를 위해 추가

public class TycoonManager : MonoBehaviour
{
    public static TycoonManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        // DayManager의 이벤트 구독
        // HandleCookedDishResult는 DayManager.OnCookingFinishedAndEvaluate 이벤트를 구독합니다.
        DayManager.OnCookingFinishedAndEvaluate += HandleCookedDishResult;
        // === 추가된 부분: 전투 씬 로드 요청 이벤트 구독 ===
        DayManager.OnRequestBattleSceneLoad += OnRequestBattleSceneLoadHandler;
        // ===============================================
    }

    void OnDisable()
    {
        // DayManager의 이벤트 구독 해제
        DayManager.OnCookingFinishedAndEvaluate -= HandleCookedDishResult;
        // === 추가된 부분: 전투 씬 로드 요청 이벤트 구독 해제 ===
        DayManager.OnRequestBattleSceneLoad -= OnRequestBattleSceneLoadHandler;
        // ===================================================
    }

    void Start()
    {
        Debug.Log("경영 씬 시작! TycoonManager가 활동을 시작합니다.");
    }

    // === 추가된 이벤트 핸들러: 전투 씬 로드 요청 시 호출 ===
    private void OnRequestBattleSceneLoadHandler(CustomerData customerData)
    {
        // DayManager가 RequestBattleSceneLoad 이벤트를 발행할 때 호출됩니다.
        // 이 함수는 StartBattleWithCurrentCustomer의 로직을 그대로 사용합니다.
        // StartBattleWithCurrentCustomer 함수는 원래 인자를 받지 않았으므로,
        // DayManager가 CustomerData를 전달하는 방식에 맞춰 StartBattleWithCurrentCustomer를 오버로드하거나,
        // CustomerSpawnManager.instance.currentSpawnedCustomerData를 직접 참조하도록 합니다.
        // 현재 StartBattleWithCurrentCustomer는 CustomerSpawnManager.instance.currentSpawnedCustomerData를 참조하므로,
        // 이 핸들러는 단순히 StartBattleWithCurrentCustomer를 호출하기만 합니다.
        StartBattleWithCurrentCustomer();
    }
    // =====================================================

    // 골드를 획득하고 UI를 업데이트하는 중앙 함수
    public void EarnGold(int amount)
    {
        GlobalManager.instance.gold += amount;
        Debug.Log(amount + " 금전 획득! 현재 총 금전: " + GlobalManager.instance.gold);

        if (TycoonUIManager.instance != null)
        {
            TycoonUIManager.instance.UpdateResourceUI();
        }
    }

    // 명성을 획득하고 UI를 업데이트하는 중앙 함수
    public void EarnReputation(int amount)
    {
        GlobalManager.instance.reputation += amount;
        Debug.Log(amount + " 명성 획득! 현재 총 명성: " + GlobalManager.instance.reputation);

        if (TycoonUIManager.instance != null)
        {
            TycoonUIManager.instance.UpdateResourceUI();
        }
    }

    /// <summary>
    /// CookingManager로부터 요리 평가 결과를 받아 보상을 처리합니다.
    /// 이 함수는 DayManager.OnCookingFinishedAndEvaluate 이벤트를 구독하여 호출됩니다.
    /// </summary>
    /// <param name="tier">요리 등급 (대만족, 만족, 평범, 실패)</param>
    /// <param name="customerOrder">현재 활성화된 손님의 주문 정보</param>
    public void HandleCookedDishResult(CustomerData.CookingTier tier, CustomerOrder customerOrder)
    {
        if (customerOrder == null)
        {
            Debug.LogError("TycoonManager: 요리 결과를 처리할 CustomerOrder가 null입니다.");
            return;
        }

        // CustomerOrder에게 해당 등급의 보상 정보를 요청
        CustomerOrder.RewardData rewards = customerOrder.GetRewardForTier(tier);

        // 보상 지급
        if (GlobalManager.instance != null)
        {
            EarnGold(rewards.gold); // TycoonManager 자신의 EarnGold 호출
            EarnReputation(rewards.reputation); // TycoonManager 자신의 EarnReputation 호출 (여기서 gold 대신 reputation을 사용해야 할 수도 있습니다.)
            Debug.Log($"최종 보상 지급 (TycoonManager 처리): 금전 +{rewards.gold}, 명성 +{rewards.reputation} (등급: {tier})");
        }
        else
        {
            Debug.LogError("GlobalManager 인스턴스를 찾을 수 없어 보상을 지급할 수 없습니다!");
        }

        // TODO: 요리 결과에 따른 손님 반응, 호감도 변화 등의 추가 로직을 여기에 구현

        // 핵심: 보상 처리가 끝났으니 DayManager에게 손님 퇴장 단계로 진행하라고 알림
        // DayManager.ProceedToCustomerDeparturePhase가 CustomerSpawnManager의 OnDishResultHandled를 호출하게 됨
        if (DayManager.instance != null)
        {
            DayManager.instance.ProceedToCustomerDeparturePhase(tier, CustomerSpawnManager.instance.currentSpawnedCustomerData, CustomerSpawnManager.instance.currentSpawnedCustomerOrder);
        }
        else
        {
            Debug.LogError("DayManager 인스턴스를 찾을 수 없어 손님 퇴장 단계를 시작할 수 없습니다!");
        }
    }

    /// <summary>
    /// 현재 활성화된 손님과 전투를 시작합니다.
    /// 이 함수는 UI 버튼의 OnClick 이벤트에 연결되거나 DayManager.OnRequestBattleSceneLoad 이벤트를 구독하여 호출됩니다.
    /// </summary>
    public void StartBattleWithCurrentCustomer()
    {
        Debug.Log("전투 시작 버튼 클릭됨! (TycoonManager)");

        if (CustomerSpawnManager.instance == null)
        {
            Debug.LogError("CustomerSpawnManager 인스턴스를 찾을 수 없습니다. 전투를 시작할 수 없습니다!");
            return;
        }

        // 현재 스폰된 손님의 CustomerData를 가져옵니다.
        CustomerData currentCustomerData = CustomerSpawnManager.instance.currentSpawnedCustomerData;

        if (currentCustomerData == null)
        {
            Debug.LogWarning("현재 스폰된 손님이 없습니다. 전투를 시작할 수 없습니다.");
            return;
        }

        // 손님에게 연결된 EnemyDataSO가 있는지 확인합니다.
        if (currentCustomerData.enemyDataSO != null)
        {
            // GlobalManager에 전달할 EnemyDataSO 리스트를 생성합니다.
            List<EnemyDataSO> enemiesToPass = new List<EnemyDataSO>();
            enemiesToPass.Add(currentCustomerData.enemyDataSO); // 현재 손님의 적 데이터를 리스트에 추가

            // GlobalManager를 통해 전투 씬으로 적 데이터 리스트를 전달합니다.
            if (GlobalManager.instance != null)
            {
                GlobalManager.instance.SetEnemiesForBattle(enemiesToPass);
                Debug.Log($"GlobalManager에 총 {enemiesToPass.Count}명의 적 데이터 전달 완료. 전투 씬으로 전환합니다.");

                // --- 전투 씬으로 전환 ---
                SceneManager.LoadScene("BattleScene"); // "BattleScene"은 실제 전투 씬의 이름으로 변경하세요!
            }
            else
            {
                Debug.LogError("GlobalManager 인스턴스를 찾을 수 없습니다. 적 데이터를 전달할 수 없습니다!");
            }
        }
        else
        {
            Debug.LogWarning($"손님 '{currentCustomerData.customerName}'에게 연결된 EnemyDataSO가 없습니다. 전투를 시작할 수 없습니다.");
            // TODO: 전투 데이터가 없을 경우 다른 처리 (예: 그냥 퇴장시키거나 다른 메시지)
        }
    }
}