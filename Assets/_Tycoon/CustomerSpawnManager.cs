using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using DG.Tweening; // DOTween ����� ���� �߰�

public class CustomerSpawnManager : MonoBehaviour
{
    public static CustomerSpawnManager instance;

    [Header("�մ� ������")]
    [Tooltip("���ӿ� ������ ��� �մ� CustomerData ScriptableObject ����� ���⿡ �Ҵ��ϼ���.")]
    public List<CustomerData> allAvailableCustomers;

    [Header("������ �մ� ��� ����")]
    [Tooltip("�Ϸ翡 �湮��ų �մ��� �ִ� ��")]
    public int maxCustomersPerDay = 5;

    [Header("�� ������ �մ� ��� (���� ����)")]
    [Tooltip("�� ������ ���� ���� ������ �մ��� �з��� ��� ���. ���� ������ �����Ͽ� �Ҵ��ϴ� ���� ����.")]
    public List<ReputationTierCustomer> reputationTiers;

    [System.Serializable]
    public class ReputationTierCustomer
    {
        public int minReputation;
        public List<CustomerData> customersInTier;
    }

    [Header("���� ���� ����")]
    [Tooltip("�մ��� ���ܿ� �����Ͽ� �� ���� ���� ��ġ.")]
    public Transform customerSpawnPoint; // �մ��� ���������� �� ���� ��ġ
    [Tooltip("�մ��� ���� �ִϸ��̼��� ������ (����) �ʱ� ��ġ.")]
    public Transform customerEnterPoint; // ���� �ִϸ��̼� ���� ��ġ (��: ȭ�� ���� ��)
    [Tooltip("�մ��� ���� �ִϸ��̼��� ��ĥ (������) ���� ��ġ.")]
    public Transform customerExitPoint;  // ���� �ִϸ��̼� ���� ��ġ (��: ȭ�� ������ ��)

    [Header("�մ� ����/���� �ִϸ��̼� ����")]
    public float customerEntryDuration = 0.8f; // ���� �ִϸ��̼� ���� �ð�
    public Ease customerEntryEase = Ease.OutQuad; // ���� �ִϸ��̼� ��¡
    public float customerExitDuration = 0.8f;  // ���� �ִϸ��̼� ���� �ð�
    public Ease customerExitEase = Ease.InQuad; // ���� �ִϸ��̼� ��¡

    [Header("���� ���� (����׿�)")]
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
        Debug.Log("[CustomerSpawnManager] OnEnable ȣ���. DayManager �̺�Ʈ ���� �õ�.");

        DayManager.OnDayStarted += GenerateTodaysCustomerList;
        TycoonUIManager.OnDayUIReadyAndRequestCustomerSpawn += SpawnNextCustomer;
        DayManager.OnRequestSpawnCustomer += SpawnNextCustomer;
        DayManager.OnCustomerDeparturePhaseStarted += OnCustomerDeparturePhaseStartedHandler;
        CookingManager.OnCookingCompleted += OnCookingCompletedHandler;

        Debug.Log("[CustomerSpawnManager] DayManager �̺�Ʈ ���� �Ϸ�.");
    }

    void OnDisable()
    {
        Debug.Log("[CustomerSpawnManager] OnDisable ȣ���. DayManager �̺�Ʈ ���� ���� �õ�.");

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
                Debug.Log("[CustomerSpawnManager] �丮 �������� �մ� �ֹ� ��ȭâ ����.");
            }
        }
    }

    private void OnCustomerDeparturePhaseStartedHandler(CustomerData.CookingTier tier)
    {
        Debug.Log($"[CustomerSpawnManager] OnCustomerDeparturePhaseStartedHandler ȣ���! Ƽ��: {tier}");
        OnDishResultHandled(tier);
    }

    void Start() { }

    public void GenerateTodaysCustomerList()
    {
        Debug.Log("[CustomerSpawnManager] GenerateTodaysCustomerList �Լ� ȣ���! ������ �մ� ��� ���� ����.");

        todaysCustomerList.Clear();

        if (allAvailableCustomers == null || allAvailableCustomers.Count == 0)
        {
            Debug.LogWarning("[CustomerSpawnManager] allAvailableCustomers ����� ����ֽ��ϴ�. �մ� ����� ������ �� �����ϴ�. �ν����Ϳ� �Ҵ��ߴ��� Ȯ���ϼ���!");
            return;
        }

        int currentReputation = GlobalManager.instance.reputation;
        Debug.Log($"���� ��: {currentReputation}");

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
            Debug.LogWarning("[CustomerSpawnManager] ���� �� ���ǿ� �´� �մ��� �����ϴ�. �� ������ Ȯ���ϰų� allAvailableCustomers�� �մ��� �߰��ϼ���.");
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
                Debug.LogWarning("[CustomerSpawnManager] ����ġ �հ谡 0�̰ų� ������ �մ��� �����ϴ�. ������ �մ� ��� ������ �ߴ��մϴ�.");
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
                Debug.Log($"[CustomerSpawnManager] ������ �մ� ��ܿ� '{selectedCustomer.customerName}' (�ֹ�: '{selectedOrder.orderPhrase}') �߰���.");

                if (selectedIndex != -1)
                {
                    totalWeight -= weightedCustomerOrders[selectedIndex].order.weight;
                    weightedCustomerOrders.RemoveAt(selectedIndex);
                }
            }
        }

        Debug.Log($"[CustomerSpawnManager] ������ �մ� ��� ���� �Ϸ�. �� {todaysCustomerList.Count}��.");
    }

    public void SpawnNextCustomer()
    {
        Debug.Log("[CustomerSpawnManager] SpawnNextCustomer �Լ� ȣ���! ���� �մ� ���� �õ�.");

        if (todaysCustomerList.Count == 0)
        {
            Debug.LogWarning("[CustomerSpawnManager] ������ �մ� ����� ����ֽ��ϴ�. ������ �մ��� �����ϴ�. GenerateTodaysCustomerList�� ����� ȣ��Ǿ�����, �մ� �����Ͱ� ������� Ȯ���ϼ���.");
            return;
        }

        if (currentCustomerInstance != null)
        {
            Debug.Log($"[CustomerSpawnManager] ���� �մ� '{currentCustomerInstance.name}' ���� ó��.");
            currentCustomerInstance = null; // ������ null��
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
            Debug.LogError("[CustomerSpawnManager] CookingManager �ν��Ͻ��� ã�� �� �����ϴ�. �ֹ��� ������ �� �����ϴ�.");
        }

        // === ����� �κ�: ���� �ִϸ��̼� �Ϸ� �� ��ȭâ ǥ�� ===
        if (currentSpawnedCustomerData.characterPrefab != null && customerSpawnPoint != null && customerEnterPoint != null)
        {
            currentCustomerInstance = Instantiate(currentSpawnedCustomerData.characterPrefab, customerEnterPoint.position, customerEnterPoint.rotation); // ���� ���� �������� ����
            currentCustomerInstance.name = currentSpawnedCustomerData.customerName;

            SpriteRenderer spriteRenderer = currentCustomerInstance.GetComponentInChildren<SpriteRenderer>();
            CustomerDialogueDisplay dialogueDisplay = currentCustomerInstance.GetComponent<CustomerDialogueDisplay>(); // ��ȭâ ������Ʈ �̸� ������

            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[CustomerSpawnManager] ������ �մ� '{currentSpawnedCustomerData.customerName}'�� SpriteRenderer ������Ʈ�� �����ϴ�. ����/���� ���̵� �ִϸ��̼� �Ұ�. ��ȭâ ��� ǥ��.");
                // �ִϸ��̼� �Ұ� �� ��ȭâ ��� ǥ��
                if (dialogueDisplay != null)
                {
                    Debug.Log($"[CustomerSpawnManager] CustomerDialogueDisplay ã��! ShowDialogue ȣ��: {currentSpawnedCustomerOrder.orderPhrase}");
                    dialogueDisplay.ShowDialogue(currentSpawnedCustomerOrder.orderPhrase, false); // �ʱ� �ֹ� ���� �ڵ� ���� ��Ȱ��ȭ
                }
            }
            else
            {
                Color startColor = spriteRenderer.color;
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f); // �ʱ� ���� 0

                // DOTween �ִϸ��̼�: ���� ��ġ�� �̵��ϰ� ���İ� �ø�
                currentCustomerInstance.transform.DOMove(customerSpawnPoint.position, customerEntryDuration)
                                         .SetEase(customerEntryEase);
                spriteRenderer.DOFade(1f, customerEntryDuration)
                              .SetEase(customerEntryEase)
                              .OnComplete(() => {
                                  // === �ִϸ��̼� �Ϸ� �� ��ȭâ ǥ�� ===
                                  if (dialogueDisplay != null)
                                  {
                                      Debug.Log($"[CustomerSpawnManager] CustomerDialogueDisplay ã��! ShowDialogue ȣ�� (�ִϸ��̼� �Ϸ� ��): {currentSpawnedCustomerOrder.orderPhrase}");
                                      dialogueDisplay.ShowDialogue(currentSpawnedCustomerOrder.orderPhrase, false); // �ʱ� �ֹ� ���� �ڵ� ���� ��Ȱ��ȭ
                                  }
                                  else
                                  {
                                      Debug.LogWarning($"[CustomerSpawnManager] ������ �մ� '{currentSpawnedCustomerData.customerName}'�� CustomerDialogueDisplay ������Ʈ�� �����ϴ�. ��ȭâ ǥ�� �Ұ�.");
                                  }
                                  Debug.Log($"[CustomerSpawnManager] '{currentSpawnedCustomerData.customerName}' �մ� ���� �ִϸ��̼� �Ϸ�!");
                              });
            }

            Debug.Log($"[CustomerSpawnManager] '{currentSpawnedCustomerData.customerName}' �մ� ���� �Ϸ�! �ֹ�: \"{currentSpawnedCustomerOrder.orderPhrase}\"");
        }
        else
        {
            Debug.LogError($"[CustomerSpawnManager] �մ� ������ ({currentSpawnedCustomerData?.name}), ���� ����(customerSpawnPoint), �Ǵ� ���� ���� ����(customerEnterPoint)�� �Ҵ���� �ʾ� �մ��� ������ �� �����ϴ�. �ν����� �Ҵ��� Ȯ���ϼ���!");
        }
    }

    private CustomerOrder SelectRandomOrderPattern(CustomerOrder[] orders)
    {
        if (orders == null || orders.Length == 0) return null;

        int totalWeight = orders.Sum(order => order.weight);
        if (totalWeight <= 0)
        {
            Debug.LogWarning("[CustomerSpawnManager] �ֹ� ������ �� ����ġ�� 0 �����Դϴ�. ù ��° �ֹ��� ��ȯ�մϴ�.");
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
                Debug.Log($"[CustomerSpawnManager] OnDishResultHandled: ��� Ƽ� ���: \"{tierDialogue}\" (Ƽ��: {tier})");

                dialogueDisplay.ShowDialogue(tierDialogue);

                Debug.Log("[CustomerSpawnManager] AfterDialogueRoutine �ڷ�ƾ ���� �õ�.");
                StartCoroutine(AfterDialogueRoutine(tier, dialogueDisplay.displayDuration));
            }
            else
            {
                Debug.LogWarning("[CustomerSpawnManager] �մ� �ν��Ͻ��� CustomerDialogueDisplay ������Ʈ�� ���� ��ȭ ó���� �Ұ��մϴ�.");
                Debug.Log("[CustomerSpawnManager] AfterDialogueRoutine �ڷ�ƾ ���� �õ� (��ȭâ ����).");
                StartCoroutine(AfterDialogueRoutine(tier, 0f));
            }
        }
        else
        {
            Debug.LogWarning("[CustomerSpawnManager] �մ� �ν��Ͻ� �Ǵ� ���� ������ �ֹ��� ���� �丮 ��� ������ ó���� �� �����ϴ�.");
            Debug.Log("[CustomerSpawnManager] CustomerExitRoutine �ڷ�ƾ ���� �õ� (���� ��Ȳ).");
            StartCoroutine(CustomerExitRoutine(0f));
        }
    }


    private IEnumerator AfterDialogueRoutine(CustomerData.CookingTier tier, float delay)
    {
        Debug.Log($"[CustomerSpawnManager] AfterDialogueRoutine ����. ��� �ð�: {delay}��.");

        yield return new WaitForSeconds(delay);

        Debug.Log("[CustomerSpawnManager] AfterDialogueRoutine ��� �ð� ����. ���� ���� ����.");

        if (tier == CustomerData.CookingTier.Failure)
        {
            if (currentSpawnedCustomerData != null && currentSpawnedCustomerData.enemyDataSO != null)
            {
                Debug.Log($"[CustomerSpawnManager] �丮 ����! '{currentSpawnedCustomerData.enemyDataSO.enemyName}'���� ������ �غ��մϴ�.");
                if (GlobalManager.instance != null)
                {
                    List<EnemyDataSO> enemiesToPass = new List<EnemyDataSO>();
                    enemiesToPass.Add(currentSpawnedCustomerData.enemyDataSO);
                    GlobalManager.instance.SetEnemiesForBattle(enemiesToPass);
                }
                else
                {
                    Debug.LogError("[CustomerSpawnManager] GlobalManager �ν��Ͻ��� ã�� �� ���� ���� �����͸� ������ �� �����ϴ�!");
                }
                DayManager.instance.RequestBattleSceneLoad(currentSpawnedCustomerData);
            }
            else
            {
                Debug.Log("[CustomerSpawnManager] �丮 ���������� ������ ���� �����ϴ�. �մ� ���� ó��.");
                Debug.Log("[CustomerSpawnManager] CustomerExitRoutine �ڷ�ƾ ���� �õ� (����, �� ����).");
                StartCoroutine(CustomerExitRoutine(0f));
            }
        }
        else
        {
            Debug.Log("[CustomerSpawnManager] �մ��� �����ϰ� ���� �غ� ��...");
            Debug.Log("[CustomerSpawnManager] CustomerExitRoutine �ڷ�ƾ ���� �õ� (����).");
            StartCoroutine(CustomerExitRoutine(0f));
        }
        Debug.Log("[CustomerSpawnManager] AfterDialogueRoutine ����.");
    }

    private IEnumerator CustomerExitRoutine(float delay)
    {
        Debug.Log($"[CustomerSpawnManager] CustomerExitRoutine ����. ��� �ð�: {delay}��.");

        yield return new WaitForSeconds(delay);

        Debug.Log("[CustomerSpawnManager] CustomerExitRoutine ��� �ð� ����. �մ� �ı� �õ�.");

        if (currentCustomerInstance != null)
        {
            Debug.Log($"[CustomerSpawnManager] �մ� '{currentCustomerInstance.name}' ���� �ִϸ��̼� ����.");
            SpriteRenderer spriteRenderer = currentCustomerInstance.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[CustomerSpawnManager] ���� �ִϸ��̼� �Ұ�: �մ� '{currentCustomerInstance.name}'�� SpriteRenderer�� �����ϴ�. ��� �ı��մϴ�.");
                Destroy(currentCustomerInstance);
                currentCustomerInstance = null;
                currentSpawnedCustomerData = null;
                currentSpawnedCustomerOrder = null;
                if (DayManager.instance != null)
                {
                    if (todaysCustomerList.Count > 0)
                    {
                        Debug.Log("[CustomerSpawnManager] ���� �մ� ����. ���� �մ� ���� ��û.");
                        DayManager.instance.RequestSpawnCustomer();
                    }
                    else
                    {
                        Debug.Log("[CustomerSpawnManager] ���� �մ� ����. �� ������ ���� ��û.");
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
                              Debug.Log($"[CustomerSpawnManager] �մ� '{currentCustomerInstance.name}' ���� �ִϸ��̼� �Ϸ�. GameObject �ı�.");
                              Destroy(currentCustomerInstance);
                              currentCustomerInstance = null;
                              currentSpawnedCustomerData = null;
                              currentSpawnedCustomerOrder = null;

                              if (DayManager.instance != null)
                              {
                                  if (todaysCustomerList.Count > 0)
                                  {
                                      Debug.Log("[CustomerSpawnManager] ���� �մ� ����. ���� �մ� ���� ��û.");
                                      DayManager.instance.RequestSpawnCustomer();
                                  }
                                  else
                                  {
                                      Debug.Log("[CustomerSpawnManager] ���� �մ� ����. �� ������ ���� ��û.");
                                      DayManager.instance.EnterNightPhase();
                                  }
                              }
                          });
        }
        else
        {
            Debug.LogWarning("[CustomerSpawnManager] CustomerExitRoutine: currentCustomerInstance�� �̹� null�Դϴ�. �ı��� �մ��� �����ϴ�.");
            if (DayManager.instance != null)
            {
                if (todaysCustomerList.Count > 0)
                {
                    Debug.Log("[CustomerSpawnManager] ���� �մ� ����. ���� �մ� ���� ��û.");
                    DayManager.instance.RequestSpawnCustomer();
                }
                else
                {
                    Debug.Log("[CustomerSpawnManager] ���� �մ� ����. �� ������ ���� ��û.");
                    DayManager.instance.EnterNightPhase();
                }
            }
        }
        Debug.Log("[CustomerSpawnManager] CustomerExitRoutine ����.");
    }
}