using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager instance;

    [Header("UI 패널 연결")]
    public GameObject battlePanel;
    public GameObject roguelikePanel;
    public GameObject rewardPanel;
    public GameObject defeatPanel;

    [Header("결과 패널 버튼")]
    public Button rewardContinueButton;
    public Button defeatContinueButton;

    [Header("멀리건 UI")]
    public GameObject mulliganIconPrefab;
    public Transform mulliganIconParent;
    private List<GameObject> activeMulliganIcons = new List<GameObject>();

    [Header("캔슬 UI")]
    public GameObject cancelIconPrefab;
    public Transform cancelIconParent;
    private List<GameObject> activeCancelIcons = new List<GameObject>();

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }

        if (CoreEventManager.instance != null)
        {
            CoreEventManager.instance.OnBattleSceneReady += HandleBattleSceneReady;
        }

        if (rewardContinueButton != null) rewardContinueButton.onClick.AddListener(OnRewardContinueClicked);
        if (defeatContinueButton != null) defeatContinueButton.onClick.AddListener(OnDefeatContinueClicked);
    }

    void OnEnable()
    {
        CardManager.OnMulliganCountChanged += UpdateMulliganIcons;
        GameManager.OnCancelCountChanged += UpdateCancelIcons;
    }

    void OnDisable()
    {
        CardManager.OnMulliganCountChanged -= UpdateMulliganIcons;
        GameManager.OnCancelCountChanged -= UpdateCancelIcons;
    }

    void OnDestroy()
    {
        if (CoreEventManager.instance != null)
        {
            CoreEventManager.instance.OnBattleSceneReady -= HandleBattleSceneReady;
        }
    }

    // ▼▼▼ 단축키 기능을 위해 Update 함수 추가 ▼▼▼
    void Update()
    {
        // 스페이스바가 눌렸는지 확인
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleSpacebarPress();
        }
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    private void HandleBattleSceneReady()
    {
        if (rewardPanel != null) rewardPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        if (GlobalManager.instance == null)
        {
            Debug.LogError("[BattleUIManager] GlobalManager를 찾을 수 없습니다!");
            return;
        }

        GameMode currentMode = GlobalManager.instance.currentGameMode;
        if (currentMode == GameMode.RoguelikeBattle)
        {
            ShowRoguelikeUI();
        }
        else // Tycoon Mode
        {
            ShowBattleUI();
            if (CoreEventManager.instance != null)
            {
                CoreEventManager.instance.RaiseCombatStartRequested();
            }
        }
    }

    public void ShowRoguelikeUI()
    {
        if (battlePanel != null) battlePanel.SetActive(false);
        if (roguelikePanel != null) roguelikePanel.SetActive(true);
    }

    public void ShowBattleUI()
    {
        if (battlePanel != null) battlePanel.SetActive(true);
        if (roguelikePanel != null) roguelikePanel.SetActive(false);
    }

    public void ShowRewardPanel()
    {
        if (battlePanel != null) battlePanel.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(true);
    }

    public void ShowDefeatPanel()
    {
        if (battlePanel != null) battlePanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(true);
    }

    private void OnRewardContinueClicked()
    {
        GameMode currentMode = GlobalManager.instance.currentGameMode;
        if (currentMode == GameMode.RoguelikeBattle)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CleanupCombat();
            }
            if (rewardPanel != null) rewardPanel.SetActive(false);
            ShowRoguelikeUI();
        }
        else
        {
            CoreEventManager.instance.RaiseSceneChangeRequested("tycoon");
        }
    }

    private void OnDefeatContinueClicked()
    {
        CoreEventManager.instance.RaiseSceneChangeRequested("CoreScene");
    }

    private void UpdateMulliganIcons(int count)
    {
        foreach (GameObject icon in activeMulliganIcons)
        {
            Destroy(icon);
        }
        activeMulliganIcons.Clear();

        if (mulliganIconPrefab == null || mulliganIconParent == null) return;

        for (int i = 0; i < count; i++)
        {
            GameObject newIcon = Instantiate(mulliganIconPrefab, mulliganIconParent);
            activeMulliganIcons.Add(newIcon);
        }
    }

    private void UpdateCancelIcons(int count)
    {
        foreach (GameObject icon in activeCancelIcons)
        {
            Destroy(icon);
        }
        activeCancelIcons.Clear();

        if (cancelIconPrefab == null || cancelIconParent == null) return;

        for (int i = 0; i < count; i++)
        {
            GameObject newIcon = Instantiate(cancelIconPrefab, cancelIconParent);
            activeCancelIcons.Add(newIcon);
        }
    }

    // ▼▼▼ 단축키 처리 함수 추가 ▼▼▼
    private void HandleSpacebarPress()
    {
        if (GameManager.Instance == null || CardManager.instance == null) return;

        if (GameManager.Instance.GetCurrentPhase() == GameManager.BattlePhase.PlayerTurn_CardSelection)
        {
            int currentActionCards = CardManager.instance.GetActionCardCount();
            int maxActionCards = GlobalManager.instance.playerMaxActionsPerTurn;

            if (currentActionCards >= maxActionCards)
            {
                // 액션 카드가 가득 찼다면: '진행' 버튼 클릭
                if (GameManager.Instance.proceedButton != null && GameManager.Instance.proceedButton.gameObject.activeInHierarchy)
                {
                    GameManager.Instance.proceedButton.onClick.Invoke();
                }
            }
            else
            {
                // 액션 카드가 비어있다면: '멀리건' 버튼 클릭
                if (CardManager.instance.mulliganButton != null && CardManager.instance.mulliganButton.interactable)
                {
                    CardManager.instance.mulliganButton.onClick.Invoke();
                }
            }
        }
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
}