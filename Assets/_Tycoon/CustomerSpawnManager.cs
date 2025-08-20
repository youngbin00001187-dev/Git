using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using DG.Tweening; // DOTween 사용을 위해 추가

public class CustomerSpawnManager : MonoBehaviour
{
    public static CustomerSpawnManager instance;

    [Header("손님 데이터")]
    [Tooltip("게임에 등장할 모든 손님 CustomerData ScriptableObject 목록을 여기에 할당하세요.")]
    public List<CustomerData> allAvailableCustomers;

    [Header("오늘의 손님 명단 설정")]
    [Tooltip("하루에 방문시킬 손님의 최대 수")]
    public int maxCustomersPerDay = 5;

    [Header("명성 구간별 손님 목록 (선택 사항)")]
    [Tooltip("명성 구간에 따라 등장 가능한 손님을 분류할 경우 사용. 높은 명성부터 정렬하여 할당하는 것을 권장.")]
    public List<ReputationTierCustomer> reputationTiers;

    [System.Serializable]
    public class ReputationTierCustomer
    {
        public int minReputation;
        public List<CustomerData> customersInTier;
    }

    [Header("스폰 지점 설정")]
    [Tooltip("손님이 객잔에 도착하여 서 있을 최종 위치.")]
    public Transform customerSpawnPoint; // 손님이 최종적으로 서 있을 위치
    [Tooltip("손님이 등장 애니메이션을 시작할 (왼쪽) 초기 위치.")]
    public Transform customerEnterPoint; // 등장 애니메이션 시작 위치 (예: 화면 왼쪽 밖)
    [Tooltip("손님이 퇴장 애니메이션을 마칠 (오른쪽) 최종 위치.")]
    public Transform customerExitPoint;  // 퇴장 애니메이션 도착 위치 (예: 화면 오른쪽 밖)

    [Header("손님 등장/퇴장 애니메이션 설정")]
    public float customerEntryDuration = 0.8f; // 등장 애니메이션 지속 시간
    public Ease customerEntryEase = Ease.OutQuad; // 등장 애니메이션 이징
    public float customerExitDuration = 0.8f;  // 퇴장 애니메이션 지속 시간
    public Ease customerExitEase = Ease.InQuad; // 퇴장 애니메이션 이징

    [Header("현재 상태 (디버그용)")]
    public List<(CustomerData customerData, CustomerOrder customerOrder)> todaysCustomerList = new List<(CustomerData, CustomerOrder)>();
    public CustomerData currentSpawnedCustomerData;
    public CustomerOrder currentSpawnedCustomerOrder;
    public GameObject currentCustomerInstance;

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
        Debug.Log("[CustomerSpawnManager] OnEnable 호출됨. DayManager 이벤트 구독 시도.");

        DayManager.OnDayStarted += GenerateTodaysCustomerList;
        TycoonUIManager.OnDayUIReadyAndRequestCustomerSpawn += SpawnNextCustomer;
        DayManager.OnRequestSpawnCustomer += SpawnNextCustomer;
        DayManager.OnCustomerDeparturePhaseStarted += OnCustomerDeparturePhaseStartedHandler;
        CookingManager.OnCookingCompleted += OnCookingCompletedHandler;

        Debug.Log("[CustomerSpawnManager] DayManager 이벤트 구독 완료.");
    }

    void OnDisable()
    {
        Debug.Log("[CustomerSpawnManager] OnDisable 호출됨. DayManager 이벤트 구독 해제 시도.");

        DayManager.OnDayStarted -= GenerateTodaysCustomerList;
        TycoonUIManager.OnDayUIReadyAndRequestCustomerSpawn -= SpawnNextCustomer;
        DayManager.OnRequestSpawnCustomer -= SpawnNextCustomer;
        DayManager.OnCustomerDeparturePhaseStarted -= OnCustomerDeparturePhaseStartedHandler;
        CookingManager.OnCookingCompleted -= OnCookingCompletedHandler;
    }

    private void OnCookingCompletedHandler(CustomerData.CookingTier tier, CustomerOrder order)
    {
        if (currentCustomerInstance != null)
        {
            CustomerDialogueDisplay dialogueDisplay = currentCustomerInstance.GetComponent<CustomerDialogueDisplay>();
            if (dialogueDisplay != null)
            {
                dialogueDisplay.ForceHideDialogue();
                Debug.Log("[CustomerSpawnManager] 요리 시작으로 손님 주문 대화창 숨김.");
            }
        }
    }

    private void OnCustomerDeparturePhaseStartedHandler(CustomerData.CookingTier tier)
    {
        Debug.Log($"[CustomerSpawnManager] OnCustomerDeparturePhaseStartedHandler 호출됨! 티어: {tier}");
        OnDishResultHandled(tier);
    }

    void Start() { }

    public void GenerateTodaysCustomerList()
    {
        Debug.Log("[CustomerSpawnManager] GenerateTodaysCustomerList 함수 호출됨! 오늘의 손님 명단 생성 시작.");

        todaysCustomerList.Clear();

        if (allAvailableCustomers == null || allAvailableCustomers.Count == 0)
        {
            Debug.LogWarning("[CustomerSpawnManager] allAvailableCustomers 목록이 비어있습니다. 손님 명단을 생성할 수 없습니다. 인스펙터에 할당했는지 확인하세요!");
            return;
        }

        int currentReputation = GlobalManager.instance.reputation;
        Debug.Log($"현재 명성: {currentReputation}");

        List<CustomerData> eligibleCustomers = new List<CustomerData>();

        if (reputationTiers != null && reputationTiers.Count > 0)
        {
            foreach (var tier in reputationTiers.OrderByDescending(t => t.minReputation))
            {
                if (currentReputation >= tier.minReputation)
                {
                    eligibleCustomers.AddRange(tier.customersInTier);
                    eligibleCustomers = eligibleCustomers.Distinct().ToList();
                }
            }
        }
        else
        {
            eligibleCustomers.AddRange(allAvailableCustomers);
        }

        if (eligibleCustomers.Count == 0)
        {
            Debug.LogWarning("[CustomerSpawnManager] 현재 명성 조건에 맞는 손님이 없습니다. 명성 조건을 확인하거나 allAvailableCustomers에 손님을 추가하세요.");
            return;
        }

        List<(CustomerData customer, CustomerOrder order)> weightedCustomerOrders = new List<(CustomerData, CustomerOrder)>();
        foreach (var customerData in eligibleCustomers)
        {
            if (customerData.orderPatterns != null)
            {
                foreach (var orderPattern in customerData.orderPatterns)
                {
                    weightedCustomerOrders.Add((customerData, orderPattern));
                }
            }
        }

        int totalWeight = weightedCustomerOrders.Sum(item => item.order.weight);

        for (int i = 0; i < maxCustomersPerDay; i++)
        {
            if (totalWeight <= 0 || weightedCustomerOrders.Count == 0)
            {
                Debug.LogWarning("[CustomerSpawnManager] 가중치 합계가 0이거나 선택할 손님이 없습니다. 오늘의 손님 명단 생성을 중단합니다.");
                break;
            }

            int randomPoint = UnityEngine.Random.Range(0, totalWeight);
            CustomerData selectedCustomer = null;
            CustomerOrder selectedOrder = null;
            int selectedIndex = -1;

            for (int j = 0; j < weightedCustomerOrders.Count; j++)
            {
                randomPoint -= weightedCustomerOrders[j].order.weight;
                if (randomPoint < 0)
                {
                    selectedCustomer = weightedCustomerOrders[j].customer;
                    selectedOrder = weightedCustomerOrders[j].order;
                    selectedIndex = j;
                    break;
                }
            }

            if (selectedCustomer != null)
            {
                todaysCustomerList.Add((selectedCustomer, selectedOrder));
                Debug.Log($"[CustomerSpawnManager] 오늘의 손님 명단에 '{selectedCustomer.customerName}' (주문: '{selectedOrder.orderPhrase}') 추가됨.");

                if (selectedIndex != -1)
                {
                    totalWeight -= weightedCustomerOrders[selectedIndex].order.weight;
                    weightedCustomerOrders.RemoveAt(selectedIndex);
                }
            }
        }

        Debug.Log($"[CustomerSpawnManager] 오늘의 손님 명단 생성 완료. 총 {todaysCustomerList.Count}명.");
    }

    public void SpawnNextCustomer()
    {
        Debug.Log("[CustomerSpawnManager] SpawnNextCustomer 함수 호출됨! 다음 손님 스폰 시도.");

        if (todaysCustomerList.Count == 0)
        {
            Debug.LogWarning("[CustomerSpawnManager] 오늘의 손님 명단이 비어있습니다. 스폰할 손님이 없습니다. GenerateTodaysCustomerList가 제대로 호출되었는지, 손님 데이터가 충분한지 확인하세요.");
            return;
        }

        if (currentCustomerInstance != null)
        {
            Debug.Log($"[CustomerSpawnManager] 이전 손님 '{currentCustomerInstance.name}' 퇴장 처리.");
            currentCustomerInstance = null; // 참조만 null로
        }

        (CustomerData nextCustomerData, CustomerOrder nextCustomerOrder) = todaysCustomerList[0];
        todaysCustomerList.RemoveAt(0);

        currentSpawnedCustomerData = nextCustomerData;
        currentSpawnedCustomerOrder = nextCustomerOrder;

        if (CookingManager.instance != null)
        {
            CookingManager.instance.activeCustomerOrder = currentSpawnedCustomerOrder;
        }
        else
        {
            Debug.LogError("[CustomerSpawnManager] CookingManager 인스턴스를 찾을 수 없습니다. 주문을 설정할 수 없습니다.");
        }

        // === 변경된 부분: 등장 애니메이션 완료 후 대화창 표시 ===
        if (currentSpawnedCustomerData.characterPrefab != null && customerSpawnPoint != null && customerEnterPoint != null)
        {
            currentCustomerInstance = Instantiate(currentSpawnedCustomerData.characterPrefab, customerEnterPoint.position, customerEnterPoint.rotation); // 등장 시작 지점에서 스폰
            currentCustomerInstance.name = currentSpawnedCustomerData.customerName;

            SpriteRenderer spriteRenderer = currentCustomerInstance.GetComponentInChildren<SpriteRenderer>();
            CustomerDialogueDisplay dialogueDisplay = currentCustomerInstance.GetComponent<CustomerDialogueDisplay>(); // 대화창 컴포넌트 미리 가져옴

            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[CustomerSpawnManager] 스폰된 손님 '{currentSpawnedCustomerData.customerName}'에 SpriteRenderer 컴포넌트가 없습니다. 등장/퇴장 페이드 애니메이션 불가. 대화창 즉시 표시.");
                // 애니메이션 불가 시 대화창 즉시 표시
                if (dialogueDisplay != null)
                {
                    Debug.Log($"[CustomerSpawnManager] CustomerDialogueDisplay 찾음! ShowDialogue 호출: {currentSpawnedCustomerOrder.orderPhrase}");
                    dialogueDisplay.ShowDialogue(currentSpawnedCustomerOrder.orderPhrase, false); // 초기 주문 대사는 자동 숨김 비활성화
                }
            }
            else
            {
                Color startColor = spriteRenderer.color;
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f); // 초기 알파 0

                // DOTween 애니메이션: 최종 위치로 이동하고 알파값 올림
                currentCustomerInstance.transform.DOMove(customerSpawnPoint.position, customerEntryDuration)
                                         .SetEase(customerEntryEase);
                spriteRenderer.DOFade(1f, customerEntryDuration)
                              .SetEase(customerEntryEase)
                              .OnComplete(() => {
                                  // === 애니메이션 완료 후 대화창 표시 ===
                                  if (dialogueDisplay != null)
                                  {
                                      Debug.Log($"[CustomerSpawnManager] CustomerDialogueDisplay 찾음! ShowDialogue 호출 (애니메이션 완료 후): {currentSpawnedCustomerOrder.orderPhrase}");
                                      dialogueDisplay.ShowDialogue(currentSpawnedCustomerOrder.orderPhrase, false); // 초기 주문 대사는 자동 숨김 비활성화
                                  }
                                  else
                                  {
                                      Debug.LogWarning($"[CustomerSpawnManager] 스폰된 손님 '{currentSpawnedCustomerData.customerName}'에 CustomerDialogueDisplay 컴포넌트가 없습니다. 대화창 표시 불가.");
                                  }
                                  Debug.Log($"[CustomerSpawnManager] '{currentSpawnedCustomerData.customerName}' 손님 등장 애니메이션 완료!");
                              });
            }

            Debug.Log($"[CustomerSpawnManager] '{currentSpawnedCustomerData.customerName}' 손님 스폰 완료! 주문: \"{currentSpawnedCustomerOrder.orderPhrase}\"");
        }
        else
        {
            Debug.LogError($"[CustomerSpawnManager] 손님 프리펩 ({currentSpawnedCustomerData?.name}), 스폰 지점(customerSpawnPoint), 또는 등장 시작 지점(customerEnterPoint)이 할당되지 않아 손님을 스폰할 수 없습니다. 인스펙터 할당을 확인하세요!");
        }
    }

    private CustomerOrder SelectRandomOrderPattern(CustomerOrder[] orders)
    {
        if (orders == null || orders.Length == 0) return null;

        int totalWeight = orders.Sum(order => order.weight);
        if (totalWeight <= 0)
        {
            Debug.LogWarning("[CustomerSpawnManager] 주문 패턴의 총 가중치가 0 이하입니다. 첫 번째 주문을 반환합니다.");
            return orders[0];
        }

        int randomPoint = UnityEngine.Random.Range(0, totalWeight);

        foreach (var order in orders)
        {
            randomPoint -= order.weight;
            if (randomPoint < 0)
            {
                return order;
            }
        }
        return orders[orders.Length - 1];
    }

    public void OnDishResultHandled(CustomerData.CookingTier tier)
    {
        if (currentCustomerInstance != null && currentSpawnedCustomerOrder != null)
        {
            CustomerDialogueDisplay dialogueDisplay = currentCustomerInstance.GetComponent<CustomerDialogueDisplay>();
            if (dialogueDisplay != null)
            {
                string tierDialogue = currentSpawnedCustomerOrder.GetDialogueForTier(tier);
                Debug.Log($"[CustomerSpawnManager] OnDishResultHandled: 띄울 티어별 대사: \"{tierDialogue}\" (티어: {tier})");

                dialogueDisplay.ShowDialogue(tierDialogue);

                Debug.Log("[CustomerSpawnManager] AfterDialogueRoutine 코루틴 시작 시도.");
                StartCoroutine(AfterDialogueRoutine(tier, dialogueDisplay.displayDuration));
            }
            else
            {
                Debug.LogWarning("[CustomerSpawnManager] 손님 인스턴스에 CustomerDialogueDisplay 컴포넌트가 없어 대화 처리가 불가합니다.");
                Debug.Log("[CustomerSpawnManager] AfterDialogueRoutine 코루틴 시작 시도 (대화창 없음).");
                StartCoroutine(AfterDialogueRoutine(tier, 0f));
            }
        }
        else
        {
            Debug.LogWarning("[CustomerSpawnManager] 손님 인스턴스 또는 현재 스폰된 주문이 없어 요리 결과 반응을 처리할 수 없습니다.");
            Debug.Log("[CustomerSpawnManager] CustomerExitRoutine 코루틴 시작 시도 (오류 상황).");
            StartCoroutine(CustomerExitRoutine(0f));
        }
    }


    private IEnumerator AfterDialogueRoutine(CustomerData.CookingTier tier, float delay)
    {
        Debug.Log($"[CustomerSpawnManager] AfterDialogueRoutine 시작. 대기 시간: {delay}초.");

        yield return new WaitForSeconds(delay);

        Debug.Log("[CustomerSpawnManager] AfterDialogueRoutine 대기 시간 종료. 다음 로직 실행.");

        if (tier == CustomerData.CookingTier.Failure)
        {
            if (currentSpawnedCustomerData != null && currentSpawnedCustomerData.enemyDataSO != null)
            {
                Debug.Log($"[CustomerSpawnManager] 요리 실패! '{currentSpawnedCustomerData.enemyDataSO.enemyName}'과의 전투를 준비합니다.");
                if (GlobalManager.instance != null)
                {
                    List<EnemyDataSO> enemiesToPass = new List<EnemyDataSO>();
                    enemiesToPass.Add(currentSpawnedCustomerData.enemyDataSO);
                    GlobalManager.instance.SetEnemiesForBattle(enemiesToPass);
                }
                else
                {
                    Debug.LogError("[CustomerSpawnManager] GlobalManager 인스턴스를 찾을 수 없어 전투 데이터를 전달할 수 없습니다!");
                }
                DayManager.instance.RequestBattleSceneLoad(currentSpawnedCustomerData);
            }
            else
            {
                Debug.Log("[CustomerSpawnManager] 요리 실패했으나 전투할 적이 없습니다. 손님 퇴장 처리.");
                Debug.Log("[CustomerSpawnManager] CustomerExitRoutine 코루틴 시작 시도 (실패, 적 없음).");
                StartCoroutine(CustomerExitRoutine(0f));
            }
        }
        else
        {
            Debug.Log("[CustomerSpawnManager] 손님이 만족하고 떠날 준비 중...");
            Debug.Log("[CustomerSpawnManager] CustomerExitRoutine 코루틴 시작 시도 (성공).");
            StartCoroutine(CustomerExitRoutine(0f));
        }
        Debug.Log("[CustomerSpawnManager] AfterDialogueRoutine 종료.");
    }

    private IEnumerator CustomerExitRoutine(float delay)
    {
        Debug.Log($"[CustomerSpawnManager] CustomerExitRoutine 시작. 대기 시간: {delay}초.");

        yield return new WaitForSeconds(delay);

        Debug.Log("[CustomerSpawnManager] CustomerExitRoutine 대기 시간 종료. 손님 파괴 시도.");

        if (currentCustomerInstance != null)
        {
            Debug.Log($"[CustomerSpawnManager] 손님 '{currentCustomerInstance.name}' 퇴장 애니메이션 시작.");
            SpriteRenderer spriteRenderer = currentCustomerInstance.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[CustomerSpawnManager] 퇴장 애니메이션 불가: 손님 '{currentCustomerInstance.name}'에 SpriteRenderer가 없습니다. 즉시 파괴합니다.");
                Destroy(currentCustomerInstance);
                currentCustomerInstance = null;
                currentSpawnedCustomerData = null;
                currentSpawnedCustomerOrder = null;
                if (DayManager.instance != null)
                {
                    if (todaysCustomerList.Count > 0)
                    {
                        Debug.Log("[CustomerSpawnManager] 남은 손님 있음. 다음 손님 스폰 요청.");
                        DayManager.instance.RequestSpawnCustomer();
                    }
                    else
                    {
                        Debug.Log("[CustomerSpawnManager] 남은 손님 없음. 밤 페이즈 진입 요청.");
                        DayManager.instance.EnterNightPhase();
                    }
                }
                yield break;
            }

            currentCustomerInstance.transform.DOMove(customerExitPoint.position, customerExitDuration)
                                     .SetEase(customerExitEase);
            spriteRenderer.DOFade(0f, customerExitDuration)
                          .SetEase(customerExitEase)
                          .OnComplete(() => {
                              Debug.Log($"[CustomerSpawnManager] 손님 '{currentCustomerInstance.name}' 퇴장 애니메이션 완료. GameObject 파괴.");
                              Destroy(currentCustomerInstance);
                              currentCustomerInstance = null;
                              currentSpawnedCustomerData = null;
                              currentSpawnedCustomerOrder = null;

                              if (DayManager.instance != null)
                              {
                                  if (todaysCustomerList.Count > 0)
                                  {
                                      Debug.Log("[CustomerSpawnManager] 남은 손님 있음. 다음 손님 스폰 요청.");
                                      DayManager.instance.RequestSpawnCustomer();
                                  }
                                  else
                                  {
                                      Debug.Log("[CustomerSpawnManager] 남은 손님 없음. 밤 페이즈 진입 요청.");
                                      DayManager.instance.EnterNightPhase();
                                  }
                              }
                          });
        }
        else
        {
            Debug.LogWarning("[CustomerSpawnManager] CustomerExitRoutine: currentCustomerInstance가 이미 null입니다. 파괴할 손님이 없습니다.");
            if (DayManager.instance != null)
            {
                if (todaysCustomerList.Count > 0)
                {
                    Debug.Log("[CustomerSpawnManager] 남은 손님 있음. 다음 손님 스폰 요청.");
                    DayManager.instance.RequestSpawnCustomer();
                }
                else
                {
                    Debug.Log("[CustomerSpawnManager] 남은 손님 없음. 밤 페이즈 진입 요청.");
                    DayManager.instance.EnterNightPhase();
                }
            }
        }
        Debug.Log("[CustomerSpawnManager] CustomerExitRoutine 종료.");
    }
}