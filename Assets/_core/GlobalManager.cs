using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public enum GameMode
{
    Tycoon,         // ���� �濵 RPG ���
    RoguelikeBattle // ���� �α׶���ũ ���
}
public class GlobalManager : MonoBehaviour
{
    public static GlobalManager instance;
    [Header("���� ���")]
    [Tooltip("���� �÷��� ���� ���� ����Դϴ�.")]
    public GameMode currentGameMode;

    [Header("�ٽ� ������")]
    public int gold;
    public int reputation;
    public Dictionary<IngredientData, int> inventory = new Dictionary<IngredientData, int>();

    [Header("�÷��̾� ī�� �÷���")]
    [Tooltip("�÷��̾ ���� �����ϰ� �ִ� ��� ī�� ������ ����Դϴ�.")]
    public List<CardDataSO> playerCardCollection = new List<CardDataSO>();

    [Header("�÷��̾� �ɹ� �� �� ����")]
    [Tooltip("�÷��̾ �����ϰ� �ִ� ��� �ɹ� ����Դϴ�.")]
    public List<SimbeopDataSO> ownedSimbeops = new List<SimbeopDataSO>();
    [Tooltip("�÷��̾ ���� ���� ���� �ɹ��Դϴ�.")]
    public SimbeopDataSO equippedSimbeop;
    [Tooltip("�ɹ� ���ǿ� ���� ���������� ������, ���� �������� ����� ���Դϴ�.")]
    public List<CardDataSO> playerBattleDeck = new List<CardDataSO>();

    [Header("���� ���õ�")]
    public Dictionary<int, int> martialArtProficiency = new Dictionary<int, int>();

    [Header("�÷��̾� �ɷ�ġ")]
    public int playerBaseMaxHealth = 100;
    public int playerCurrentHealth;
    public int playerMaxActionsPerTurn = 2;
    public int playerAttackPower = 0;
    public float simbeopDamageMultiplier = 0f;
    public int playerCancelCountPerRound = 2;

    // --- ���� �� �κ��� �ٽ� ���� �����Դϴ� ���� ---
    [Header("�׽�Ʈ�� ��� ������")]
    [Tooltip("�׽�Ʈ������, ���� ���� �� �÷��̾ ������ ��� ����Դϴ�.")]
    public List<IngredientData> testIngredients;
    [Tooltip("�׽�Ʈ������, ���� ���� �� �÷��̾ ������ ����� �����Դϴ�.")]
    public List<int> testIngredientQuantities;
    // --- ���� �ٽ� ���� ���� �� ���� ---

    private List<EnemyDataSO> _enemiesToTransfer;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // �׽�Ʈ�� ��� �κ��丮�� �ʱ�ȭ�ϴ� �Լ��� �ٽ� ȣ���մϴ�.
            InitializeInventoryForTest();

            // ü��ó�� ���� ���� �� �ݵ�� �ʱⰪ�� �ʿ��� ��츸 ó���մϴ�.
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
    /// [����] �׽�Ʈ�� ���� �κ��丮�� ä��� �Լ��Դϴ�.
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
        Debug.Log($"���� ID {martialArtID}�� ���õ��� {amount}��ŭ �����߽��ϴ�. ���� ���õ�: {martialArtProficiency[martialArtID]}");
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