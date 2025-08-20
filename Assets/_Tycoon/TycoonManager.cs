using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager�� ����ϱ� ���� �߰�
using System.Collections.Generic; // List�� ����ϱ� ���� �߰�
using System; // Action �̺�Ʈ�� ���� �߰�

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
        // DayManager�� �̺�Ʈ ����
        // HandleCookedDishResult�� DayManager.OnCookingFinishedAndEvaluate �̺�Ʈ�� �����մϴ�.
        DayManager.OnCookingFinishedAndEvaluate += HandleCookedDishResult;
        // === �߰��� �κ�: ���� �� �ε� ��û �̺�Ʈ ���� ===
        DayManager.OnRequestBattleSceneLoad += OnRequestBattleSceneLoadHandler;
        // ===============================================
    }

    void OnDisable()
    {
        // DayManager�� �̺�Ʈ ���� ����
        DayManager.OnCookingFinishedAndEvaluate -= HandleCookedDishResult;
        // === �߰��� �κ�: ���� �� �ε� ��û �̺�Ʈ ���� ���� ===
        DayManager.OnRequestBattleSceneLoad -= OnRequestBattleSceneLoadHandler;
        // ===================================================
    }

    void Start()
    {
        Debug.Log("�濵 �� ����! TycoonManager�� Ȱ���� �����մϴ�.");
    }

    // === �߰��� �̺�Ʈ �ڵ鷯: ���� �� �ε� ��û �� ȣ�� ===
    private void OnRequestBattleSceneLoadHandler(CustomerData customerData)
    {
        // DayManager�� RequestBattleSceneLoad �̺�Ʈ�� ������ �� ȣ��˴ϴ�.
        // �� �Լ��� StartBattleWithCurrentCustomer�� ������ �״�� ����մϴ�.
        // StartBattleWithCurrentCustomer �Լ��� ���� ���ڸ� ���� �ʾ����Ƿ�,
        // DayManager�� CustomerData�� �����ϴ� ��Ŀ� ���� StartBattleWithCurrentCustomer�� �����ε��ϰų�,
        // CustomerSpawnManager.instance.currentSpawnedCustomerData�� ���� �����ϵ��� �մϴ�.
        // ���� StartBattleWithCurrentCustomer�� CustomerSpawnManager.instance.currentSpawnedCustomerData�� �����ϹǷ�,
        // �� �ڵ鷯�� �ܼ��� StartBattleWithCurrentCustomer�� ȣ���ϱ⸸ �մϴ�.
        StartBattleWithCurrentCustomer();
    }
    // =====================================================

    // ��带 ȹ���ϰ� UI�� ������Ʈ�ϴ� �߾� �Լ�
    public void EarnGold(int amount)
    {
        GlobalManager.instance.gold += amount;
        Debug.Log(amount + " ���� ȹ��! ���� �� ����: " + GlobalManager.instance.gold);

        if (TycoonUIManager.instance != null)
        {
            TycoonUIManager.instance.UpdateResourceUI();
        }
    }

    // ���� ȹ���ϰ� UI�� ������Ʈ�ϴ� �߾� �Լ�
    public void EarnReputation(int amount)
    {
        GlobalManager.instance.reputation += amount;
        Debug.Log(amount + " �� ȹ��! ���� �� ��: " + GlobalManager.instance.reputation);

        if (TycoonUIManager.instance != null)
        {
            TycoonUIManager.instance.UpdateResourceUI();
        }
    }

    /// <summary>
    /// CookingManager�κ��� �丮 �� ����� �޾� ������ ó���մϴ�.
    /// �� �Լ��� DayManager.OnCookingFinishedAndEvaluate �̺�Ʈ�� �����Ͽ� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="tier">�丮 ��� (�븸��, ����, ���, ����)</param>
    /// <param name="customerOrder">���� Ȱ��ȭ�� �մ��� �ֹ� ����</param>
    public void HandleCookedDishResult(CustomerData.CookingTier tier, CustomerOrder customerOrder)
    {
        if (customerOrder == null)
        {
            Debug.LogError("TycoonManager: �丮 ����� ó���� CustomerOrder�� null�Դϴ�.");
            return;
        }

        // CustomerOrder���� �ش� ����� ���� ������ ��û
        CustomerOrder.RewardData rewards = customerOrder.GetRewardForTier(tier);

        // ���� ����
        if (GlobalManager.instance != null)
        {
            EarnGold(rewards.gold); // TycoonManager �ڽ��� EarnGold ȣ��
            EarnReputation(rewards.reputation); // TycoonManager �ڽ��� EarnReputation ȣ�� (���⼭ gold ��� reputation�� ����ؾ� �� ���� �ֽ��ϴ�.)
            Debug.Log($"���� ���� ���� (TycoonManager ó��): ���� +{rewards.gold}, �� +{rewards.reputation} (���: {tier})");
        }
        else
        {
            Debug.LogError("GlobalManager �ν��Ͻ��� ã�� �� ���� ������ ������ �� �����ϴ�!");
        }

        // TODO: �丮 ����� ���� �մ� ����, ȣ���� ��ȭ ���� �߰� ������ ���⿡ ����

        // �ٽ�: ���� ó���� �������� DayManager���� �մ� ���� �ܰ�� �����϶�� �˸�
        // DayManager.ProceedToCustomerDeparturePhase�� CustomerSpawnManager�� OnDishResultHandled�� ȣ���ϰ� ��
        if (DayManager.instance != null)
        {
            DayManager.instance.ProceedToCustomerDeparturePhase(tier, CustomerSpawnManager.instance.currentSpawnedCustomerData, CustomerSpawnManager.instance.currentSpawnedCustomerOrder);
        }
        else
        {
            Debug.LogError("DayManager �ν��Ͻ��� ã�� �� ���� �մ� ���� �ܰ踦 ������ �� �����ϴ�!");
        }
    }

    /// <summary>
    /// ���� Ȱ��ȭ�� �մ԰� ������ �����մϴ�.
    /// �� �Լ��� UI ��ư�� OnClick �̺�Ʈ�� ����ǰų� DayManager.OnRequestBattleSceneLoad �̺�Ʈ�� �����Ͽ� ȣ��˴ϴ�.
    /// </summary>
    public void StartBattleWithCurrentCustomer()
    {
        Debug.Log("���� ���� ��ư Ŭ����! (TycoonManager)");

        if (CustomerSpawnManager.instance == null)
        {
            Debug.LogError("CustomerSpawnManager �ν��Ͻ��� ã�� �� �����ϴ�. ������ ������ �� �����ϴ�!");
            return;
        }

        // ���� ������ �մ��� CustomerData�� �����ɴϴ�.
        CustomerData currentCustomerData = CustomerSpawnManager.instance.currentSpawnedCustomerData;

        if (currentCustomerData == null)
        {
            Debug.LogWarning("���� ������ �մ��� �����ϴ�. ������ ������ �� �����ϴ�.");
            return;
        }

        // �մԿ��� ����� EnemyDataSO�� �ִ��� Ȯ���մϴ�.
        if (currentCustomerData.enemyDataSO != null)
        {
            // GlobalManager�� ������ EnemyDataSO ����Ʈ�� �����մϴ�.
            List<EnemyDataSO> enemiesToPass = new List<EnemyDataSO>();
            enemiesToPass.Add(currentCustomerData.enemyDataSO); // ���� �մ��� �� �����͸� ����Ʈ�� �߰�

            // GlobalManager�� ���� ���� ������ �� ������ ����Ʈ�� �����մϴ�.
            if (GlobalManager.instance != null)
            {
                GlobalManager.instance.SetEnemiesForBattle(enemiesToPass);
                Debug.Log($"GlobalManager�� �� {enemiesToPass.Count}���� �� ������ ���� �Ϸ�. ���� ������ ��ȯ�մϴ�.");

                // --- ���� ������ ��ȯ ---
                SceneManager.LoadScene("BattleScene"); // "BattleScene"�� ���� ���� ���� �̸����� �����ϼ���!
            }
            else
            {
                Debug.LogError("GlobalManager �ν��Ͻ��� ã�� �� �����ϴ�. �� �����͸� ������ �� �����ϴ�!");
            }
        }
        else
        {
            Debug.LogWarning($"�մ� '{currentCustomerData.customerName}'���� ����� EnemyDataSO�� �����ϴ�. ������ ������ �� �����ϴ�.");
            // TODO: ���� �����Ͱ� ���� ��� �ٸ� ó�� (��: �׳� �����Ű�ų� �ٸ� �޽���)
        }
    }
}