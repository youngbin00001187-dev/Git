using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.UI.LayoutRebuilder;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;
    public static event System.Action<int> OnMulliganCountChanged;

    public PlayerController player;
    public TileManager tileManager;
    private GameManager gameManager;

    private CardDataSO selectedCardForTargeting;
    private GameObject selectedCardUIForTargeting;
    private bool isTargetingMode = false;

    private CardDataSO hoveredCardDataForPreview;

    [Header("UI Settings")]
    public GameObject cardUIPrefab;
    public Transform handTransform;
    public Transform actionPanelTransform;

    [Header("멀리건 UI")]
    public Button mulliganButton;

    [Header("UI Animation Settings")]
    public float cardMoveDuration = 0.2f;

    private List<CardDataSO> deck = new List<CardDataSO>();
    private List<CardDataSO> hand = new List<CardDataSO>();
    private List<CardDataSO> actionCards = new List<CardDataSO>();
    private List<CardDataSO> discardPile = new List<CardDataSO>();
    private List<GameObject> handUIObjects = new List<GameObject>();
    private List<GameObject> actionCardUIObjects = new List<GameObject>();

    private int mulligansLeftThisTurn;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (mulliganButton != null)
        {
            mulliganButton.onClick.AddListener(PerformMulligan);
        }
    }

    void Update()
    {
        HandleHighlightUpdate();
        if (isTargetingMode && Input.GetMouseButtonDown(0))
        {
            OnTileClickedForTargeting();
        }
    }

    public void SetupNewRound()
    {
        discardPile.AddRange(actionCards);
        actionCards.Clear();
        foreach (var ui in actionCardUIObjects) Destroy(ui);
        actionCardUIObjects.Clear();

        int mulligansForThisRound = 1;
        if (GlobalManager.instance != null && GlobalManager.instance.equippedSimbeop != null)
        {
            mulligansForThisRound = GlobalManager.instance.equippedSimbeop.mulliganPerTurn;
        }
        mulligansLeftThisTurn = mulligansForThisRound;
        UpdateMulliganUI();
        OnMulliganCountChanged?.Invoke(mulligansLeftThisTurn);

        if (deck.Count == 0 && discardPile.Count == 0 && hand.Count == 0)
        {
            deck.AddRange(GlobalManager.instance.playerBattleDeck);
            deck = deck.OrderBy(a => System.Guid.NewGuid()).ToList();
        }

        int maxHandSize = GlobalManager.instance.equippedSimbeop != null ? GlobalManager.instance.equippedSimbeop.maxHandSize : 7;
        int requiredDraws = maxHandSize - hand.Count;

        for (int i = 0; i < requiredDraws; i++)
        {
            DrawCard();
        }
    }

    public void PerformMulligan()
    {
        if (gameManager.GetCurrentPhase() != GameManager.BattlePhase.PlayerTurn_CardSelection) return;
        if (mulligansLeftThisTurn <= 0) return;
        mulligansLeftThisTurn--;
        UpdateMulliganUI();
        OnMulliganCountChanged?.Invoke(mulligansLeftThisTurn);
        discardPile.AddRange(hand);
        hand.Clear();
        foreach (var ui in handUIObjects) Destroy(ui);
        handUIObjects.Clear();
        int maxHandSize = GlobalManager.instance.equippedSimbeop != null ? GlobalManager.instance.equippedSimbeop.maxHandSize : 7;
        for (int i = 0; i < maxHandSize; i++)
        {
            DrawCard();
        }
    }

    private void UpdateMulliganUI()
    {
        if (mulliganButton != null)
        {
            mulliganButton.interactable = (mulligansLeftThisTurn > 0 && gameManager.GetCurrentPhase() == GameManager.BattlePhase.PlayerTurn_CardSelection);
        }
    }

    void DrawCard()
    {
        if (deck.Count == 0)
        {
            if (discardPile.Count > 0)
            {
                deck.AddRange(discardPile);
                discardPile.Clear();
                deck = deck.OrderBy(a => System.Guid.NewGuid()).ToList();
            }
            else return;
        }

        CardDataSO drawnCard = deck[0];
        hand.Add(drawnCard);
        deck.RemoveAt(0);
        GameObject newCardUI = Instantiate(cardUIPrefab, handTransform);
        newCardUI.GetComponent<CardUI>().Setup(drawnCard, this);
        handUIObjects.Add(newCardUI);
    }

    private void OnTileClickedForTargeting()
    {
        RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
        if (hit.collider == null || !hit.collider.CompareTag("Tile"))
        {
            isTargetingMode = false;
            selectedCardForTargeting = null;
            selectedCardUIForTargeting = null;
            if (tileManager != null) tileManager.ClearPlayerHighlights();
            return;
        }

        GameObject clickedTile = hit.collider.gameObject;
        var sequence = selectedCardForTargeting.actionSequence;
        GameAction targetingAction = sequence.OfType<MoveAction>().FirstOrDefault() ?? (GameAction)sequence.OfType<AttackAction>().FirstOrDefault();

        if (targetingAction == null) return;

        List<GameObject> targetableTiles = targetingAction.GetTargetableTiles(player);
        if (!targetableTiles.Contains(clickedTile))
        {
            return;
        }

        // --- ▼▼▼ [핵심 수정] 로직 분기 처리 ▼▼▼ ---
        if (GameManager.Instance != null && GameManager.Instance.IsBonusTimeActive())
        {
            // [분기 1] 보너스 타임: '캔슬' 시도
            if (GameManager.Instance.TryPerformCancel())
            {
                Debug.Log("<color=purple>[CardManager] 캔슬 성공. 인터럽트 큐에 액션을 추가하고 다음 카드를 선택합니다.</color>");

                List<GameAction> actionsToInterrupt = CreateActionsFromCard(selectedCardForTargeting, clickedTile);
                GameManager.Instance.AddActionsToInterruptQueue(actionsToInterrupt);

                if (selectedCardUIForTargeting != null)
                {
                    actionCards.Remove(selectedCardForTargeting);
                    actionCardUIObjects.Remove(selectedCardUIForTargeting);
                    Destroy(selectedCardUIForTargeting);
                }
                discardPile.Add(selectedCardForTargeting);

                // 즉시 다음 카드 자동 선택
                SelectNextActionCard();
            }
            // 캔슬 실패 시에는 아무것도 하지 않고 타겟팅 모드를 유지합니다.
        }
        else
        {
            // [분기 2] 일반 상황: 일반 액션 큐에 추가
            Debug.Log("[CardManager] 일반 행동을 실행합니다.");
            StartCoroutine(ActionCoroutine(selectedCardForTargeting, clickedTile, selectedCardUIForTargeting));
        }
    }

    private List<GameAction> CreateActionsFromCard(CardDataSO card, GameObject clickedTile)
    {
        List<GameAction> executableActions = new List<GameAction>();
        foreach (var actionTemplate in card.actionSequence)
        {
            actionTemplate.Prepare(player, clickedTile);
            executableActions.Add(actionTemplate);
        }
        return executableActions;
    }

    IEnumerator ActionCoroutine(CardDataSO card, GameObject clickedTile, GameObject cardUI)
    {
        if (cardUI != null)
        {
            actionCards.Remove(card);
            actionCardUIObjects.Remove(cardUI);
            Destroy(cardUI);
        }
        discardPile.Add(card);

        List<GameAction> executableActions = CreateActionsFromCard(card, clickedTile);
        GameManager.Instance.AddActionsToQueue(executableActions);

        if (GameManager.Instance != null && GameManager.Instance.GetCurrentActingUnit() is PlayerController)
        {
            if (EventManager.Instance != null) EventManager.Instance.RaisePlayerActionCompleted();
        }

        // ▼▼▼ [핵심 수정] 일반 행동 후에도 CardManager가 직접 다음 카드를 선택합니다. ▼▼▼
        SelectNextActionCard();
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        yield return null;
    }

    public void SetGameManager(GameManager manager) { this.gameManager = manager; }

    public void HandleHighlightUpdate()
    {
        if (tileManager == null || player == null) return;
        tileManager.ClearPlayerHighlights();
        CardDataSO cardToDisplay = hoveredCardDataForPreview ?? selectedCardForTargeting;
        if (cardToDisplay == null) return;
        var sequence = cardToDisplay.actionSequence;
        if (!sequence.Any()) return;
        GameAction targetingAction = sequence.OfType<MoveAction>().FirstOrDefault() ?? (GameAction)sequence.OfType<AttackAction>().FirstOrDefault();
        if (targetingAction == null) return;
        List<GameObject> displayableTiles = targetingAction.GetTargetableTiles(player);
        Color baseColor = (targetingAction is MoveAction) ? Color.green : Color.yellow;
        tileManager.SetTilesHighlight(TileManager.HighlightType.PlayerTarget, displayableTiles, baseColor);
        AttackAction impactAction = sequence.OfType<AttackAction>().LastOrDefault();
        if (impactAction != null)
        {
            RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
            GameObject hoveredTile = (hit.collider != null && hit.collider.CompareTag("Tile")) ? hit.collider.gameObject : null;
            if (hoveredTile != null && displayableTiles.Contains(hoveredTile))
            {
                List<GameObject> impactTiles = impactAction.GetActionImpactTiles(player, hoveredTile);
                tileManager.SetTilesHighlight(TileManager.HighlightType.PlayerPreview, impactTiles, Color.red);
            }
        }
    }

    public void OnActionPanelCardClicked(CardDataSO clickedCardData, GameObject clickedCardUIObject)
    {
        if (gameManager.GetCurrentPhase() != GameManager.BattlePhase.ActionPhase) return;
        isTargetingMode = true;
        selectedCardForTargeting = clickedCardData;
        selectedCardUIForTargeting = clickedCardUIObject;
    }

    private IEnumerator AnimateCardMovement(GameObject cardUIObject, Transform targetParent)
    {
        RectTransform cardRect = cardUIObject.GetComponent<RectTransform>();
        if (cardRect == null) yield break;
        Vector3 startWorldPosition = cardRect.position;
        Transform canvasRoot = cardUIObject.transform.root;
        cardRect.SetParent(canvasRoot);
        cardRect.position = startWorldPosition;
        cardRect.SetParent(targetParent);
        LayoutGroup targetLayoutGroup = targetParent.GetComponent<LayoutGroup>();
        if (targetLayoutGroup != null)
        {
            ForceRebuildLayoutImmediate(targetParent.GetComponent<RectTransform>());
        }
        Vector3 targetWorldPosition = cardRect.position;
        cardRect.SetParent(canvasRoot);
        cardRect.position = startWorldPosition;
        float elapsedTime = 0f;
        while (elapsedTime < cardMoveDuration)
        {
            cardRect.position = Vector3.Lerp(startWorldPosition, targetWorldPosition, elapsedTime / cardMoveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cardRect.position = targetWorldPosition;
        cardRect.SetParent(targetParent);
        cardRect.localPosition = targetParent.InverseTransformPoint(targetWorldPosition);
        if (targetLayoutGroup != null)
        {
            ForceRebuildLayoutImmediate(targetParent.GetComponent<RectTransform>());
        }
    }

    public void SelectCard(GameObject cardUI, CardDataSO cardData)
    {
        if (gameManager.GetCurrentPhase() != GameManager.BattlePhase.PlayerTurn_CardSelection) return;
        if (GlobalManager.instance != null && actionCards.Count >= GlobalManager.instance.playerMaxActionsPerTurn) return;
        hand.Remove(cardData);
        actionCards.Add(cardData);
        handUIObjects.Remove(cardUI);
        actionCardUIObjects.Add(cardUI);
        StartCoroutine(AnimateCardMovement(cardUI, actionPanelTransform));
        gameManager.CheckProceedButtonState();
    }

    public void ReturnCardToHand(GameObject cardUI, CardDataSO cardData)
    {
        actionCards.Remove(cardData);
        hand.Add(cardData);
        actionCardUIObjects.Remove(cardUI);
        handUIObjects.Add(cardUI);
        StartCoroutine(AnimateCardMovement(cardUI, handTransform));
        gameManager.CheckProceedButtonState();
    }

    public void HandleCardHoverEnter(CardDataSO cardData)
    {
        hoveredCardDataForPreview = cardData;
    }

    public void HandleCardHoverExit()
    {
        hoveredCardDataForPreview = null;
    }

    public int GetActionCardCount() { return actionCards.Count; }

    public void BeginActionPhase()
    {
        SelectNextActionCard();
    }

    public void SelectNextActionCard()
    {
        selectedCardForTargeting = null;
        selectedCardUIForTargeting = null;
        if (actionCards.Any() && actionCardUIObjects.Any())
        {
            selectedCardForTargeting = actionCards[0];
            selectedCardUIForTargeting = actionCardUIObjects[0];
            isTargetingMode = true;
        }
        else
        {
            isTargetingMode = false;
        }
    }
}