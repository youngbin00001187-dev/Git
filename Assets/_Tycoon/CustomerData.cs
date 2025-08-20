using UnityEngine;
using System.Collections.Generic;

// IngredientProperty�� ������ ���� (IngredientProperty.cs)�� ���ǵǾ�� �մϴ�.
// �� ���Ͽ����� IngredientProperty Ŭ������ ���� �������� �ʽ��ϴ�.

// �մ��� ���� �Ӽ� �䱸ġ�� �����ϴ� Ŭ����
// IngredientProperty�� ����������, ���⼭�� �մ��� '���ϴ�' �Ӽ� ��ġ
[System.Serializable]
public class CustomerPropertyRequirement
{
    public string propertyName; // �Ӽ� �̸� (��: "������", "�޴���")
    public int requiredValue;   // �䱸 �Ӽ� ��ġ (�� ���� ����� ��)
    [Tooltip("�� �Ӽ��� �丮 �������� ��ġ�� �߿䵵. �������� �߿�.")]
    [Range(1, 10)] // 1���� 10 ������ ������ ���� ����
    public int importance = 5; // �߿䵵 �߰�
}

// �� �մ��� ���� �� �ִ� �ϳ��� �ֹ� ������ �����ϴ� Ŭ����
[System.Serializable] // <<=== �� �κ��� [System.2025-07-23] Serializable]�� �ƴ��� �� Ȯ���ϼ���!
public class CustomerOrder
{
    [TextArea(3, 5)]
    public string orderPhrase; // �մ��� �߻����� �ֹ� (��: "������ �ʰ� �޴��� �� ���±�.")
    public CustomerPropertyRequirement[] desiredProperties; // �� �ֹ��� ���������� �䱸�ϴ� �Ӽ���
    [Tooltip("�� �ֹ��� ��Ÿ�� Ȯ�� (�ٸ� �ֹ������ ����� ����)")]
    [Range(1, 100)] // 1���� 100 ������ ������ ���� ����
    public int weight = 10; // �ش� �ֹ� ������ ����ġ (Ȯ��)

    [Header("�丮 ��� Ƽ�� ���� (������ �Ӽ� ���� ����)")]
    [Tooltip("�䱸 �Ӽ� �� �� �� �̻��� �����ؾ� '�븸��'�ΰ�? (��: 3�� �� 3��)")]
    public int greatSuccessThreshold; // �븸���� ���� �ּ� ���� �Ӽ� ����
    [Tooltip("�䱸 �Ӽ� �� �� �� �̻��� �����ؾ� '����'�ΰ�? (��: 3�� �� 2��)")]
    public int successThreshold;      // ������ ���� �ּ� ���� �Ӽ� ����
    [Tooltip("�䱸 �Ӽ� �� �� �� �̻��� �����ؾ� '���'�ΰ�? (��: 3�� �� 1��)")]
    public int normalThreshold;       // ����� ���� �ּ� ���� �Ӽ� ����
    // ���д� �� ��� ������ �������� ���� ���

    [Header("Ƽ� ����")]
    public int greatSuccessGold;
    public int greatSuccessReputation;
    [Tooltip("�븸�� �� ������ ������ ID (���� ����, ������ ���� �� ��)")]
    public string greatSuccessRewardItemId;

    public int successGold;
    public int successReputation;
    [Tooltip("���� �� ������ ������ ID (���� ����, ������ ���� �� ��)")]
    public string successRewardItemId;

    public int normalGold;
    public int normalReputation;
    [Tooltip("��� �� ������ ������ ID (���� ����, ������ ���� �� ��)")]
    public string normalRewardItemId;

    // ���� �����͸� ��ȯ�ϴ� ����� Ŭ���� (���������θ� ���)
    public class RewardData
    {
        public int gold;
        public int reputation;
        public string rewardItemId;
    }

    /// <summary>
    /// Ư�� CookingTier�� �ش��ϴ� ���� �����͸� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="tier">�丮 ���</param>
    /// <returns>�ش� ����� ���, ��, ������ ID ����</returns>
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
                Debug.LogWarning($"�� �� ���� CookingTier: {tier}. �⺻ ���� 0���� ��ȯ.");
                reward.gold = 0;
                reward.reputation = 0;
                reward.rewardItemId = null;
                break;
        }
        return reward;
    }

    // Ƽ� �մ� ��� �ʵ�
    [Header("Ƽ� �մ� ���")]
    [TextArea(2, 3)]
    [Tooltip("�븸�� �� �մ��� �� ����Դϴ�.")]
    public string greatSuccessDialogue;
    [TextArea(2, 3)]
    [Tooltip("���� �� �մ��� �� ����Դϴ�.")]
    public string successDialogue;
    [TextArea(2, 3)]
    [Tooltip("��� �� �մ��� �� ����Դϴ�.")]
    public string normalDialogue;
    [TextArea(2, 3)]
    [Tooltip("���� �� �մ��� �� ����Դϴ�.")]
    public string failureDialogue;

    /// <summary>
    /// Ư�� CookingTier�� �ش��ϴ� �մ� ��縦 ��ȯ�մϴ�.
    /// </summary>
    /// <param name="tier">�丮 ���</param>
    /// <returns>�ش� ����� �մ� ���</returns>
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
                Debug.LogWarning($"�� �� ���� CookingTier: {tier}. �� ���ڿ� ��ȯ.");
                return "";
        }
    }
}

// ScriptableObject�� ����� ���� �޴� �׸��� �߰��մϴ�.
[CreateAssetMenu(fileName = "New Customer", menuName = "ǳ���/Customer")]
public class CustomerData : ScriptableObject
{
    public enum CookingTier
    {
        Failure,      // ����
        Normal,       // ���
        Success,      // ����
        GreatSuccess  // �븸��
    }

    [Header("�⺻ ����")]
    public string customerName;      // �մ� �̸� (��: "������ �մ�", "�簡�� ��ȭ")
    public Sprite characterPortrait; // �մ� ĳ���� �ʻ�ȭ (2D �Ϸ���Ʈ)
    public GameObject characterPrefab; // ���ܿ� ������ �մ� 2D �� ������ (���� ����)
    [TextArea(2, 4)]
    public string customerDescription; // �մԿ� ���� ������ ���� (����Ʈ/Ư�� �մԿ��� ����)

    [Header("�մ� Ÿ��")]
    public CustomerType type; // �Ϲ�, ����Ʈ, Ư�� �մ� ����

    // �մ� Ÿ���� �����ϱ� ���� ������
    public enum CustomerType
    {
        Normal,    // �Ϲ� �մ�
        Quest,     // ����Ʈ�� �ִ� �մ�
        Special    // ������ ���� �رݵǰ� ȣ���� �ý����� �ִ� �մ�
    }

    [Header("���� ���� (����Ʈ/Ư�� �մ� ����)")]
    [Tooltip("�� �մ԰� ����� ���� ������. null�̸� ������ �߻����� �ʽ��ϴ�.")]
    public EnemyDataSO enemyDataSO; // EnemyDataSO Ÿ������ ���� Ȯ�εǾ����ϴ�.

    [Header("�ֹ� ����")]
    public CustomerOrder[] orderPatterns; // �� �մ��� ���� �� �ִ� ���� �ֹ� ���ϵ�

    void OnValidate()
    {
        if (orderPatterns == null || orderPatterns.Length == 0)
        {
            Debug.LogWarning($"[CustomerData - {customerName}] 'Order Patterns' �迭�� ����ֽ��ϴ�. �մ��� �ֹ��� ���� ���� �� �ֽ��ϴ�.", this);
        }
        else
        {
            foreach (var order in orderPatterns)
            {
                if (order.desiredProperties == null || order.desiredProperties.Length == 0)
                {
                    Debug.LogWarning($"[CustomerData - {customerName}] �ֹ� ���� '{order.orderPhrase}'�� 'Desired Properties'�� ���ǵ��� �ʾҽ��ϴ�.", this);
                    continue;
                }

                int numProps = order.desiredProperties.Length;
                if (order.greatSuccessThreshold > numProps || order.greatSuccessThreshold < 0)
                {
                    Debug.LogWarning($"[CustomerData - {customerName}] '{order.orderPhrase}'�� �븸�� �Ӱ谪({order.greatSuccessThreshold})�� �䱸 �Ӽ� ����({numProps}) ������ ����ϴ�.", this);
                }
                if (order.successThreshold > numProps || order.successThreshold < 0)
                {
                    Debug.LogWarning($"[CustomerData - {customerName}] '{order.successThreshold}'�� ���� �Ӱ谪({order.successThreshold})�� �䱸 �Ӽ� ����({numProps}) ������ ����ϴ�.", this);
                }
                if (order.normalThreshold > numProps || order.normalThreshold < 0)
                {
                    Debug.LogWarning($"[CustomerData - {customerName}] '{order.orderPhrase}'�� ��� �Ӱ谪({order.normalThreshold})�� �䱸 �Ӽ� ����({numProps}) ������ ����ϴ�.", this);
                }
                if (order.greatSuccessThreshold < order.successThreshold ||
                    order.successThreshold < order.normalThreshold)
                {
                    Debug.LogWarning($"[CustomerData - {customerName}] '{order.orderPhrase}'�� �丮 Ƽ�� �Ӱ谪 ������ �߸��Ǿ����ϴ� (�븸�� < ���� < ���).", this);
                }
            }
        }
    }
}