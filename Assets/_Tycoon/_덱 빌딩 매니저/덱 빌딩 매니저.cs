using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 밤 단계에서 심법을 장착하고 전투 덱을 편성하는 모든 로직을 관리합니다.
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
            Debug.LogError("[DeckBuildingManager] GlobalManager 인턴스를 찾을 수 없습니다!");
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

    // ▼▼▼ 추가된 부분 (요청사항 3) ▼▼▼
    /// <summary>
    /// 현재 덱에 포함된 카드의 요구 능력치를 모두 제외한, 남은 능력치를 계산하여 반환합니다.
    /// </summary>
    public FiveElementsStats GetRemainingStats()
    {
        FiveElementsStats remainingStats = GetCurrentPlayerStats(); // 심법의 총 능력치로 시작

        FiveElementsStats totalRequiredStats = new FiveElementsStats();
        foreach (var card in currentDeck)
        {
            totalRequiredStats.metal += card.requiredStats.metal;
            totalRequiredStats.wood += card.requiredStats.wood;
            totalRequiredStats.water += card.requiredStats.water;
            totalRequiredStats.fire += card.requiredStats.fire;
            totalRequiredStats.earth += card.requiredStats.earth;
        }

        // 총 능력치에서 요구 능력치 총합을 뺍니다.
        remainingStats.metal -= totalRequiredStats.metal;
        remainingStats.wood -= totalRequiredStats.wood;
        remainingStats.water -= totalRequiredStats.water;
        remainingStats.fire -= totalRequiredStats.fire;
        remainingStats.earth -= totalRequiredStats.earth;

        return remainingStats;
    }
    // ▲▲▲ 추가된 부분 ▲▲▲

    public bool IsCardUsable(CardDataSO cardData)
    {
        // ▼▼▼ 수정된 부분 (요청사항 3) ▼▼▼
        // 이제 '남은 능력치'를 기준으로 카드를 추가할 수 있는지 확인합니다.
        FiveElementsStats remainingStats = GetRemainingStats();
        // ▲▲▲ 수정된 부분 ▲▲▲
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
        Debug.Log($"[DeckBuildingManager] '{simbeopToEquip.simbeopName}' 심법을 장착했습니다.");

        // ▼▼▼ 추가된 부분 (요청사항 1) ▼▼▼
        // 심법을 변경하면 현재 덱을 모두 비웁니다.
        currentDeck.Clear();
        Debug.Log("심법이 변경되어 덱을 초기화합니다.");
        // ▲▲▲ 추가된 부분 ▲▲▲
    }

    public bool AddCardToBattleDeck(CardDataSO cardToAdd)
    {
        if (currentDeck.Contains(cardToAdd))
        {
            Debug.LogWarning($"'{cardToAdd.cardName}' 카드는 이미 덱에 포함되어 있습니다.");
            return false;
        }

        if (!IsCardUsable(cardToAdd))
        {
            Debug.LogWarning($"능력치가 부족하여 '{cardToAdd.cardName}' 카드를 덱에 추가할 수 없습니다.");
            return false;
        }

        currentDeck.Add(cardToAdd);
        Debug.Log($"'{cardToAdd.cardName}' 카드를 전투 덱에 추가했습니다. 현재 덱: {currentDeck.Count}장");
        return true;
    }

    public void RemoveCardFromBattleDeck(CardDataSO cardToRemove)
    {
        if (currentDeck.Remove(cardToRemove))
        {
            Debug.Log($"'{cardToRemove.cardName}' 카드를 전투 덱에서 제거했습니다. 현재 덱: {currentDeck.Count}장");
        }
    }

    public List<CardDataSO> GetCurrentDeck()
    {
        return currentDeck;
    }

    public void ConfirmDeck()
    {
        globalManager.playerBattleDeck = new List<CardDataSO>(currentDeck);
        Debug.Log($"[DeckBuildingManager] 덱 편성을 완료하고 저장했습니다. 최종 덱 카드 수: {globalManager.playerBattleDeck.Count}장");
    }
}