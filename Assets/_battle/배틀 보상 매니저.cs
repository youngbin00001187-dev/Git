using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    private List<EnemyDataSO> defeatedEnemies = new List<EnemyDataSO>();

    [Header("UI 연결")]
    public GameObject rewardPanel;   // 보상 결과창 전체 패널
    public TextMeshProUGUI rewardText;     // 보상 메시지를 출력할 텍스트 컴포넌트

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterDefeatedEnemy(EnemyDataSO enemyData)
    {
        if (enemyData != null)
        {
            defeatedEnemies.Add(enemyData);
            Debug.Log($"[RewardManager] {enemyData.enemyName} 보상 등록됨.");
        }
    }

    /// <summary>
    /// 보상을 계산하고 UI에 결과 출력까지 처리
    /// </summary>
    public void ProcessRewards()
    {
        int totalGold = 0;
        int totalFame = 0;
        List<CardDataSO> earnedCards = new List<CardDataSO>();
        List<SimbeopDataSO> earnedXinfa = new List<SimbeopDataSO>();

        foreach (var enemy in defeatedEnemies)
        {
            totalGold += Random.Range(enemy.goldMin, enemy.goldMax + 1);
            totalFame += Random.Range(enemy.fameMin, enemy.fameMax + 1);

            foreach (var drop in enemy.cardDrops)
            {
                if (Random.value < drop.dropChance && drop.rewardItem != null)
                {
                    earnedCards.Add(drop.rewardItem);
                }
            }

            foreach (var drop in enemy.xinfaDrops)
            {
                if (Random.value < drop.dropChance && drop.rewardItem != null)
                {
                    earnedXinfa.Add(drop.rewardItem);
                }
            }
        }

        // GlobalManager에 적용
        GlobalManager.instance.gold += totalGold;
        GlobalManager.instance.reputation += totalFame;

        foreach (var card in earnedCards)
        {
            if (!GlobalManager.instance.playerCardCollection.Contains(card))
                GlobalManager.instance.playerCardCollection.Add(card);
        }

        foreach (var xinfa in earnedXinfa)
        {
            if (!GlobalManager.instance.ownedSimbeops.Contains(xinfa))
                GlobalManager.instance.ownedSimbeops.Add(xinfa);
        }

        // 보상 메시지 조합
        string message = $"<b>전투 보상</b>\n";
        message += $"골드: {totalGold}\n";
        message += $"명성: {totalFame}\n";

        if (earnedCards.Count > 0)
        {
            message += $"획득 카드:\n";
            foreach (var card in earnedCards)
            {
                message += $"- {card.cardName}\n";
            }
        }

        if (earnedXinfa.Count > 0)
        {
            message += $"획득 심법:\n";
            foreach (var xinfa in earnedXinfa)
            {
                message += $"- {xinfa.name}\n";
            }
        }

        Debug.Log("[RewardManager] 보상 메시지 내용:\n" + message);  // 디버그 로그 추가

        ShowRewardPanel(message);
        ClearDefeatedEnemies();
    }

    /// <summary>
    /// 보상 패널 띄우고 텍스트 세팅
    /// </summary>
    /// <param name="text">보상 텍스트</param>
    public void ShowRewardPanel(string text)
    {
        if (rewardPanel != null && rewardText != null)
        {
            Debug.Log("[RewardManager] ShowRewardPanel 호출 - 텍스트 세팅 중");
            rewardText.text = text;
            rewardPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[RewardManager] ShowRewardPanel 호출 실패 - rewardPanel 또는 rewardText가 할당되지 않았습니다.");
        }
    }

    /// <summary>
    /// 보상 패널 숨기기
    /// </summary>
    public void HideRewardPanel()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }

    public void ClearDefeatedEnemies()
    {
        defeatedEnemies.Clear();
    }
}