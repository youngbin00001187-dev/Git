using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public enum GameMode
{
    Tycoon,         // 객잔 경영 RPG 모드
    RoguelikeBattle // 전투 로그라이크 모드
}
public class GlobalManager : MonoBehaviour
{
    public static GlobalManager instance;
    [Header("게임 모드")]
    [Tooltip("현재 플레이 중인 게임 모드입니다.")]
    public GameMode currentGameMode;

    [Header("핵심 데이터")]
    public int gold;
    public int reputation;
    public Dictionary<IngredientData, int> inventory = new Dictionary<IngredientData, int>();

    [Header("플레이어 카드 컬렉션")]
    [Tooltip("플레이어가 현재 소유하고 있는 모든 카드 데이터 목록입니다.")]
    public List<CardDataSO> playerCardCollection = new List<CardDataSO>();

    [Header("플레이어 심법 및 덱 정보")]
    [Tooltip("플레이어가 소유하고 있는 모든 심법 목록입니다.")]
    public List<SimbeopDataSO> ownedSimbeops = new List<SimbeopDataSO>();
    [Tooltip("플레이어가 현재 장착 중인 심법입니다.")]
    public SimbeopDataSO equippedSimbeop;
    [Tooltip("심법 조건에 맞춰 최종적으로 구성된, 실제 전투에서 사용할 덱입니다.")]
    public List<CardDataSO> playerBattleDeck = new List<CardDataSO>();

    [Header("무공 숙련도")]
    public Dictionary<int, int> martialArtProficiency = new Dictionary<int, int>();

    [Header("플레이어 능력치")]
    public int playerBaseMaxHealth = 100;
    public int playerCurrentHealth;
    public int playerMaxActionsPerTurn = 2;
    public int playerAttackPower = 0;
    public float simbeopDamageMultiplier = 0f;
    public int playerCancelCountPerRound = 2;

    // --- ▼▼▼ 이 부분이 핵심 수정 내용입니다 ▼▼▼ ---
    [Header("테스트용 재료 데이터")]
    [Tooltip("테스트용으로, 게임 시작 시 플레이어가 소지할 재료 목록입니다.")]
    public List<IngredientData> testIngredients;
    [Tooltip("테스트용으로, 게임 시작 시 플레이어가 소지할 재료의 수량입니다.")]
    public List<int> testIngredientQuantities;
    // --- ▲▲▲ 핵심 수정 내용 끝 ▲▲▲ ---

    private List<EnemyDataSO> _enemiesToTransfer;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // 테스트용 재료 인벤토리를 초기화하는 함수를 다시 호출합니다.
            InitializeInventoryForTest();

            // 체력처럼 게임 시작 시 반드시 초기값이 필요한 경우만 처리합니다.
            if (playerCurrentHealth == 0)
            {
                playerCurrentHealth = playerBaseMaxHealth;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// [복구] 테스트를 위해 인벤토리를 채우는 함수입니다.
    /// </summary>
    void InitializeInventoryForTest()
    {
        inventory.Clear();
        if (testIngredients != null && testIngredientQuantities != null && testIngredients.Count == testIngredientQuantities.Count)
        {
            for (int i = 0; i < testIngredients.Count; i++)
            {
                if (testIngredients[i] != null)
                {
                    inventory.Add(testIngredients[i], testIngredientQuantities[i]);
                }
            }
        }
    }

    public int GetProficiency(int martialArtID)
    {
        martialArtProficiency.TryGetValue(martialArtID, out int proficiency);
        return proficiency;
    }

    public void AddProficiency(int martialArtID, int amount)
    {
        if (martialArtID == 0) return;

        if (martialArtProficiency.ContainsKey(martialArtID))
        {
            martialArtProficiency[martialArtID] += amount;
        }
        else
        {
            martialArtProficiency.Add(martialArtID, amount);
        }
        Debug.Log($"무공 ID {martialArtID}의 숙련도가 {amount}만큼 증가했습니다. 현재 숙련도: {martialArtProficiency[martialArtID]}");
    }

    public void SetEnemiesForBattle(List<EnemyDataSO> enemyDataList)
    {
        _enemiesToTransfer = enemyDataList;
    }

    public List<EnemyDataSO> GetEnemiesForBattle()
    {
        List<EnemyDataSO> data = _enemiesToTransfer;
        _enemiesToTransfer = null;
        return data;
    }
}