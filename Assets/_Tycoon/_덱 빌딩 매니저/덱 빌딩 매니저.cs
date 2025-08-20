using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// �� �ܰ迡�� �ɹ��� �����ϰ� ���� ���� ���ϴ� ��� ������ �����մϴ�.
/// </summary>
public class DeckBuildingManager : MonoBehaviour
{
    public static DeckBuildingManager instance;

    private GlobalManager globalManager;
    private List<CardDataSO> currentDeck;

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

    void Start()
    {
        globalManager = GlobalManager.instance;
        if (globalManager == null)
        {
            Debug.LogError("[DeckBuildingManager] GlobalManager ���Ͻ��� ã�� �� �����ϴ�!");
            return;
        }
        currentDeck = new List<CardDataSO>(globalManager.playerBattleDeck);
    }

    public FiveElementsStats GetCurrentPlayerStats()
    {
        if (globalManager.equippedSimbeop != null)
        {
            return globalManager.equippedSimbeop.providedStats;
        }
        return new FiveElementsStats();
    }

    // ���� �߰��� �κ� (��û���� 3) ����
    /// <summary>
    /// ���� ���� ���Ե� ī���� �䱸 �ɷ�ġ�� ��� ������, ���� �ɷ�ġ�� ����Ͽ� ��ȯ�մϴ�.
    /// </summary>
    public FiveElementsStats GetRemainingStats()
    {
        FiveElementsStats remainingStats = GetCurrentPlayerStats(); // �ɹ��� �� �ɷ�ġ�� ����

        FiveElementsStats totalRequiredStats = new FiveElementsStats();
        foreach (var card in currentDeck)
        {
            totalRequiredStats.metal += card.requiredStats.metal;
            totalRequiredStats.wood += card.requiredStats.wood;
            totalRequiredStats.water += card.requiredStats.water;
            totalRequiredStats.fire += card.requiredStats.fire;
            totalRequiredStats.earth += card.requiredStats.earth;
        }

        // �� �ɷ�ġ���� �䱸 �ɷ�ġ ������ ���ϴ�.
        remainingStats.metal -= totalRequiredStats.metal;
        remainingStats.wood -= totalRequiredStats.wood;
        remainingStats.water -= totalRequiredStats.water;
        remainingStats.fire -= totalRequiredStats.fire;
        remainingStats.earth -= totalRequiredStats.earth;

        return remainingStats;
    }
    // ���� �߰��� �κ� ����

    public bool IsCardUsable(CardDataSO cardData)
    {
        // ���� ������ �κ� (��û���� 3) ����
        // ���� '���� �ɷ�ġ'�� �������� ī�带 �߰��� �� �ִ��� Ȯ���մϴ�.
        FiveElementsStats remainingStats = GetRemainingStats();
        // ���� ������ �κ� ����
        FiveElementsStats requiredStats = cardData.requiredStats;

        return remainingStats.metal >= requiredStats.metal &&
               remainingStats.wood >= requiredStats.wood &&
               remainingStats.water >= requiredStats.water &&
               remainingStats.fire >= requiredStats.fire &&
               remainingStats.earth >= requiredStats.earth;
    }

    public void EquipSimbeop(SimbeopDataSO simbeopToEquip)
    {
        globalManager.equippedSimbeop = simbeopToEquip;
        Debug.Log($"[DeckBuildingManager] '{simbeopToEquip.simbeopName}' �ɹ��� �����߽��ϴ�.");

        // ���� �߰��� �κ� (��û���� 1) ����
        // �ɹ��� �����ϸ� ���� ���� ��� ���ϴ�.
        currentDeck.Clear();
        Debug.Log("�ɹ��� ����Ǿ� ���� �ʱ�ȭ�մϴ�.");
        // ���� �߰��� �κ� ����
    }

    public bool AddCardToBattleDeck(CardDataSO cardToAdd)
    {
        if (currentDeck.Contains(cardToAdd))
        {
            Debug.LogWarning($"'{cardToAdd.cardName}' ī��� �̹� ���� ���ԵǾ� �ֽ��ϴ�.");
            return false;
        }

        if (!IsCardUsable(cardToAdd))
        {
            Debug.LogWarning($"�ɷ�ġ�� �����Ͽ� '{cardToAdd.cardName}' ī�带 ���� �߰��� �� �����ϴ�.");
            return false;
        }

        currentDeck.Add(cardToAdd);
        Debug.Log($"'{cardToAdd.cardName}' ī�带 ���� ���� �߰��߽��ϴ�. ���� ��: {currentDeck.Count}��");
        return true;
    }

    public void RemoveCardFromBattleDeck(CardDataSO cardToRemove)
    {
        if (currentDeck.Remove(cardToRemove))
        {
            Debug.Log($"'{cardToRemove.cardName}' ī�带 ���� ������ �����߽��ϴ�. ���� ��: {currentDeck.Count}��");
        }
    }

    public List<CardDataSO> GetCurrentDeck()
    {
        return currentDeck;
    }

    public void ConfirmDeck()
    {
        globalManager.playerBattleDeck = new List<CardDataSO>(currentDeck);
        Debug.Log($"[DeckBuildingManager] �� ���� �Ϸ��ϰ� �����߽��ϴ�. ���� �� ī�� ��: {globalManager.playerBattleDeck.Count}��");
    }
}