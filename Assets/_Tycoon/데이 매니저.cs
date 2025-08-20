using UnityEngine;
using System; // Action �̺�Ʈ�� ���� �ʿ�
using UnityEngine.SceneManagement; // �� ������ ���� �ʿ�

public class DayManager : MonoBehaviour
{
    public static DayManager instance;

    // === �̺�Ʈ ���� ===
    public static event Action OnDayStarted; // ���ο� ���� ���۵� �� ����
    public static event Action OnCustomerVisitPhaseStarted; // �մ� �湮 �ܰ� ����
    public static event Action OnRewardProcessingPhaseStarted; // ���� ó�� �ܰ� ����
    public static event Action<CustomerData.CookingTier> OnCustomerDeparturePhaseStarted; // �մ� ���� �ܰ� ���� (CookingTier ���� �߰�)

    public static event Action OnNightPhaseEntered; // �� ������� ��ȯ�� �� ����
    public static event Action OnDayEnded; // �Ϸ簡 ������ ���� �� ���� (���� ���� �Ѿ�� ��)

    public static event Action OnRequestSpawnCustomer; // �� ������ �� �մ� ���� ��û �� ����
    public static event Action<CustomerData.CookingTier, CustomerOrder> OnCookingFinishedAndEvaluate; // �丮 �Ϸ� �� �� ��û (CookingManager���� ȣ��)
    public static event Action<CustomerData> OnRequestBattleSceneLoad; // ���� �� ��ȯ ��û �� ���� (� �մ԰� �������� ���� ����)
    public static event Action OnRequestNextDay; // �� Ȱ�� �� ���� �� ��ȯ ��û �� ����
    // ==================

    [Header("���� ���� ����")]
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
            Debug.Log("[DayManager] Awake ȣ���. �ν��Ͻ� ���� �Ϸ�.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("[DayManager] Start ȣ���. ���ο� �� ���� �õ�.");
        currentDay = 0;
        StartNewDay();
    }

    /// <summary>
    /// ���ο� ���� �����ϰ� ���� �̺�Ʈ�� �����մϴ�.
    /// </summary>
    public void StartNewDay()
    {
        currentDay++;
        Debug.Log($"[DayManager] --- ���ο� �� ����: {currentDay}���� ---");

        currentDayPhase = DayPhase.CustomerVisit;
        currentUIMode = UIMode.Day;

        OnDayStarted?.Invoke();
        OnCustomerVisitPhaseStarted?.Invoke();

        // �Ϸ� ���� �� ù �մ� ������ ���� ��û (Ȯ���� �׽�Ʈ�� ����)
        RequestSpawnCustomer();
    }

    /// <summary>
    /// ���� �մ԰��� ��ȣ�ۿ��� ������ ���� �ܰ�� �Ѿ �� ȣ��˴ϴ�.
    /// (��: �丮�� �����Ǿ��� ��, TycoonUIManager.AnimateDishToCustomerRoutine �Ϸ� ��)
    /// </summary>
    public void ProceedToRewardProcessingPhase(CustomerData.CookingTier tier, CustomerOrder order)
    {
        // ���� �ܰ谡 CustomerVisit�� �ƴ϶�� ���� �ź�
        if (currentDayPhase != DayPhase.CustomerVisit)
        {
            Debug.LogWarning($"[DayManager] RewardProcessing �ܰ�� ������ �� �����ϴ�. ���� �ܰ�({currentDayPhase})�� CustomerVisit �ܰ谡 �ƴϰų� �̹� �Ѿ���ϴ�.");
            return;
        }

        currentDayPhase = DayPhase.RewardProcessing;
        OnRewardProcessingPhaseStarted?.Invoke();
        Debug.Log("[DayManager] --- �� ������: ���� ó�� �ܰ� ���� ---");

        // �� �̺�Ʈ�� TycoonUIManager�� �����Ͽ� ���� ó���� ������ ���Դϴ�.
        OnCookingFinishedAndEvaluate?.Invoke(tier, order);
    }

    /// <summary>
    /// ���� ó���� ������ �մ� ���� �ܰ�� �Ѿ �� ȣ��˴ϴ�.
    /// (TycoonManager.HandleCookedDishResult���� ȣ��� ����)
    /// </summary>
    public void ProceedToCustomerDeparturePhase(CustomerData.CookingTier tier, CustomerData currentCustomerData, CustomerOrder currentCustomerOrder)
    {
        // ���� �ܰ谡 RewardProcessing�� �ƴ϶�� ���� �ź�
        if (currentDayPhase != DayPhase.RewardProcessing)
        {
            Debug.LogWarning($"[DayManager] CustomerDeparture �ܰ�� ������ �� �����ϴ�. ���� �ܰ�({currentDayPhase})�� RewardProcessing�� �ƴմϴ�.");
            return;
        }

        currentDayPhase = DayPhase.CustomerDeparture;
        // �մ� ���� �ܰ� ���� �̺�Ʈ�� �����ϸ� CookingTier ������ �Բ� ����
        OnCustomerDeparturePhaseStarted?.Invoke(tier);
        Debug.Log("[DayManager] --- �� ������: �մ� ���� �ܰ� ���� ---");
    }

    /// <summary>
    /// �� Ȱ���� ������ �� ������� ��ȯ�մϴ�. (��� �մ� ���� �Ϸ� �Ǵ� '����' ��ư Ŭ�� ��)
    /// </summary>
    public void EnterNightPhase()
    {
        if (!currentUIMode.Equals(UIMode.Day)) return;

        Debug.Log("[DayManager] --- �� ������ ���� ---");
        currentUIMode = UIMode.Night;

        OnNightPhaseEntered?.Invoke();
    }

    /// <summary>
    /// �Ϸ��� ��� Ȱ���� �����ϰ� ���� ���� �Ѿ �غ� �մϴ�. (��ħ �Ǵ� ���� 1ȸ �Ϸ� ��)
    /// </summary>
    public void EndDay()
    {
        Debug.Log($"[DayManager] --- {currentDay}���� ���� ---");

        OnDayEnded?.Invoke();

        // �ʿ��ϴٸ� ���⼭ ���� ���� ȣ��
        // SaveManager.instance.SaveGame();

        // ���� �� ���� (�÷��̾��� '��ħ' �Ǵ� '���� �Ϸ�' ��ư�� ����)
        // StartNewDay(); // �� �κ��� �÷��̾��� ������� �ൿ(��ħ/���� �Ϸ�)�� ���� ȣ��ǵ��� �и�
    }

    /// <summary>
    /// �� ������ ���� ���ο� �մ� ������ ��û�մϴ�.
    /// (��: �Ϸ� ���� ��, ���� �մ� ���� ��)
    /// </summary>
    public void RequestSpawnCustomer()
    {
        if (currentUIMode.Equals(UIMode.Day) && currentDayPhase.Equals(DayPhase.CustomerVisit))
        {
            Debug.Log("[DayManager] OnRequestSpawnCustomer �̺�Ʈ ���� �õ�.");
            OnRequestSpawnCustomer?.Invoke();
        }
        else
        {
            Debug.LogWarning("[DayManager] �մ� ���� ��û �Ұ�: ���� �� �������� �մ� �湮 �ܰ谡 �ƴմϴ�.");
        }
    }

    /// <summary>
    /// CookingManager���� �丮 �Ϸ� �� �� ����� DayManager���� �����մϴ�.
    /// (CookingManager.FinalizeCooking()���� ȣ��� ����)
    /// </summary>
    public void NotifyCookingFinishedAndEvaluate(CustomerData.CookingTier tier, CustomerOrder order)
    {
        OnCookingFinishedAndEvaluate?.Invoke(tier, order);
    }

    /// <summary>
    /// ���� ������ ��ȯ�� ��û�մϴ�.
    /// (TycoonManager.StartBattleWithCurrentCustomer()���� ȣ��� ����)
    /// </summary>
    /// <param name="customerData">������ �մ��� ������</param>
    public void RequestBattleSceneLoad(CustomerData customerData)
    {
        OnRequestBattleSceneLoad?.Invoke(customerData);
    }

    /// <summary>
    /// �� Ȱ�� �� ���� �� ��ȯ�� ��û�մϴ�. (��ħ �Ǵ� ���� 1ȸ �Ϸ� �� ȣ��)
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
            Debug.LogWarning("[DayManager] ���� ���� ��ȯ ��û �Ұ�: ���� �� ����� �ƴմϴ�.");
        }
    }
}